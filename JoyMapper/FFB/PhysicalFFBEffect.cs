using JoyMapper.Controller;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper.FFB {
    public enum ForceType {
        None,
        Constant,
        Ramp,
        Periodic,
        Condition
    }
    public class PhysEffectParameters : SharpDX.DirectInput.EffectParameters {
        public Guid Type;
        public ForceType FType = ForceType.None;
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
                FFBEType type = FFBEType.ET_NONE;
                if (packet._FFBPType == FFBPType.PT_NEWEFREP)
                    type = packet.FFBENextType;
                else
                    type = packet.FFB_EFF_REPORT.EffectType;
                this.Parameters.Type = VirtualController.virtualEffectGuidMap[type];
                this.Parameters.Index = packet.BlockIndex;
                switch (type) {
                    case FFBEType.ET_CONST: {
                        if (this.Parameters.Parameters == null)
                            this.Parameters.Parameters = new ConstantForce() {
                                Magnitude = 10000,
                            };
                        this.Parameters.FType = ForceType.Constant;
                        break;
                    }
                    case FFBEType.ET_RAMP: {
                        if (this.Parameters.Parameters == null)
                            this.Parameters.Parameters = new RampForce() {
                                Start = -10000,
                                End = 10000,
                            };
                        this.Parameters.FType = ForceType.Ramp;
                        break;
                    }
                    case FFBEType.ET_SINE:
                    case FFBEType.ET_SQR:
                    case FFBEType.ET_STDN:
                    case FFBEType.ET_STUP:
                    case FFBEType.ET_TRNGL: {
                        if (this.Parameters.Parameters == null)
                            this.Parameters.Parameters = new PeriodicForce() {
                            Magnitude = 10000,
                            Offset = 0,
                            Period = 500000,
                            Phase = 0
                        };
                        this.Parameters.FType = ForceType.Periodic;
                        break;
                    }
                    case FFBEType.ET_DMPR:
                    case FFBEType.ET_FRCTN:
                    case FFBEType.ET_INRT:
                    case FFBEType.ET_SPRNG: {
                        if (this.Parameters.Parameters == null) {
                            this.Parameters.Parameters = new ConditionSet();
                            this.Parameters.Parameters.As<ConditionSet>().Conditions = new Condition[1];
                            this.Parameters.Parameters.As<ConditionSet>().Conditions[0] = new Condition() {
                                DeadBand = 0,
                                NegativeCoefficient = 10000,
                                NegativeSaturation = 10000,
                                Offset = 0,
                                PositiveCoefficient = 10000,
                                PositiveSaturation = 10000
                            };
                        }
                        this.Parameters.FType = ForceType.Condition;
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
            Parameters.Gain = (int)(packet.FFB_EFF_REPORT.Gain * (10000 / 255.0));
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

        public override string ToString() {
            string x = "";
            switch (Parameters.FType) {
                case ForceType.Condition: {
                    ConditionSet set = Parameters.Parameters.As<ConditionSet>();
                    x = $"offset={set.Conditions[0].Offset}";
                    break;
                }
                case ForceType.Constant: {
                    ConstantForce force = Parameters.Parameters.As<ConstantForce>();
                    x = $"mag={force.Magnitude}";
                    break;
                }
                case ForceType.Periodic: {
                    PeriodicForce force = Parameters.Parameters.As<PeriodicForce>();
                    x = $"mag={force.Magnitude}, peri={force.Period}";
                    break;
                }
                case ForceType.Ramp: {
                    RampForce force = Parameters.Parameters.As<RampForce>();
                    x = $"start={force.Start}, end={force.End}";
                    break;
                }
                default: x = "FF"; break;
            }
            //try {
                return $"[{Parameters.Index}] {VirtualController.virtualEffectGuidMapToString[Parameters.Type]}, {Object?.Status}, {Parameters.FType}, {x}";
            //} catch (Exception ex) {
            //    Program.logger.Warn($"Generating phys.toString() error = {ex}");
            //    return "";
            //}
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
