using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation.PTC;
using System;
using System.Diagnostics;
using System.IO;

namespace ARMeilleure.CodeGen.X86
{
    class Assembler
    {
        private const int BadOp       = 0;
        private const int OpModRMBits = 24;

        private const byte RexPrefix  = 0x40;
        private const byte RexWPrefix = 0x48;
        private const byte LockPrefix = 0xf0;

        private const int MaxRegNumber = 15;

        [Flags]
        private enum InstructionFlags
        {
            None     = 0,
            RegOnly  = 1 << 0,
            Reg8Src  = 1 << 1,
            Reg8Dest = 1 << 2,
            RexW     = 1 << 3,
            Vex      = 1 << 4,

            PrefixBit  = 16,
            PrefixMask = 7 << PrefixBit,
            Prefix66   = 1 << PrefixBit,
            PrefixF3   = 2 << PrefixBit,
            PrefixF2   = 4 << PrefixBit
        }

        private struct InstructionInfo
        {
            public int OpRMR     { get; }
            public int OpRMImm8  { get; }
            public int OpRMImm32 { get; }
            public int OpRImm64  { get; }
            public int OpRRM     { get; }

            public InstructionFlags Flags { get; }

            public InstructionInfo(
                int              opRMR,
                int              opRMImm8,
                int              opRMImm32,
                int              opRImm64,
                int              opRRM,
                InstructionFlags flags)
            {
                OpRMR     = opRMR;
                OpRMImm8  = opRMImm8;
                OpRMImm32 = opRMImm32;
                OpRImm64  = opRImm64;
                OpRRM     = opRRM;
                Flags     = flags;
            }
        }

        private static InstructionInfo[] _instTable;

        private Stream _stream;

        private PtcInfo _ptcInfo;
        private bool    _ptcDisabled;

