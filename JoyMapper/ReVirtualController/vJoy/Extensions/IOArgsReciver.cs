using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JoyMapper.ReVirtualController.vJoy;

namespace JoyMapper {
    public partial class GameController {
        public void SetCooperativeLevel(SharpDX.DirectInput.CooperativeLevel Level) {
            this.joystick.SetCooperativeLevel(MainForm.PublicHandle, Level);
        }
        public void OnIOEvent(object sender, FFBAction IOEArgs) {
            vJoyFfbState vJoyFfbState = (vJoyFfbState)IOEArgs.CustomData;
            int gain = vJoyFfbState.Gain;
            int loopCount = 1;
            bool solo = false;
            int count = vJoyFfbState.Effects.Count;
            VirtualController.DirectInput.EffectParameters effectParameters = new VirtualController.DirectInput.EffectParameters();
            if (count > 0) {
                vJoyFFBEffect vJoyFFBEffect = vJoyFfbState.Effects[count - 1];
                loopCount = vJoyFFBEffect.LoopCount;
                solo = vJoyFFBEffect.Solo;
                effectParameters.CopyFrom(vJoyFFBEffect.Parameters);
            } else {
                //VirtualController.DirectInput.Functions.RandomEffectParameters(ref effectParameters);
            }
            this.FFBHandler_DirectInput(IOEArgs, effectParameters, (double)gain, loopCount, solo);
        }

        public void StartEffect(VirtualController.DirectInput.Effect Effect, int LoopCount, SharpDX.DirectInput.EffectPlayFlags Flags) {
            Effect.Object.Start(LoopCount, Flags);
            Effect.Status = VirtualController.DirectInput.EffectStatus.Playing;
            Effect.StartTime = DateTime.Now;
        }
        public void StopEffect(VirtualController.DirectInput.Effect Effect) {
            Effect.Object.Stop();
            Effect.Status = VirtualController.DirectInput.EffectStatus.Stopped;
            Effect.StopTime = DateTime.Now;
        }

        public void SendForceFeedbackCommand(SharpDX.DirectInput.ForceFeedbackCommand FFBCommand) {
            lock (this.lockObj) {
                // this.SetCooperativeLevel(SharpDX.DirectInput.CooperativeLevel.Exclusive | SharpDX.DirectInput.CooperativeLevel.Background);
                try {
                    this.joystick.SendForceFeedbackCommand(FFBCommand);
                }
                catch(SharpDX.SharpDXException ex) {
                    if(ex.HResult == -2147220987) {
                        this.joystick.Unacquire();
                        this.SetCooperativeLevel(SharpDX.DirectInput.CooperativeLevel.Exclusive | SharpDX.DirectInput.CooperativeLevel.Background);
                        this.joystick.Acquire();
                        this.SendForceFeedbackCommand(FFBCommand);
                        Console.WriteLine("Reaq joystic in exclusive mode because of idk why it brokes");
                    }
                    else 
                        throw ex;
                }
            }
        }

        public Dictionary<VirtualController.AxisType, int> GetFFBAxisIdsList() {
            Dictionary<VirtualController.AxisType, int> dictionary = new Dictionary<VirtualController.AxisType, int>();
            foreach (SharpDX.DirectInput.DeviceObjectInstance deviceObjectInstance in this.joystick.GetObjects()) {
                int num = (int)deviceObjectInstance.ObjectId;
                if ((num & 3) != 0 && (num & 16777216) != 0) {
                    VirtualController.AxisType axis = GameController.GetAxis(deviceObjectInstance.Usage, num, dictionary);
                    if (axis != VirtualController.AxisType.None) {
                        dictionary.Add(axis, num);
                    }
                }
            }
            return dictionary;
        }

        // Token: 0x06000212 RID: 530 RVA: 0x00014458 File Offset: 0x00012658
        private static VirtualController.AxisType GetAxis(short Usage, int ObjectId, Dictionary<VirtualController.AxisType, int> UsedAxes) {
            VirtualController.AxisType axisType = (VirtualController.AxisType)Usage;
            if (UsedAxes.ContainsKey(axisType)) {
                if (ObjectId != UsedAxes[axisType]) {
                    axisType = VirtualController.AxisType.None;
                    foreach (VirtualController.AxisType axisType2 in Enum.GetValues(typeof(VirtualController.AxisType))) {
                        if (axisType2 >= VirtualController.AxisType.X1 && axisType2 <= VirtualController.AxisType.X6 && !UsedAxes.ContainsKey(axisType2))
                            return axisType2;
                    }
                }
            }
            return VirtualController.AxisType.None;
        }

