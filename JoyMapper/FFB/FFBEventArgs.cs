using SharpDX.DirectInput;
using System;

namespace JoyMapper.FFB
{
    public class FFBEventArgs : EventArgs
    {
        public EffectParameters Parameters { get; set; }
        public Guid EffectGuid { get; set; }
        public int LoopCount { get; set; }

        public FFBEventArgs() : base()
        {
            this.Parameters = new EffectParameters();
            this.EffectGuid = Guid.Empty;
        }
    }
}
