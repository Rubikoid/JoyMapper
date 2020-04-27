using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper {
    public partial class GameController {
        // Token: 0x06000216 RID: 534 RVA: 0x00014690 File Offset: 0x00012890
        public static List<int> GetFFBAxisIds(VirtualController.DirectInput.EffectParameters Parameters, Dictionary<VirtualController.AxisType, int> FFBAxisIdsList) {
            List<int> list = new List<int>();
            List<VirtualController.AxisType> list2 = new List<VirtualController.AxisType>();
            for (int i = 0; i < Parameters.AxisSettings.Axes.Length; i++) {
                list2.Add(Parameters.AxisSettings.Axes[i]);
            }
            foreach (VirtualController.AxisType key in list2) {
                if (FFBAxisIdsList.ContainsKey(key)) {
                    list.Add(FFBAxisIdsList[key]);
                }
            }
            return list;
        }
        public void SetEffectParameters(VirtualController.DirectInput.Effect Effect, EffectParameterFlags Flags, EffectParameters Parameters) {
            if (Effect.Object == null) {
                Effect.Object = new Effect(Effect.Info.ParentDevice, Effect.Info.Guid, Parameters);
                Effect.CreationTime = DateTime.Now;
                Effect.Downloaded = true;
            } else if (Flags == EffectParameterFlags.None) {
                // this.DisposeEffect(Effect.Object);
                Effect.Object.Unload();
                Effect.Object.Dispose();
                Effect.Object = null;

                Effect.Object = null;
                Effect.Object = new Effect(Effect.Info.ParentDevice, Effect.Info.Guid, Parameters);
                Effect.CreationTime = DateTime.Now;
                Effect.Downloaded = true;
            } else if (Effect.Downloaded) {
                Effect.Object.SetParameters(Parameters, Flags);
                Effect.Downloaded = true;
            }
            Effect.UpdateTime = DateTime.Now;
        }

        // Token: 0x06000223 RID: 547 RVA: 0x000150D0 File Offset: 0x000132D0
        public void SetEffectParameters(VirtualController.DirectInput.Effect Effect, EffectParameterFlags Flags) {
            EffectParameters effectParameters = GameController.GetEffectParameters(Effect.FFBAxisIds, Effect.Parameters);
            this.SetEffectParameters(Effect, Flags, effectParameters);
        }

        // Token: 0x06000237 RID: 567 RVA: 0x000159A0 File Offset: 0x00013BA0
        public static EffectParameters GetEffectParameters(List<int> FFBAxisIds, VirtualController.DirectInput.EffectParameters Parameters) {
            EffectParameters baseEffectParameters = GameController.GetBaseEffectParameters(FFBAxisIds, Parameters);
            baseEffectParameters.Parameters = GameController.GetTypeSpecificParameters(Parameters);
            if (!Parameters.Envelope.IsDisabled()) {
                baseEffectParameters.Envelope = GameController.GetEnvelope(Parameters.Envelope);
            }
            return baseEffectParameters;
        }

        private static int EffectComparison_UpdateDateTime(VirtualController.DirectInput.Effect A, VirtualController.DirectInput.Effect B) {
            if (A.UpdateTime > B.UpdateTime) {
                return 1;
            }
            if (A.UpdateTime < B.UpdateTime) {
                return -1;
            }
            return 0;
        }

        // Token: 0x06000203 RID: 515 RVA: 0x00013E91 File Offset: 0x00012091
        private static int EffectComparison_CreateDateTime(VirtualController.DirectInput.Effect A, VirtualController.DirectInput.Effect B) {
            if (A.CreationTime > B.CreationTime) {
                return 1;
            }
            if (A.CreationTime < B.CreationTime) {
                return -1;
            }
            return 0;
        }

        // Token: 0x06000204 RID: 516 RVA: 0x00013EBE File Offset: 0x000120BE
        public void SortEffectsByUpdateDateTime() {
            this.ForceFeedbackEffects.Sort(new Comparison<VirtualController.DirectInput.Effect>(GameController.EffectComparison_UpdateDateTime));
        }

        // Token: 0x06000205 RID: 517 RVA: 0x00013ED7 File Offset: 0x000120D7
        public void SortEffectsByCreateDateTime() {
            this.ForceFeedbackEffects.Sort(new Comparison<VirtualController.DirectInput.Effect>(GameController.EffectComparison_CreateDateTime));
        }

        /*
        // Token: 0x06000206 RID: 518 RVA: 0x00013EF0 File Offset: 0x000120F0
        public void Dispose() {
            try {
                foreach (VirtualController.DirectInput.Effect effect in this.ForceFeedbackEffects) {
                    effect.Dispose();
                }
                this.joystick.Dispose();
            } catch (Exception) {
            }
        }
        */

        public static ConstantForce GetDefaultConstantForce() {
            return new ConstantForce {
                Magnitude = 10000
            };
        }

        // Token: 0x06000225 RID: 549 RVA: 0x0001516A File Offset: 0x0001336A
        public static RampForce GetDefaultRampForce() {
            return new RampForce {
                Start = -10000,
                End = 10000
            };
        }

        // Token: 0x06000226 RID: 550 RVA: 0x00015188 File Offset: 0x00013388
        public static CustomForce GetDefaultCustomForce(int SampleCount, int AffectedAxisCount) {
            CustomForce customForce = new CustomForce();
            customForce = new CustomForce();
            customForce.ChannelCount = AffectedAxisCount;
            customForce.SampleCount = SampleCount;
            customForce.SamplePeriod = 0;
            customForce.ForceData = new int[SampleCount];
            for (int i = 0; i < SampleCount; i++) {
                customForce.ForceData[i] = 0;
            }
            return customForce;
        }

        // Token: 0x06000227 RID: 551 RVA: 0x000151D8 File Offset: 0x000133D8
        public static PeriodicForce GetDefaultPeriodicForce() {
            return new PeriodicForce {
                Magnitude = 10000,
                Offset = 0,
                Period = 500000,
                Phase = 0
            };
        }

        // Token: 0x06000228 RID: 552 RVA: 0x00015204 File Offset: 0x00013404
        public static Condition GetDefaultCondition() {
            return new Condition {
                DeadBand = 0,
                NegativeCoefficient = 10000,
                NegativeSaturation = 10000,
                Offset = 0,
                PositiveCoefficient = 10000,
                PositiveSaturation = 10000
            };
        }

        // Token: 0x06000229 RID: 553 RVA: 0x0001525C File Offset: 0x0001345C
        public static ConditionSet GetDefaultConditionSet(int ConditionCount) {
            ConditionSet conditionSet = new ConditionSet();
            conditionSet.Conditions = new Condition[ConditionCount];
            for (int i = 0; i < ConditionCount; i++) {
                conditionSet.Conditions[i] = GameController.GetDefaultCondition();
            }
            return conditionSet;
        }

        // Token: 0x0600022A RID: 554 RVA: 0x00015299 File Offset: 0x00013499
        public static Envelope GetDefaultEnvelope() {
            return new Envelope {
                AttackLevel = 0,
                AttackTime = 0,
                FadeLevel = 0,
                FadeTime = 0
            };
        }

        // Token: 0x0600022B RID: 555 RVA: 0x000152BC File Offset: 0x000134BC
        public static TypeSpecificParameters GetDefaultTypeSpecificParameters(VirtualController.DirectInput.FFBEffect Effect, int AxisCount) {
            TypeSpecificParameters result;
            if (Effect != VirtualController.DirectInput.FFBEffect.ConstantForce) {
                if (Effect != VirtualController.DirectInput.FFBEffect.RampForce) {
                    if (Effect != VirtualController.DirectInput.FFBEffect.CustomForce) {
                        bool flag = VirtualController.DirectInput.TypeSpecificParameters.IsPeriodicEffect(Effect);
                        bool flag2 = VirtualController.DirectInput.TypeSpecificParameters.IsConditionEffect(Effect);
                        if (flag) {
                            result = GameController.GetDefaultPeriodicForce();
                        } else if (flag2) {
                            result = GameController.GetDefaultConditionSet(AxisCount);
                        } else {
                            result = GameController.GetDefaultConstantForce();
                        }
                    } else {
                        result = GameController.GetDefaultCustomForce(5, AxisCount);
                    }
                } else {
                    result = GameController.GetDefaultRampForce();
                }
            } else {
                result = GameController.GetDefaultConstantForce();
            }
            return result;
        }

        // Token: 0x0600022C RID: 556 RVA: 0x0001531C File Offset: 0x0001351C
        public static EffectParameters GetDefaultBaseEffectParameters(List<int> FFBAxisIds) {
            EffectParameters effectParameters = new EffectParameters();
            effectParameters.Duration = -1;
            effectParameters.Gain = 10000;
            effectParameters.SamplePeriod = 0;
            effectParameters.StartDelay = 0;
            effectParameters.TriggerButton = -1;
            effectParameters.TriggerRepeatInterval = 0;
            effectParameters.Flags = (EffectFlags.ObjectIds | EffectFlags.Cartesian);
            int count = FFBAxisIds.Count;
            int num = count;
            int[] array = new int[count];
            int[] array2 = new int[num];
            for (int i = 0; i < count; i++) {
                array[i] = FFBAxisIds[i];
            }
            for (int j = 0; j < num; j++) {
                array2[j] = 0;
            }
            effectParameters.SetAxes(array, array2);
            return effectParameters;
        }

        // Token: 0x0600022D RID: 557 RVA: 0x000153BC File Offset: 0x000135BC
        public static EffectParameters GetDefaultEffectParameters(VirtualController.DirectInput.FFBEffect Effect, List<int> FFBAxisIds) {
            int count = FFBAxisIds.Count;
            EffectParameters defaultBaseEffectParameters = GameController.GetDefaultBaseEffectParameters(FFBAxisIds);
            if (Effect == VirtualController.DirectInput.FFBEffect.RampForce) {
                defaultBaseEffectParameters.Duration = 3000000;
            }
            defaultBaseEffectParameters.Parameters = GameController.GetDefaultTypeSpecificParameters(Effect, count);
            return defaultBaseEffectParameters;
        }

        public static ConstantForce GetConstantForce(VirtualController.DirectInput.ConstantForce Force) {
            ConstantForce defaultConstantForce = GameController.GetDefaultConstantForce();
            if (Force != null && Force.Magnitude != -2147483648) {
                defaultConstantForce.Magnitude = Force.Magnitude;
            }
            return defaultConstantForce;
        }

        // Token: 0x0600022F RID: 559 RVA: 0x0001542C File Offset: 0x0001362C
        public static RampForce GetRampForce(VirtualController.DirectInput.RampForce Force) {
            RampForce defaultRampForce = GameController.GetDefaultRampForce();
            if (Force != null) {
                if (Force.Start != -2147483648) {
                    defaultRampForce.Start = Force.Start;
                }
                if (Force.End != -2147483648) {
                    defaultRampForce.End = Force.End;
                }
            }
            return defaultRampForce;
        }

        // Token: 0x06000230 RID: 560 RVA: 0x0001547C File Offset: 0x0001367C
        public static CustomForce GetCustomForce(VirtualController.DirectInput.CustomForce Force) {
            CustomForce defaultCustomForce = GameController.GetDefaultCustomForce(5, 2);
            if (Force != null) {
                if (Force.Channels != -2147483648) {
                    defaultCustomForce.ChannelCount = Force.Channels;
                }
                if (Force.SamplePeriod != -2147483648) {
                    defaultCustomForce.SamplePeriod = Force.SamplePeriod;
                }
                if (Force.Samples != -2147483648) {
                    if (Force.Samples != defaultCustomForce.SampleCount) {
                        defaultCustomForce.ForceData = new int[Force.Samples];
                    }
                    defaultCustomForce.SampleCount = Force.Samples;
                }
                int num = Math.Min(Force.ForceData.Length, defaultCustomForce.ForceData.Length);
                for (int i = 0; i < num; i++) {
                    if (Force.ForceData[i] != -2147483648) {
                        defaultCustomForce.ForceData[i] = Force.ForceData[i];
                    }
                }
            }
            return defaultCustomForce;
        }

        // Token: 0x06000231 RID: 561 RVA: 0x0001554C File Offset: 0x0001374C
        public static PeriodicForce GetPeriodicForce(VirtualController.DirectInput.PeriodicForce Force) {
            PeriodicForce defaultPeriodicForce = GameController.GetDefaultPeriodicForce();
            if (Force != null) {
                if (Force.Magnitude != -2147483648) {
                    defaultPeriodicForce.Magnitude = Force.Magnitude;
                }
                if (Force.Offset != -2147483648) {
                    defaultPeriodicForce.Offset = Force.Offset;
                }
                if (Force.Period != -2147483648) {
                    defaultPeriodicForce.Period = Force.Period;
                }
                if (Force.Phase != -2147483648) {
                    defaultPeriodicForce.Phase = Force.Phase;
                }
            }
            return defaultPeriodicForce;
        }

        // Token: 0x06000232 RID: 562 RVA: 0x000155D0 File Offset: 0x000137D0
        public static Condition GetCondition(VirtualController.DirectInput.Condition Force) {
            Condition defaultCondition = GameController.GetDefaultCondition();
            if (Force != null) {
                if (Force.DeadBand != -2147483648) {
                    defaultCondition.DeadBand = Force.DeadBand;
                }
                if (Force.NegativeCoefficient != -2147483648) {
                    defaultCondition.NegativeCoefficient = Force.NegativeCoefficient;
                }
                if (Force.NegativeSaturation != -2147483648) {
                    defaultCondition.NegativeSaturation = Force.NegativeSaturation;
                }
                if (Force.Offset != -2147483648) {
                    defaultCondition.Offset = Force.Offset;
                }
                if (Force.PositiveCoefficient != -2147483648) {
                    defaultCondition.PositiveCoefficient = Force.PositiveCoefficient;
                }
                if (Force.PositiveSaturation != -2147483648) {
                    defaultCondition.PositiveSaturation = Force.PositiveSaturation;
                }
            }
            return defaultCondition;
        }

        // Token: 0x06000233 RID: 563 RVA: 0x0001568C File Offset: 0x0001388C
        public static ConditionSet GetConditionSet(VirtualController.DirectInput.ConditionSet Force) {
            ConditionSet defaultConditionSet = GameController.GetDefaultConditionSet(2);
            if (Force != null && Force.Conditions != null && Force.Conditions.Length != 0) {
                if (defaultConditionSet.Conditions.Length != Force.Conditions.Length) {
                    defaultConditionSet.Conditions = new Condition[Force.Conditions.Length];
                }
                for (int i = 0; i < defaultConditionSet.Conditions.Length; i++) {
                    defaultConditionSet.Conditions[i] = GameController.GetCondition(Force.Conditions[i]);
                }
            }
            return defaultConditionSet;
        }

        // Token: 0x06000234 RID: 564 RVA: 0x0001570C File Offset: 0x0001390C
        public static Envelope GetEnvelope(VirtualController.DirectInput.Envelope Force) {
            Envelope defaultEnvelope = GameController.GetDefaultEnvelope();
            if (Force != null) {
                if (Force.AttackLevel != -2147483648) {
                    defaultEnvelope.AttackLevel = Force.AttackLevel;
                }
                if (Force.AttackTime != -2147483648) {
                    defaultEnvelope.AttackTime = Force.AttackTime;
                }
                if (Force.FadeLevel != -2147483648) {
                    defaultEnvelope.FadeLevel = Force.FadeLevel;
                }
                if (Force.FadeTime != -2147483648) {
                    defaultEnvelope.FadeTime = Force.FadeTime;
                }
            }
            return defaultEnvelope;
        }

        // Token: 0x06000235 RID: 565 RVA: 0x00015790 File Offset: 0x00013990
        public static TypeSpecificParameters GetTypeSpecificParameters(VirtualController.DirectInput.EffectParameters Parameters) {
            VirtualController.DirectInput.FFBEffect type = Parameters.Type;
            TypeSpecificParameters result;
            if (type != VirtualController.DirectInput.FFBEffect.ConstantForce) {
                if (type != VirtualController.DirectInput.FFBEffect.RampForce) {
                    if (type != VirtualController.DirectInput.FFBEffect.CustomForce) {
                        bool flag = VirtualController.DirectInput.TypeSpecificParameters.IsPeriodicEffect(type);
                        bool flag2 = VirtualController.DirectInput.TypeSpecificParameters.IsConditionEffect(type);
                        if (flag) {
                            result = GameController.GetPeriodicForce(Parameters.TypeSpecificParameters.PeriodicForce);
                        } else if (flag2) {
                            result = GameController.GetConditionSet(Parameters.TypeSpecificParameters.ConditionSet);
                        } else {
                            result = GameController.GetDefaultConstantForce();
                        }
                    } else {
                        result = GameController.GetCustomForce(Parameters.TypeSpecificParameters.CustomForce);
                    }
                } else {
                    result = GameController.GetRampForce(Parameters.TypeSpecificParameters.RampForce);
                }
            } else {
                result = GameController.GetConstantForce(Parameters.TypeSpecificParameters.ConstantForce);
            }
            return result;
        }

        // Token: 0x06000236 RID: 566 RVA: 0x0001582C File Offset: 0x00013A2C
        public static EffectParameters GetBaseEffectParameters(List<int> FFBAxisIds, VirtualController.DirectInput.EffectParameters Parameters) {
            EffectParameters defaultBaseEffectParameters = GameController.GetDefaultBaseEffectParameters(FFBAxisIds);
            if (Parameters != null) {
                if (Parameters.Duration != -2147483648) {
                    if (Parameters.Duration < 0) {
                        defaultBaseEffectParameters.Duration = -1;
                    } else {
                        defaultBaseEffectParameters.Duration = Parameters.Duration;
                    }
                }
                if (Parameters.Gain != -2147483648) {
                    defaultBaseEffectParameters.Gain = Parameters.Gain;
                }
                if (Parameters.SamplePeriod != -2147483648) {
                    defaultBaseEffectParameters.SamplePeriod = Parameters.SamplePeriod;
                }
                if (Parameters.StartDelay != -2147483648) {
                    defaultBaseEffectParameters.StartDelay = Parameters.StartDelay;
                }
                if (Parameters.TriggerButton != -2147483648) {
                    defaultBaseEffectParameters.TriggerButton = Parameters.TriggerButton;
                }
                if (Parameters.TriggerRepeatInterval != -2147483648) {
                    defaultBaseEffectParameters.TriggerRepeatInterval = Parameters.TriggerRepeatInterval;
                }
                if (Parameters.AxisSettings != null) {
                    int count = FFBAxisIds.Count;
                    int val = Parameters.AxisSettings.Axes.Length;
                    int num = Math.Min(count, val);
                    int[] array = new int[num];
                    int[] array2 = new int[num];
                    for (int i = 0; i < num; i++) {
                        array[i] = FFBAxisIds[i];
                        if (Parameters.AxisSettings.Directions[i] != -2147483648) {
                            array2[i] = Parameters.AxisSettings.Directions[i];
                            if (Parameters.AxisSettings.Inverted[i]) {
                                array2[i] = -array2[i];
                            }
                        } else {
                            array2[i] = 0;
                        }
                    }
                    defaultBaseEffectParameters.SetAxes(array, array2);
                }
            }
            return defaultBaseEffectParameters;
        }
    }
}
