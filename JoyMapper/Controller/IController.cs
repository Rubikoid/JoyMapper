using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper
{
    interface IController
    {
        bool Connected { get; }
        IList<Guid> SupportedFFBEffects { get; }
        void Connect();
        void Disconnect();
        void FillInfo(ref State state);
        void UpdateInfo(in State state);
    }
}
