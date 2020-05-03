using JoyMapper.Controller.Internal;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper.Controller {
    public static class ControllerCache {
        private static VirtualController _vc;
        public static VirtualController vc {
            get {
                if (_vc == null) {
                    _vc = new VirtualController(1);
                    _vc.loadCapabilities();
                }
                return _vc;
            }
        }
        
        private static VirtualController _vc2;
        public static VirtualController vc2 {
            get {
                if (_vc2 == null) {
                    _vc2 = new VirtualController(2);
                    _vc2.loadCapabilities();
                }
                return _vc2;
            }
        }

        public static bool VCCheck = false;

        public static Dictionary<string, GameController> controllerDictionary = new Dictionary<string, GameController>();

        public static void Update(Action<string, GameController> callback) {
            foreach (GameController controller in GameController.GetAll()) {
                if (!controllerDictionary.ContainsKey(controller.ID.ToString())) {
                    controllerDictionary.Add(controller.ToString(), controller);
                    callback(controller.ToString(), controller);
                }
            }
        }
    }
    public enum JoystickCapabilities {
        NONE,
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
        IList<IMap> Mappings { get; }

        void Connect();
        void Disconnect();
        void FillExternalInfo(ref State state);
        void UpdateInfo(in State state);
    }
}
