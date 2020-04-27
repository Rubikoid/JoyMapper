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

        public State(VirtualController vc) {
            this.AxisX = new Axis(vc, HID_USAGES.HID_USAGE_X);
            this.AxisY = new Axis(vc, HID_USAGES.HID_USAGE_Y);
            this.AxisZ = new Axis(vc, HID_USAGES.HID_USAGE_Z);

            this.AxisXR = new Axis(vc, HID_USAGES.HID_USAGE_RX);
            this.AxisYR = new Axis(vc, HID_USAGES.HID_USAGE_RY);
            this.AxisZR = new Axis(vc, HID_USAGES.HID_USAGE_RZ);

            this.buttons = new List<Button>();
        }
    }
}
