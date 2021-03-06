﻿using JoyMapper.FFB;
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
        private void InternalHandlerFFBPacket(VirtualFFBPacket packet) {
            uint eID = packet.BlockIndex - 1;
            switch (packet._FFBPType) {
                case FFBPType.PT_NEWEFREP: {
                    if (packet.BlockIndex == 0 || packet.NextBlockIndex == 0)
                        break;
                    //this.RunExclusive(() => {
                    if (FFBEffects[eID] != null) {
                        FFBEffects[eID].Dispose();
                        FFBEffects[eID] = null;
                    }
                    PhysicalFFBEffect effect = new PhysicalFFBEffect();
                    FFBEffects[eID] = effect;
                    effect.InitFromFFBPacket(packet);
                    FFBEffects[eID].Parameters.SetAxes(new int[1] { this.FFBAxes[0] }, FFBEffects[eID].Parameters.Directions);
                    FFBEffects[eID].Object = new Effect(this.joystick, FFBEffects[eID].Parameters.Type, FFBEffects[eID].Parameters);
                    logger.Info($"Created effect [EBI={eID + 1}] {VirtualController.virtualEffectGuidMapToString[FFBEffects[eID].Parameters.Type]} from packet {packet.CreationTime.ToString("o")}");
                    //});
                    break;
                }
                case FFBPType.PT_EFFREP: {
                    if (packet.BlockIndex == 0)
                        break;
                    logger.Info($"Starting update effect [EBI={eID + 1}] __ from packet {packet.CreationTime.ToString("o")}");
                    if (FFBEffects[eID] == null) {
                        PhysicalFFBEffect effect = new PhysicalFFBEffect();
                        FFBEffects[eID] = effect;
                        effect.InitFromFFBPacket(packet);
                        FFBEffects[eID].Parameters.SetAxes(new int[1] { this.FFBAxes[0] }, FFBEffects[eID].Parameters.Directions);
                        try {
                            FFBEffects[eID].Object = new Effect(this.joystick, FFBEffects[eID].Parameters.Type, FFBEffects[eID].Parameters);
                        }
                        catch(Exception ex) {
                            ;
                        }
                    }
                    FFBEffects[eID].UpdateFromFFBPacket(packet);
                    FFBEffects[eID].Parameters.SetAxes(new int[1] { this.FFBAxes[0] }, FFBEffects[eID].Parameters.Directions);
                    //this.RunExclusive(() => {
                    FFBEffects[eID].Object.SetParameters(FFBEffects[eID].Parameters,
                        EffectParameterFlags.TypeSpecificParameters |
                        //EffectParameterFlags.Axes |
                        EffectParameterFlags.Direction |
                        EffectParameterFlags.Duration |
                        //EffectParameterFlags.Envelope |
                        EffectParameterFlags.Gain |
                        EffectParameterFlags.SamplePeriod |
                        EffectParameterFlags.StartDelay |
                        EffectParameterFlags.TriggerButton |
                        EffectParameterFlags.TriggerRepeatInterval
                        );
                    logger.Info($"Updated effect [EBI={eID + 1}] {VirtualController.virtualEffectGuidMapToString[FFBEffects[eID].Parameters.Type]} from packet {packet.CreationTime.ToString("o")}");
                    //FFBEffects[eID].Object.Start(1);
                    //FFBEffects[eID].Object.Stop();
                    //});

                    break;
                }
                case FFBPType.PT_CONSTREP: {
                    if (packet.BlockIndex == 0)
                        break;
                    if (FFBEffects[eID] == null)
                        break;
                    if (FFBEffects[eID].Parameters.Parameters == null)
                        FFBEffects[eID].Parameters.Parameters = new ConstantForce();
                    FFBEffects[eID].Parameters.Parameters.As<ConstantForce>().Magnitude = packet.FFB_EFF_CONSTANT.Magnitude;
                    if (FFBEffects[eID].Object != null) {
                        //this.RunExclusive(() => {
                        FFBEffects[eID].Object.SetParameters(FFBEffects[eID].Parameters, EffectParameterFlags.TypeSpecificParameters);
                        //});
                    }
                    break;
                }
                case FFBPType.PT_CONDREP: {
                    if (packet.BlockIndex == 0)
                        break;
                    if (FFBEffects[eID] == null)
                        break;
                    if (FFBEffects[eID].Parameters.Parameters == null) {
                        FFBEffects[eID].Parameters.Parameters = new ConditionSet();
                        FFBEffects[eID].Parameters.Parameters.As<ConditionSet>().Conditions = new Condition[1];
                    }
                    ConditionSet set = FFBEffects[eID].Parameters.Parameters.As<ConditionSet>();
                    if (set.Conditions != null) {
                        set.Conditions[0].DeadBand = packet.FFB_EFF_COND.DeadBand;
                        set.Conditions[0].NegativeCoefficient = packet.FFB_EFF_COND.NegCoeff;
                        set.Conditions[0].NegativeSaturation = (int)packet.FFB_EFF_COND.NegSatur;
                        set.Conditions[0].PositiveCoefficient = packet.FFB_EFF_COND.PosCoeff;
                        set.Conditions[0].PositiveSaturation = (int)packet.FFB_EFF_COND.PosSatur;
                        set.Conditions[0].Offset = packet.FFB_EFF_COND.CenterPointOffset;
                    }
                    if (FFBEffects[eID].Object != null) {
                        //this.RunExclusive(() => {
                        FFBEffects[eID].Object.SetParameters(FFBEffects[eID].Parameters, EffectParameterFlags.TypeSpecificParameters);
                        //});
                    }
                    break;
                }
                case FFBPType.PT_PRIDREP: {
                    if (packet.BlockIndex == 0)
                        break;
                    if (FFBEffects[eID] == null)
                        break;
                    if (FFBEffects[eID].Parameters.Parameters == null)
                        FFBEffects[eID].Parameters.Parameters = new PeriodicForce();
                    PeriodicForce force = FFBEffects[eID].Parameters.Parameters.As<PeriodicForce>();
                    force.Magnitude = (int)packet.FFB_EFF_PERIOD.Magnitude;
                    force.Offset = packet.FFB_EFF_PERIOD.Offset;
                    force.Period = (int)packet.FFB_EFF_PERIOD.Period * 1000;
                    force.Phase = (int)packet.FFB_EFF_PERIOD.Phase;
                    if (FFBEffects[eID].Object != null) {
                        //this.RunExclusive(() => {
                        FFBEffects[eID].Object.SetParameters(FFBEffects[eID].Parameters, EffectParameterFlags.TypeSpecificParameters);
                        //});
                    }
                    break;
                }
                case FFBPType.PT_RAMPREP: {
                    if (packet.BlockIndex == 0)
                        break;
                    if (FFBEffects[eID] == null)
                        break;
                    if (FFBEffects[eID].Parameters.Parameters == null)
                        FFBEffects[eID].Parameters.Parameters = new RampForce();
                    RampForce force = FFBEffects[eID].Parameters.Parameters.As<RampForce>();
                    force.Start = packet.FFB_EFF_RAMP.Start;
                    force.End = packet.FFB_EFF_RAMP.End;
                    if (FFBEffects[eID].Object != null) {
                        //this.RunExclusive(() => {
                        FFBEffects[eID].Object.SetParameters(FFBEffects[eID].Parameters, EffectParameterFlags.TypeSpecificParameters);
                        //});
                    }
                    break;
                }
                case FFBPType.PT_EFOPREP: {
                    if (packet.BlockIndex == 0)
                        break;
                    if (FFBEffects[eID] == null)
                        break;
                    //this.RunExclusive(() => {
                    switch (packet.FFB_EFF_OP.EffectOp) {
                        case FFBOP.EFF_START: {
                            //logger.Debug($"Trying run object {FFBEffects[eID].ToString()} from packet {packet.CreationTime.ToString("o")}");
                            FFBEffects[eID].Object?.Start(packet.FFB_EFF_OP.LoopCount, EffectPlayFlags.NoDownload);
                            break;
                        }
                        case FFBOP.EFF_STOP: {
                            FFBEffects[eID].Object?.Stop();
                            break;
                        }
                        case FFBOP.EFF_SOLO: {
                            logger.Warn($"EFF_SOLO on {eID} not implemented");
                            break;
                        }
                    }
                    //});
                    break;
                }
                case FFBPType.PT_BLKFRREP: {
                    if (packet.BlockIndex == 0)
                        break;
                    if (FFBEffects[eID] == null)
                        break;
                    FFBEffects[eID].Dispose();
                    FFBEffects[eID] = null;
                    break;
                }
                case FFBPType.PT_CTRLREP: {
                    //this.RunExclusive(() => {
                    this.SendFFBCommand(packet.GetFFBCommand());
                    switch (packet.FFB_CTRL) {
                        case FFB_CTRL.CTRL_STOPALL: {
                            for (int i = 0; i < this.FFBEffects.Length; i++) {
                                if (FFBEffects[i] != null) {
                                    //FFBEffects[i].UpdateEffectStatus();
                                    //if (FFBEffects[i].Status == FFB.EffectStatus.Playing && FFBEffects[i].Parameters.TriggerButton == -1)
                                    if (FFBEffects[i] != null)
                                        FFBEffects[i].Object?.Stop();
                                }
                            }
                            break;
                        }
                        case FFB_CTRL.CTRL_DEVRST: {
                            for (int i = 0; i < this.FFBEffects.Length; i++) {
                                if (FFBEffects[i] != null) {
                                    //FFBEffects[i].UpdateEffectStatus();
                                    //if (FFBEffects[i].Status == FFB.EffectStatus.Playing && FFBEffects[i].Parameters.TriggerButton == -1)
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
                    //});
                    break;
                }
                case FFBPType.PT_GAINREP: {
                    //this.RunExclusive(() => {  });
                    this.joystick.Properties.ForceFeedbackGain = (int)(packet.Gain * (10000 / 255.0));
                    break;
                }
                default:
                    break;
            }
        }
        public void HandleFFBPacket(VirtualFFBPacket packet) {
            try {
                if (!Exclusive) {
                    this.joystick.Unacquire();
                    this.SetCooperativeLevel(CooperativeLevel.Exclusive | CooperativeLevel.Background);
                    this.joystick.Acquire();
                    Exclusive = true;
                    logger.Warn("PREVENTIVE Reaq joystic in exclusive mode because of idk why it brokes");
                }
                this.InternalHandlerFFBPacket(packet);
            } catch (SharpDX.SharpDXException ex) {
                if (ex.HResult == -2147220987) {
                    Exclusive = false;
                    HandleFFBPacket(packet);
                } else
                    logger.Warn($"Packet handling SHARP error [EBI={packet.BlockIndex + 1}], {ex} {ex.StackTrace}");
            } catch (Exception ex) {
                logger.Warn($"Packet handling error [EBI={packet.BlockIndex + 1}], {ex} {ex.StackTrace}");
            }
        }
    }
}