        private void FFBHandler_DirectInput(FFBAction IOEArgs, VirtualController.DirectInput.EffectParameters Params, double InputValue, int LoopCount, bool Solo) {
            //VirtualController.DirectInput.FFBEffect ffbeffect = (VirtualController.DirectInput.FFBEffect)Control.InputData.Number;
            VirtualController.DirectInput.FFBEffect ffbeffect2 = (VirtualController.DirectInput.FFBEffect)IOEArgs.Number;
            if (ffbeffect2 != VirtualController.DirectInput.FFBEffect.None) {
                if (ffbeffect2 != VirtualController.DirectInput.FFBEffect.Gain) {
                    /*if (ffbeffect != ffbeffect2) {
                        Params.Transform(ffbeffect2);
                    }*/
                    // VirtualController.DirectInput.Functions.CorrectEffectParameters(ref Params, Control.OutputData.DirectInputForceFeedbackSettings);
                    // idk about this func, so just comment and belive, that it will work.
                    VirtualController.DirectInput.Effect effect = null;
                    int count = this.SupportedFFBEffects.Count;
                    if (Params.Index > 0) {
                        if (count <= 0) {
                            goto IL_21C;
                        }
                        this.SortEffectsByUpdateDateTime();
                        using (List<VirtualController.DirectInput.Effect>.Enumerator enumerator = this.ForceFeedbackEffects.GetEnumerator()) {
                            while (enumerator.MoveNext()) {
                                VirtualController.DirectInput.Effect effect2 = enumerator.Current;
                                if (effect2.Parameters.Index == Params.Index && effect2.Parameters.Type == ffbeffect2) {
                                    effect = effect2;
                                    effect.UpdateEffectStatus();
                                    break;
                                }
                            }
                            goto IL_21C;
                        }
                    }
                    if (count > 0) {
                        this.SortEffectsByUpdateDateTime();
                        effect = this.ForceFeedbackEffects[count - 1];
                        effect.UpdateEffectStatus();
                    }
                IL_21C:
                    VirtualController.ControlModifierType modifier = IOEArgs.Modifier;
                    if (modifier <= VirtualController.ControlModifierType.Off) {
                        if (modifier != VirtualController.ControlModifierType.On) {
                            if (modifier != VirtualController.ControlModifierType.Off) {
                                return;
                            }
                            if (effect != null && effect.Status != VirtualController.DirectInput.EffectStatus.Stopped) {
                                this.StopEffect(effect);
                                return;
                            }
                        } else if (effect != null && effect.Status != VirtualController.DirectInput.EffectStatus.Playing && effect.Status != VirtualController.DirectInput.EffectStatus.None) {
                            SharpDX.DirectInput.EffectPlayFlags flags = SharpDX.DirectInput.EffectPlayFlags.None;
                            if (IOEArgs.Value1 != -1.7976931348623157E+308) {
                                LoopCount = (int)IOEArgs.Value1;
                            }
                            /*if (IOEArgs.Value2 != -1.7976931348623157E+308) {
                                Solo = (IOEArgs.Value2 > 0.0);
                            }*/
                            if (Solo) {
                                flags = SharpDX.DirectInput.EffectPlayFlags.Solo;
                            }
                            this.StartEffect(effect, LoopCount, flags);
                            return;
                        }
                    } else if (modifier != VirtualController.ControlModifierType.Create) {
                        if (modifier != VirtualController.ControlModifierType.Update) {
                            if (modifier != VirtualController.ControlModifierType.Release) {
                                return;
                            }
                            if (effect != null) {
                                if (effect.Status != VirtualController.DirectInput.EffectStatus.Stopped) {
                                    this.StopEffect(effect);
                                }
                                effect.Dispose();
                                this.ForceFeedbackEffects.Remove(effect);
                            }
                        } else if (effect != null) {
                            SharpDX.DirectInput.EffectParameterFlags effectParameterFlags = SharpDX.DirectInput.EffectParameterFlags.None;
                            if (Params.Duration != effect.Parameters.Duration) {
                                effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.Duration;
                            }
                            if (Params.Gain != effect.Parameters.Gain) {
                                effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.Gain;
                            }
                            if (Params.SamplePeriod != effect.Parameters.SamplePeriod) {
                                effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.SamplePeriod;
                            }
                            if (Params.StartDelay != effect.Parameters.StartDelay) {
                                effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.StartDelay;
                            }
                            if (Params.TriggerButton != effect.Parameters.TriggerButton) {
                                effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.TriggerButton;
                            }
                            if (Params.TriggerRepeatInterval != effect.Parameters.TriggerRepeatInterval) {
                                effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.TriggerRepeatInterval;
                            }
                            if (Params.AxisSettings.Axes != effect.Parameters.AxisSettings.Axes) {
                                effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.Axes;
                            }
                            if (Params.AxisSettings.Directions != effect.Parameters.AxisSettings.Directions) {
                                effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.Direction;
                            }
                            if (Params.Envelope != effect.Parameters.Envelope) {
                                effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.Envelope;
                            }
                            VirtualController.DirectInput.FFBEffect type = effect.Parameters.Type;
                            if (type != VirtualController.DirectInput.FFBEffect.ConstantForce) {
                                if (type != VirtualController.DirectInput.FFBEffect.RampForce) {
                                    if (type != VirtualController.DirectInput.FFBEffect.CustomForce) {
                                        if (VirtualController.DirectInput.TypeSpecificParameters.IsPeriodicEffect(Params.Type) && Params.TypeSpecificParameters.PeriodicForce != effect.Parameters.TypeSpecificParameters.PeriodicForce) {
                                            effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.TypeSpecificParameters;
                                        }
                                        if (VirtualController.DirectInput.TypeSpecificParameters.IsConditionEffect(Params.Type) && Params.TypeSpecificParameters.ConditionSet != effect.Parameters.TypeSpecificParameters.ConditionSet) {
                                            effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.TypeSpecificParameters;
                                        }
                                    } else if (Params.TypeSpecificParameters.CustomForce != effect.Parameters.TypeSpecificParameters.CustomForce) {
                                        effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.TypeSpecificParameters;
                                    }
                                } else if (Params.TypeSpecificParameters.RampForce != effect.Parameters.TypeSpecificParameters.RampForce) {
                                    effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.TypeSpecificParameters;
                                }
                            } else if (Params.TypeSpecificParameters.ConstantForce != effect.Parameters.TypeSpecificParameters.ConstantForce) {
                                effectParameterFlags |= SharpDX.DirectInput.EffectParameterFlags.TypeSpecificParameters;
                            }
                            if (effectParameterFlags != SharpDX.DirectInput.EffectParameterFlags.None) {
                                effect.Parameters = Params;
                                this.SetEffectParameters(effect, effectParameterFlags);
                                return;
                            }
                        }
                    } else {
                        if (effect != null) {
                            if (effect.Status == VirtualController.DirectInput.EffectStatus.Playing) {
                                this.StopEffect(effect);
                            }
                            effect.Dispose();
                            this.ForceFeedbackEffects.Remove(effect);
                        }
                        effect = new VirtualController.DirectInput.Effect();
                        effect.Parameters = Params;
                        effect.FFBAxisIds = GameController.GetFFBAxisIds(Params, this.FFBAxisIds);
                        Guid ffbsharpDXEffectGuid = vJoyController.virtualEffectGuidMap[(FFBEType)(byte)effect.Parameters.Type];
                        foreach (VirtualController.DirectInput.EffectInfo effectInfo in this.SupportedFFBEffects_Full) {
                            if (effectInfo.Guid == ffbsharpDXEffectGuid) {
                                effect.Info = effectInfo;
                                break;
                            }
                        }
                        if (effect.Info.ParentDevice != null) {
                            this.SetEffectParameters(effect, SharpDX.DirectInput.EffectParameterFlags.None);
                            effect.Status = VirtualController.DirectInput.EffectStatus.Idle;
                            this.ForceFeedbackEffects.Add(effect);
                            return;
                        }
                    }
                    return;
                }
                int forceFeedbackGain = this.joystick.Properties.ForceFeedbackGain;
                double num = IOEArgs.Value1;
                /*if (Control.OutputData.UseInput) {
                    num *= InputValue;
                }*/
                int num2 = (int)(num * 10000.0);
                switch (IOEArgs.Modifier) {
                    case VirtualController.ControlModifierType.Set:
                        this.joystick.Properties.ForceFeedbackGain = num2;
                        return;
                    case VirtualController.ControlModifierType.Increase:
                        this.joystick.Properties.ForceFeedbackGain = forceFeedbackGain + num2;
                        return;
                    case VirtualController.ControlModifierType.Decrease:
                        this.joystick.Properties.ForceFeedbackGain = forceFeedbackGain - num2;
                        return;
                    default:
                        return;
                }
            } else {
                VirtualController.ControlModifierType modifier = IOEArgs.Modifier;
                if (modifier <= VirtualController.ControlModifierType.Continue) {
                    if (modifier == VirtualController.ControlModifierType.On) {
                        this.SendForceFeedbackCommand(SharpDX.DirectInput.ForceFeedbackCommand.SetActuatorsOn);
                        return;
                    }
                    if (modifier == VirtualController.ControlModifierType.Off) {
                        this.SendForceFeedbackCommand(SharpDX.DirectInput.ForceFeedbackCommand.SetActuatorsOff);
                        return;
                    }
                    if (modifier != VirtualController.ControlModifierType.Continue) {
                        return;
                    }
                    this.SendForceFeedbackCommand(SharpDX.DirectInput.ForceFeedbackCommand.Continue);
                    return;
                } else {
                    if (modifier == VirtualController.ControlModifierType.Pause) {
                        this.SendForceFeedbackCommand(SharpDX.DirectInput.ForceFeedbackCommand.Pause);
                        return;
                    }
                    if (modifier == VirtualController.ControlModifierType.Release) {
                        this.SendForceFeedbackCommand(SharpDX.DirectInput.ForceFeedbackCommand.StopAll);
                        return;
                    }
                    if (modifier != VirtualController.ControlModifierType.Reset) {
                        return;
                    }
                    this.SendForceFeedbackCommand(SharpDX.DirectInput.ForceFeedbackCommand.Reset);
                    return;
                }
            }
        }
    }
}
