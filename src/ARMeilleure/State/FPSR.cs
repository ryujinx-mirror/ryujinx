using System;

namespace ARMeilleure.State
{
    [Flags]
    public enum FPSR : uint
    {
        Ioc = 1u << 0,
        Dzc = 1u << 1,
        Ofc = 1u << 2,
        Ufc = 1u << 3,
        Ixc = 1u << 4,
        Idc = 1u << 7,
        Qc = 1u << 27,

        Mask = Qc | Idc | Ixc | Ufc | Ofc | Dzc | Ioc, // 0x0800009Fu
    }
}
