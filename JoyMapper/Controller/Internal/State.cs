using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper {
    public class State {
        public Axis AxisX;
        public Axis AxisY;
        public Axis AxisZ;

        public Axis AxisXR;
        public Axis AxisYR;
        public Axis AxisZR;

        public List<Button> buttons;

        public State(VirtualController vc, int buttonCount) {
            this.AxisX = new Axis(vc, HID_USAGES.HID_USAGE_X);
            this.AxisY = new Axis(vc, HID_USAGES.HID_USAGE_Y);
            this.AxisZ = new Axis(vc, HID_USAGES.HID_USAGE_Z);

            this.AxisXR = new Axis(vc, HID_USAGES.HID_USAGE_RX);
            this.AxisYR = new Axis(vc, HID_USAGES.HID_USAGE_RY);
            this.AxisZR = new Axis(vc, HID_USAGES.HID_USAGE_RZ);

            this.buttons = new List<Button>();
            for (int i = 0; i < buttonCount; i++)
                this.buttons.Add(new Button(vc));
        }

        public Axis getAxis(JoystickCapabilities cap) {
            switch (cap) {
                case JoystickCapabilities.AXIS_X: return this.AxisX;
                case JoystickCapabilities.AXIS_Y: return this.AxisY;
                case JoystickCapabilities.AXIS_Z: return this.AxisZ;

                case JoystickCapabilities.AXIS_RX: return this.AxisXR;
                case JoystickCapabilities.AXIS_RY: return this.AxisYR;
                case JoystickCapabilities.AXIS_RZ: return this.AxisZR;

                default: return null;
            }
        }
    }
}
