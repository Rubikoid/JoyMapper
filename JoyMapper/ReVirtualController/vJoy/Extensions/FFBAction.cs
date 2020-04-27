using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper {
    public class FFBAction {
        public ReVirtualController.vJoy.vJoyFFBPacket packet;

        public object CustomData;
        public VirtualController.ControlModifierType Modifier;
        public ValueType CustomDataType;

        public int Number { get; internal set; }
        public double Value1 { get; internal set; }
    }
}
