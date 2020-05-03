using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace JoyMapper.FFB {
    public class VirtualFFBPacket {
        public FFB_CTRL FFB_CTRL = FFB_CTRL.CTRL_ENACT;
        public FFBPType _FFBPType = FFBPType.PT_SETCREP;

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

        public FFBEType FFBThisType;

        public byte Gain;
        public uint BlockIndex;
        public bool Handled = false;

        public uint ID;

        public DateTime CreationTime { get; internal set; }

        public ForceFeedbackCommand GetFFBCommand() {
            switch (this.FFB_CTRL) {
                case FFB_CTRL.CTRL_ENACT:
                    return ForceFeedbackCommand.SetActuatorsOn;
                case FFB_CTRL.CTRL_DISACT:
                    return ForceFeedbackCommand.SetActuatorsOff;
                case FFB_CTRL.CTRL_DEVCONT:
                    return ForceFeedbackCommand.Continue;
                case FFB_CTRL.CTRL_DEVPAUSE:
                    return ForceFeedbackCommand.Pause;
                case FFB_CTRL.CTRL_DEVRST:
                    return ForceFeedbackCommand.Reset;
                case FFB_CTRL.CTRL_STOPALL:
                    return ForceFeedbackCommand.StopAll;
                default:
                    return ForceFeedbackCommand.SetActuatorsOn;
            }
        }

        public bool IsPeriodicEffect() {
            return //FFBThisType == FFBEType.SawtoothDown ||
                   //FFBThisType == FFBEffect.SawtoothUp ||
                   FFBThisType == FFBEType.ET_SINE ||
                   FFBThisType == FFBEType.ET_SQR ||
                   FFBThisType == FFBEType.ET_TRNGL;
        }

        public bool IsConditionEffect() {
            return FFBThisType == FFBEType.ET_DMPR ||
                   FFBThisType == FFBEType.ET_FRCTN ||
                   FFBThisType == FFBEType.ET_INRT ||
                   FFBThisType == FFBEType.ET_SPRNG;
        }

        public string GenerateEffectInfo() {
            StringBuilder dat = new StringBuilder();
            dat.AppendFormat($"[{ID}][EBI={BlockIndex}] Effect Report\n");
            dat.AppendFormat($"\t EBI={FFB_EFF_REPORT.EffectBlockIndex}\n");
            dat.AppendFormat($"\t Type={FFB_EFF_REPORT.EffectType}\n");
            dat.AppendFormat($"\t Dir={FFB_EFF_REPORT.Direction}\n");
            dat.AppendFormat($"\t DirX={FFB_EFF_REPORT.DirX}\n");
            dat.AppendFormat($"\t DirY={FFB_EFF_REPORT.DirY}\n");
            dat.AppendFormat($"\t Duration={FFB_EFF_REPORT.Duration}\n");
            dat.AppendFormat($"\t Gain={FFB_EFF_REPORT.Gain}\n");
            dat.AppendFormat($"\t Polar={FFB_EFF_REPORT.Polar}\n");
            dat.AppendFormat($"\t SamplePrd={FFB_EFF_REPORT.SamplePrd}\n");
            dat.AppendFormat($"\t TriggerBtn={FFB_EFF_REPORT.TrigerBtn}\n");
            dat.AppendFormat($"\t TriggerRpt={FFB_EFF_REPORT.TrigerRpt}\n");
            return dat.ToString();
        }

        public VirtualFFBPacket() {
            this.CreationTime = DateTime.Now;
        }
    }

    public static class Utils {
        public static int GetDIPolarValue(int vJoyPolarValue) {
            return (int)Math.Round((double)vJoyPolarValue * 360.0 / 255.0 * 100.0);
        }
        public static void GetDICartesianXY(int vJoyPolarValue, ref int X, ref int Y) {
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
