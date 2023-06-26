using System.Diagnostics.CodeAnalysis;

namespace ARMeilleure.CodeGen.X86
{
    [SuppressMessage("Design", "CA1069: Enums values should not be duplicated")]
    enum X86Register
    {
        Invalid = -1,

        Rax = 0,
        Rcx = 1,
        Rdx = 2,
        Rbx = 3,
        Rsp = 4,
        Rbp = 5,
        Rsi = 6,
        Rdi = 7,
        R8 = 8,
        R9 = 9,
        R10 = 10,
        R11 = 11,
        R12 = 12,
        R13 = 13,
        R14 = 14,
        R15 = 15,

        Xmm0 = 0,
        Xmm1 = 1,
        Xmm2 = 2,
        Xmm3 = 3,
        Xmm4 = 4,
        Xmm5 = 5,
        Xmm6 = 6,
        Xmm7 = 7,
        Xmm8 = 8,
        Xmm9 = 9,
        Xmm10 = 10,
        Xmm11 = 11,
        Xmm12 = 12,
        Xmm13 = 13,
        Xmm14 = 14,
        Xmm15 = 15,
    }
}
