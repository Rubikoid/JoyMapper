using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper.ReVirtualController.vJoy {
	public class vJoyFFBEffect {
		// Token: 0x0600017E RID: 382 RVA: 0x00010878 File Offset: 0x0000EA78
		public void Init() {
			DateTime now = DateTime.Now;
			this.CreationTime = now;
			this.UpdateTime = now;
			this.BaseUpdateTime = now;
			this.StartTime = DateTime.MinValue;
			this.StopTime = DateTime.MinValue;
			this.Status = VirtualController.DirectInput.EffectStatus.None;
			this.Downloaded = false;
			this.Parameters = new VirtualController.DirectInput.EffectParameters();
			this.ConditionX = false;
			this.ConditionY = false;
			this.Solo = false;
			this.LoopCount = 1;
			this.LAPacket = null;
		}

		// Token: 0x0600017F RID: 383 RVA: 0x000108F4 File Offset: 0x0000EAF4
		public void UpdateStatus() {
			if (this.Status != VirtualController.DirectInput.EffectStatus.None && this.Status == VirtualController.DirectInput.EffectStatus.Playing && this.Parameters.Duration != -1) {
				double totalSeconds = (DateTime.Now - this.StartTime).TotalSeconds;
				double num = (double)this.Parameters.Duration / 1000.0;
				if (totalSeconds >= num) {
					this.Status = VirtualController.DirectInput.EffectStatus.Expired;
					this.StopTime = this.StartTime;
					this.StopTime = this.StopTime.AddSeconds(num);
				}
			}
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00010977 File Offset: 0x0000EB77
		public void Dispose() {
			if (this.Downloaded) {
				this.Downloaded = false;
				this.Status = VirtualController.DirectInput.EffectStatus.None;
				this.UpdateTime = DateTime.Now;
			}
		}

		// Token: 0x06000181 RID: 385 RVA: 0x0001099A File Offset: 0x0000EB9A
		public vJoyFFBEffect() {
			this.Init();
		}

		// Token: 0x0400016B RID: 363
		public DateTime CreationTime;

		// Token: 0x0400016C RID: 364
		public DateTime UpdateTime;

		// Token: 0x0400016D RID: 365
		public DateTime TypeSpecificUpdateTime;

		// Token: 0x0400016E RID: 366
		public DateTime BaseUpdateTime;

		// Token: 0x0400016F RID: 367
		public DateTime StartTime;

		// Token: 0x04000170 RID: 368
		public DateTime StopTime;

		// Token: 0x04000171 RID: 369
		public VirtualController.DirectInput.EffectStatus Status;

		// Token: 0x04000172 RID: 370
		public bool Downloaded;

		// Token: 0x04000173 RID: 371
		public VirtualController.DirectInput.EffectParameters Parameters;

		// Token: 0x04000174 RID: 372
		public bool ConditionX;

		// Token: 0x04000175 RID: 373
		public bool ConditionY;

		// Token: 0x04000176 RID: 374
		public bool Solo;

		// Token: 0x04000177 RID: 375
		public int LoopCount;

		// Token: 0x04000178 RID: 376
		public vJoyFFBPacket LAPacket;
	}
}
