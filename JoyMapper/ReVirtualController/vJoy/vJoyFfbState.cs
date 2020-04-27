using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper.ReVirtualController.vJoy {
    // Token: 0x02000031 RID: 49
    public class vJoyFfbState {
        // Token: 0x06000183 RID: 387 RVA: 0x000109CA File Offset: 0x0000EBCA
        private static int EffectComparison_CreateDateTime(vJoyFFBEffect A, vJoyFFBEffect B) {
            if (A.CreationTime > B.CreationTime) {
                return 1;
            }
            if (A.CreationTime < B.CreationTime) {
                return -1;
            }
            return 0;
        }

        // Token: 0x06000184 RID: 388 RVA: 0x000109F7 File Offset: 0x0000EBF7
        private static int EffectComparison_UpdateDateTime(vJoyFFBEffect A, vJoyFFBEffect B) {
            if (A.UpdateTime > B.UpdateTime) {
                return 1;
            }
            if (A.UpdateTime < B.UpdateTime) {
                return -1;
            }
            return 0;
        }

        // Token: 0x06000185 RID: 389 RVA: 0x00010A24 File Offset: 0x0000EC24
        public void CopyFrom(vJoyFfbState State) {
            this.Packets.Clear();
            foreach (vJoyFFBPacket item in State.Packets) {
                this.Packets.Add(item);
            }
            this.EnabledActuators = State.EnabledActuators;
            this.DevicePause = State.DevicePause;
            this.Gain = State.Gain;
            this.Effects.Clear();
            foreach (vJoyFFBEffect item2 in State.Effects) {
                this.Effects.Add(item2);
            }
        }

        // Token: 0x06000186 RID: 390 RVA: 0x00010B00 File Offset: 0x0000ED00
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

        // Token: 0x06000187 RID: 391 RVA: 0x00010B9C File Offset: 0x0000ED9C
        private void EffectCleaning() {
            DateTime now = DateTime.Now;
            int num = this.Effects.Count;
            for (int i = 0; i < num; i++) {
                this.Effects[i].UpdateStatus();
                TimeSpan timeSpan = now - this.Effects[i].StopTime;
                switch (this.Effects[i].Status) {
                    case VirtualController.DirectInput.EffectStatus.None:
                        // now - this.Effects[i].CreationTime;
                        // WTF: ???
                        if (timeSpan.TotalMinutes > 30.0) {
                            this.Effects.RemoveAt(i);
                            num--;
                        }
                        break;
                    case VirtualController.DirectInput.EffectStatus.Stopped:
                        if (timeSpan.TotalMinutes > 30.0) {
                            this.Effects.RemoveAt(i);
                            num--;
                        }
                        break;
                    case VirtualController.DirectInput.EffectStatus.Expired:
                        if (timeSpan.TotalMinutes > 30.0) {
                            this.Effects.RemoveAt(i);
                            num--;
                        }
                        break;
                }
            }
        }

        // Token: 0x06000188 RID: 392 RVA: 0x00010CAC File Offset: 0x0000EEAC
        private bool CreateBaseParams(vJoyFFBPacket Packet) {
            if (Packet.FFBPType != FFBPType.PT_EFFREP) {
                return false;
            }
            if (this.DevicePause) {
                return false;
            }
            VirtualController.DirectInput.EffectParameters baseEffectParameters = Utils.GetBaseEffectParameters(Packet.FFB_EFF_REPORT);
            vJoyFFBEffect vJoyFFBEffect_x = new vJoyFFBEffect();
            vJoyFFBEffect_x.Parameters = baseEffectParameters;
            vJoyFFBEffect_x.Parameters.Index = Packet.BlockIndex;
            int num = 0;
            for (int i = this.Packets.Count - 1; i >= 0; i--) {
                TimeSpan timeSpan = vJoyFFBEffect_x.CreationTime - this.Packets[i].CreationTime;
                if (this.Packets[i].FFBPType == FFBPType.PT_EFFREP || timeSpan.TotalSeconds > 5.0) {
                    num = i;
                    break;
                }
            }
            bool flag = false;
            for (int j = num; j < this.Packets.Count; j++) {
                this.UpdateEnvelopeParams(this.Packets[j], vJoyFFBEffect_x);
                if (this.UpdateTypeSpecificParams(this.Packets[j], vJoyFFBEffect_x)) {
                    flag = true;
                }
            }
            if (flag) {
                vJoyFFBEffect_x.Status = VirtualController.DirectInput.EffectStatus.Idle;
            }
            vJoyFFBEffect_x.LAPacket = Packet;
            this.Effects.Add(vJoyFFBEffect_x);
            Packet.Handled = true;
            return true;
        }

        // Token: 0x06000189 RID: 393 RVA: 0x00010DD4 File Offset: 0x0000EFD4
        private bool UpdateBaseParams(vJoyFFBPacket Packet) {
            if (Packet.FFBPType == FFBPType.PT_EFFREP) {
                if (this.DevicePause) {
                    return false;
                }
                VirtualController.DirectInput.EffectParameters baseEffectParameters = Utils.GetBaseEffectParameters(Packet.FFB_EFF_REPORT);
                for (int i = 0; i < this.Effects.Count; i++) {
                    vJoyFFBEffect vJoyFFBEffect_x = this.Effects[i];
                    if (Packet.BlockIndex == vJoyFFBEffect_x.Parameters.Index && baseEffectParameters.Type == vJoyFFBEffect_x.Parameters.Type) {
                        vJoyFFBEffect_x.Parameters.AxisSettings = baseEffectParameters.AxisSettings;
                        vJoyFFBEffect_x.Parameters.Duration = baseEffectParameters.Duration;
                        vJoyFFBEffect_x.Parameters.Gain = baseEffectParameters.Gain;
                        vJoyFFBEffect_x.Parameters.SamplePeriod = baseEffectParameters.SamplePeriod;
                        vJoyFFBEffect_x.Parameters.StartDelay = baseEffectParameters.StartDelay;
                        vJoyFFBEffect_x.Parameters.TriggerButton = baseEffectParameters.TriggerButton;
                        vJoyFFBEffect_x.Parameters.TriggerRepeatInterval = baseEffectParameters.TriggerRepeatInterval;
                        vJoyFFBEffect_x.UpdateTime = DateTime.Now;
                        vJoyFFBEffect_x.BaseUpdateTime = DateTime.Now;
                        vJoyFFBEffect_x.LAPacket = Packet;
                        Packet.Handled = true;
                        return true;
                    }
                }
            }
            return false;
        }

        // Token: 0x0600018A RID: 394 RVA: 0x00010EF8 File Offset: 0x0000F0F8
        private bool UpdateTypeSpecificParams(vJoyFFBPacket Packet, vJoyFFBEffect Effect) {
            if (Packet.BlockIndex != Effect.Parameters.Index) {
                return false;
            }
            switch (Packet.FFBPType) {
                case FFBPType.PT_CONDREP: {
                    if (!VirtualController.DirectInput.TypeSpecificParameters.IsConditionEffect(Effect.Parameters.Type)) {
                        return false;
                    }
                    VirtualController.DirectInput.Condition condition = Utils.GetCondition(Packet.FFB_EFF_COND);
                    VirtualController.DirectInput.ConditionSet conditionSet = Effect.Parameters.TypeSpecificParameters.ConditionSet;
                    int num = conditionSet.Conditions.Length;
                    if (num > 0) {
                        if (num == 1) {
                            if (conditionSet.Conditions[0] != condition) {
                                conditionSet.Conditions[0] = condition;
                                Packet.Handled = true;
                            }
                        } else if (Packet.FFB_EFF_COND.isY) {
                            Effect.ConditionY = true;
                            if (conditionSet.Conditions[1] != condition) {
                                conditionSet.Conditions[1] = condition;
                                Packet.Handled = true;
                            }
                            if (!Effect.ConditionX && conditionSet.Conditions[0] != condition) {
                                conditionSet.Conditions[0] = condition;
                                Packet.Handled = true;
                            }
                        } else {
                            Effect.ConditionY = true;
                            if (conditionSet.Conditions[0] != condition) {
                                conditionSet.Conditions[0] = condition;
                                Packet.Handled = true;
                            }
                            if (!Effect.ConditionY && conditionSet.Conditions[1] != condition) {
                                conditionSet.Conditions[1] = condition;
                                Packet.Handled = true;
                            }
                        }
                    }
                    break;
                }
                case FFBPType.PT_PRIDREP: {
                    if (!VirtualController.DirectInput.TypeSpecificParameters.IsPeriodicEffect(Effect.Parameters.Type)) {
                        return false;
                    }
                    VirtualController.DirectInput.PeriodicForce periodicForce = Utils.GetPeriodicForce(Packet.FFB_EFF_PERIOD);
                    if (Effect.Parameters.TypeSpecificParameters.PeriodicForce != periodicForce) {
                        Effect.Parameters.TypeSpecificParameters.PeriodicForce = periodicForce;
                        Packet.Handled = true;
                    }
                    break;
                }
                case FFBPType.PT_CONSTREP: {
                    if (Effect.Parameters.Type != VirtualController.DirectInput.FFBEffect.ConstantForce) {
                        return false;
                    }
                    VirtualController.DirectInput.ConstantForce constantForce = Utils.GetConstantForce(Packet.FFB_EFF_CONSTANT);
                    if (Effect.Parameters.TypeSpecificParameters.ConstantForce != constantForce) {
                        Effect.Parameters.TypeSpecificParameters.ConstantForce = constantForce;
                        Packet.Handled = true;
                    }
                    break;
                }
                case FFBPType.PT_RAMPREP: {
                    if (Effect.Parameters.Type != VirtualController.DirectInput.FFBEffect.RampForce) {
                        return false;
                    }
                    VirtualController.DirectInput.RampForce rampForce = Utils.GetRampForce(Packet.FFB_EFF_RAMP);
                    if (Effect.Parameters.TypeSpecificParameters.RampForce != rampForce) {
                        Effect.Parameters.TypeSpecificParameters.RampForce = rampForce;
                        Packet.Handled = true;
                    }
                    break;
                }
            }
            if (Packet.Handled) {
                if (Effect.Status == VirtualController.DirectInput.EffectStatus.None) {
                    Effect.Status = VirtualController.DirectInput.EffectStatus.Idle;
                    Effect.Downloaded = true;
                }
                Effect.UpdateTime = DateTime.Now;
                Effect.TypeSpecificUpdateTime = DateTime.Now;
                Effect.LAPacket = Packet;
                return true;
            }
            return false;
        }

        // Token: 0x0600018B RID: 395 RVA: 0x000111B4 File Offset: 0x0000F3B4
        private bool UpdateEnvelopeParams(vJoyFFBPacket Packet, vJoyFFBEffect Effect) {
            if (Packet.BlockIndex != Effect.Parameters.Index || Packet.FFBPType != FFBPType.PT_ENVREP) {
                return false;
            }
            VirtualController.DirectInput.Envelope envelope = Utils.GetEnvelope(Packet.FFB_EFF_ENVLP);
            if (Effect.Parameters.Envelope != envelope) {
                Effect.Parameters.Envelope = envelope;
                Packet.Handled = true;
                Effect.LAPacket = Packet;
                return true;
            }
            return false;
        }

        // Token: 0x0600018C RID: 396 RVA: 0x0001121C File Offset: 0x0000F41C
        private bool UpdateTypeSpecificParams(vJoyFFBPacket Packet) {
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

        // Token: 0x0600018D RID: 397 RVA: 0x00011268 File Offset: 0x0000F468
        private bool UpdateEnvelopeParams(vJoyFFBPacket Packet) {
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

        // Token: 0x0600018E RID: 398 RVA: 0x000112B4 File Offset: 0x0000F4B4
        private bool EffectPlayback(vJoyFFBPacket Packet, vJoyFFBEffect Effect) {
            if (Packet.FFBPType == FFBPType.PT_EFOPREP && (int)Packet.FFB_EFF_OP.EffectBlockIndex == Effect.Parameters.Index && Effect.Status != VirtualController.DirectInput.EffectStatus.None && Effect.Parameters.TriggerButton == -1) {
                Effect.UpdateStatus();
                switch (Packet.FFB_EFF_OP.EffectOp) {
                    case FFBOP.EFF_START:
                        if (Effect.Status != VirtualController.DirectInput.EffectStatus.Playing) {
                            Effect.LoopCount = (int)Packet.FFB_EFF_OP.LoopCount;
                            Effect.Solo = false;
                            this.StartEffect(Effect);
                            Effect.LAPacket = Packet;
                            Packet.Handled = true;
                            return true;
                        }
                        break;
                    case FFBOP.EFF_SOLO:
                        if (Effect.Status != VirtualController.DirectInput.EffectStatus.Playing) {
                            for (int i = 0; i < this.Effects.Count; i++) {
                                if (this.Effects[i] != Effect) {
                                    this.StopEffect(this.Effects[i]);
                                    this.Effects[i].LAPacket = Packet;
                                }
                            }
                            Effect.LoopCount = (int)Packet.FFB_EFF_OP.LoopCount;
                            Effect.Solo = true;
                            this.StartEffect(Effect);
                            Effect.LAPacket = Packet;
                            Packet.Handled = true;
                            return true;
                        }
                        break;
                    case FFBOP.EFF_STOP:
                        if (Effect.Status == VirtualController.DirectInput.EffectStatus.Playing) {
                            this.StopEffect(Effect);
                            Effect.LAPacket = Packet;
                            Packet.Handled = true;
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        // Token: 0x0600018F RID: 399 RVA: 0x00011410 File Offset: 0x0000F610
        private void StartEffect(vJoyFFBEffect Effect) {
            Effect.Status = VirtualController.DirectInput.EffectStatus.Playing;
            Effect.StartTime = DateTime.Now;
            Effect.UpdateTime = DateTime.Now;
        }

        // Token: 0x06000190 RID: 400 RVA: 0x0001142F File Offset: 0x0000F62F
        private void StopEffect(vJoyFFBEffect Effect) {
            Effect.Status = VirtualController.DirectInput.EffectStatus.Stopped;
            Effect.StopTime = DateTime.Now;
            Effect.UpdateTime = DateTime.Now;
        }

        // Token: 0x06000191 RID: 401 RVA: 0x00011450 File Offset: 0x0000F650
        private bool EffectPlayback(vJoyFFBPacket Packet) {
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

        // Token: 0x06000192 RID: 402 RVA: 0x0001149B File Offset: 0x0000F69B
        private bool EffectDispose(vJoyFFBPacket Packet, vJoyFFBEffect Effect) {
            if (Packet.BlockIndex == Effect.Parameters.Index) {
                Effect.Dispose();
                Effect.LAPacket = Packet;
                return true;
            }
            return false;
        }

        // Token: 0x06000193 RID: 403 RVA: 0x000114C0 File Offset: 0x0000F6C0
        private bool EffectDispose(vJoyFFBPacket Packet) {
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

        // Token: 0x06000194 RID: 404 RVA: 0x0001150C File Offset: 0x0000F70C
        private bool DeviceControl(vJoyFFBPacket Packet) {
            if (Packet.FFBPType == FFBPType.PT_CTRLREP) {
                switch (Packet.FFB_CTRL) {
                    case FFB_CTRL.CTRL_ENACT:
                        this.EnabledActuators = true;
                        break;
                    case FFB_CTRL.CTRL_DISACT:
                        this.EnabledActuators = false;
                        break;
                    case FFB_CTRL.CTRL_STOPALL:
                        for (int i = 0; i < this.Effects.Count; i++) {
                            if (this.Effects[i].Status == VirtualController.DirectInput.EffectStatus.Playing && this.Effects[i].Parameters.TriggerButton == -1) {
                                this.Effects[i].LAPacket = Packet;
                                this.StopEffect(this.Effects[i]);
                            }
                        }
                        this.DevicePause = false;
                        break;
                    case FFB_CTRL.CTRL_DEVRST:
                        for (int j = 0; j < this.Effects.Count; j++) {
                            if (this.Effects[j].Status == VirtualController.DirectInput.EffectStatus.Playing && this.Effects[j].Parameters.TriggerButton == -1) {
                                this.Effects[j].LAPacket = Packet;
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

        // Token: 0x06000195 RID: 405 RVA: 0x00011680 File Offset: 0x0000F880
        private bool MiscControl(vJoyFFBPacket Packet) {
            bool flag = false;
            switch (Packet.FFBPType) {
                case FFBPType.PT_BLKFRREP:
                    flag = this.EffectDispose(Packet);
                    break;
            }
            Packet.Handled = flag;
            return flag;
        }

        // Token: 0x06000196 RID: 406 RVA: 0x000116E4 File Offset: 0x0000F8E4
        private bool GainControl(vJoyFFBPacket Packet) {
            if (Packet.FFBPType == FFBPType.PT_GAINREP) {
                this.Gain = (int)Packet.Gain * 10000 / 255;
                Packet.Handled = true;
                return true;
            }
            return false;
        }

        // Token: 0x06000197 RID: 407 RVA: 0x00011714 File Offset: 0x0000F914
        public bool Do(vJoyFFBPacket Packet) {
            if (this.Packets.Count > 60) {
                this.PacketCleaning();
            }
            if (this.Effects.Count > 256) {
                this.EffectCleaning();
            }
            bool flag = true;
            if (!this.UpdateBaseParams(Packet) && !this.CreateBaseParams(Packet) && !this.UpdateTypeSpecificParams(Packet) && !this.UpdateEnvelopeParams(Packet) && !this.EffectPlayback(Packet) && !this.GainControl(Packet) && !this.DeviceControl(Packet) && !this.MiscControl(Packet)) {
                flag = false;
            }
            this.Packets.Add(Packet);
            if (flag) {
                this.Effects.Sort(new Comparison<vJoyFFBEffect>(vJoyFfbState.EffectComparison_UpdateDateTime));
                return true;
            }
            return false;
        }

        // Token: 0x04000188 RID: 392
        public List<vJoyFFBPacket> Packets = new List<vJoyFFBPacket>();

        // Token: 0x04000189 RID: 393
        public bool EnabledActuators = true;

        // Token: 0x0400018A RID: 394
        public bool DevicePause;

        // Token: 0x0400018B RID: 395
        public int Gain = 10000;

        // Token: 0x0400018C RID: 396
        public List<vJoyFFBEffect> Effects = new List<vJoyFFBEffect>();
    }
}
