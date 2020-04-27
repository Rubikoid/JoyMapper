using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper {
    /**
     * A wrapper around the DirectInput Joystick class.
     **/
    public partial class GameController : IController {
        private static Dictionary<JoystickOffset, JoystickCapabilities> joysticCapsMap = new Dictionary<JoystickOffset, JoystickCapabilities>
        {
            { JoystickOffset.X, JoystickCapabilities.AXIS_X },
            { JoystickOffset.Y, JoystickCapabilities.AXIS_Y },
            { JoystickOffset.Z, JoystickCapabilities.AXIS_Z },
            { JoystickOffset.RotationX, JoystickCapabilities.AXIS_RX },
            { JoystickOffset.RotationY, JoystickCapabilities.AXIS_RY },
            { JoystickOffset.RotationZ, JoystickCapabilities.AXIS_RZ },
            { JoystickOffset.Sliders0, JoystickCapabilities.SLIDER_0 },
            { JoystickOffset.Sliders1, JoystickCapabilities.SLIDER_1 },
            { JoystickOffset.PointOfViewControllers0, JoystickCapabilities.POV },
        };

        public string Name { get; private set; }
        public bool Connected { get; private set; } = false;

        public Guid ID { get; private set; }
        public Guid FFBDriverID { get; private set; }


        public List<JoystickCapabilities> Capabilities { get; private set; }
        public int ButtonCount { get; private set; }
        public int ContinuousPOVCount { get; private set; }
        public int DirectionalPOVCount { get; private set; }
        public IList<Guid> SupportedFFBEffects { get; private set; }
        public List<VirtualController.DirectInput.EffectInfo> SupportedFFBEffects_Full { get; private set; }
        public List<VirtualController.DirectInput.Effect> ForceFeedbackEffects = new List<VirtualController.DirectInput.Effect>();
        public Dictionary<VirtualController.AxisType, int> FFBAxisIds = new Dictionary<VirtualController.AxisType, int>();

        private State internalState { get; set; }
        public IList<IMap> Mappings { get; private set; } = new List<IMap>();

        public Joystick joystick = null;
        private static DirectInput directInput = new DirectInput();

        private object lockObj = 1;

        public void Connect() {
            if (!this.Connected) {
                if (this.joystick == null) {
                    this.joystick = new Joystick(GameController.directInput, this.ID);
                }
                this.SetCooperativeLevel(CooperativeLevel.Exclusive | CooperativeLevel.Background);
                this.joystick.Acquire();
                this.loadCapabilities();

                // this.SetCooperativeLevel(CooperativeLevel.NonExclusive | CooperativeLevel.Background);
                // this.joystick.Acquire();

                if (this.SupportedFFBEffects.Count > 0)
                    ControllerCache.vc.FFBDataReceived += this.OnIOEvent;
                this.Connected = true;
            }
        }

        public void Disconnect() {
            if (this.Connected) {
                this.joystick.Unacquire();
                if (this.SupportedFFBEffects.Count > 0)
                    ControllerCache.vc.FFBDataReceived -= this.OnIOEvent;
                this.Connected = false;
            }
        }

        private void loadCapabilities() {
            this.Capabilities = new List<JoystickCapabilities>();
            // a little hacky solution, thanks to @evilC, https://github.com/sharpdx/SharpDX/issues/866, https://github.com/evilC/JoystickWrapper/blob/master/JoystickWrapper/JoystickWrapper.cs#L248
            foreach (KeyValuePair<JoystickOffset, JoystickCapabilities> cap in joysticCapsMap) {
                try {
                    var info = this.joystick.GetObjectInfoByName(cap.Key.ToString());
                    this.Capabilities.Add(cap.Value);
                } catch { }
            }

            this.SupportedFFBEffects = new List<Guid>();
            foreach (EffectInfo effectInfo in this.joystick.GetEffects()) {
                this.SupportedFFBEffects.Add(effectInfo.Guid);
            }

            this.ButtonCount = this.joystick.Capabilities.ButtonCount;

            this.ContinuousPOVCount = this.joystick.Capabilities.PovCount;
            this.DirectionalPOVCount = this.joystick.Capabilities.PovCount; // shrug, i belive i don't need this shit

                
            // ReVirtualController
            this.FFBAxisIds = this.GetFFBAxisIdsList();

            this.SupportedFFBEffects_Full = new List<VirtualController.DirectInput.EffectInfo>();
            using (IEnumerator<SharpDX.DirectInput.EffectInfo> enumerator3 = this.joystick.GetEffects().GetEnumerator()) {
                while (enumerator3.MoveNext()) {
                    SharpDX.DirectInput.EffectInfo effectInfo = enumerator3.Current;
                    VirtualController.DirectInput.EffectInfo effectInfo2 = new VirtualController.DirectInput.EffectInfo();
                    effectInfo2.Guid = effectInfo.Guid;
                    effectInfo2.ParentDevice = this.joystick;
                    effectInfo2.StaticParameters = effectInfo.StaticParameters;
                    this.SupportedFFBEffects_Full.Add(effectInfo2);
                }
            }
        }

        private void FillInternalInfo() {
            JoystickState iState = this.GetState();
            this.internalState = new State(ControllerCache.vc, this.ButtonCount);

            if (this.Capabilities.Contains(JoystickCapabilities.AXIS_X)) // double-check capbs
                this.internalState.AxisX.setVal(iState.X, (long)Math.Pow(2, 16), -1 * (long)Math.Pow(2, 16));

            if (this.Capabilities.Contains(JoystickCapabilities.AXIS_Y))
                this.internalState.AxisY.setVal(iState.Y, (long)Math.Pow(2, 16), -1 * (long)Math.Pow(2, 16));

            if (this.Capabilities.Contains(JoystickCapabilities.AXIS_Z))
                this.internalState.AxisZ.setVal(iState.Z, (long)Math.Pow(2, 16), -1 * (long)Math.Pow(2, 16));

            if (this.Capabilities.Contains(JoystickCapabilities.AXIS_RX))
                this.internalState.AxisXR.setVal(iState.RotationX, (long)Math.Pow(2, 16), -1 * (long)Math.Pow(2, 16));

            if (this.Capabilities.Contains(JoystickCapabilities.AXIS_RY))
                this.internalState.AxisYR.setVal(iState.RotationY, (long)Math.Pow(2, 16), -1 * (long)Math.Pow(2, 16));

            if (this.Capabilities.Contains(JoystickCapabilities.AXIS_RZ))
                this.internalState.AxisZR.setVal(iState.RotationZ, (long)Math.Pow(2, 16), -1 * (long)Math.Pow(2, 16));

            //if (iState.Buttons.Length != this.ButtonCount)
            //    throw new Exception($"WTF {this.Name} {iState.Buttons.Length} != {this.ButtonCount}");

            for (int i = 0; i < this.ButtonCount /* iState.Buttons.Length */; i++)
                this.internalState.buttons[i].setVal(iState.Buttons[i]);
        }

        public void FillExternalInfo(ref State state) {
            FillInternalInfo();
            foreach (IMap map in this.Mappings)
                map.Map(this.internalState, ref state);
        }

        public void UpdateInfo(in State inState) { }

        public JoystickState GetState() {
            return this.Connected ? this.joystick.GetCurrentState() : null;
        }

        /*
        public void SendFFBEffect(object sender, FFBEventArgs args) {
            if (this.Connected && args.EffectGuid != Guid.Empty) {
                Effect effect = new Effect(this.joystick, args.EffectGuid, args.Parameters);
                if (effect != null) {
                    effect.Start(args.LoopCount, EffectPlayFlags.NoDownload);
                } else {
                    // effect not supported
                    throw new Exception($"Force Feedback Effect '{args.EffectGuid}' is not supported.");
                }
            }
        }
        */

        /**
         * Returns a list of all connected game controllers.
         **/
        public static IList<GameController> GetAll() {
            List<GameController> devices = new List<GameController>();
            IList<DeviceInstance> connectedDevices = GameController.directInput.GetDevices(
                DeviceClass.GameControl,
                DeviceEnumerationFlags.AttachedOnly
            );

            foreach (DeviceInstance deviceInstance in connectedDevices) {
                devices.Add(new GameController() {
                    Name = deviceInstance.InstanceName,
                    ID = deviceInstance.InstanceGuid,
                    FFBDriverID = deviceInstance.ForceFeedbackDriverGuid
                });
            }

            return devices;
        }

        public override string ToString() {
            return string.Format("Device ID: {0}, Device Name: {1}", this.ID, this.Name);
        }
    }
}
