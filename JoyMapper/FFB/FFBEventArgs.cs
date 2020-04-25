using SharpDX.DirectInput;
using System;

namespace JoyMapper
{
    class FFBEventArgs : EventArgs
    {
        public EffectParameters Parameters { get; set; }
        public Guid EffectGuid { get; set; }
        public int LoopCount { get; set; }

        public FFBEventArgs() : base()
        {
            this.Parameters = new EffectParameters();
        }
    }
}
