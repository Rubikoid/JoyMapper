using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace JoyMapper.ReVirtualController.vJoy {
	public class vJoyFFBPacket {
		// Token: 0x0400017A RID: 378
		public DateTime CreationTime = DateTime.Now;

		// Token: 0x0400017B RID: 379
		public FFB_CTRL FFB_CTRL = FFB_CTRL.CTRL_ENACT;

		// Token: 0x0400017C RID: 380
		public FFBPType FFBPType = FFBPType.PT_SETCREP;

		// Token: 0x0400017D RID: 381
		public FFBEType FFBENextType;

		// Token: 0x0400017E RID: 382
		public vJoyInterfaceWrap.vJoy.FFB_EFF_COND FFB_EFF_COND;

		// Token: 0x0400017F RID: 383
		public vJoyInterfaceWrap.vJoy.FFB_EFF_CONSTANT FFB_EFF_CONSTANT;

		// Token: 0x04000180 RID: 384
		public vJoyInterfaceWrap.vJoy.FFB_EFF_ENVLP FFB_EFF_ENVLP;

		// Token: 0x04000181 RID: 385
		public vJoyInterfaceWrap.vJoy.FFB_EFF_OP FFB_EFF_OP;

		// Token: 0x04000182 RID: 386
		public vJoyInterfaceWrap.vJoy.FFB_EFF_PERIOD FFB_EFF_PERIOD;

		// Token: 0x04000183 RID: 387
		public vJoyInterfaceWrap.vJoy.FFB_EFF_RAMP FFB_EFF_RAMP;

		// Token: 0x04000184 RID: 388
		public vJoyInterfaceWrap.vJoy.FFB_EFF_REPORT FFB_EFF_REPORT;

		// Token: 0x04000185 RID: 389
		public byte Gain;

		// Token: 0x04000186 RID: 390
		public int BlockIndex;

		// Token: 0x04000187 RID: 391
		public bool Handled;
	}

	class Utils {
		public static VirtualController.DirectInput.FFBEffect GetFriendlyFFBEffect(FFBEType FFBEType) {
			switch (FFBEType) {
				case FFBEType.ET_CONST:
					return VirtualController.DirectInput.FFBEffect.ConstantForce;
				case FFBEType.ET_RAMP:
					return VirtualController.DirectInput.FFBEffect.RampForce;
				case FFBEType.ET_SQR:
					return VirtualController.DirectInput.FFBEffect.Square;
				case FFBEType.ET_SINE:
					return VirtualController.DirectInput.FFBEffect.Sine;
				case FFBEType.ET_TRNGL:
					return VirtualController.DirectInput.FFBEffect.Triangle;
				case FFBEType.ET_STUP:
					return VirtualController.DirectInput.FFBEffect.SawtoothUp;
				case FFBEType.ET_STDN:
					return VirtualController.DirectInput.FFBEffect.SawtoothDown;
				case FFBEType.ET_SPRNG:
					return VirtualController.DirectInput.FFBEffect.Spring;
				case FFBEType.ET_DMPR:
					return VirtualController.DirectInput.FFBEffect.Damper;
				case FFBEType.ET_INRT:
					return VirtualController.DirectInput.FFBEffect.Inertia;
				case FFBEType.ET_FRCTN:
					return VirtualController.DirectInput.FFBEffect.Friction;
				case FFBEType.ET_CSTM:
					return VirtualController.DirectInput.FFBEffect.CustomForce;
				default:
					return VirtualController.DirectInput.FFBEffect.None;
			}
		}

		// Token: 0x060001DC RID: 476 RVA: 0x000123F8 File Offset: 0x000105F8
		public static FFBEType GetvJoyAPIFFBEffect(VirtualController.DirectInput.FFBEffect FFBType) {
			switch (FFBType) {
				case VirtualController.DirectInput.FFBEffect.ConstantForce:
					return FFBEType.ET_CONST;
				case VirtualController.DirectInput.FFBEffect.RampForce:
					return FFBEType.ET_RAMP;
				case VirtualController.DirectInput.FFBEffect.Square:
					return FFBEType.ET_SQR;
				case VirtualController.DirectInput.FFBEffect.Sine:
					return FFBEType.ET_SINE;
				case VirtualController.DirectInput.FFBEffect.Triangle:
					return FFBEType.ET_TRNGL;
				case VirtualController.DirectInput.FFBEffect.SawtoothUp:
					return FFBEType.ET_STUP;
				case VirtualController.DirectInput.FFBEffect.SawtoothDown:
					return FFBEType.ET_STDN;
				case VirtualController.DirectInput.FFBEffect.Spring:
					return FFBEType.ET_SPRNG;
				case VirtualController.DirectInput.FFBEffect.Damper:
					return FFBEType.ET_DMPR;
				case VirtualController.DirectInput.FFBEffect.Inertia:
					return FFBEType.ET_INRT;
				case VirtualController.DirectInput.FFBEffect.Friction:
					return FFBEType.ET_FRCTN;
				case VirtualController.DirectInput.FFBEffect.CustomForce:
					return FFBEType.ET_CSTM;
				default:
					return FFBEType.ET_NONE;
			}
		}

		public static VirtualController.DirectInput.ConstantForce GetConstantForce(vJoyInterfaceWrap.vJoy.FFB_EFF_CONSTANT Effect) {
			return new VirtualController.DirectInput.ConstantForce {
				Magnitude = (int)Effect.Magnitude
			};
		}

		// Token: 0x060001DE RID: 478 RVA: 0x0001246F File Offset: 0x0001066F
		public static VirtualController.DirectInput.RampForce GetRampForce(vJoyInterfaceWrap.vJoy.FFB_EFF_RAMP Effect) {
			return new VirtualController.DirectInput.RampForce {
				Start = (int)Effect.Start,
				End = (int)Effect.End
			};
		}

		// Token: 0x060001DF RID: 479 RVA: 0x0001248E File Offset: 0x0001068E
		public static VirtualController.DirectInput.PeriodicForce GetPeriodicForce(vJoyInterfaceWrap.vJoy.FFB_EFF_PERIOD Effect) {
			return new VirtualController.DirectInput.PeriodicForce {
				Magnitude = (int)Effect.Magnitude,
				Offset = (int)Effect.Offset,
				Period = (int)(Effect.Period * 1000u),
				Phase = (int)Effect.Phase
			};
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x000124CC File Offset: 0x000106CC
		public static VirtualController.DirectInput.Condition GetCondition(vJoyInterfaceWrap.vJoy.FFB_EFF_COND Effect) {
			return new VirtualController.DirectInput.Condition {
				DeadBand = Effect.DeadBand,
				Offset = (int)Effect.CenterPointOffset,
				NegativeCoefficient = (int)Effect.NegCoeff,
				NegativeSaturation = (int)Effect.NegSatur,
				PositiveCoefficient = (int)Effect.PosCoeff,
				PositiveSaturation = (int)Effect.PosSatur
			};
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x00012528 File Offset: 0x00010728
		public static VirtualController.DirectInput.ConditionSet GetConditionSet(vJoyFFBPacket FfbData) {
			VirtualController.DirectInput.ConditionSet conditionSet = new VirtualController.DirectInput.ConditionSet(2);
			VirtualController.DirectInput.Condition condition = GetCondition(FfbData.FFB_EFF_COND);
			conditionSet.Conditions[0] = condition;
			conditionSet.Conditions[1] = condition;
			return conditionSet;
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x0001255C File Offset: 0x0001075C
		public static VirtualController.DirectInput.Envelope GetEnvelope(vJoyInterfaceWrap.vJoy.FFB_EFF_ENVLP Effect) {
			return new VirtualController.DirectInput.Envelope {
				AttackLevel = (int)Effect.AttackLevel,
				AttackTime = (int)(Effect.AttackTime * 1000u),
				FadeLevel = (int)Effect.FadeLevel,
				FadeTime = (int)(Effect.FadeTime * 1000u)
			};
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x000125AA File Offset: 0x000107AA
		private static int GetDIPolarValue(int vJoyPolarValue) {
			return (int)Math.Round((double)vJoyPolarValue * 360.0 / 255.0 * 100.0);
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x000125D4 File Offset: 0x000107D4
		private static void GetDICartesianXY(int vJoyPolarValue, ref int X, ref int Y) {
			double num = Math.Round((double)vJoyPolarValue * 360.0 / 255.0) * 3.1415926535897931 / 180.0;
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

		// Token: 0x060001E5 RID: 485 RVA: 0x00012670 File Offset: 0x00010870
		public static VirtualController.DirectInput.EffectParameters GetBaseEffectParameters(vJoyInterfaceWrap.vJoy.FFB_EFF_REPORT Effect) {
			VirtualController.DirectInput.EffectParameters effectParameters = new VirtualController.DirectInput.EffectParameters();
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
			VirtualController.DirectInput.FFBEffect friendlyFFBEffect = GetFriendlyFFBEffect(Effect.EffectType);
			effectParameters.Type = friendlyFFBEffect;
			effectParameters.AxisSettings = new VirtualController.DirectInput.AxisSettings(2);
			int num = 0;
			int num2 = 0;
			if (Effect.Polar) {
				GetDICartesianXY((int)Effect.DirX, ref num, ref num2);
			} else {
				num = (int)Effect.DirX;
				num2 = (int)Effect.DirY;
			}
			effectParameters.AxisSettings.Directions[0] = num;
			effectParameters.AxisSettings.Directions[1] = num2;
			return effectParameters;
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x00012784 File Offset: 0x00010984
		public static VirtualController.DirectInput.EffectParameters GetEffectParameters(vJoyFFBPacket FfbData) {
			VirtualController.DirectInput.EffectParameters baseEffectParameters = GetBaseEffectParameters(FfbData.FFB_EFF_REPORT);
			bool flag = VirtualController.DirectInput.TypeSpecificParameters.IsConditionEffect(baseEffectParameters.Type);
			bool flag2 = VirtualController.DirectInput.TypeSpecificParameters.IsPeriodicEffect(baseEffectParameters.Type);
			VirtualController.DirectInput.FFBEffect type = baseEffectParameters.Type;
			if (type != VirtualController.DirectInput.FFBEffect.ConstantForce) {
				if (type != VirtualController.DirectInput.FFBEffect.RampForce) {
					if (flag) {
						baseEffectParameters.TypeSpecificParameters.ConditionSet = GetConditionSet(FfbData);
					} else if (flag2) {
						baseEffectParameters.TypeSpecificParameters.PeriodicForce = GetPeriodicForce(FfbData.FFB_EFF_PERIOD);
					}
				} else {
					baseEffectParameters.TypeSpecificParameters.RampForce = GetRampForce(FfbData.FFB_EFF_RAMP);
				}
			} else {
				baseEffectParameters.TypeSpecificParameters.ConstantForce = GetConstantForce(FfbData.FFB_EFF_CONSTANT);
			}
			return baseEffectParameters;
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x00012826 File Offset: 0x00010A26
		public static bool IsTypeSpecificPacket(FFBPType Type) {
			return Type == FFBPType.PT_CONSTREP || Type == FFBPType.PT_RAMPREP || Type == FFBPType.PT_PRIDREP || Type == FFBPType.PT_CONDREP || Type == FFBPType.PT_CSTMREP;
		}
	}
}
