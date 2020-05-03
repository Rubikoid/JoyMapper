using JoyMapper.Controller;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper.FFB {
    public class PhysEffectParameters : SharpDX.DirectInput.EffectParameters {
        public Guid Type;
        public uint Index = 10000;

        //public Envelope Envelope;
        //public TypeSpecificParameters TypeSpecificParameters;

    }
    public class PhysicalFFBEffect {
        public void Dispose() {
            try {
                if (this.Downloaded && this.Object != null) {
                    //DirectInputAPI.DisposeEffect(this.Object);
                    this.Object.Stop();
                    this.Object.Dispose();
                }
                this.Downloaded = false;
                this.UpdateTime = DateTime.Now;
                this.Status = EffectStatus.None;
                this.Object = null;
            } catch (Exception) {

            }
        }

        public void InitFromFFBPacket(VirtualFFBPacket packet) {
            if (packet._FFBPType != FFBPType.PT_NEWEFREP && packet._FFBPType != FFBPType.PT_EFFREP)
                return;
            if (this.Parameters.Index == 10000) {
                this.Parameters.Type = VirtualController.virtualEffectGuidMap[packet.FFBENextType];
                this.Parameters.Index = packet.BlockIndex;
                switch (packet.FFBENextType) {
                    case FFBEType.ET_CONST: {
                        this.Parameters.Parameters = new ConstantForce();
                        break;
                    }
                    case FFBEType.ET_RAMP: {
                        this.Parameters.Parameters = new RampForce();
                        break;
                    }
                    case FFBEType.ET_SINE:
                    case FFBEType.ET_SQR:
                    case FFBEType.ET_STDN:
                    case FFBEType.ET_STUP:
                    case FFBEType.ET_TRNGL: {
                        this.Parameters.Parameters = new PeriodicForce();
                        break;
                    }
                    case FFBEType.ET_DMPR:
                    case FFBEType.ET_FRCTN:
                    case FFBEType.ET_INRT:
                    case FFBEType.ET_SPRNG: {
                        this.Parameters.Parameters = new ConditionSet();
                        break;
                    }
                    case FFBEType.ET_CSTM: { break; }
                }
            }
        }

        public void UpdateFromFFBPacket(VirtualFFBPacket packet) {
            if (packet._FFBPType != FFBPType.PT_EFFREP)
                return;

            if (packet.FFB_EFF_REPORT.Duration == 65535) {
                Parameters.Duration = -1;
            } else {
                Parameters.Duration = (int)(packet.FFB_EFF_REPORT.Duration * 1000);
            }
            Parameters.Gain = (int)packet.FFB_EFF_REPORT.Gain * 10000 / 255;
            Parameters.SamplePeriod = (int)(packet.FFB_EFF_REPORT.SamplePrd * 1000);
            Parameters.StartDelay = 0;
            Parameters.TriggerButton = (int)packet.FFB_EFF_REPORT.TrigerBtn;
            if (packet.FFB_EFF_REPORT.TrigerBtn == 255) {
                Parameters.TriggerButton = -1;
            } else {
                Parameters.TriggerButton = (int)packet.FFB_EFF_REPORT.TrigerBtn;
            }
            Parameters.TriggerRepeatInterval = (int)(packet.FFB_EFF_REPORT.TrigerRpt * 1000);
            Parameters.Flags = (EffectFlags.ObjectIds | EffectFlags.Cartesian);

            /* 
               So there a very strange moment: vjoy always sends packet.FFB_EFF_REPORT.Polar is true.
               So, a solution also very strange and hacky.
            */

            if (packet.FFB_EFF_REPORT.Polar) {
                Parameters.Directions = new int[1];
                // 1/0 + 0 = 8191
                // -1  + 0 = 24575
                // 0   + 1 = 16383
                // 0   +-1 = 0
                // 
                switch (packet.FFB_EFF_REPORT.Direction) {
                    case 0: {
                        Program.logger.Fatal($"RECIVED ZERO PACKET _START");
                        Program.logger.Fatal(packet.GenerateEffectInfo());
                        Program.logger.Fatal($"RECIVED ZERO PACKET _END");
                        Parameters.Directions[0] = 0;
                        break;
                    }
                    case 8191: {
                        Parameters.Directions[0] = 1;
                        break;
                    }
                    case 24575: {
                        Parameters.Directions[0] = -1;
                        break;
                    }
                    default: {
                        Parameters.Directions[0] = 0;
                        break;
                    }
                }
            } else {
                Parameters.Directions = new int[2];
                Parameters.Directions[0] = (int)packet.FFB_EFF_REPORT.DirX;
                Parameters.Directions[1] = (int)packet.FFB_EFF_REPORT.DirY;

                Program.logger.Fatal($"RECIVED NOT POLAR PACKET _START");
                Program.logger.Fatal(packet.GenerateEffectInfo());
                Program.logger.Fatal($"RECIVED NOT POLAR PACKET _END");
            }
            //effectParameters.AxisSettings = new AxisSettings(2);
            //Parameters.Directions = new int[2];
            /*int num = 0;
            int num2 = 0;
            if (packet.FFB_EFF_REPORT.Polar) {
                Utils.GetDICartesianXY((int)packet.FFB_EFF_REPORT.DirX, ref num, ref num2);
            } else {
                num = (int)packet.FFB_EFF_REPORT.DirX;
                num2 = (int)packet.FFB_EFF_REPORT.DirY;
            }
            Parameters.Directions[0] = num;
            Parameters.Directions[1] = num2; */
        }

        public void UpdateEffectStatus() {
            if (this.Status != EffectStatus.None && this.Status == EffectStatus.Playing && this.Parameters.Duration != -1) {
                double totalSeconds = (DateTime.Now - this.StartTime).TotalSeconds;
                double num = (double)this.Parameters.Duration / 1000000.0;
                if (totalSeconds >= num) {
                    this.Status = EffectStatus.Expired;
                    this.StopTime = this.StartTime;
                    this.StopTime = this.StopTime.AddSeconds(num);
                }
            }
        }

        public PhysicalFFBEffect() {
            DateTime now = DateTime.Now;
            this.Parameters = new PhysEffectParameters();
            this.CreationTime = now;
            this.UpdateTime = now;
            this.StartTime = DateTime.MinValue;
            this.StopTime = DateTime.MinValue;
            this.Downloaded = false;
            this.Status = EffectStatus.None;
            this.FFBAxisIds = new List<int>();
            this.Info = new EffectInfo();
        }

        public PhysEffectParameters Parameters;
        public DateTime CreationTime;
        public DateTime UpdateTime;
        public DateTime StartTime;
        public DateTime StopTime;
        public bool Downloaded;
        public Effect Object;
        public EffectStatus Status;
        public List<int> FFBAxisIds;
        public EffectInfo Info;
    }
}
