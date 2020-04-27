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
    public class GameController : IController {
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

        public Joystick joystick;
        private static DirectInput directInput = new DirectInput();

        public void Connect() {
            if (!this.Connected) {
                this.joystick = new Joystick(GameController.directInput, this.ID);
                this.joystick.Acquire();
                this.loadCapabilities();
                this.Connected = true;
            }
        }

        public void Disconnect() {
            if (this.Connected) {
                this.joystick.Unacquire();
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
        }

        public void FillInfo(ref State state) {
            JoystickState x = this.GetState();
            state.AxisXR.setVal(x.RotationX, (long)Math.Pow(2, 16), -1 * (long)Math.Pow(2, 16));
        }

        public void UpdateInfo(in State inState) { }

        public JoystickState GetState() {
            return this.Connected ? this.joystick.GetCurrentState() : null;
        }

        public void SendFFBEffect(Guid effectGuid, EffectParameters effectParams) {
            if (this.Connected) {
                Effect effect = new Effect(this.joystick, effectGuid, effectParams);
                if (effect == null)
                //{
                //    effect.Start(loopCount, EffectPlayFlags.NoDownload);
                //}
                //else
                {
                    // effect not supported
                    throw new Exception(string.Format("Force Feedback Effect '{0}' is not supported.", effectGuid));
                }
            }
        }

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
