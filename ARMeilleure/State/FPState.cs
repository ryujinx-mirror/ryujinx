using System;
using System.Collections.Generic;
using System.Text;

namespace ARMeilleure.State
{
    [Flags]
    public enum FPState
    {
        VFlag = 28,
        CFlag = 29,
        ZFlag = 30,
        NFlag = 31
    }
}
