using System;
using System.Diagnostics.CodeAnalysis;

namespace ARMeilleure.CodeGen.X86
{
    partial class Assembler
    {
        public static bool SupportsVexPrefix(X86Instruction inst)
        {
            return _instTable[(int)inst].Flags.HasFlag(InstructionFlags.Vex);
        }

        private const int BadOp = 0;

        [Flags]
        [SuppressMessage("Design", "CA1069: Enums values should not be duplicated")]
        private enum InstructionFlags
        {
            None = 0,
            RegOnly = 1 << 0,
            Reg8Src = 1 << 1,
            Reg8Dest = 1 << 2,
            RexW = 1 << 3,
            Vex = 1 << 4,
            Evex = 1 << 5,

            PrefixBit = 16,
            PrefixMask = 7 << PrefixBit,
            Prefix66 = 1 << PrefixBit,
            PrefixF3 = 2 << PrefixBit,
            PrefixF2 = 4 << PrefixBit,
        }

        private readonly struct InstructionInfo
        {
            public int OpRMR { get; }
            public int OpRMImm8 { get; }
            public int OpRMImm32 { get; }
            public int OpRImm64 { get; }
            public int OpRRM { get; }

            public InstructionFlags Flags { get; }

            public InstructionInfo(
                int opRMR,
                int opRMImm8,
                int opRMImm32,
                int opRImm64,
                int opRRM,
                InstructionFlags flags)
            {
                OpRMR = opRMR;
                OpRMImm8 = opRMImm8;
                OpRMImm32 = opRMImm32;
                OpRImm64 = opRImm64;
                OpRRM = opRRM;
                Flags = flags;
            }
        }

        private readonly static InstructionInfo[] _instTable;

