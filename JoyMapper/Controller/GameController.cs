using JoyMapper.Controller.Internal;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper.Controller {
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
        public int[] FFBAxes { get; private set; }

        private State internalState { get; set; }
        public IList<IMap> Mappings { get; private set; } = new List<IMap>();

        public Joystick joystick = null;
        private static DirectInput directInput = new DirectInput();

        public void Connect() {
            if (!this.Connected) {
                if (this.joystick == null)
                    this.joystick = new Joystick(GameController.directInput, this.ID);
                this.SetCooperativeLevel(CooperativeLevel.Exclusive | CooperativeLevel.Background);
                this.joystick.Properties.AxisMode = DeviceAxisMode.Absolute;
                this.joystick.Properties.AutoCenter = false;

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

        public void SetCooperativeLevel(SharpDX.DirectInput.CooperativeLevel Level) {
            this.joystick.SetCooperativeLevel(MainForm.PublicHandle, Level);
        }

        public void RunExclusive(Action action) {
            try {
                action();
            } catch (SharpDX.SharpDXException ex) {
                if (ex.HResult == -2147220987) {
                    this.joystick.Unacquire();
                    this.SetCooperativeLevel(CooperativeLevel.Exclusive | CooperativeLevel.Background);
                    this.joystick.Acquire();
                    action();
                    Console.WriteLine("Reaq joystic in exclusive mode because of idk why it brokes");
                } else
                    throw ex;
            }
        }

        public void SendForceFeedbackCommand(ForceFeedbackCommand FFBCommand) {
            // this.SetCooperativeLevel(SharpDX.DirectInput.CooperativeLevel.Exclusive | SharpDX.DirectInput.CooperativeLevel.Background);
            this.RunExclusive(() => this.joystick.SendForceFeedbackCommand(FFBCommand));
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

            FFBAxes = null;
            this.joystick.Properties.Range = new InputRange(-1 * (int)Math.Pow(2, 16), (int)Math.Pow(2, 16));
            // Enumerate any axes
            foreach (DeviceObjectInstance doi in this.joystick.GetObjects()) {
                // Console.WriteLine($"{doi.Name}\t\t{doi.ObjectId.Flags}\t\t{doi.ObjectType}");
                if (doi.ObjectType == ObjectGuid.XAxis || doi.ObjectType == ObjectGuid.YAxis ||
                    doi.ObjectType == ObjectGuid.ZAxis || doi.ObjectType == ObjectGuid.RxAxis ||
                    doi.ObjectType == ObjectGuid.RyAxis || doi.ObjectType == ObjectGuid.RzAxis) {
                    // We found an axis, set the range to a max of 10,000
                    /*Dev.Properties.SetRange(ParameterHow.ById,
                    doi.ObjectId, new InputRange(-5000, 5000)); */
                }

                int[] temp;
                // Get info about first two FF axii on the device
                if ((doi.ObjectId.Flags & DeviceObjectTypeFlags.ForceFeedbackActuator) != 0) {
                    if (FFBAxes != null) {
                        temp = new int[FFBAxes.Length + 1];
                        FFBAxes.CopyTo(temp, 0);
                        FFBAxes = temp;
                    } else {
                        FFBAxes = new int[1];
                    }

                    // Store the offset of each axis.
                    FFBAxes[FFBAxes.Length - 1] = (int)doi.ObjectId;
                    if (FFBAxes.Length == 2) {
                        break;
                    }
                }
            }
            Console.WriteLine($"Loaded axis: {string.Join(", ", FFBAxes.Select(x => x.ToString())) }");
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

        public void SendFFBEffect(Guid effectGuid, EffectParameters effectParams) {
            if (this.Connected) {
                Effect effect = new Effect(this.joystick, effectGuid, effectParams);
                if (effect != null) {
                    effect.Start(EffectPlayFlags.NoDownload);
                } else {
                    // effect not supported
                    throw new Exception($"Force Feedback Effect '{effectGuid}' is not supported.");
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
                DeviceEnumerationFlags.AllDevices
            );

            foreach (DeviceInstance deviceInstance in connectedDevices) {
                //if (deviceInstance.InstanceName.ToLower().Contains("vjoy"))
                //    continue; // skip vjoy devices
                devices.Add(new GameController() {
                    Name = deviceInstance.InstanceName,
                    ID = deviceInstance.InstanceGuid,
                    FFBDriverID = deviceInstance.ForceFeedbackDriverGuid
                });
            }

            return devices;
        }

        public override string ToString() {
            return string.Format("{1}, {0}", this.ID, this.Name);
        }
    }
}
