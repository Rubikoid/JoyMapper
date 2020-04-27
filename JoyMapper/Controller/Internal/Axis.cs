using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace JoyMapper.Controller.Internal {
    public class Axis {
        public uint ID { get; private set; }
        private VirtualController vc;
        private HID_USAGES axis;
        
        private float _value { get; set; }
        public float raw_val { get { return _value; } set { _value = value; } }

        private long _min_value;
        private long _max_value;
        private long half_range;

        public Axis(VirtualController vc, HID_USAGES axis) {
            this.vc = vc;
            this.ID = vc.ID;
            this.axis = axis;
            this._value = 0;

            this.vc.joystick.GetVJDAxisMin(this.ID, this.axis, ref this._min_value);
            this.vc.joystick.GetVJDAxisMax(this.ID, this.axis, ref this._max_value);
            this.half_range = this._max_value / 2;
        }

        public int getVal() {
            int ret = (int)(this.half_range + this.half_range * this._value);
            //Console.WriteLine($"[G] {this.axis.ToString()} {ret}");
            return ret;
        }

        public void setVal(int val, long max, long min) {
            long half = max / 2;
            this._value = (val - half) / (float)half;
            //Console.WriteLine($"[S] {this.axis.ToString()} {val}->{this._value}");
        }
    }
}
