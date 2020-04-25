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
    class GameController : IController {
        public String Name { get; private set; }
        public Guid ID { get; private set; }
        public Guid FFBDriverID { get; private set; }
        public bool Connected { get; private set; } = false;
        public IList<Guid> SupportedFFBEffects { get; private set; }
        public Capabilities Capabilities { get { return this.Connected ? this.joystick.Capabilities : null; } }

        public Joystick joystick;
        private static DirectInput directInput = new DirectInput();

        public void Connect() {
            if (!this.Connected) {
                this.joystick = new Joystick(GameController.directInput, this.ID);
                this.joystick.Acquire();
                this.SupportedFFBEffects = new List<Guid>();
                foreach (EffectInfo effectInfo in this.joystick.GetEffects()) {
                    this.SupportedFFBEffects.Add(effectInfo.Guid);
                }
                this.Connected = true;
            }
        }

        public void Disconnect() {
            if (this.Connected) {
                this.joystick.Unacquire();
                this.Connected = false;
            }
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

        public override string ToString() {
            return string.Format("Device ID: {0}, Device Name: {1}", this.ID, this.Name);
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
    }
}
