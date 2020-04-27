using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper {
    /*public enum MapType {
        AXIS,
        BUTTON,
    }*/
    public interface IMap {
        //MapType type { get; }
        void Map(in State inState, ref State outState);
    }

    public class AxisMap : IMap {
        //public MapType type { get; } = MapType.AXIS;
        public JoystickCapabilities inAxis { get; private set; } = JoystickCapabilities.NONE;
        public JoystickCapabilities outAxis { get; private set; } = JoystickCapabilities.NONE;
        public AxisMap(JoystickCapabilities inAxis, JoystickCapabilities outAxis) {
            this.inAxis = inAxis;
            this.outAxis = outAxis;
        }
        public void Map(in State inState, ref State outState) {
            outState.getAxis(this.outAxis).raw_val = inState.getAxis(this.inAxis).raw_val;
        }

        public void SetOut(JoystickCapabilities cap) {
            this.outAxis = cap;
        }
    }

    public class ButtonMap : IMap {
        //public MapType type { get; } = MapType.BUTTON;
        public int inButton { get; private set; }
        public int outButton { get; private set; }
        public ButtonMap(int inButton, int outButton) {
            this.inButton = inButton;
            this.outButton = outButton;
        }
        public void Map(in State inState, ref State outState) {
            if (inState.buttons.Count <= this.inButton || outState.buttons.Count <= this.outButton)
                throw new Exception($"Too big button number");
            outState.buttons[this.outButton].setVal(inState.buttons[this.inButton].getVal());
        }
    }
}