        static Assembler()
        {
            _instTable = new InstructionInfo[(int)X86Instruction.Count];

#pragma warning disable IDE0055 // Disable formatting
            //  Name                                             RM/R        RM/I8       RM/I32      R/I64       R/RM        Flags
            Add(X86Instruction.Add,           new InstructionInfo(0x00000001, 0x00000083, 0x00000081, BadOp,      0x00000003, InstructionFlags.None));
            Add(X86Instruction.Addpd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f58, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Addps,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f58, InstructionFlags.Vex));
            Add(X86Instruction.Addsd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f58, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Addss,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f58, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Aesdec,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38de, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Aesdeclast,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38df, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Aesenc,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38dc, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Aesenclast,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38dd, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Aesimc,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38db, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.And,           new InstructionInfo(0x00000021, 0x04000083, 0x04000081, BadOp,      0x00000023, InstructionFlags.None));
            Add(X86Instruction.Andnpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f55, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Andnps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f55, InstructionFlags.Vex));
            Add(X86Instruction.Andpd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f54, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Andps,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f54, InstructionFlags.Vex));
            Add(X86Instruction.Blendvpd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3815, InstructionFlags.Prefix66));
            Add(X86Instruction.Blendvps,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3814, InstructionFlags.Prefix66));
            Add(X86Instruction.Bsr,           new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fbd, InstructionFlags.None));
            Add(X86Instruction.Bswap,         new InstructionInfo(0x00000fc8, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.RegOnly));
            Add(X86Instruction.Call,          new InstructionInfo(0x020000ff, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Cmovcc,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f40, InstructionFlags.None));
            Add(X86Instruction.Cmp,           new InstructionInfo(0x00000039, 0x07000083, 0x07000081, BadOp,      0x0000003b, InstructionFlags.None));
            Add(X86Instruction.Cmppd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Cmpps,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc2, InstructionFlags.Vex));
            Add(X86Instruction.Cmpsd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc2, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Cmpss,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc2, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Cmpxchg,       new InstructionInfo(0x00000fb1, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Cmpxchg16b,    new InstructionInfo(0x01000fc7, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.RexW));
            Add(X86Instruction.Cmpxchg8,      new InstructionInfo(0x00000fb0, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Reg8Src));
            Add(X86Instruction.Comisd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2f, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Comiss,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2f, InstructionFlags.Vex));
            Add(X86Instruction.Crc32,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38f1, InstructionFlags.PrefixF2));
            Add(X86Instruction.Crc32_16,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38f1, InstructionFlags.PrefixF2 | InstructionFlags.Prefix66));
            Add(X86Instruction.Crc32_8,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38f0, InstructionFlags.PrefixF2 | InstructionFlags.Reg8Src));
            Add(X86Instruction.Cvtdq2pd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fe6, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Cvtdq2ps,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5b, InstructionFlags.Vex));
            Add(X86Instruction.Cvtpd2dq,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fe6, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Cvtpd2ps,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Cvtps2dq,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Cvtps2pd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5a, InstructionFlags.Vex));
            Add(X86Instruction.Cvtsd2si,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2d, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Cvtsd2ss,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5a, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Cvtsi2sd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2a, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Cvtsi2ss,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2a, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Cvtss2sd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5a, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Cvtss2si,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f2d, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Div,           new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x060000f7, InstructionFlags.None));
            Add(X86Instruction.Divpd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5e, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Divps,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5e, InstructionFlags.Vex));
            Add(X86Instruction.Divsd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5e, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Divss,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5e, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Gf2p8affineqb, new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3ace, InstructionFlags.Prefix66));
            Add(X86Instruction.Haddpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f7c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Haddps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f7c, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Idiv,          new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x070000f7, InstructionFlags.None));
            Add(X86Instruction.Imul,          new InstructionInfo(BadOp,      0x0000006b, 0x00000069, BadOp,      0x00000faf, InstructionFlags.None));
            Add(X86Instruction.Imul128,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x050000f7, InstructionFlags.None));
            Add(X86Instruction.Insertps,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a21, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Jmp,           new InstructionInfo(0x040000ff, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Ldmxcsr,       new InstructionInfo(0x02000fae, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex));
            Add(X86Instruction.Lea,           new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x0000008d, InstructionFlags.None));
            Add(X86Instruction.Maxpd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5f, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Maxps,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5f, InstructionFlags.Vex));
            Add(X86Instruction.Maxsd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5f, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Maxss,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5f, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Minpd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5d, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Minps,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5d, InstructionFlags.Vex));
            Add(X86Instruction.Minsd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5d, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Minss,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5d, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Mov,           new InstructionInfo(0x00000089, BadOp,      0x000000c7, 0x000000b8, 0x0000008b, InstructionFlags.None));
            Add(X86Instruction.Mov16,         new InstructionInfo(0x00000089, BadOp,      0x000000c7, BadOp,      0x0000008b, InstructionFlags.Prefix66));
            Add(X86Instruction.Mov8,          new InstructionInfo(0x00000088, 0x000000c6, BadOp,      BadOp,      0x0000008a, InstructionFlags.Reg8Src | InstructionFlags.Reg8Dest));
            Add(X86Instruction.Movd,          new InstructionInfo(0x00000f7e, BadOp,      BadOp,      BadOp,      0x00000f6e, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Movdqu,        new InstructionInfo(0x00000f7f, BadOp,      BadOp,      BadOp,      0x00000f6f, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Movhlps,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f12, InstructionFlags.Vex));
            Add(X86Instruction.Movlhps,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f16, InstructionFlags.Vex));
            Add(X86Instruction.Movq,          new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f7e, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Movsd,         new InstructionInfo(0x00000f11, BadOp,      BadOp,      BadOp,      0x00000f10, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Movss,         new InstructionInfo(0x00000f11, BadOp,      BadOp,      BadOp,      0x00000f10, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Movsx16,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fbf, InstructionFlags.None));
            Add(X86Instruction.Movsx32,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000063, InstructionFlags.None));
            Add(X86Instruction.Movsx8,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fbe, InstructionFlags.Reg8Src));
            Add(X86Instruction.Movzx16,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fb7, InstructionFlags.None));
            Add(X86Instruction.Movzx8,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fb6, InstructionFlags.Reg8Src));
            Add(X86Instruction.Mul128,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x040000f7, InstructionFlags.None));
            Add(X86Instruction.Mulpd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f59, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Mulps,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f59, InstructionFlags.Vex));
            Add(X86Instruction.Mulsd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f59, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Mulss,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f59, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Neg,           new InstructionInfo(0x030000f7, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Not,           new InstructionInfo(0x020000f7, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Or,            new InstructionInfo(0x00000009, 0x01000083, 0x01000081, BadOp,      0x0000000b, InstructionFlags.None));
            Add(X86Instruction.Paddb,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ffc, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Paddd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ffe, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Paddq,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fd4, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Paddw,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ffd, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Palignr,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a0f, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pand,          new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fdb, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pandn,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fdf, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pavgb,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fe0, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pavgw,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fe3, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pblendvb,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3810, InstructionFlags.Prefix66));
            Add(X86Instruction.Pclmulqdq,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a44, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpeqb,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f74, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpeqd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f76, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpeqq,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3829, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpeqw,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f75, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpgtb,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f64, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpgtd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f66, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpgtq,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3837, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pcmpgtw,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f65, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pextrb,        new InstructionInfo(0x000f3a14, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pextrd,        new InstructionInfo(0x000f3a16, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pextrq,        new InstructionInfo(0x000f3a16, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.RexW | InstructionFlags.Prefix66));
            Add(X86Instruction.Pextrw,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc5, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pinsrb,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a20, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pinsrd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a22, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pinsrq,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a22, InstructionFlags.Vex | InstructionFlags.RexW | InstructionFlags.Prefix66));
            Add(X86Instruction.Pinsrw,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc4, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxsb,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxsd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383d, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxsw,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fee, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxub,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fde, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxud,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383f, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmaxuw,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383e, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminsb,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3838, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminsd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3839, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminsw,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fea, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminub,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fda, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminud,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pminuw,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f383a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovsxbw,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3820, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovsxdq,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3825, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovsxwd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3823, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovzxbw,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3830, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovzxdq,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3835, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmovzxwd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3833, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmulld,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3840, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pmullw,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fd5, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pop,           new InstructionInfo(0x0000008f, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Popcnt,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fb8, InstructionFlags.PrefixF3));
            Add(X86Instruction.Por,           new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000feb, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pshufb,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3800, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pshufd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f70, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pslld,         new InstructionInfo(BadOp,      0x06000f72, BadOp,      BadOp,      0x00000ff2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Pslldq,        new InstructionInfo(BadOp,      0x07000f73, BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psllq,         new InstructionInfo(BadOp,      0x06000f73, BadOp,      BadOp,      0x00000ff3, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psllw,         new InstructionInfo(BadOp,      0x06000f71, BadOp,      BadOp,      0x00000ff1, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psrad,         new InstructionInfo(BadOp,      0x04000f72, BadOp,      BadOp,      0x00000fe2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psraw,         new InstructionInfo(BadOp,      0x04000f71, BadOp,      BadOp,      0x00000fe1, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psrld,         new InstructionInfo(BadOp,      0x02000f72, BadOp,      BadOp,      0x00000fd2, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psrlq,         new InstructionInfo(BadOp,      0x02000f73, BadOp,      BadOp,      0x00000fd3, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psrldq,        new InstructionInfo(BadOp,      0x03000f73, BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psrlw,         new InstructionInfo(BadOp,      0x02000f71, BadOp,      BadOp,      0x00000fd1, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psubb,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ff8, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psubd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ffa, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psubq,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ffb, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Psubw,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000ff9, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpckhbw,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f68, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpckhdq,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f6a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpckhqdq,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f6d, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpckhwd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f69, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpcklbw,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f60, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpckldq,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f62, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpcklqdq,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f6c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Punpcklwd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f61, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Push,          new InstructionInfo(BadOp,      0x0000006a, 0x00000068, BadOp,      0x060000ff, InstructionFlags.None));
            Add(X86Instruction.Pxor,          new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fef, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Rcpps,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f53, InstructionFlags.Vex));
            Add(X86Instruction.Rcpss,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f53, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Ror,           new InstructionInfo(0x010000d3, 0x010000c1, BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Roundpd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a09, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Roundps,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a08, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Roundsd,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a0b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Roundss,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a0a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Rsqrtps,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f52, InstructionFlags.Vex));
            Add(X86Instruction.Rsqrtss,       new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f52, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Sar,           new InstructionInfo(0x070000d3, 0x070000c1, BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Setcc,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f90, InstructionFlags.Reg8Dest));
            Add(X86Instruction.Sha256Msg1,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38cc, InstructionFlags.None));
            Add(X86Instruction.Sha256Msg2,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38cd, InstructionFlags.None));
            Add(X86Instruction.Sha256Rnds2,   new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38cb, InstructionFlags.None));
            Add(X86Instruction.Shl,           new InstructionInfo(0x040000d3, 0x040000c1, BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Shr,           new InstructionInfo(0x050000d3, 0x050000c1, BadOp,      BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Shufpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc6, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Shufps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000fc6, InstructionFlags.Vex));
            Add(X86Instruction.Sqrtpd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f51, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Sqrtps,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f51, InstructionFlags.Vex));
            Add(X86Instruction.Sqrtsd,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f51, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Sqrtss,        new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f51, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Stmxcsr,       new InstructionInfo(0x03000fae, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex));
            Add(X86Instruction.Sub,           new InstructionInfo(0x00000029, 0x05000083, 0x05000081, BadOp,      0x0000002b, InstructionFlags.None));
            Add(X86Instruction.Subpd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Subps,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5c, InstructionFlags.Vex));
            Add(X86Instruction.Subsd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5c, InstructionFlags.Vex | InstructionFlags.PrefixF2));
            Add(X86Instruction.Subss,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f5c, InstructionFlags.Vex | InstructionFlags.PrefixF3));
            Add(X86Instruction.Test,          new InstructionInfo(0x00000085, BadOp,      0x000000f7, BadOp,      BadOp,      InstructionFlags.None));
            Add(X86Instruction.Unpckhpd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f15, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Unpckhps,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f15, InstructionFlags.Vex));
            Add(X86Instruction.Unpcklpd,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f14, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Unpcklps,      new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f14, InstructionFlags.Vex));
            Add(X86Instruction.Vblendvpd,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a4b, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vblendvps,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a4a, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vcvtph2ps,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3813, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vcvtps2ph,     new InstructionInfo(0x000f3a1d, BadOp,      BadOp,      BadOp,      BadOp,      InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfmadd231pd,   new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38b8, InstructionFlags.Vex | InstructionFlags.Prefix66 | InstructionFlags.RexW));
            Add(X86Instruction.Vfmadd231ps,   new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38b8, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfmadd231sd,   new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38b9, InstructionFlags.Vex | InstructionFlags.Prefix66 | InstructionFlags.RexW));
            Add(X86Instruction.Vfmadd231ss,   new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38b9, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfmsub231sd,   new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bb, InstructionFlags.Vex | InstructionFlags.Prefix66 | InstructionFlags.RexW));
            Add(X86Instruction.Vfmsub231ss,   new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bb, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfnmadd231pd,  new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bc, InstructionFlags.Vex | InstructionFlags.Prefix66 | InstructionFlags.RexW));
            Add(X86Instruction.Vfnmadd231ps,  new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bc, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfnmadd231sd,  new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bd, InstructionFlags.Vex | InstructionFlags.Prefix66 | InstructionFlags.RexW));
            Add(X86Instruction.Vfnmadd231ss,  new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bd, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vfnmsub231sd,  new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bf, InstructionFlags.Vex | InstructionFlags.Prefix66 | InstructionFlags.RexW));
            Add(X86Instruction.Vfnmsub231ss,  new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f38bf, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vpblendvb,     new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a4c, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Vpternlogd,    new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x000f3a25, InstructionFlags.Evex | InstructionFlags.Prefix66));
            Add(X86Instruction.Xor,           new InstructionInfo(0x00000031, 0x06000083, 0x06000081, BadOp,      0x00000033, InstructionFlags.None));
            Add(X86Instruction.Xorpd,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f57, InstructionFlags.Vex | InstructionFlags.Prefix66));
            Add(X86Instruction.Xorps,         new InstructionInfo(BadOp,      BadOp,      BadOp,      BadOp,      0x00000f57, InstructionFlags.Vex));
#pragma warning restore IDE0055

            static void Add(X86Instruction inst, in InstructionInfo info)
            {
                _instTable[(int)inst] = info;
            }
        }
    }
}
