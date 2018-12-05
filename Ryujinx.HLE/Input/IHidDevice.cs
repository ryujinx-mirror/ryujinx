using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.HLE.Input
{
    interface IHidDevice
    {
        long Offset    { get; }
        bool Connected { get; }
    }
}
