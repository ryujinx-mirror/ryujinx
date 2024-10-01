using ARMeilleure.Common;
using ARMeilleure.IntermediateRepresentation;

namespace ARMeilleure.CodeGen.X86
{
    static class IntrinsicTable
    {
        private static readonly IntrinsicInfo[] _intrinTable;

        static IntrinsicTable()
        {
            _intrinTable = new IntrinsicInfo[EnumUtils.GetCount(typeof(Intrinsic))];

#pragma warning disable IDE0055 // Disable formatting
            Add(Intrinsic.X86Addpd,         new IntrinsicInfo(X86Instruction.Addpd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Addps,         new IntrinsicInfo(X86Instruction.Addps,         IntrinsicType.Binary));
            Add(Intrinsic.X86Addsd,         new IntrinsicInfo(X86Instruction.Addsd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Addss,         new IntrinsicInfo(X86Instruction.Addss,         IntrinsicType.Binary));
            Add(Intrinsic.X86Aesdec,        new IntrinsicInfo(X86Instruction.Aesdec,        IntrinsicType.Binary));
            Add(Intrinsic.X86Aesdeclast,    new IntrinsicInfo(X86Instruction.Aesdeclast,    IntrinsicType.Binary));
            Add(Intrinsic.X86Aesenc,        new IntrinsicInfo(X86Instruction.Aesenc,        IntrinsicType.Binary));
            Add(Intrinsic.X86Aesenclast,    new IntrinsicInfo(X86Instruction.Aesenclast,    IntrinsicType.Binary));
            Add(Intrinsic.X86Aesimc,        new IntrinsicInfo(X86Instruction.Aesimc,        IntrinsicType.Unary));
            Add(Intrinsic.X86Andnpd,        new IntrinsicInfo(X86Instruction.Andnpd,        IntrinsicType.Binary));
            Add(Intrinsic.X86Andnps,        new IntrinsicInfo(X86Instruction.Andnps,        IntrinsicType.Binary));
            Add(Intrinsic.X86Andpd,         new IntrinsicInfo(X86Instruction.Andpd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Andps,         new IntrinsicInfo(X86Instruction.Andps,         IntrinsicType.Binary));
            Add(Intrinsic.X86Blendvpd,      new IntrinsicInfo(X86Instruction.Blendvpd,      IntrinsicType.Ternary));
            Add(Intrinsic.X86Blendvps,      new IntrinsicInfo(X86Instruction.Blendvps,      IntrinsicType.Ternary));
            Add(Intrinsic.X86Cmppd,         new IntrinsicInfo(X86Instruction.Cmppd,         IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Cmpps,         new IntrinsicInfo(X86Instruction.Cmpps,         IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Cmpsd,         new IntrinsicInfo(X86Instruction.Cmpsd,         IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Cmpss,         new IntrinsicInfo(X86Instruction.Cmpss,         IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Comisdeq,      new IntrinsicInfo(X86Instruction.Comisd,        IntrinsicType.Comis_));
            Add(Intrinsic.X86Comisdge,      new IntrinsicInfo(X86Instruction.Comisd,        IntrinsicType.Comis_));
            Add(Intrinsic.X86Comisdlt,      new IntrinsicInfo(X86Instruction.Comisd,        IntrinsicType.Comis_));
            Add(Intrinsic.X86Comisseq,      new IntrinsicInfo(X86Instruction.Comiss,        IntrinsicType.Comis_));
            Add(Intrinsic.X86Comissge,      new IntrinsicInfo(X86Instruction.Comiss,        IntrinsicType.Comis_));
            Add(Intrinsic.X86Comisslt,      new IntrinsicInfo(X86Instruction.Comiss,        IntrinsicType.Comis_));
            Add(Intrinsic.X86Crc32,         new IntrinsicInfo(X86Instruction.Crc32,         IntrinsicType.Crc32));
            Add(Intrinsic.X86Crc32_16,      new IntrinsicInfo(X86Instruction.Crc32_16,      IntrinsicType.Crc32));
            Add(Intrinsic.X86Crc32_8,       new IntrinsicInfo(X86Instruction.Crc32_8,       IntrinsicType.Crc32));
            Add(Intrinsic.X86Cvtdq2pd,      new IntrinsicInfo(X86Instruction.Cvtdq2pd,      IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtdq2ps,      new IntrinsicInfo(X86Instruction.Cvtdq2ps,      IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtpd2dq,      new IntrinsicInfo(X86Instruction.Cvtpd2dq,      IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtpd2ps,      new IntrinsicInfo(X86Instruction.Cvtpd2ps,      IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtps2dq,      new IntrinsicInfo(X86Instruction.Cvtps2dq,      IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtps2pd,      new IntrinsicInfo(X86Instruction.Cvtps2pd,      IntrinsicType.Unary));
            Add(Intrinsic.X86Cvtsd2si,      new IntrinsicInfo(X86Instruction.Cvtsd2si,      IntrinsicType.UnaryToGpr));
            Add(Intrinsic.X86Cvtsd2ss,      new IntrinsicInfo(X86Instruction.Cvtsd2ss,      IntrinsicType.Binary));
            Add(Intrinsic.X86Cvtsi2sd,      new IntrinsicInfo(X86Instruction.Cvtsi2sd,      IntrinsicType.BinaryGpr));
            Add(Intrinsic.X86Cvtsi2si,      new IntrinsicInfo(X86Instruction.Movd,          IntrinsicType.UnaryToGpr));
            Add(Intrinsic.X86Cvtsi2ss,      new IntrinsicInfo(X86Instruction.Cvtsi2ss,      IntrinsicType.BinaryGpr));
            Add(Intrinsic.X86Cvtss2sd,      new IntrinsicInfo(X86Instruction.Cvtss2sd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Cvtss2si,      new IntrinsicInfo(X86Instruction.Cvtss2si,      IntrinsicType.UnaryToGpr));
            Add(Intrinsic.X86Divpd,         new IntrinsicInfo(X86Instruction.Divpd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Divps,         new IntrinsicInfo(X86Instruction.Divps,         IntrinsicType.Binary));
            Add(Intrinsic.X86Divsd,         new IntrinsicInfo(X86Instruction.Divsd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Divss,         new IntrinsicInfo(X86Instruction.Divss,         IntrinsicType.Binary));
            Add(Intrinsic.X86Gf2p8affineqb, new IntrinsicInfo(X86Instruction.Gf2p8affineqb, IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Haddpd,        new IntrinsicInfo(X86Instruction.Haddpd,        IntrinsicType.Binary));
            Add(Intrinsic.X86Haddps,        new IntrinsicInfo(X86Instruction.Haddps,        IntrinsicType.Binary));
            Add(Intrinsic.X86Insertps,      new IntrinsicInfo(X86Instruction.Insertps,      IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Ldmxcsr,       new IntrinsicInfo(X86Instruction.None,          IntrinsicType.Mxcsr));
            Add(Intrinsic.X86Maxpd,         new IntrinsicInfo(X86Instruction.Maxpd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Maxps,         new IntrinsicInfo(X86Instruction.Maxps,         IntrinsicType.Binary));
            Add(Intrinsic.X86Maxsd,         new IntrinsicInfo(X86Instruction.Maxsd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Maxss,         new IntrinsicInfo(X86Instruction.Maxss,         IntrinsicType.Binary));
            Add(Intrinsic.X86Minpd,         new IntrinsicInfo(X86Instruction.Minpd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Minps,         new IntrinsicInfo(X86Instruction.Minps,         IntrinsicType.Binary));
            Add(Intrinsic.X86Minsd,         new IntrinsicInfo(X86Instruction.Minsd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Minss,         new IntrinsicInfo(X86Instruction.Minss,         IntrinsicType.Binary));
            Add(Intrinsic.X86Movhlps,       new IntrinsicInfo(X86Instruction.Movhlps,       IntrinsicType.Binary));
            Add(Intrinsic.X86Movlhps,       new IntrinsicInfo(X86Instruction.Movlhps,       IntrinsicType.Binary));
            Add(Intrinsic.X86Movss,         new IntrinsicInfo(X86Instruction.Movss,         IntrinsicType.Binary));
            Add(Intrinsic.X86Mulpd,         new IntrinsicInfo(X86Instruction.Mulpd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Mulps,         new IntrinsicInfo(X86Instruction.Mulps,         IntrinsicType.Binary));
            Add(Intrinsic.X86Mulsd,         new IntrinsicInfo(X86Instruction.Mulsd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Mulss,         new IntrinsicInfo(X86Instruction.Mulss,         IntrinsicType.Binary));
            Add(Intrinsic.X86Paddb,         new IntrinsicInfo(X86Instruction.Paddb,         IntrinsicType.Binary));
            Add(Intrinsic.X86Paddd,         new IntrinsicInfo(X86Instruction.Paddd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Paddq,         new IntrinsicInfo(X86Instruction.Paddq,         IntrinsicType.Binary));
            Add(Intrinsic.X86Paddw,         new IntrinsicInfo(X86Instruction.Paddw,         IntrinsicType.Binary));
            Add(Intrinsic.X86Palignr,       new IntrinsicInfo(X86Instruction.Palignr,       IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Pand,          new IntrinsicInfo(X86Instruction.Pand,          IntrinsicType.Binary));
            Add(Intrinsic.X86Pandn,         new IntrinsicInfo(X86Instruction.Pandn,         IntrinsicType.Binary));
            Add(Intrinsic.X86Pavgb,         new IntrinsicInfo(X86Instruction.Pavgb,         IntrinsicType.Binary));
            Add(Intrinsic.X86Pavgw,         new IntrinsicInfo(X86Instruction.Pavgw,         IntrinsicType.Binary));
            Add(Intrinsic.X86Pblendvb,      new IntrinsicInfo(X86Instruction.Pblendvb,      IntrinsicType.Ternary));
            Add(Intrinsic.X86Pclmulqdq,     new IntrinsicInfo(X86Instruction.Pclmulqdq,     IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Pcmpeqb,       new IntrinsicInfo(X86Instruction.Pcmpeqb,       IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpeqd,       new IntrinsicInfo(X86Instruction.Pcmpeqd,       IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpeqq,       new IntrinsicInfo(X86Instruction.Pcmpeqq,       IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpeqw,       new IntrinsicInfo(X86Instruction.Pcmpeqw,       IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpgtb,       new IntrinsicInfo(X86Instruction.Pcmpgtb,       IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpgtd,       new IntrinsicInfo(X86Instruction.Pcmpgtd,       IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpgtq,       new IntrinsicInfo(X86Instruction.Pcmpgtq,       IntrinsicType.Binary));
            Add(Intrinsic.X86Pcmpgtw,       new IntrinsicInfo(X86Instruction.Pcmpgtw,       IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxsb,        new IntrinsicInfo(X86Instruction.Pmaxsb,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxsd,        new IntrinsicInfo(X86Instruction.Pmaxsd,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxsw,        new IntrinsicInfo(X86Instruction.Pmaxsw,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxub,        new IntrinsicInfo(X86Instruction.Pmaxub,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxud,        new IntrinsicInfo(X86Instruction.Pmaxud,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pmaxuw,        new IntrinsicInfo(X86Instruction.Pmaxuw,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pminsb,        new IntrinsicInfo(X86Instruction.Pminsb,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pminsd,        new IntrinsicInfo(X86Instruction.Pminsd,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pminsw,        new IntrinsicInfo(X86Instruction.Pminsw,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pminub,        new IntrinsicInfo(X86Instruction.Pminub,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pminud,        new IntrinsicInfo(X86Instruction.Pminud,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pminuw,        new IntrinsicInfo(X86Instruction.Pminuw,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pmovsxbw,      new IntrinsicInfo(X86Instruction.Pmovsxbw,      IntrinsicType.Unary));
            Add(Intrinsic.X86Pmovsxdq,      new IntrinsicInfo(X86Instruction.Pmovsxdq,      IntrinsicType.Unary));
            Add(Intrinsic.X86Pmovsxwd,      new IntrinsicInfo(X86Instruction.Pmovsxwd,      IntrinsicType.Unary));
            Add(Intrinsic.X86Pmovzxbw,      new IntrinsicInfo(X86Instruction.Pmovzxbw,      IntrinsicType.Unary));
            Add(Intrinsic.X86Pmovzxdq,      new IntrinsicInfo(X86Instruction.Pmovzxdq,      IntrinsicType.Unary));
            Add(Intrinsic.X86Pmovzxwd,      new IntrinsicInfo(X86Instruction.Pmovzxwd,      IntrinsicType.Unary));
            Add(Intrinsic.X86Pmulld,        new IntrinsicInfo(X86Instruction.Pmulld,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pmullw,        new IntrinsicInfo(X86Instruction.Pmullw,        IntrinsicType.Binary));
            Add(Intrinsic.X86Popcnt,        new IntrinsicInfo(X86Instruction.Popcnt,        IntrinsicType.PopCount));
            Add(Intrinsic.X86Por,           new IntrinsicInfo(X86Instruction.Por,           IntrinsicType.Binary));
            Add(Intrinsic.X86Pshufb,        new IntrinsicInfo(X86Instruction.Pshufb,        IntrinsicType.Binary));
            Add(Intrinsic.X86Pshufd,        new IntrinsicInfo(X86Instruction.Pshufd,        IntrinsicType.BinaryImm));
            Add(Intrinsic.X86Pslld,         new IntrinsicInfo(X86Instruction.Pslld,         IntrinsicType.Binary));
            Add(Intrinsic.X86Pslldq,        new IntrinsicInfo(X86Instruction.Pslldq,        IntrinsicType.Binary));
            Add(Intrinsic.X86Psllq,         new IntrinsicInfo(X86Instruction.Psllq,         IntrinsicType.Binary));
            Add(Intrinsic.X86Psllw,         new IntrinsicInfo(X86Instruction.Psllw,         IntrinsicType.Binary));
            Add(Intrinsic.X86Psrad,         new IntrinsicInfo(X86Instruction.Psrad,         IntrinsicType.Binary));
            Add(Intrinsic.X86Psraw,         new IntrinsicInfo(X86Instruction.Psraw,         IntrinsicType.Binary));
            Add(Intrinsic.X86Psrld,         new IntrinsicInfo(X86Instruction.Psrld,         IntrinsicType.Binary));
            Add(Intrinsic.X86Psrlq,         new IntrinsicInfo(X86Instruction.Psrlq,         IntrinsicType.Binary));
            Add(Intrinsic.X86Psrldq,        new IntrinsicInfo(X86Instruction.Psrldq,        IntrinsicType.Binary));
            Add(Intrinsic.X86Psrlw,         new IntrinsicInfo(X86Instruction.Psrlw,         IntrinsicType.Binary));
            Add(Intrinsic.X86Psubb,         new IntrinsicInfo(X86Instruction.Psubb,         IntrinsicType.Binary));
            Add(Intrinsic.X86Psubd,         new IntrinsicInfo(X86Instruction.Psubd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Psubq,         new IntrinsicInfo(X86Instruction.Psubq,         IntrinsicType.Binary));
            Add(Intrinsic.X86Psubw,         new IntrinsicInfo(X86Instruction.Psubw,         IntrinsicType.Binary));
            Add(Intrinsic.X86Punpckhbw,     new IntrinsicInfo(X86Instruction.Punpckhbw,     IntrinsicType.Binary));
            Add(Intrinsic.X86Punpckhdq,     new IntrinsicInfo(X86Instruction.Punpckhdq,     IntrinsicType.Binary));
            Add(Intrinsic.X86Punpckhqdq,    new IntrinsicInfo(X86Instruction.Punpckhqdq,    IntrinsicType.Binary));
            Add(Intrinsic.X86Punpckhwd,     new IntrinsicInfo(X86Instruction.Punpckhwd,     IntrinsicType.Binary));
            Add(Intrinsic.X86Punpcklbw,     new IntrinsicInfo(X86Instruction.Punpcklbw,     IntrinsicType.Binary));
            Add(Intrinsic.X86Punpckldq,     new IntrinsicInfo(X86Instruction.Punpckldq,     IntrinsicType.Binary));
            Add(Intrinsic.X86Punpcklqdq,    new IntrinsicInfo(X86Instruction.Punpcklqdq,    IntrinsicType.Binary));
            Add(Intrinsic.X86Punpcklwd,     new IntrinsicInfo(X86Instruction.Punpcklwd,     IntrinsicType.Binary));
            Add(Intrinsic.X86Pxor,          new IntrinsicInfo(X86Instruction.Pxor,          IntrinsicType.Binary));
            Add(Intrinsic.X86Rcpps,         new IntrinsicInfo(X86Instruction.Rcpps,         IntrinsicType.Unary));
            Add(Intrinsic.X86Rcpss,         new IntrinsicInfo(X86Instruction.Rcpss,         IntrinsicType.Unary));
            Add(Intrinsic.X86Roundpd,       new IntrinsicInfo(X86Instruction.Roundpd,       IntrinsicType.BinaryImm));
            Add(Intrinsic.X86Roundps,       new IntrinsicInfo(X86Instruction.Roundps,       IntrinsicType.BinaryImm));
            Add(Intrinsic.X86Roundsd,       new IntrinsicInfo(X86Instruction.Roundsd,       IntrinsicType.BinaryImm));
            Add(Intrinsic.X86Roundss,       new IntrinsicInfo(X86Instruction.Roundss,       IntrinsicType.BinaryImm));
            Add(Intrinsic.X86Rsqrtps,       new IntrinsicInfo(X86Instruction.Rsqrtps,       IntrinsicType.Unary));
            Add(Intrinsic.X86Rsqrtss,       new IntrinsicInfo(X86Instruction.Rsqrtss,       IntrinsicType.Unary));
            Add(Intrinsic.X86Sha256Msg1,    new IntrinsicInfo(X86Instruction.Sha256Msg1,    IntrinsicType.Binary));
            Add(Intrinsic.X86Sha256Msg2,    new IntrinsicInfo(X86Instruction.Sha256Msg2,    IntrinsicType.Binary));
            Add(Intrinsic.X86Sha256Rnds2,   new IntrinsicInfo(X86Instruction.Sha256Rnds2,   IntrinsicType.Ternary));
            Add(Intrinsic.X86Shufpd,        new IntrinsicInfo(X86Instruction.Shufpd,        IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Shufps,        new IntrinsicInfo(X86Instruction.Shufps,        IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Sqrtpd,        new IntrinsicInfo(X86Instruction.Sqrtpd,        IntrinsicType.Unary));
            Add(Intrinsic.X86Sqrtps,        new IntrinsicInfo(X86Instruction.Sqrtps,        IntrinsicType.Unary));
            Add(Intrinsic.X86Sqrtsd,        new IntrinsicInfo(X86Instruction.Sqrtsd,        IntrinsicType.Unary));
            Add(Intrinsic.X86Sqrtss,        new IntrinsicInfo(X86Instruction.Sqrtss,        IntrinsicType.Unary));
            Add(Intrinsic.X86Stmxcsr,       new IntrinsicInfo(X86Instruction.None,          IntrinsicType.Mxcsr));
            Add(Intrinsic.X86Subpd,         new IntrinsicInfo(X86Instruction.Subpd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Subps,         new IntrinsicInfo(X86Instruction.Subps,         IntrinsicType.Binary));
            Add(Intrinsic.X86Subsd,         new IntrinsicInfo(X86Instruction.Subsd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Subss,         new IntrinsicInfo(X86Instruction.Subss,         IntrinsicType.Binary));
            Add(Intrinsic.X86Unpckhpd,      new IntrinsicInfo(X86Instruction.Unpckhpd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Unpckhps,      new IntrinsicInfo(X86Instruction.Unpckhps,      IntrinsicType.Binary));
            Add(Intrinsic.X86Unpcklpd,      new IntrinsicInfo(X86Instruction.Unpcklpd,      IntrinsicType.Binary));
            Add(Intrinsic.X86Unpcklps,      new IntrinsicInfo(X86Instruction.Unpcklps,      IntrinsicType.Binary));
            Add(Intrinsic.X86Vcvtph2ps,     new IntrinsicInfo(X86Instruction.Vcvtph2ps,     IntrinsicType.Unary));
            Add(Intrinsic.X86Vcvtps2ph,     new IntrinsicInfo(X86Instruction.Vcvtps2ph,     IntrinsicType.BinaryImm));
            Add(Intrinsic.X86Vfmadd231pd,   new IntrinsicInfo(X86Instruction.Vfmadd231pd,   IntrinsicType.Fma));
            Add(Intrinsic.X86Vfmadd231ps,   new IntrinsicInfo(X86Instruction.Vfmadd231ps,   IntrinsicType.Fma));
            Add(Intrinsic.X86Vfmadd231sd,   new IntrinsicInfo(X86Instruction.Vfmadd231sd,   IntrinsicType.Fma));
            Add(Intrinsic.X86Vfmadd231ss,   new IntrinsicInfo(X86Instruction.Vfmadd231ss,   IntrinsicType.Fma));
            Add(Intrinsic.X86Vfmsub231sd,   new IntrinsicInfo(X86Instruction.Vfmsub231sd,   IntrinsicType.Fma));
            Add(Intrinsic.X86Vfmsub231ss,   new IntrinsicInfo(X86Instruction.Vfmsub231ss,   IntrinsicType.Fma));
            Add(Intrinsic.X86Vfnmadd231pd,  new IntrinsicInfo(X86Instruction.Vfnmadd231pd,  IntrinsicType.Fma));
            Add(Intrinsic.X86Vfnmadd231ps,  new IntrinsicInfo(X86Instruction.Vfnmadd231ps,  IntrinsicType.Fma));
            Add(Intrinsic.X86Vfnmadd231sd,  new IntrinsicInfo(X86Instruction.Vfnmadd231sd,  IntrinsicType.Fma));
            Add(Intrinsic.X86Vfnmadd231ss,  new IntrinsicInfo(X86Instruction.Vfnmadd231ss,  IntrinsicType.Fma));
            Add(Intrinsic.X86Vfnmsub231sd,  new IntrinsicInfo(X86Instruction.Vfnmsub231sd,  IntrinsicType.Fma));
            Add(Intrinsic.X86Vfnmsub231ss,  new IntrinsicInfo(X86Instruction.Vfnmsub231ss,  IntrinsicType.Fma));
            Add(Intrinsic.X86Vpternlogd,    new IntrinsicInfo(X86Instruction.Vpternlogd,    IntrinsicType.TernaryImm));
            Add(Intrinsic.X86Xorpd,         new IntrinsicInfo(X86Instruction.Xorpd,         IntrinsicType.Binary));
            Add(Intrinsic.X86Xorps,         new IntrinsicInfo(X86Instruction.Xorps,         IntrinsicType.Binary));
#pragma warning restore IDE0055
        }

        private static void Add(Intrinsic intrin, IntrinsicInfo info)
        {
            _intrinTable[(int)intrin] = info;
        }

        public static IntrinsicInfo GetInfo(Intrinsic intrin)
        {
            return _intrinTable[(int)intrin];
        }
    }
}