        static Assembler()
        {
            _instTable = new InstructionInfo[(int)X86Instruction.Count];

            //  Name                                             RM/R        RM/I8       RM/I32      R/I64       R/RM        Flags
            Add(X86Instruction.Add,          new InstructionInfo(0x00000001, 0x00000083, 0x00000081, BadOp,      0x00000003, InstructionFlags.None));
            Add(X86Instruction.Addpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f58, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Addps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f58, InstructionFlags.Vex));
            Add(X86Instruction.Addsd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f58, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Addss,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f58, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Aesdec,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38de, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Aesdeclast,   new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38df, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Aesenc,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38dc, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Aesenclast,   new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38dd, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Aesimc,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38db, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.And,          new InstructionInfo(0x00000021, 0x04000083, 0x04000081, BadOp,      0x00000023, InstructionFlags.None));
            Add(X86Instruction.Andnpd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f55, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Andnps,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f55, InstructionFlags.Vex));
            Add(X86Instruction.Andpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f54, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Andps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f54, InstructionFlags.Vex));
            Add(X86Instruction.Blendvpd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3815, InstructionFlags.Prefix66));
            Add(X86Instruction.Blendvps,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3814, InstructionFlags.Prefix66));
            Add(X86Instruction.Bsr,          new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fbd, InstructionFlags.None));
            Add(X86Instruction.Bswap,        new InstructionInfo(0x00000fc8, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.RegOnly));
            Add(X86Instruction.Call,         new InstructionInfo(0x020000ff, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Cmovcc,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f40, InstructionFlags.None));
            Add(X86Instruction.Cmp,          new InstructionInfo(0x00000039, 0x07000083, 0x07000081, BadOp,      0x0000003b, InstructionFlags.None));
            Add(X86Instruction.Cmppd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Cmpps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc2, InstructionFlags.Vex));
            Add(X86Instruction.Cmpsd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc2, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Cmpss,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc2, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Cmpxchg,      new InstructionInfo(0x00000fb1, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Cmpxchg16b,   new InstructionInfo(0x01000fc7, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.RexW));
            Add(X86Instruction.Cmpxchg8,     new InstructionInfo(0x00000fb0, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Reg8Src));
            Add(X86Instruction.Comisd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2f, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Comiss,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2f, InstructionFlags.Vex));
            Add(X86Instruction.Crc32,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38f1, InstructionFlags.PrefixF2));
            Add(X86Instruction.Crc32_16,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38f1, InstructionFlags.PrefixF2 | InstructionFlags.Prefix66));
            Add(X86Instruction.Crc32_8,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38f0, InstructionFlags.PrefixF2 | InstructionFlags.Reg8Src));
            Add(X86Instruction.Cvtdq2pd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fe6, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Cvtdq2ps,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5b, InstructionFlags.Vex));
            Add(X86Instruction.Cvtpd2dq,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fe6, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Cvtpd2ps,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Cvtps2dq,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Cvtps2pd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5a, InstructionFlags.Vex));
            Add(X86Instruction.Cvtsd2si,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2d, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Cvtsd2ss,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5a, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Cvtsi2sd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2a, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Cvtsi2ss,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2a, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Cvtss2sd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5a, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Cvtss2si,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2d, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Div,          new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x060000f7, InstructionFlags.None));
            Add(X86Instruction.Divpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5e, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Divps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5e, InstructionFlags.Vex));
            Add(X86Instruction.Divsd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5e, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Divss,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5e, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Haddpd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f7c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Haddps,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f7c, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Idiv,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x070000f7, InstructionFlags.None));
            Add(X86Instruction.Imul,         new InstructionInfo(BadOp,      0x0000006b, 0x00000069, BadOp,      0x00000faf, InstructionFlags.None));
            Add(X86Instruction.Imul128,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x050000f7, InstructionFlags.None));
            Add(X86Instruction.Insertps,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a21, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Jmp,          new InstructionInfo(0x040000ff, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Ldmxcsr,      new InstructionInfo(0x02000fae, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex));
            Add(X86Instruction.Lea,          new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x0000008d, InstructionFlags.None));
            Add(X86Instruction.Maxpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5f, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Maxps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5f, InstructionFlags.Vex));
            Add(X86Instruction.Maxsd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5f, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Maxss,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5f, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Minpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5d, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Minps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5d, InstructionFlags.Vex));
            Add(X86Instruction.Minsd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5d, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Minss,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5d, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Mov,          new InstructionInfo(0x00000089, BadOp,      0x000000c7, 0x000000b8, 0x0000008b, InstructionFlags.None));
            Add(X86Instruction.Mov16,        new InstructionInfo(0x00000089, BadOp,      0x000000c7, BadOp,      0x0000008b, InstructionFlags.Prefix66));
            Add(X86Instruction.Mov8,         new InstructionInfo(0x00000088, 0x000000c6, BadOp,      BadOp,      0x0000008a, InstructionFlags.Reg8Src | InstructionFlags.Reg8Dest));
            Add(X86Instruction.Movd,         new InstructionInfo(0x00000f7e, BadOp,      BadOp,      BadOp,      0x00000f6e, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Movdqu,       new InstructionInfo(0x00000f7f, BadOp,      BadOp,      BadOp,      0x00000f6f, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Movhlps,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f12, InstructionFlags.Vex));
            Add(X86Instruction.Movlhps,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f16, InstructionFlags.Vex));
            Add(X86Instruction.Movq,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f7e, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Movsd,        new InstructionInfo(0x00000f11, BadOp,      BadOp,      BadOp,      0x00000f10, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Movss,        new InstructionInfo(0x00000f11, BadOp,      BadOp,      BadOp,      0x00000f10, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Movsx16,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fbf, InstructionFlags.None));
            Add(X86Instruction.Movsx32,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000063, InstructionFlags.None));
            Add(X86Instruction.Movsx8,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fbe, InstructionFlags.Reg8Src));
            Add(X86Instruction.Movzx16,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fb7, InstructionFlags.None));
            Add(X86Instruction.Movzx8,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fb6, InstructionFlags.Reg8Src));
            Add(X86Instruction.Mul128,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x040000f7, InstructionFlags.None));
            Add(X86Instruction.Mulpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f59, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Mulps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f59, InstructionFlags.Vex));
            Add(X86Instruction.Mulsd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f59, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Mulss,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f59, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Neg,          new InstructionInfo(0x030000f7, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Not,          new InstructionInfo(0x020000f7, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Or,           new InstructionInfo(0x00000009, 0x01000083, 0x01000081, BadOp,      0x0000000b, InstructionFlags.None));
            Add(X86Instruction.Paddb,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ffc, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Paddd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ffe, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Paddq,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fd4, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Paddw,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ffd, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pand,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fdb, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pandn,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fdf, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pavgb,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fe0, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pavgw,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fe3, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pblendvb,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3810, InstructionFlags.Prefix66));
            Add(X86Instruction.Pclmulqdq,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a44, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpeqb,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f74, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpeqd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f76, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpeqq,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3829, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpeqw,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f75, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpgtb,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f64, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpgtd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f66, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpgtq,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3837, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpgtw,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f65, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pextrb,       new InstructionInfo(0x000f3a14, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pextrd,       new InstructionInfo(0x000f3a16, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pextrq,       new InstructionInfo(0x000f3a16, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.RexW | InstructionFlags.Prefix66));
            Add(X86Instruction.Pextrw,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc5, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pinsrb,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a20, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pinsrd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a22, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pinsrq,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a22, InstructionFlags.Vex | InstructionFlags.RexW | InstructionFlags.Prefix66));
            Add(X86Instruction.Pinsrw,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc4, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxsb,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxsd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383d, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxsw,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fee, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxub,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fde, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxud,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383f, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxuw,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383e, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminsb,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3838, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminsd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3839, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminsw,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fea, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminub,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fda, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminud,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminuw,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovsxbw,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3820, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovsxdq,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3825, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovsxwd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3823, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovzxbw,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3830, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovzxdq,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3835, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovzxwd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3833, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmulld,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3840, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmullw,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fd5, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pop,          new InstructionInfo(0x0000008f, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Popcnt,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fb8, InstructionFlags.PrefixF3));
            Add(X86Instruction.Por,          new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000feb, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pshufb,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3800, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pshufd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f70, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pslld,        new InstructionInfo(BadOp,      0x06000f72, BadOp,      BadOp,      0x00000ff2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pslldq,       new InstructionInfo(BadOp,      0x07000f73, BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psllq,        new InstructionInfo(BadOp,      0x06000f73, BadOp,      BadOp,      0x00000ff3, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psllw,        new InstructionInfo(BadOp,      0x06000f71, BadOp,      BadOp,      0x00000ff1, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psrad,        new InstructionInfo(BadOp,      0x04000f72, BadOp,      BadOp,      0x00000fe2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psraw,        new InstructionInfo(BadOp,      0x04000f71, BadOp,      BadOp,      0x00000fe1, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psrld,        new InstructionInfo(BadOp,      0x02000f72, BadOp,      BadOp,      0x00000fd2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psrlq,        new InstructionInfo(BadOp,      0x02000f73, BadOp,      BadOp,      0x00000fd3, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psrldq,       new InstructionInfo(BadOp,      0x03000f73, BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psrlw,        new InstructionInfo(BadOp,      0x02000f71, BadOp,      BadOp,      0x00000fd1, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psubb,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ff8, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psubd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ffa, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psubq,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ffb, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psubw,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ff9, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpckhbw,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f68, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpckhdq,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f6a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpckhqdq,   new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f6d, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpckhwd,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f69, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpcklbw,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f60, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpckldq,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f62, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpcklqdq,   new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f6c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpcklwd,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f61, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Push,         new InstructionInfo(BadOp,      0x0000006a, 0x00000068, BadOp,      0x060000ff, InstructionFlags.None));
            Add(X86Instruction.Pxor,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fef, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Rcpps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f53, InstructionFlags.Vex));
            Add(X86Instruction.Rcpss,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f53, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Ror,          new InstructionInfo(0x010000d3, 0x010000c1, BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Roundpd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a09, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Roundps,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a08, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Roundsd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a0b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Roundss,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a0a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Rsqrtps,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f52, InstructionFlags.Vex));
            Add(X86Instruction.Rsqrtss,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f52, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Sar,          new InstructionInfo(0x070000d3, 0x070000c1, BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Setcc,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f90, InstructionFlags.Reg8Dest));
            Add(X86Instruction.Shl,          new InstructionInfo(0x040000d3, 0x040000c1, BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Shr,          new InstructionInfo(0x050000d3, 0x050000c1, BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Shufpd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc6, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Shufps,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc6, InstructionFlags.Vex));
            Add(X86Instruction.Sqrtpd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f51, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Sqrtps,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f51, InstructionFlags.Vex));
            Add(X86Instruction.Sqrtsd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f51, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Sqrtss,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f51, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Stmxcsr,      new InstructionInfo(0x03000fae, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex));
            Add(X86Instruction.Sub,          new InstructionInfo(0x00000029, 0x05000083, 0x05000081, BadOp,      0x0000002b, InstructionFlags.None));
            Add(X86Instruction.Subpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Subps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5c, InstructionFlags.Vex));
            Add(X86Instruction.Subsd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5c, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Subss,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5c, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Test,         new InstructionInfo(0x00000085, BadOp,      0x000000f7, BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Unpckhpd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f15, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Unpckhps,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f15, InstructionFlags.Vex));
            Add(X86Instruction.Unpcklpd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f14, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Unpcklps,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f14, InstructionFlags.Vex));
            Add(X86Instruction.Vblendvpd,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a4b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vblendvps,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a4a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vcvtph2ps,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3813, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vcvtps2ph,    new InstructionInfo(0x000f3a1d, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfmadd231ps,  new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38b8, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfmadd231sd,  new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38b9, InstructionFlags.Vex | InstructionFlags.Prefix66 | InstructionFlags.RexW));
            Add(X86Instruction.Vfmadd231ss,  new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38b9, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfmsub231sd,  new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bb, InstructionFlags.Vex | InstructionFlags.Prefix66 | InstructionFlags.RexW));
            Add(X86Instruction.Vfmsub231ss,  new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bb, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfnmadd231ps, new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bc, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfnmadd231sd, new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bd, InstructionFlags.Vex | InstructionFlags.Prefix66 | InstructionFlags.RexW));
            Add(X86Instruction.Vfnmadd231ss, new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bd, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfnmsub231sd, new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bf, InstructionFlags.Vex | InstructionFlags.Prefix66 | InstructionFlags.RexW));
            Add(X86Instruction.Vfnmsub231ss, new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bf, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vpblendvb,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a4c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Xor,          new InstructionInfo(0x00000031, 0x06000083, 0x06000081, BadOp,      0x00000033, InstructionFlags.None));
            Add(X86Instruction.Xorpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f57, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Xorps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f57, InstructionFlags.Vex));
        }

        private static void Add(X86Instruction inst, InstructionInfo info)
        {
            _instTable[(int)inst] = info;
        }

        public Assembler(Stream stream, PtcInfo ptcInfo = null)
        {
            _stream = stream;

            _ptcInfo     = ptcInfo;
            _ptcDisabled = ptcInfo == null;
        }

        public void Add(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Add);
        }

        public void Addsd(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Addsd);
        }

        public void Addss(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Addss);
        }

        public void And(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.And);
        }

        public void Bsr(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Bsr);
        }

        public void Bswap(Operand dest)
        {
            WriteInstruction(dest, null, dest.Type, X86Instruction.Bswap);
        }

        public void Call(Operand dest)
        {
            WriteInstruction(dest, null, OperandType.None, X86Instruction.Call);
        }

        public void Cdq()
        {
            WriteByte(0x99);
        }

        public void Cmovcc(Operand dest, Operand source, OperandType type, X86Condition condition)
        {
            InstructionInfo info = _instTable[(int)X86Instruction.Cmovcc];

            WriteOpCode(dest, null, source, type, info.Flags, info.OpRRM | (int)condition, rrm: true);
        }

        public void Cmp(Operand src1, Operand src2, OperandType type)
        {
            WriteInstruction(src1, src2, type, X86Instruction.Cmp);
        }

        public void Cqo()
        {
            WriteByte(0x48);
            WriteByte(0x99);
        }

        public void Cmpxchg(MemoryOperand memOp, Operand src)
        {
            WriteByte(LockPrefix);

            WriteInstruction(memOp, src, src.Type, X86Instruction.Cmpxchg);
        }

        public void Cmpxchg16(MemoryOperand memOp, Operand src)
        {
            WriteByte(LockPrefix);
            WriteByte(0x66);

            WriteInstruction(memOp, src, src.Type, X86Instruction.Cmpxchg);
        }

        public void Cmpxchg16b(MemoryOperand memOp)
        {
            WriteByte(LockPrefix);

            WriteInstruction(memOp, null, OperandType.None, X86Instruction.Cmpxchg16b);
        }

        public void Cmpxchg8(MemoryOperand memOp, Operand src)
        {
            WriteByte(LockPrefix);

            WriteInstruction(memOp, src, src.Type, X86Instruction.Cmpxchg8);
        }

        public void Comisd(Operand src1, Operand src2)
        {
            WriteInstruction(src1, null, src2, X86Instruction.Comisd);
        }

        public void Comiss(Operand src1, Operand src2)
        {
            WriteInstruction(src1, null, src2, X86Instruction.Comiss);
        }

        public void Cvtsd2ss(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Cvtsd2ss);
        }

        public void Cvtsi2sd(Operand dest, Operand src1, Operand src2, OperandType type)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Cvtsi2sd, type);
        }

        public void Cvtsi2ss(Operand dest, Operand src1, Operand src2, OperandType type)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Cvtsi2ss, type);
        }

        public void Cvtss2sd(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Cvtss2sd);
        }

        public void Div(Operand source)
        {
            WriteInstruction(null, source, source.Type, X86Instruction.Div);
        }

        public void Divsd(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Divsd);
        }

        public void Divss(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Divss);
        }

        public void Idiv(Operand source)
        {
            WriteInstruction(null, source, source.Type, X86Instruction.Idiv);
        }

        public void Imul(Operand source)
        {
            WriteInstruction(null, source, source.Type, X86Instruction.Imul128);
        }

        public void Imul(Operand dest, Operand source, OperandType type)
        {
            if (source.Kind != OperandKind.Register)
            {
                throw new ArgumentException($"Invalid source operand kind \"{source.Kind}\".");
            }

            WriteInstruction(dest, source, type, X86Instruction.Imul);
        }

        public void Imul(Operand dest, Operand src1, Operand src2, OperandType type)
        {
            InstructionInfo info = _instTable[(int)X86Instruction.Imul];

            if (src2.Kind != OperandKind.Constant)
            {
                throw new ArgumentException($"Invalid source 2 operand kind \"{src2.Kind}\".");
            }

            if (IsImm8(src2.Value, src2.Type) && info.OpRMImm8 != BadOp)
            {
                WriteOpCode(dest, null, src1, type, info.Flags, info.OpRMImm8, rrm: true);

                WriteByte(src2.AsByte());
            }
            else if (IsImm32(src2.Value, src2.Type) && info.OpRMImm32 != BadOp)
            {
                WriteOpCode(dest, null, src1, type, info.Flags, info.OpRMImm32, rrm: true);

                WriteInt32(src2.AsInt32());
            }
            else
            {
                throw new ArgumentException($"Failed to encode constant 0x{src2.Value:X}.");
            }
        }

        public void Insertps(Operand dest, Operand src1, Operand src2, byte imm)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Insertps);

            WriteByte(imm);
        }

        public void Jcc(X86Condition condition, long offset)
        {
            if (_ptcDisabled && ConstFitsOnS8(offset))
            {
                WriteByte((byte)(0x70 | (int)condition));

                WriteByte((byte)offset);
            }
            else if (ConstFitsOnS32(offset))
            {
                WriteByte(0x0f);
                WriteByte((byte)(0x80 | (int)condition));

                WriteInt32((int)offset);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }

        public void Jmp(long offset)
        {
            if (_ptcDisabled && ConstFitsOnS8(offset))
            {
                WriteByte(0xeb);

                WriteByte((byte)offset);
            }
            else if (ConstFitsOnS32(offset))
            {
                WriteByte(0xe9);

                WriteInt32((int)offset);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }

        public void Jmp(Operand dest)
        {
            WriteInstruction(dest, null, OperandType.None, X86Instruction.Jmp);
        }

        public void Ldmxcsr(Operand dest)
        {
            WriteInstruction(dest, null, OperandType.I32, X86Instruction.Ldmxcsr);
        }

        public void Lea(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Lea);
        }

        public void Mov(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Mov);
        }

        public void Mov16(Operand dest, Operand source)
        {
            WriteInstruction(dest, source, OperandType.None, X86Instruction.Mov16);
        }

        public void Mov8(Operand dest, Operand source)
        {
            WriteInstruction(dest, source, OperandType.None, X86Instruction.Mov8);
        }

        public void Movd(Operand dest, Operand source)
        {
            InstructionInfo info = _instTable[(int)X86Instruction.Movd];

            if (source.Type.IsInteger() || source.Kind == OperandKind.Memory)
            {
                WriteOpCode(dest, null, source, OperandType.None, info.Flags, info.OpRRM, rrm: true);
            }
            else
            {
                WriteOpCode(dest, null, source, OperandType.None, info.Flags, info.OpRMR);
            }
        }

        public void Movdqu(Operand dest, Operand source)
        {
            WriteInstruction(dest, null, source, X86Instruction.Movdqu);
        }

        public void Movhlps(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Movhlps);
        }

        public void Movlhps(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Movlhps);
        }

        public void Movq(Operand dest, Operand source)
        {
            InstructionInfo info = _instTable[(int)X86Instruction.Movd];

            InstructionFlags flags = info.Flags | InstructionFlags.RexW;

            if (source.Type.IsInteger() || source.Kind == OperandKind.Memory)
            {
                WriteOpCode(dest, null, source, OperandType.None, flags, info.OpRRM, rrm: true);
            }
            else if (dest.Type.IsInteger() || dest.Kind == OperandKind.Memory)
            {
                WriteOpCode(dest, null, source, OperandType.None, flags, info.OpRMR);
            }
            else
            {
                WriteInstruction(dest, source, OperandType.None, X86Instruction.Movq);
            }
        }

        public void Movsd(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Movsd);
        }

        public void Movss(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Movss);
        }

        public void Movsx16(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Movsx16);
        }

        public void Movsx32(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Movsx32);
        }

        public void Movsx8(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Movsx8);
        }

        public void Movzx16(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Movzx16);
        }

        public void Movzx8(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Movzx8);
        }

        public void Mul(Operand source)
        {
            WriteInstruction(null, source, source.Type, X86Instruction.Mul128);
        }

        public void Mulsd(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Mulsd);
        }

        public void Mulss(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Mulss);
        }

        public void Neg(Operand dest)
        {
            WriteInstruction(dest, null, dest.Type, X86Instruction.Neg);
        }

        public void Not(Operand dest)
        {
            WriteInstruction(dest, null, dest.Type, X86Instruction.Not);
        }

        public void Or(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Or);
        }

        public void Pclmulqdq(Operand dest, Operand source, byte imm)
        {
            WriteInstruction(dest, null, source, X86Instruction.Pclmulqdq);

            WriteByte(imm);
        }

        public void Pcmpeqw(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Pcmpeqw);
        }

        public void Pextrb(Operand dest, Operand source, byte imm)
        {
            WriteInstruction(dest, null, source, X86Instruction.Pextrb);

            WriteByte(imm);
        }

        public void Pextrd(Operand dest, Operand source, byte imm)
        {
            WriteInstruction(dest, null, source, X86Instruction.Pextrd);

            WriteByte(imm);
        }

        public void Pextrq(Operand dest, Operand source, byte imm)
        {
            WriteInstruction(dest, null, source, X86Instruction.Pextrq);

            WriteByte(imm);
        }

        public void Pextrw(Operand dest, Operand source, byte imm)
        {
            WriteInstruction(dest, null, source, X86Instruction.Pextrw);

            WriteByte(imm);
        }

        public void Pinsrb(Operand dest, Operand src1, Operand src2, byte imm)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Pinsrb);

            WriteByte(imm);
        }

        public void Pinsrd(Operand dest, Operand src1, Operand src2, byte imm)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Pinsrd);

            WriteByte(imm);
        }

        public void Pinsrq(Operand dest, Operand src1, Operand src2, byte imm)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Pinsrq);

            WriteByte(imm);
        }

        public void Pinsrw(Operand dest, Operand src1, Operand src2, byte imm)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Pinsrw);

            WriteByte(imm);
        }

        public void Pop(Operand dest)
        {
            if (dest.Kind == OperandKind.Register)
            {
                WriteCompactInst(dest, 0x58);
            }
            else
            {
                WriteInstruction(dest, null, dest.Type, X86Instruction.Pop);
            }
        }

        public void Popcnt(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Popcnt);
        }

        public void Pshufd(Operand dest, Operand source, byte imm)
        {
            WriteInstruction(dest, null, source, X86Instruction.Pshufd);

            WriteByte(imm);
        }

        public void Push(Operand source)
        {
            if (source.Kind == OperandKind.Register)
            {
                WriteCompactInst(source, 0x50);
            }
            else
            {
                WriteInstruction(null, source, source.Type, X86Instruction.Push);
            }
        }

        public void Return()
        {
            WriteByte(0xc3);
        }

        public void Ror(Operand dest, Operand source, OperandType type)
        {
            WriteShiftInst(dest, source, type, X86Instruction.Ror);
        }

        public void Sar(Operand dest, Operand source, OperandType type)
        {
            WriteShiftInst(dest, source, type, X86Instruction.Sar);
        }

        public void Shl(Operand dest, Operand source, OperandType type)
        {
            WriteShiftInst(dest, source, type, X86Instruction.Shl);
        }

        public void Shr(Operand dest, Operand source, OperandType type)
        {
            WriteShiftInst(dest, source, type, X86Instruction.Shr);
        }

        public void Setcc(Operand dest, X86Condition condition)
        {
            InstructionInfo info = _instTable[(int)X86Instruction.Setcc];

            WriteOpCode(dest, null, null, OperandType.None, info.Flags, info.OpRRM | (int)condition);
        }

        public void Stmxcsr(Operand dest)
        {
            WriteInstruction(dest, null, OperandType.I32, X86Instruction.Stmxcsr);
        }

        public void Sub(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Sub);
        }

        public void Subsd(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Subsd);
        }

        public void Subss(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Subss);
        }

        public void Test(Operand src1, Operand src2, OperandType type)
        {
            WriteInstruction(src1, src2, type, X86Instruction.Test);
        }

        public void Xor(Operand dest, Operand source, OperandType type)
        {
            WriteInstruction(dest, source, type, X86Instruction.Xor);
        }

        public void Xorps(Operand dest, Operand src1, Operand src2)
        {
            WriteInstruction(dest, src1, src2, X86Instruction.Xorps);
        }

        public void WriteInstruction(
            X86Instruction inst,
            Operand dest,
            Operand source,
            OperandType type = OperandType.None)
        {
            WriteInstruction(dest, null, source, inst, type);
        }

        public void WriteInstruction(X86Instruction inst, Operand dest, Operand src1, Operand src2)
        {
            if (src2.Kind == OperandKind.Constant)
            {
                WriteInstruction(src1, dest, src2, inst);
            }
            else
            {
                WriteInstruction(dest, src1, src2, inst);
            }
        }

        public void WriteInstruction(
            X86Instruction inst,
            Operand dest,
            Operand src1,
            Operand src2,
            OperandType type)
        {
            WriteInstruction(dest, src1, src2, inst, type);
        }

        public void WriteInstruction(X86Instruction inst, Operand dest, Operand source, byte imm)
        {
            WriteInstruction(dest, null, source, inst);

            WriteByte(imm);
        }

        public void WriteInstruction(
            X86Instruction inst,
            Operand dest,
            Operand src1,
            Operand src2,
            Operand src3)
        {
            // 3+ operands can only be encoded with the VEX encoding scheme.
            Debug.Assert(HardwareCapabilities.SupportsVexEncoding);

            WriteInstruction(dest, src1, src2, inst);

            WriteByte((byte)(src3.AsByte() << 4));
        }

        public void WriteInstruction(
            X86Instruction inst,
            Operand dest,
            Operand src1,
            Operand src2,
            byte imm)
        {
            WriteInstruction(dest, src1, src2, inst);

            WriteByte(imm);
        }

        private void WriteShiftInst(Operand dest, Operand source, OperandType type, X86Instruction inst)
        {
            if (source.Kind == OperandKind.Register)
            {
                X86Register shiftReg = (X86Register)source.GetRegister().Index;

                Debug.Assert(shiftReg == X86Register.Rcx, $"Invalid shift register \"{shiftReg}\".");

                source = null;
            }
            else if (source.Kind == OperandKind.Constant)
            {
                source = source.With((int)source.Value & (dest.Type == OperandType.I32 ? 0x1f : 0x3f));
            }

            WriteInstruction(dest, source, type, inst);
        }

        private void WriteInstruction(Operand dest, Operand source, OperandType type, X86Instruction inst)
        {
            InstructionInfo info = _instTable[(int)inst];

            if (source != null)
            {
                if (source.Kind == OperandKind.Constant)
                {
                    ulong imm = source.Value;

                    if (inst == X86Instruction.Mov8)
                    {
                        WriteOpCode(dest, null, null, type, info.Flags, info.OpRMImm8);

                        WriteByte((byte)imm);
                    }
                    else if (inst == X86Instruction.Mov16)
                    {
                        WriteOpCode(dest, null, null, type, info.Flags, info.OpRMImm32);

                        WriteInt16((short)imm);
                    }
                    else if (IsImm8(imm, type) && info.OpRMImm8 != BadOp)
                    {
                        WriteOpCode(dest, null, null, type, info.Flags, info.OpRMImm8);

                        WriteByte((byte)imm);
                    }
                    else if (!source.Relocatable && IsImm32(imm, type) && info.OpRMImm32 != BadOp)
                    {
                        WriteOpCode(dest, null, null, type, info.Flags, info.OpRMImm32);

                        WriteInt32((int)imm);
                    }
                    else if (dest?.Kind == OperandKind.Register && info.OpRImm64 != BadOp)
                    {
                        int? index = source.PtcIndex;

                        int rexPrefix = GetRexPrefix(dest, source, type, rrm: false);

                        if (rexPrefix != 0)
                        {
                            WriteByte((byte)rexPrefix);
                        }

                        WriteByte((byte)(info.OpRImm64 + (dest.GetRegister().Index & 0b111)));

                        if (_ptcInfo != null && index != null)
                        {
                            _ptcInfo.WriteRelocEntry(new RelocEntry((int)_stream.Position, (int)index));
                        }

                        WriteUInt64(imm);
                    }
                    else
                    {
                        throw new ArgumentException($"Failed to encode constant 0x{imm:X}.");
                    }
                }
                else if (source.Kind == OperandKind.Register && info.OpRMR != BadOp)
                {
                    WriteOpCode(dest, null, source, type, info.Flags, info.OpRMR);
                }
                else if (info.OpRRM != BadOp)
                {
                    WriteOpCode(dest, null, source, type, info.Flags, info.OpRRM, rrm: true);
                }
                else
                {
                    throw new ArgumentException($"Invalid source operand kind \"{source.Kind}\".");
                }
            }
            else if (info.OpRRM != BadOp)
            {
                WriteOpCode(dest, null, source, type, info.Flags, info.OpRRM, rrm: true);
            }
            else if (info.OpRMR != BadOp)
            {
                WriteOpCode(dest, null, source, type, info.Flags, info.OpRMR);
            }
            else
            {
                throw new ArgumentNullException(nameof(source));
            }
        }

        private void WriteInstruction(
            Operand dest,
            Operand src1,
            Operand src2,
            X86Instruction inst,
            OperandType type = OperandType.None)
        {
            InstructionInfo info = _instTable[(int)inst];

            if (src2 != null)
            {
                if (src2.Kind == OperandKind.Constant)
                {
                    ulong imm = src2.Value;

                    if ((byte)imm == imm && info.OpRMImm8 != BadOp)
                    {
                        WriteOpCode(dest, src1, null, type, info.Flags, info.OpRMImm8);

                        WriteByte((byte)imm);
                    }
                    else
                    {
                        throw new ArgumentException($"Failed to encode constant 0x{imm:X}.");
                    }
                }
                else if (src2.Kind == OperandKind.Register && info.OpRMR != BadOp)
                {
                    WriteOpCode(dest, src1, src2, type, info.Flags, info.OpRMR);
                }
                else if (info.OpRRM != BadOp)
                {
                    WriteOpCode(dest, src1, src2, type, info.Flags, info.OpRRM, rrm: true);
                }
                else
                {
                    throw new ArgumentException($"Invalid source operand kind \"{src2.Kind}\".");
                }
            }
            else if (info.OpRRM != BadOp)
            {
                WriteOpCode(dest, src1, src2, type, info.Flags, info.OpRRM, rrm: true);
            }
            else if (info.OpRMR != BadOp)
            {
                WriteOpCode(dest, src1, src2, type, info.Flags, info.OpRMR);
            }
            else
            {
                throw new ArgumentNullException(nameof(src2));
            }
        }

        private void WriteOpCode(
            Operand dest,
            Operand src1,
            Operand src2,
            OperandType type,
            InstructionFlags flags,
            int opCode,
            bool rrm = false)
        {
            int rexPrefix = GetRexPrefix(dest, src2, type, rrm);

            if ((flags & InstructionFlags.RexW) != 0)
            {
                rexPrefix |= RexWPrefix;
            }

            int modRM = (opCode >> OpModRMBits) << 3;

            MemoryOperand memOp = null;

            if (dest != null)
            {
                if (dest.Kind == OperandKind.Register)
                {
                    int regIndex = dest.GetRegister().Index;

                    modRM |= (regIndex & 0b111) << (rrm ? 3 : 0);

                    if ((flags & InstructionFlags.Reg8Dest) != 0 && regIndex >= 4)
                    {
                        rexPrefix |= RexPrefix;
                    }
                }
                else if (dest.Kind == OperandKind.Memory)
                {
                    memOp = dest as MemoryOperand;
                }
                else
                {
                    throw new ArgumentException("Invalid destination operand kind \"" + dest.Kind + "\".");
                }
            }

            if (src2 != null)
            {
                if (src2.Kind == OperandKind.Register)
                {
                    int regIndex = src2.GetRegister().Index;

                    modRM |= (regIndex & 0b111) << (rrm ? 0 : 3);

                    if ((flags & InstructionFlags.Reg8Src) != 0 && regIndex >= 4)
                    {
                        rexPrefix |= RexPrefix;
                    }
                }
                else if (src2.Kind == OperandKind.Memory && memOp == null)
                {
                    memOp = src2 as MemoryOperand;
                }
                else
                {
                    throw new ArgumentException("Invalid source operand kind \"" + src2.Kind + "\".");
                }
            }

            bool needsSibByte      = false;
            bool needsDisplacement = false;

            int sib = 0;

            if (memOp != null)
            {
                // Either source or destination is a memory operand.
                Register baseReg = memOp.BaseAddress.GetRegister();

                X86Register baseRegLow = (X86Register)(baseReg.Index & 0b111);

                needsSibByte      = memOp.Index != null     || baseRegLow == X86Register.Rsp;
                needsDisplacement = memOp.Displacement != 0 || baseRegLow == X86Register.Rbp;

                if (needsDisplacement)
                {
                    if (ConstFitsOnS8(memOp.Displacement))
                    {
                        modRM |= 0x40;
                    }
                    else /* if (ConstFitsOnS32(memOp.Displacement)) */
                    {
                        modRM |= 0x80;
                    }
                }

                if (baseReg.Index >= 8)
                {
                    Debug.Assert((uint)baseReg.Index <= MaxRegNumber);

                    rexPrefix |= RexPrefix | (baseReg.Index >> 3);
                }

                if (needsSibByte)
                {
                    sib = (int)baseRegLow;

                    if (memOp.Index != null)
                    {
                        int indexReg = memOp.Index.GetRegister().Index;

                        Debug.Assert(indexReg != (int)X86Register.Rsp, "Using RSP as index register on the memory operand is not allowed.");

                        if (indexReg >= 8)
                        {
                            Debug.Assert((uint)indexReg <= MaxRegNumber);

                            rexPrefix |= RexPrefix | (indexReg >> 3) << 1;
                        }

                        sib |= (indexReg & 0b111) << 3;
                    }
                    else
                    {
                        sib |= 0b100 << 3;
                    }

                    sib |= (int)memOp.Scale << 6;

                    modRM |= 0b100;
                }
                else
                {
                    modRM |= (int)baseRegLow;
                }
            }
            else
            {
                // Source and destination are registers.
                modRM |= 0xc0;
            }

            Debug.Assert(opCode != BadOp, "Invalid opcode value.");

            if ((flags & InstructionFlags.Vex) != 0 && HardwareCapabilities.SupportsVexEncoding)
            {
                // In a vex encoding, only one prefix can be active at a time. The active prefix is encoded in the second byte using two bits.

                int vexByte2 = (flags & InstructionFlags.PrefixMask) switch
                {
                    InstructionFlags.Prefix66 => 1,
                    InstructionFlags.PrefixF3 => 2,
                    InstructionFlags.PrefixF2 => 3,
                    _ => 0
                };

                if (src1 != null)
                {
                    vexByte2 |= (src1.GetRegister().Index ^ 0xf) << 3;
                }
                else
                {
                    vexByte2 |= 0b1111 << 3;
                }

                ushort opCodeHigh = (ushort)(opCode >> 8);

                if ((rexPrefix & 0b1011) == 0 && opCodeHigh == 0xf)
                {
                    // Two-byte form.
                    WriteByte(0xc5);

                    vexByte2 |= (~rexPrefix & 4) << 5;

                    WriteByte((byte)vexByte2);
                }
                else
                {
                    // Three-byte form.
                    WriteByte(0xc4);

                    int vexByte1 = (~rexPrefix & 7) << 5;

                    switch (opCodeHigh)
                    {
                        case 0xf:   vexByte1 |= 1; break;
                        case 0xf38: vexByte1 |= 2; break;
                        case 0xf3a: vexByte1 |= 3; break;

                        default: Debug.Assert(false, $"Failed to VEX encode opcode 0x{opCode:X}."); break;
                    }

                    vexByte2 |= (rexPrefix & 8) << 4;

                    WriteByte((byte)vexByte1);
                    WriteByte((byte)vexByte2);
                }

                opCode &= 0xff;
            }
            else
            {
                if (flags.HasFlag(InstructionFlags.Prefix66))
                {
                    WriteByte(0x66);
                }

                if (flags.HasFlag(InstructionFlags.PrefixF2))
                {
                    WriteByte(0xf2);
                }

                if (flags.HasFlag(InstructionFlags.PrefixF3))
                {
                    WriteByte(0xf3);
                }

                if (rexPrefix != 0)
                {
                    WriteByte((byte)rexPrefix);
                }
            }

            if (dest != null && (flags & InstructionFlags.RegOnly) != 0)
            {
                opCode += dest.GetRegister().Index & 7;
            }

            if ((opCode & 0xff0000) != 0)
            {
                WriteByte((byte)(opCode >> 16));
            }

            if ((opCode & 0xff00) != 0)
            {
                WriteByte((byte)(opCode >> 8));
            }

            WriteByte((byte)opCode);

            if ((flags & InstructionFlags.RegOnly) == 0)
            {
                WriteByte((byte)modRM);

                if (needsSibByte)
                {
                    WriteByte((byte)sib);
                }

                if (needsDisplacement)
                {
                    if (ConstFitsOnS8(memOp.Displacement))
                    {
                        WriteByte((byte)memOp.Displacement);
                    }
                    else /* if (ConstFitsOnS32(memOp.Displacement)) */
                    {
                        WriteInt32(memOp.Displacement);
                    }
                }
            }
        }

        private void WriteCompactInst(Operand operand, int opCode)
        {
            int regIndex = operand.GetRegister().Index;

            if (regIndex >= 8)
            {
                WriteByte(0x41);
            }

            WriteByte((byte)(opCode + (regIndex & 0b111)));
        }

        private static int GetRexPrefix(Operand dest, Operand source, OperandType type, bool rrm)
        {
            int rexPrefix = 0;

            if (Is64Bits(type))
            {
                rexPrefix = RexWPrefix;
            }

            void SetRegisterHighBit(Register reg, int bit)
            {
                if (reg.Index >= 8)
                {
                    rexPrefix |= RexPrefix | (reg.Index >> 3) << bit;
                }
            }

            if (dest != null && dest.Kind == OperandKind.Register)
            {
                SetRegisterHighBit(dest.GetRegister(), rrm ? 2 : 0);
            }

            if (source != null && source.Kind == OperandKind.Register)
            {
                SetRegisterHighBit(source.GetRegister(), rrm ? 0 : 2);
            }

            return rexPrefix;
        }

        private static bool Is64Bits(OperandType type)
        {
            return type == OperandType.I64 || type == OperandType.FP64;
        }

        private static bool IsImm8(ulong immediate, OperandType type)
        {
            long value = type == OperandType.I32 ? (int)immediate : (long)immediate;

            return ConstFitsOnS8(value);
        }

        private static bool IsImm32(ulong immediate, OperandType type)
        {
            long value = type == OperandType.I32 ? (int)immediate : (long)immediate;

            return ConstFitsOnS32(value);
        }

        public static int GetJccLength(long offset, bool ptcDisabled = true)
        {
            if (ptcDisabled && ConstFitsOnS8(offset < 0 ? offset - 2 : offset))
            {
                return 2;
            }
            else if (ConstFitsOnS32(offset < 0 ? offset - 6 : offset))
            {
                return 6;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }

        public static int GetJmpLength(long offset, bool ptcDisabled = true)
        {
            if (ptcDisabled && ConstFitsOnS8(offset < 0 ? offset - 2 : offset))
            {
                return 2;
            }
            else if (ConstFitsOnS32(offset < 0 ? offset - 5 : offset))
            {
                return 5;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }

        private static bool ConstFitsOnS8(long value)
        {
            return value == (sbyte)value;
        }

        private static bool ConstFitsOnS32(long value)
        {
            return value == (int)value;
        }

        private void WriteInt16(short value)
        {
            WriteUInt16((ushort)value);
        }

        private void WriteInt32(int value)
        {
            WriteUInt32((uint)value);
        }

        private void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        private void WriteUInt16(ushort value)
        {
            _stream.WriteByte((byte)(value >> 0));
            _stream.WriteByte((byte)(value >> 8));
        }

        private void WriteUInt32(uint value)
        {
            _stream.WriteByte((byte)(value >> 0));
            _stream.WriteByte((byte)(value >> 8));
            _stream.WriteByte((byte)(value >> 16));
            _stream.WriteByte((byte)(value >> 24));
        }

        private void WriteUInt64(ulong value)
        {
            _stream.WriteByte((byte)(value >> 0));
            _stream.WriteByte((byte)(value >> 8));
            _stream.WriteByte((byte)(value >> 16));
            _stream.WriteByte((byte)(value >> 24));
            _stream.WriteByte((byte)(value >> 32));
            _stream.WriteByte((byte)(value >> 40));
            _stream.WriteByte((byte)(value >> 48));
            _stream.WriteByte((byte)(value >> 56));
        }
    }
}