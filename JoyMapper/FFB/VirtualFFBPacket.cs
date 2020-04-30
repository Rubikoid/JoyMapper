using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace JoyMapper.FFB {
    public class VirtualFFBPacket {
        public FFB_CTRL FFB_CTRL = FFB_CTRL.CTRL_ENACT;
        public FFBPType FFBPType = FFBPType.PT_SETCREP;

        public FFBEType FFBENextType;
        public uint NextBlockIndex;

        public vJoy.FFB_DEVICE_PID FFB_DEVICE_PID;

        public vJoy.FFB_EFF_REPORT FFB_EFF_REPORT;
        public vJoy.FFB_EFF_COND FFB_EFF_COND;
        public vJoy.FFB_EFF_CONSTANT FFB_EFF_CONSTANT;
        public vJoy.FFB_EFF_ENVLP FFB_EFF_ENVLP;
        public vJoy.FFB_EFF_OP FFB_EFF_OP;
        public vJoy.FFB_EFF_PERIOD FFB_EFF_PERIOD;
        public vJoy.FFB_EFF_RAMP FFB_EFF_RAMP;

        public vJoy.FFB_PID_BLOCK_LOAD_REPORT PID_BLOCK_LOAD_REPORT;
        public vJoy.FFB_PID_EFFECT_STATE_REPORT PID_EFFECT_STATE_REPORT;
        public vJoy.FFB_PID_POOL_REPORT PID_POOL_REPORT;

        public byte Gain;
        public uint BlockIndex;
        public bool Handled;

        public uint ID;
    }

    public static class Utils {
        private static int GetDIPolarValue(int vJoyPolarValue) {
            return (int)Math.Round((double)vJoyPolarValue * 360.0 / 255.0 * 100.0);
        }
        private static void GetDICartesianXY(int vJoyPolarValue, ref int X, ref int Y) {
            double num = Math.Round((double)vJoyPolarValue * 360.0 / 255.0) * Math.PI / 180.0;
            double num2 = -Math.Cos(num);
            double num3 = Math.Sin(num);
            double num4 = Math.Min(Math.Abs(num3), Math.Abs(num2));
            if (num4 > 0.0001) {
                num3 /= num4;
                num2 /= num4;
            }
            num3 *= 64.0;
            num2 *= 64.0;
            X = (int)Math.Round(num3, 0);
            Y = (int)Math.Round(num2, 0);
        }
    }
}
