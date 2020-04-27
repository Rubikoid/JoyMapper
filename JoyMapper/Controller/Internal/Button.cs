using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper {
    public class Button {
        public uint ID { get; private set; }
        private vJoyController vc;
        private bool _value { get; set; } = false;

        public Button(vJoyController vc) {
            this.vc = vc;
            this.ID = vc.ID;

        }

        public bool getVal() {
            return this._value;
        }

        public void setVal(bool val) {
            this._value = val;
        }
    }
}
