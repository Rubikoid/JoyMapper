using JoyMapper.Controller;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace JoyMapper.FFB {
    class VirtualFFBState {
        public List<VirtualFFBEffect> Effects = new List<VirtualFFBEffect>();
        public List<VirtualFFBPacket> Packets = new List<VirtualFFBPacket>();
        public bool EnabledActuators = true;
        public bool DevicePause;
        public int Gain = 10000;

        private static int EffectComparison_UpdateDateTime(VirtualFFBEffect A, VirtualFFBEffect B) {
            if (A.UpdateTime > B.UpdateTime) {
                return 1;
            }
            if (A.UpdateTime < B.UpdateTime) {
                return -1;
            }
            return 0;
        }

        private bool GainControl(VirtualFFBPacket Packet) {
            if (Packet._FFBPType == FFBPType.PT_GAINREP) {
                this.Gain = (int)Packet.Gain * 10000 / 255;
                Packet.Handled = true;
                return true;
            }
            return false;
        }

        private bool MiscControl(VirtualFFBPacket Packet) {
            bool flag = false;
            switch (Packet._FFBPType) {
                case FFBPType.PT_BLKFRREP:
                    flag = this.EffectDispose(Packet);
                    break;
            }
            Packet.Handled = flag;
            return flag;
        }

        private bool DeviceControl(VirtualFFBPacket Packet) {
            if (Packet._FFBPType == FFBPType.PT_CTRLREP) {
                switch (Packet.FFB_CTRL) {
                    case FFB_CTRL.CTRL_ENACT:
                        this.EnabledActuators = true;
                        break;
                    case FFB_CTRL.CTRL_DISACT:
                        this.EnabledActuators = false;
                        break;
                    case FFB_CTRL.CTRL_STOPALL:
                        for (int i = 0; i < this.Effects.Count; i++) {
                            if (this.Effects[i].Status == EffectStatus.Playing && this.Effects[i].Parameters.TriggerButton == -1) {
                                this.Effects[i].Packet = Packet;
                                this.StopEffect(this.Effects[i]);
                            }
                        }
                        this.DevicePause = false;
                        break;
                    case FFB_CTRL.CTRL_DEVRST:
                        for (int j = 0; j < this.Effects.Count; j++) {
                            if (this.Effects[j].Status == EffectStatus.Playing && this.Effects[j].Parameters.TriggerButton == -1) {
                                this.Effects[j].Packet = Packet;
                                this.StopEffect(this.Effects[j]);
                            }
                        }
                        this.EnabledActuators = false;
                        this.Effects.Clear();
                        this.DevicePause = false;
                        this.EnabledActuators = true;
                        break;
                    case FFB_CTRL.CTRL_DEVPAUSE:
                        this.DevicePause = true;
                        break;
                    case FFB_CTRL.CTRL_DEVCONT:
                        this.DevicePause = false;
                        break;
                }
                Packet.Handled = true;
                return true;
            }
            return false;
        }

        public static EffectParametersEx GetBaseEffectParameters(vJoy.FFB_EFF_REPORT Effect) {
            EffectParametersEx effectParameters = new EffectParametersEx();
            if (Effect.Duration == 65535) {
                effectParameters.Duration = -1;
            } else {
                effectParameters.Duration = (int)(Effect.Duration * 1000);
            }
            effectParameters.Gain = (int)Effect.Gain * 10000 / 255;
            effectParameters.SamplePeriod = (int)(Effect.SamplePrd * 1000);
            effectParameters.StartDelay = 0;
            effectParameters.TriggerButton = (int)Effect.TrigerBtn;
            if (Effect.TrigerBtn == 255) {
                effectParameters.TriggerButton = -1;
            } else {
                effectParameters.TriggerButton = (int)Effect.TrigerBtn;
            }
            effectParameters.TriggerRepeatInterval = (int)(Effect.TrigerRpt * 1000);
            effectParameters.Type = VirtualController.virtualEffectGuidMap[Effect.EffectType];
            // effectParameters.AxisSettings = new AxisSettings(2);
            int[] dirs = new int[2];
            int num = 0;
            int num2 = 0;
            if (Effect.Polar) {
                Utils.GetDICartesianXY((int)Effect.DirX, ref num, ref num2);
            } else {
                num = (int)Effect.DirX;
                num2 = (int)Effect.DirY;
            }
            dirs[0] = num;
            dirs[1] = num2;
            effectParameters.Directions = dirs;
            return effectParameters;
        }

        private bool CreateBaseParams(VirtualFFBPacket Packet) {
            if (Packet._FFBPType != FFBPType.PT_EFFREP) {
                return false;
            }
            if (this.DevicePause) {
                return false;
            }
            EffectParametersEx baseEffectParameters = GetBaseEffectParameters(Packet.FFB_EFF_REPORT);
            VirtualFFBEffect vJoyFFBEffect = new VirtualFFBEffect();
            vJoyFFBEffect.Parameters = baseEffectParameters;
            vJoyFFBEffect.Index = Packet.BlockIndex;
            int num = 0;
            for (int i = this.Packets.Count - 1; i >= 0; i--) {
                TimeSpan timeSpan = vJoyFFBEffect.CreationTime - this.Packets[i].CreationTime;
                if (this.Packets[i]._FFBPType == FFBPType.PT_EFFREP || timeSpan.TotalSeconds > 5.0) {
                    num = i;
                    break;
                }
            }
            bool flag = false;
            //for (int j = num; j < this.Packets.Count; j++) {
                //this.UpdateEnvelopeParams(this.Packets[j], vJoyFFBEffect);
                //if (this.UpdateTypeSpecificParams(this.Packets[j], vJoyFFBEffect)) {
                //    flag = true;
                //}
            //}
            if (flag) {
                vJoyFFBEffect.Status = EffectStatus.Idle;
            }
            vJoyFFBEffect.Packet = Packet;
            this.Effects.Add(vJoyFFBEffect);
            Packet.Handled = true;
            return true;
        }

        private bool EffectDispose(VirtualFFBPacket Packet) {
            if (this.DevicePause) {
                return false;
            }
            bool result = false;
            for (int i = this.Effects.Count - 1; i >= 0; i--) {
                if (this.EffectDispose(Packet, this.Effects[i])) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private bool EffectDispose(VirtualFFBPacket Packet, VirtualFFBEffect Effect) {
            if (Packet.BlockIndex == Effect.Index) {
                Effect.Dispose();
                Effect.Packet = Packet;
                return true;
            }
            return false;
        }

        private bool EffectPlayback(VirtualFFBPacket Packet) {
            if (this.DevicePause) {
                return false;
            }
            bool result = false;
            for (int i = this.Effects.Count - 1; i >= 0; i--) {
                if (this.EffectPlayback(Packet, this.Effects[i])) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private void StopEffect(VirtualFFBEffect Effect) {
            Effect.Status = EffectStatus.Stopped;
            Effect.StopTime = DateTime.Now;
            Effect.UpdateTime = DateTime.Now;
        }

        private void StartEffect(VirtualFFBEffect Effect) {
            Effect.Status = EffectStatus.Playing;
            Effect.StartTime = DateTime.Now;
            Effect.UpdateTime = DateTime.Now;
        }

        private bool EffectPlayback(VirtualFFBPacket Packet, VirtualFFBEffect Effect) {
            if (Packet._FFBPType == FFBPType.PT_EFOPREP &&
                (int)Packet.FFB_EFF_OP.EffectBlockIndex == Effect.Index &&
                Effect.Status != EffectStatus.None &&
                Effect.Parameters.TriggerButton == -1) {

                Effect.UpdateStatus();
                switch (Packet.FFB_EFF_OP.EffectOp) {
                    case FFBOP.EFF_START:
                        if (Effect.Status != EffectStatus.Playing) {
                            Effect.LoopCount = (int)Packet.FFB_EFF_OP.LoopCount;
                            Effect.Solo = false;
                            this.StartEffect(Effect);
                            Effect.Packet = Packet;
                            Packet.Handled = true;
                            return true;
                        }
                        break;
                    case FFBOP.EFF_SOLO:
                        if (Effect.Status != EffectStatus.Playing) {
                            for (int i = 0; i < this.Effects.Count; i++) {
                                if (this.Effects[i] != Effect) {
                                    this.StopEffect(this.Effects[i]);
                                    this.Effects[i].Packet = Packet;
                                }
                            }
                            Effect.LoopCount = (int)Packet.FFB_EFF_OP.LoopCount;
                            Effect.Solo = true;
                            this.StartEffect(Effect);
                            Effect.Packet = Packet;
                            Packet.Handled = true;
                            return true;
                        }
                        break;
                    case FFBOP.EFF_STOP:
                        if (Effect.Status == EffectStatus.Playing) {
                            this.StopEffect(Effect);
                            Effect.Packet = Packet;
                            Packet.Handled = true;
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        private void EffectCleaning() {
            DateTime now = DateTime.Now;
            int num = this.Effects.Count;
            for (int i = 0; i < num; i++) {
                this.Effects[i].UpdateStatus();
                TimeSpan timeSpan = now - this.Effects[i].StopTime;
                switch (this.Effects[i].Status) {
                    case EffectStatus.None:
                        // now - this.Effects[i].CreationTime;
                        if (timeSpan.TotalMinutes > 30.0) {
                            this.Effects.RemoveAt(i);
                            num--;
                        }
                        break;
                    case EffectStatus.Stopped:
                        if (timeSpan.TotalMinutes > 30.0) {
                            this.Effects.RemoveAt(i);
                            num--;
                        }
                        break;
                    case EffectStatus.Expired:
                        if (timeSpan.TotalMinutes > 30.0) {
                            this.Effects.RemoveAt(i);
                            num--;
                        }
                        break;
                }
            }
        }

        private void PacketCleaning() {
            DateTime now = DateTime.Now;
            int num = this.Packets.Count;
            for (int i = 0; i < num; i++) {
                TimeSpan timeSpan = now - this.Packets[i].CreationTime;
                if (this.Packets[i].Handled) {
                    if (timeSpan.TotalSeconds > 30.0) {
                        this.Packets.RemoveAt(i);
                        num--;
                    }
                } else if (timeSpan.TotalMinutes > 5.0) {
                    this.Packets.RemoveAt(i);
                    num--;
                }
            }
        }

        public bool Do(VirtualFFBPacket Packet) {
            if (this.Packets.Count > 60) {
                this.PacketCleaning();
            }
            if (this.Effects.Count > 256) {
                this.EffectCleaning();
            }
            bool flag = true;
            if (//!this.UpdateBaseParams(Packet) &&
                !this.CreateBaseParams(Packet) &&
                //!this.UpdateTypeSpecificParams(Packet) &&
                //!this.UpdateEnvelopeParams(Packet) &&
                !this.EffectPlayback(Packet) &&
                !this.GainControl(Packet) &&
                !this.DeviceControl(Packet) &&
                !this.MiscControl(Packet)) {
                flag = false;
            }
            this.Packets.Add(Packet);
            if (flag) {
                this.Effects.Sort(new Comparison<VirtualFFBEffect>(VirtualFFBState.EffectComparison_UpdateDateTime));
                return true;
            }
            return false;
        }

        /*
        private bool UpdateEnvelopeParams(VirtualFFBPacket Packet) {
            if (this.DevicePause) {
                return false;
            }
            bool result = false;
            for (int i = 0; i < this.Effects.Count; i++) {
                if (this.UpdateEnvelopeParams(Packet, this.Effects[i])) {
                    result = true;
                }
            }
            return result;
        }

        private bool UpdateTypeSpecificParams(VirtualFFBPacket Packet) {
            if (this.DevicePause) {
                return false;
            }
            bool result = false;
            for (int i = 0; i < this.Effects.Count; i++) {
                if (this.UpdateTypeSpecificParams(Packet, this.Effects[i])) {
                    result = true;
                }
            }
            return result;
        }
        */
    }
}
