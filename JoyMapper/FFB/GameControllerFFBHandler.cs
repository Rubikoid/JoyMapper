using JoyMapper.FFB;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace JoyMapper.Controller {
    public partial class GameController {
        public PhysicalFFBEffect[] FFBEffects = new PhysicalFFBEffect[128];
        public void HandleFFBPacket(VirtualFFBPacket packet) {
            uint eID = packet.BlockIndex - 1;
            switch (packet._FFBPType) {
                case FFBPType.PT_NEWEFREP: {
                    if (packet.BlockIndex == 0)
                        break;
                    if (FFBEffects[eID] != null) {
                        FFBEffects[eID].Dispose();
                        FFBEffects[eID] = null;
                    }
                    PhysicalFFBEffect effect = new PhysicalFFBEffect();
                    FFBEffects[eID] = effect;
                    effect.InitFromFFBPacket(packet);
                    break;
                }
                case FFBPType.PT_EFFREP: {
                    if (packet.BlockIndex == 0)
                        break;
                    FFBEffects[eID].InitFromFFBPacket(packet);
                    FFBEffects[eID].UpdateFromFFBPacket(packet);
                    FFBEffects[eID].Parameters.SetAxes(new int[1] { this.FFBAxes[0] }, FFBEffects[eID].Parameters.Directions);
                    FFBEffects[eID].Object = new Effect(this.joystick, FFBEffects[eID].Parameters.Type, FFBEffects[eID].Parameters);
                    break;
                }
                case FFBPType.PT_EFOPREP: {
                    if (packet.BlockIndex == 0)
                        break;
                    switch (packet.FFB_EFF_OP.EffectOp) {
                        case FFBOP.EFF_START: {
                            if (FFBEffects[eID] != null) {
                                FFBEffects[eID].Object?.Start();
                            }
                            break;
                        }
                        case FFBOP.EFF_STOP: {
                            if (FFBEffects[eID] != null) {
                                FFBEffects[eID].Object?.Stop();
                            }
                            break;
                        }
                        case FFBOP.EFF_SOLO: {
                            if (FFBEffects[eID] != null) {
                                logger.Warn($"EFF_SOLO on {eID} not implemented");
                            }
                            break;
                        }
                    }
                    break;
                }
                case FFBPType.PT_BLKFRREP: {
                    if (packet.BlockIndex == 0)
                        break;
                    if (FFBEffects[eID] != null) {
                        FFBEffects[eID].Dispose();
                        FFBEffects[eID] = null;
                    }
                    break;
                }
                case FFBPType.PT_CTRLREP: {
                    this.SendFFBCommand(packet.GetFFBCommand());
                    switch (packet.FFB_CTRL) {
                        case FFB_CTRL.CTRL_STOPALL: {
                            for (int i = 0; i < this.FFBEffects.Length; i++) {
                                if (FFBEffects[i] != null) {
                                    FFBEffects[i].UpdateEffectStatus();
                                    if (FFBEffects[i].Status == FFB.EffectStatus.Playing && FFBEffects[i].Parameters.TriggerButton == -1)
                                        if (FFBEffects[i] != null)
                                            FFBEffects[i].Object?.Stop();
                                }
                            }
                            break;
                        }
                        case FFB_CTRL.CTRL_DEVRST: {
                            for (int i = 0; i < this.FFBEffects.Length; i++) {
                                if (FFBEffects[i] != null) {
                                    FFBEffects[i].UpdateEffectStatus();
                                    if (FFBEffects[i].Status == FFB.EffectStatus.Playing && FFBEffects[i].Parameters.TriggerButton == -1)
                                        if (FFBEffects[i] != null) {
                                            FFBEffects[i].Dispose();
                                            FFBEffects[i] = null;
                                        }
                                }
                            }
                            break;
                        }
                        default: { break; }
                    }
                    break;
                }
                case FFBPType.PT_GAINREP: {
                    this.RunExclusive(() => { this.joystick.Properties.ForceFeedbackGain = packet.Gain * (10000 / 255); });
                    break;
                }
                default:
                    break;
            }
        }
    }
}
