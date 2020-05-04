using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper.FFB {
    public enum EffectStatus : byte {
        None,
        Idle,
        Playing,
        Stopped,
        Expired
    }
    public class EffectParametersEx : SharpDX.DirectInput.EffectParameters {
        public Guid Type;
    }
    class VirtualFFBEffect {
        public DateTime CreationTime;
        public DateTime UpdateTime;
        // public DateTime TypeSpecificUpdateTime;
        public DateTime BaseUpdateTime;
        public DateTime StartTime;
        public DateTime StopTime;
        public EffectStatus Status;
        public bool Downloaded;
        public EffectParametersEx Parameters;
        public bool ConditionX;
        public bool ConditionY;
        public bool Solo;
        public int LoopCount;
        public VirtualFFBPacket Packet;

        public uint Index;

        public VirtualFFBEffect() {
            DateTime now = DateTime.Now;
            this.CreationTime = now;
            this.UpdateTime = now;
            this.BaseUpdateTime = now;
            this.StartTime = DateTime.MinValue;
            this.StopTime = DateTime.MinValue;
            this.Status = EffectStatus.None;
            this.Downloaded = false;
            this.Parameters = new EffectParametersEx();
            this.ConditionX = false;
            this.ConditionY = false;
            this.Solo = false;
            this.LoopCount = 1;
            this.Packet = null;

            this.Index = 0;
        }

        public void UpdateStatus() {
            if (this.Status != EffectStatus.None && this.Status == EffectStatus.Playing && this.Parameters.Duration != -1) {
                double totalSeconds = (DateTime.Now - this.StartTime).TotalSeconds;
                double num = (double)this.Parameters.Duration / 1000.0;
                if (totalSeconds >= num) {
                    this.Status = EffectStatus.Expired;
                    this.StopTime = this.StartTime;
                    this.StopTime = this.StopTime.AddSeconds(num);
                }
            }
        }

        public void Dispose() {
            if (this.Downloaded) {
                this.Downloaded = false;
                this.Status = EffectStatus.None;
                this.UpdateTime = DateTime.Now;
            }
        }
    }
}
