using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper {
    public static class ControllerCache {
        private static VirtualController _vc;
        public static VirtualController vc {
            get {
                if (_vc == null) {
                    _vc = new VirtualController(1);
                    _vc.Connect(); // this is a little strange way, but i want do load all capbs of joystic but don't keep connection
                    _vc.Disconnect();
                }
                return _vc;
            }
        }
        public static Dictionary<string, GameController> controllerDictionary = new Dictionary<string, GameController>();

        public static void Update(Action<string, GameController> callback) {
            foreach (GameController controller in GameController.GetAll()) {
                if (!controllerDictionary.ContainsKey(controller.Name)) {
                    controllerDictionary.Add(controller.Name, controller);
                    callback(controller.Name, controller);
                }
            }
        }
    }
    public enum JoystickCapabilities {
        AXIS_X,
        AXIS_Y,
        AXIS_Z,
        AXIS_RX,
        AXIS_RY,
        AXIS_RZ,
        POV,
        SLIDER_0,
        SLIDER_1,
        WHEEL
    }
    public interface IController {
        string Name { get; }
        bool Connected { get; }

        // capabs
        List<JoystickCapabilities> Capabilities { get; }
        int ButtonCount { get; }
        int ContinuousPOVCount { get; }
        int DirectionalPOVCount { get; }
        IList<Guid> SupportedFFBEffects { get; }

        void Connect();
        void Disconnect();
        void FillInfo(ref State state);
        void UpdateInfo(in State state);
    }
}
