using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JoyMapper.ReVirtualController.vJoy;

namespace JoyMapper {
    public partial class vJoyController {
        private void GenerateIOEArgs(vJoyFFBPacket Packet) {
            if (Packet.Handled) {
                vJoyFfbState ffbState = this.ffbState;
                FFBAction ioeventArgs = new FFBAction();
                ioeventArgs.Modifier = VirtualController.ControlModifierType.Change;
                ioeventArgs.CustomDataType = VirtualController.ValueType.Class;//ValueType.Class;
                ioeventArgs.CustomData = ffbState;
                this.FFBDataReceived?.Invoke(this, ioeventArgs);
                int count = ffbState.Effects.Count;
                vJoyFFBEffect vJoyFFBEffect = null;
                if (count > 0) {
                    vJoyFFBEffect = ffbState.Effects[count - 1];
                }
                FFBPType ffbptype = Packet.FFBPType;
                if (ffbptype != FFBPType.PT_EFFREP) {
                    switch (ffbptype) {
                        case FFBPType.PT_EFOPREP:
                            if (vJoyFFBEffect != null) {
                                VirtualController.ControlModifierType controlModifierType = VirtualController.ControlModifierType.None;
                                switch (Packet.FFB_EFF_OP.EffectOp) {
                                    case FFBOP.EFF_START:
                                        controlModifierType = VirtualController.ControlModifierType.On;
                                        if (vJoyFFBEffect.LAPacket != Packet || vJoyFFBEffect.Status != VirtualController.DirectInput.EffectStatus.Playing) {
                                            vJoyFFBEffect = null;
                                        }
                                        break;
                                    case FFBOP.EFF_SOLO:
                                        foreach (vJoyFFBEffect vJoyFFBEffect2 in ffbState.Effects) {
                                            if (vJoyFFBEffect2.LAPacket == Packet && vJoyFFBEffect2.Status == VirtualController.DirectInput.EffectStatus.Stopped) {
                                                FFBAction ioeventArgs2 = new FFBAction();
                                                ioeventArgs2.Number = (int)vJoyFFBEffect2.Parameters.Type;
                                                ioeventArgs2.Modifier = VirtualController.ControlModifierType.Off;
                                                ioeventArgs2.CustomDataType = VirtualController.ValueType.Class;
                                                ioeventArgs2.CustomData = ffbState;
                                                this.FFBDataReceived?.Invoke(this, ioeventArgs2);
                                            }
                                        }
                                        if (vJoyFFBEffect.LAPacket != Packet || vJoyFFBEffect.Status != VirtualController.DirectInput.EffectStatus.Playing) {
                                            vJoyFFBEffect = null;
                                        } else {
                                            controlModifierType = VirtualController.ControlModifierType.On;
                                        }
                                        break;
                                    case FFBOP.EFF_STOP:
                                        controlModifierType = VirtualController.ControlModifierType.Off;
                                        if (vJoyFFBEffect.LAPacket != Packet || vJoyFFBEffect.Status != VirtualController.DirectInput.EffectStatus.Stopped) {
                                            vJoyFFBEffect = null;
                                        }
                                        break;
                                }
                                if (vJoyFFBEffect != null) {
                                    FFBAction ioeventArgs3 = new FFBAction();
                                    ioeventArgs3.Number = (int)vJoyFFBEffect.Parameters.Type;
                                    ioeventArgs3.Modifier = controlModifierType;
                                    ioeventArgs3.CustomDataType = VirtualController.ValueType.Class;
                                    ioeventArgs3.CustomData = ffbState;
                                    VirtualController.DirectInput.TypeSpecificParameters typeSpecificParameters = vJoyFFBEffect.Parameters.TypeSpecificParameters;
                                    VirtualController.DirectInput.FFBEffect type = vJoyFFBEffect.Parameters.Type;
                                    if (type != VirtualController.DirectInput.FFBEffect.ConstantForce) {
                                        if (type != VirtualController.DirectInput.FFBEffect.RampForce) {
                                            if (VirtualController.DirectInput.TypeSpecificParameters.IsPeriodicEffect(vJoyFFBEffect.Parameters.Type)) {
                                                ioeventArgs3.Value1 = (double)typeSpecificParameters.PeriodicForce.Magnitude / 10000.0;
                                            }
                                            if (VirtualController.DirectInput.TypeSpecificParameters.IsConditionEffect(vJoyFFBEffect.Parameters.Type) && typeSpecificParameters.ConditionSet.Conditions.Length != 0) {
                                                ioeventArgs3.Value1 = (double)(typeSpecificParameters.ConditionSet.Conditions[0].PositiveCoefficient + typeSpecificParameters.ConditionSet.Conditions[0].NegativeCoefficient) / 20000.0;
                                            }
                                        } else {
                                            ioeventArgs3.Value1 = (double)(typeSpecificParameters.RampForce.End - typeSpecificParameters.RampForce.Start);
                                        }
                                    } else {
                                        ioeventArgs3.Value1 = (double)typeSpecificParameters.ConstantForce.Magnitude / 10000.0;
                                    }
                                    if (controlModifierType == VirtualController.ControlModifierType.Off) {
                                        ioeventArgs3.Value1 = -ioeventArgs3.Value1;
                                    }
                                    this.FFBDataReceived?.Invoke(this, ioeventArgs3);
                                    return;
                                }
                            }
                            break;
                        case FFBPType.PT_BLKFRREP:
                            if (vJoyFFBEffect.LAPacket == Packet && !vJoyFFBEffect.Downloaded) {
                                FFBAction ioeventArgs4 = new FFBAction();
                                ioeventArgs4.Number = (int)vJoyFFBEffect.Parameters.Type;
                                ioeventArgs4.Modifier = VirtualController.ControlModifierType.Release;
                                ioeventArgs4.CustomDataType = VirtualController.ValueType.Class;
                                ioeventArgs4.CustomData = ffbState;
                                this.FFBDataReceived?.Invoke(this, ioeventArgs4);
                                return;
                            }
                            break;
                        case FFBPType.PT_CTRLREP:
                            switch (Packet.FFB_CTRL) {
                                case FFB_CTRL.CTRL_ENACT: {
                                    FFBAction ioeventArgs5 = new FFBAction();
                                    ioeventArgs5.Number = 0;
                                    ioeventArgs5.Modifier = VirtualController.ControlModifierType.On;
                                    ioeventArgs5.CustomDataType = VirtualController.ValueType.Class;
                                    ioeventArgs5.CustomData = ffbState;
                                    this.FFBDataReceived?.Invoke(this, ioeventArgs5);
                                    return;
                                }
                                case FFB_CTRL.CTRL_DISACT: {
                                    FFBAction ioeventArgs6 = new FFBAction();
                                    ioeventArgs6.Number = 0;
                                    ioeventArgs6.Modifier = VirtualController.ControlModifierType.Off;
                                    ioeventArgs6.CustomDataType = VirtualController.ValueType.Class;
                                    ioeventArgs6.CustomData = ffbState;
                                    this.FFBDataReceived?.Invoke(this, ioeventArgs6);
                                    return;
                                }
                                case FFB_CTRL.CTRL_STOPALL: {
                                    foreach (vJoyFFBEffect vJoyFFBEffect3 in ffbState.Effects) {
                                        if (vJoyFFBEffect3.LAPacket == Packet && vJoyFFBEffect3.Status == VirtualController.DirectInput.EffectStatus.Stopped) {
                                            FFBAction ioeventArgs7 = new FFBAction();
                                            ioeventArgs7.Number = (int)vJoyFFBEffect3.Parameters.Type;
                                            ioeventArgs7.Modifier = VirtualController.ControlModifierType.Off;
                                            ioeventArgs7.CustomDataType = VirtualController.ValueType.Class;
                                            ioeventArgs7.CustomData = ffbState;
                                            this.FFBDataReceived?.Invoke(this,ioeventArgs7);
                                        }
                                    }
                                    FFBAction ioeventArgs8 = new FFBAction();
                                    ioeventArgs8.Number = 0;
                                    ioeventArgs8.Modifier = VirtualController.ControlModifierType.Release;
                                    ioeventArgs8.CustomDataType = VirtualController.ValueType.Class;
                                    ioeventArgs8.CustomData = ffbState;
                                    this.FFBDataReceived?.Invoke(this,ioeventArgs8);
                                    return;
                                }
                                case FFB_CTRL.CTRL_DEVRST: {
                                    foreach (vJoyFFBEffect vJoyFFBEffect4 in ffbState.Effects) {
                                        if (vJoyFFBEffect4.LAPacket == Packet && vJoyFFBEffect4.Status == VirtualController.DirectInput.EffectStatus.Stopped) {
                                            FFBAction ioeventArgs9 = new FFBAction();
                                            ioeventArgs9.Number = (int)vJoyFFBEffect4.Parameters.Type;
                                            ioeventArgs9.Modifier = VirtualController.ControlModifierType.Off;
                                            ioeventArgs9.CustomDataType = VirtualController.ValueType.Class;
                                            ioeventArgs9.CustomData = ffbState;
                                            this.FFBDataReceived?.Invoke(this,ioeventArgs9);
                                        }
                                    }
                                    FFBAction ioeventArgs10 = new FFBAction();
                                    ioeventArgs10.Number = 0;
                                    ioeventArgs10.Modifier = VirtualController.ControlModifierType.Reset;
                                    ioeventArgs10.CustomDataType = VirtualController.ValueType.Class;
                                    ioeventArgs10.CustomData = ffbState;
                                    this.FFBDataReceived?.Invoke(this,ioeventArgs10);
                                    return;
                                }
                                case FFB_CTRL.CTRL_DEVPAUSE: {
                                    FFBAction ioeventArgs11 = new FFBAction();
                                    ioeventArgs11.Number = 0;
                                    ioeventArgs11.Modifier = VirtualController.ControlModifierType.Pause;
                                    ioeventArgs11.CustomDataType = VirtualController.ValueType.Class;
                                    ioeventArgs11.CustomData = ffbState;
                                    this.FFBDataReceived?.Invoke(this,ioeventArgs11);
                                    return;
                                }
                                case FFB_CTRL.CTRL_DEVCONT: {
                                    FFBAction ioeventArgs12 = new FFBAction();
                                    ioeventArgs12.Number = 0;
                                    ioeventArgs12.Modifier = VirtualController.ControlModifierType.Continue;
                                    ioeventArgs12.CustomDataType = VirtualController.ValueType.Class;
                                    ioeventArgs12.CustomData = ffbState;
                                    this.FFBDataReceived?.Invoke(this,ioeventArgs12);
                                    return;
                                }
                                default:
                                    return;
                            }
                            break;
                        case FFBPType.PT_GAINREP: {
                            FFBAction ioeventArgs13 = new FFBAction();
                            ioeventArgs13.Number = 13;
                            ioeventArgs13.Modifier = VirtualController.ControlModifierType.Change;
                            ioeventArgs13.CustomDataType = VirtualController.ValueType.Class;
                            ioeventArgs13.CustomData = ffbState;
                            ioeventArgs13.Value1 = (double)ffbState.Gain / 10000.0;
                            this.FFBDataReceived?.Invoke(this,ioeventArgs13);
                            return;
                        }
                        default:
                            if (ReVirtualController.vJoy.Utils.IsTypeSpecificPacket(Packet.FFBPType)) {
                                vJoyFFBEffect = ffbState.Effects[count - 1];
                                if (vJoyFFBEffect.LAPacket == Packet) {
                                    if (vJoyFFBEffect.Parameters.TriggerButton == -1) {
                                        if (vJoyFFBEffect.Status == VirtualController.DirectInput.EffectStatus.Playing) {
                                            FFBAction ioeventArgs14 = new FFBAction();
                                            ioeventArgs14.Number = (int)vJoyFFBEffect.Parameters.Type;
                                            ioeventArgs14.Modifier = VirtualController.ControlModifierType.Update;
                                            ioeventArgs14.CustomDataType = VirtualController.ValueType.Class;
                                            ioeventArgs14.CustomData = ffbState;
                                            this.FFBDataReceived?.Invoke(this,ioeventArgs14);
                                            return;
                                        }
                                    } else if (vJoyFFBEffect.Status != VirtualController.DirectInput.EffectStatus.None) {
                                        FFBAction ioeventArgs15 = new FFBAction();
                                        ioeventArgs15.Number = (int)vJoyFFBEffect.Parameters.Type;
                                        ioeventArgs15.Modifier = VirtualController.ControlModifierType.Update;
                                        ioeventArgs15.CustomDataType = VirtualController.ValueType.Class;
                                        ioeventArgs15.CustomData = ffbState;
                                        this.FFBDataReceived?.Invoke(this,ioeventArgs15);
                                    }
                                }
                            }
                            break;
                    }
                } else if (vJoyFFBEffect != null && vJoyFFBEffect.LAPacket == Packet && vJoyFFBEffect.Status == VirtualController.DirectInput.EffectStatus.Idle) {
                    if (vJoyFFBEffect.Parameters.TriggerButton == -1) {
                        if (vJoyFFBEffect.StartTime == DateTime.MinValue) {
                            FFBAction ioeventArgs16 = new FFBAction();
                            ioeventArgs16.Number = (int)vJoyFFBEffect.Parameters.Type;
                            ioeventArgs16.Modifier = VirtualController.ControlModifierType.Create;
                            ioeventArgs16.CustomDataType = VirtualController.ValueType.Class;
                            ioeventArgs16.CustomData = ffbState;
                            this.FFBDataReceived?.Invoke(this,ioeventArgs16);
                            return;
                        }
                        FFBAction ioeventArgs17 = new FFBAction();
                        ioeventArgs17.Number = (int)vJoyFFBEffect.Parameters.Type;
                        ioeventArgs17.Modifier = VirtualController.ControlModifierType.Update;
                        ioeventArgs17.CustomDataType = VirtualController.ValueType.Class;
                        ioeventArgs17.CustomData = ffbState;
                        this.FFBDataReceived?.Invoke(this,ioeventArgs17);
                        return;
                    } else {
                        if (vJoyFFBEffect.BaseUpdateTime == vJoyFFBEffect.CreationTime) {
                            FFBAction ioeventArgs18 = new FFBAction();
                            ioeventArgs18.Number = (int)vJoyFFBEffect.Parameters.Type;
                            ioeventArgs18.Modifier = VirtualController.ControlModifierType.Create;
                            ioeventArgs18.CustomDataType = VirtualController.ValueType.Class;
                            ioeventArgs18.CustomData = ffbState;
                            this.FFBDataReceived?.Invoke(this,ioeventArgs18);
                            return;
                        }
                        FFBAction ioeventArgs19 = new FFBAction();
                        ioeventArgs19.Number = (int)vJoyFFBEffect.Parameters.Type;
                        ioeventArgs19.Modifier = VirtualController.ControlModifierType.Update;
                        ioeventArgs19.CustomDataType = VirtualController.ValueType.Class;
                        ioeventArgs19.CustomData = ffbState;
                        this.FFBDataReceived?.Invoke(this,ioeventArgs19);
                        return;
                    }
                }
            }
        }
    }
}
