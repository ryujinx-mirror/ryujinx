using System;

namespace Ryujinx.Graphics.Shader.Decoders
{
    enum AlSize
    {
        _32 = 0,
        _64 = 1,
        _96 = 2,
        _128 = 3,
    }

    enum AtomSize
    {
        U32 = 0,
        S32 = 1,
        U64 = 2,
        F32FtzRn = 3,
        F16x2FtzRn = 4,
        S64 = 5,
    }

    enum AtomOp
    {
        Add = 0,
        Min = 1,
        Max = 2,
        Inc = 3,
        Dec = 4,
        And = 5,
        Or = 6,
        Xor = 7,
        Exch = 8,
        Safeadd = 10,
    }

    enum AtomsSize
    {
        U32 = 0,
        S32 = 1,
        U64 = 2,
        S64 = 3,
    }

    enum BarMode
    {
        Bar = 0,
        Result = 1,
        Warp = 2,
    }

    enum BarOp
    {
        Sync = 0,
        Arv = 1,
        Red = 2,
        Scan = 3,
        SyncAll = 4,
    }

    enum BarRedOp
    {
        Popc = 0,
        And = 1,
        Or = 2,
    }

    enum Bpt
    {
        DrainIllegal = 0,
        Cal = 1,
        Pause = 2,
        Trap = 3,
        Int = 4,
        Drain = 5,
    }

    enum Ccc
    {
        F = 0,
        Lt = 1,
        Eq = 2,
        Le = 3,
        Gt = 4,
        Ne = 5,
        Ge = 6,
        Num = 7,
        Nan = 8,
        Ltu = 9,
        Equ = 10,
        Leu = 11,
        Gtu = 12,
        Neu = 13,
        Geu = 14,
        T = 15,
        Off = 16,
        Lo = 17,
        Sff = 18,
        Ls = 19,
        Hi = 20,
        Sft = 21,
        Hs = 22,
        Oft = 23,
        CsmTa = 24,
        CsmTr = 25,
        CsmMx = 26,
        FcsmTa = 27,
        FcsmTr = 28,
        FcsmMx = 29,
        Rle = 30,
        Rgt = 31,
    }

    enum CacheType
    {
        U = 1,
        C = 2,
        I = 3,
        Crs = 4,
    }

    enum CctlOp
    {
        Pf1 = 1,
        Pf1_5 = 2,
        Pf2 = 3,
        Wb = 4,
        Iv = 5,
        Ivall = 6,
        Rs = 7,
        Rslb = 9,
    }

    enum CctltOp
    {
        Ivth = 1,
    }

    enum BoolOp
    {
        And = 0,
        Or = 1,
        Xor = 2,
    }

    enum SReg
    {
        LaneId = 0,
        Clock = 1,
        VirtCfg = 2,
        VirtId = 3,
        Pm0 = 4,
        Pm1 = 5,
        Pm2 = 6,
        Pm3 = 7,
        Pm4 = 8,
        Pm5 = 9,
        Pm6 = 10,
        Pm7 = 11,
        OrderingTicket = 15,
        PrimType = 16,
        InvocationId = 17,
        YDirection = 18,
        ThreadKill = 19,
        ShaderType = 20,
        DirectCbeWriteAddressLow = 21,
        DirectCbeWriteAddressHigh = 22,
        DirectCbeWriteEnabled = 23,
        MachineId0 = 24,
        MachineId1 = 25,
        MachineId2 = 26,
        MachineId3 = 27,
        Affinity = 28,
        InvocationInfo = 29,
        WScaleFactorXY = 30,
        WScaleFactorZ = 31,
        TId = 32,
        TIdX = 33,
        TIdY = 34,
        TIdZ = 35,
        CtaParam = 36,
        CtaIdX = 37,
        CtaIdY = 38,
        CtaIdZ = 39,
        Ntid = 40,
        CirQueueIncrMinusOne = 41,
        Nlatc = 42,
        Swinlo = 48,
        Swinsz = 49,
        Smemsz = 50,
        Smembanks = 51,
        LWinLo = 52,
        LWinSz = 53,
        LMemLoSz = 54,
        LMemHiOff = 55,
        EqMask = 56,
        LtMask = 57,
        LeMask = 58,
        GtMask = 59,
        GeMask = 60,
        RegAlloc = 61,
        CtxAddr = 62,
        GlobalErrorStatus = 64,
        WarpErrorStatus = 66,
        WarpErrorStatusClear = 67,
        PmHi0 = 72,
        PmHi1 = 73,
        PmHi2 = 74,
        PmHi3 = 75,
        PmHi4 = 76,
        PmHi5 = 77,
        PmHi6 = 78,
        PmHi7 = 79,
        ClockLo = 80,
        ClockHi = 81,
        GlobalTimerLo = 82,
        GlobalTimerHi = 83,
        HwTaskId = 96,
        CircularQueueEntryIndex = 97,
        CircularQueueEntryAddressLow = 98,
        CircularQueueEntryAddressHigh = 99,
    }

    enum RoundMode
    {
        Rn = 0,
        Rm = 1,
        Rp = 2,
        Rz = 3,
    }

    enum FComp
    {
        F = 0,
        Lt = 1,
        Eq = 2,
        Le = 3,
        Gt = 4,
        Ne = 5,
        Ge = 6,
        Num = 7,
        Nan = 8,
        Ltu = 9,
        Equ = 10,
        Leu = 11,
        Gtu = 12,
        Neu = 13,
        Geu = 14,
        T = 15,
    }

    enum IntegerRound
    {
        Pass = 1,
        Round = 4,
        Floor = 5,
        Ceil = 6,
        Trunc = 7,
    }

    enum IDstFmt
    {
        U16 = 1,
        U32 = 2,
        U64 = 3,
        S16 = 5,
        S32 = 6,
        S64 = 7,
    }

    enum ISrcFmt
    {
        U8 = 0,
        U16 = 1,
        U32 = 2,
        U64 = 3,
        S8 = 4,
        S16 = 5,
        S32 = 6,
        S64 = 7,
    }

    enum ISrcDstFmt
    {
        U8 = 0,
        U16 = 1,
        U32 = 2,
        S8 = 4,
        S16 = 5,
        S32 = 6,
    }

    enum RoundMode2
    {
        Round = 0,
        Floor = 1,
        Ceil = 2,
        Trunc = 3,
    }

    enum ChkModeF
    {
        Divide = 0,
    }

    enum Fmz
    {
        Ftz = 1,
        Fmz = 2,
    }

    enum MultiplyScale
    {
        NoScale = 0,
        D2 = 1,
        D4 = 2,
        D8 = 3,
        M8 = 4,
        M4 = 5,
        M2 = 6,
    }

    enum OFmt
    {
        F16 = 0,
        F32 = 1,
        MrgH0 = 2,
        MrgH1 = 3,
    }

    enum HalfSwizzle
    {
        F16 = 0,
        F32 = 1,
        H0H0 = 2,
        H1H1 = 3,
    }

    enum ByteSel
    {
        B0 = 0,
        B1 = 1,
        B2 = 2,
        B3 = 3,
    }

    enum DstFmt
    {
        F16 = 1,
        F32 = 2,
        F64 = 3,
    }

    enum AvgMode
    {
        NoNeg = 0,
        NegB = 1,
        NegA = 2,
        PlusOne = 3,
    }

    enum Lrs
    {
        None = 0,
        RightShift = 1,
        LeftShift = 2,
    }

    enum HalfSelect
    {
        B32 = 0,
        H0 = 1,
        H1 = 2,
    }

    enum IComp
    {
        F = 0,
        Lt = 1,
        Eq = 2,
        Le = 3,
        Gt = 4,
        Ne = 5,
        Ge = 6,
        T = 7,
    }

    enum XMode
    {
        Xlo = 1,
        Xmed = 2,
        Xhi = 3,
    }

    enum IpaOp
    {
        Pass = 0,
        Multiply = 1,
        Constant = 2,
        Sc = 3,
    }

    enum IBase
    {
        Patch = 1,
        Prim = 2,
        Attr = 3,
    }

    enum CacheOpLd
    {
        Ca = 0,
        Cg = 1,
        Ci = 2,
        Cv = 3,
    }

    enum CacheOpSt
    {
        Wb = 0,
        Cg = 1,
        Ci = 2,
        Wt = 3,
    }

    enum LsSize
    {
        U8 = 0,
        S8 = 1,
        U16 = 2,
        S16 = 3,
        B32 = 4,
        B64 = 5,
        B128 = 6,
        UB128 = 7,
    }

    enum LsSize2
    {
        U8 = 0,
        S8 = 1,
        U16 = 2,
        S16 = 3,
        B32 = 4,
        B64 = 5,
        B128 = 6,
    }

    enum AddressMode
    {
        Il = 1,
        Is = 2,
        Isl = 3,
    }

    enum CacheOp2
    {
        Lu = 1,
        Ci = 2,
        Cv = 3,
    }

    enum PredicateOp
    {
        F = 0,
        T = 1,
        Z = 2,
        Nz = 3,
    }

    enum LogicOp
    {
        And = 0,
        Or = 1,
        Xor = 2,
        PassB = 3,
    }

    enum Membar
    {
        Cta = 0,
        Gl = 1,
        Sys = 2,
        Vc = 3,
    }

    enum Ivall
    {
        Ivalld = 1,
        Ivallt = 2,
        Ivalltd = 3,
    }

    enum MufuOp
    {
        Cos = 0,
        Sin = 1,
        Ex2 = 2,
        Lg2 = 3,
        Rcp = 4,
        Rsq = 5,
        Rcp64h = 6,
        Rsq64h = 7,
        Sqrt = 8,
    }

    enum OutType
    {
        Emit = 1,
        Cut = 2,
        EmitThenCut = 3,
    }

    enum PixMode
    {
        Covmask = 1,
        Covered = 2,
        Offset = 3,
        CentroidOffset = 4,
        MyIndex = 5,
    }

    enum PMode
    {
        F4e = 1,
        B4e = 2,
        Rc8 = 3,
        Ecl = 4,
        Ecr = 5,
        Rc16 = 6,
    }

    enum RedOp
    {
        Add = 0,
        Min = 1,
        Max = 2,
        Inc = 3,
        Dec = 4,
        And = 5,
        Or = 6,
        Xor = 7,
    }

    enum XModeShf
    {
        Hi = 1,
        X = 2,
        Xhi = 3,
    }

    enum MaxShift
    {
        U64 = 2,
        S64 = 3,
    }

    enum ShflMode
    {
        Idx = 0,
        Up = 1,
        Down = 2,
        Bfly = 3,
    }

    enum Clamp
    {
        Ign = 0,
        Trap = 2,
    }

    enum SuatomSize
    {
        U32 = 0,
        S32 = 1,
        U64 = 2,
        F32FtzRn = 3,
        F16x2FtzRn = 4,
        S64 = 5,
        Sd32 = 6,
        Sd64 = 7,
    }

    enum SuDim
    {
        _1d = 0,
        _1dBuffer = 1,
        _1dArray = 2,
        _2d = 3,
        _2dArray = 4,
        _3d = 5,
    }

    enum SuatomOp
    {
        Add = 0,
        Min = 1,
        Max = 2,
        Inc = 3,
        Dec = 4,
        And = 5,
        Or = 6,
        Xor = 7,
        Exch = 8,
    }

    enum SuSize
    {
        U8 = 0,
        S8 = 1,
        U16 = 2,
        S16 = 3,
        B32 = 4,
        B64 = 5,
        B128 = 6,
        UB128 = 7,
    }

    enum SuRgba
    {
        R = 1,
        G = 2,
        Rg = 3,
        B = 4,
        Rb = 5,
        Gb = 6,
        Rgb = 7,
        A = 8,
        Ra = 9,
        Ga = 10,
        Rga = 11,
        Ba = 12,
        Rba = 13,
        Gba = 14,
        Rgba = 15,
    }

    enum Lod
    {
        Lz = 1,
        Lb = 2,
        Ll = 3,
        Lba = 6,
        Lla = 7,
    }

    enum TexDim
    {
        _1d = 0,
        Array1d = 1,
        _2d = 2,
        Array2d = 3,
        _3d = 4,
        Array3d = 5,
        Cube = 6,
        ArrayCube = 7,
    }

    enum TexsTarget
    {
        Texture1DLodZero = 0,
        Texture2D = 1,
        Texture2DLodZero = 2,
        Texture2DLodLevel = 3,
        Texture2DDepthCompare = 4,
        Texture2DLodLevelDepthCompare = 5,
        Texture2DLodZeroDepthCompare = 6,
        Texture2DArray = 7,
        Texture2DArrayLodZero = 8,
        Texture2DArrayLodZeroDepthCompare = 9,
        Texture3D = 10,
        Texture3DLodZero = 11,
        TextureCube = 12,
        TextureCubeLodLevel = 13,
    }

    enum TldsTarget
    {
        Texture1DLodZero = 0x0,
        Texture1DLodLevel = 0x1,
        Texture2DLodZero = 0x2,
        Texture2DLodZeroOffset = 0x4,
        Texture2DLodLevel = 0x5,
        Texture2DLodZeroMultisample = 0x6,
        Texture3DLodZero = 0x7,
        Texture2DArrayLodZero = 0x8,
        Texture2DLodLevelOffset = 0xc,
    }

    enum TexComp
    {
        R = 0,
        G = 1,
        B = 2,
        A = 3,
    }

    enum TexOffset
    {
        None = 0,
        Aoffi = 1,
        Ptp = 2,
    }

    enum TexQuery
    {
        TexHeaderDimension = 1,
        TexHeaderTextureType = 2,
        TexHeaderSamplerPos = 5,
        TexSamplerFilter = 16,
        TexSamplerLod = 18,
        TexSamplerWrap = 20,
        TexSamplerBorderColor = 22,
    }

    [Flags]
    enum VectorSelect
    {
        U8B0 = 0,
        U8B1 = 1,
        U8B2 = 2,
        U8B3 = 3,
        U16H0 = 4,
        U16H1 = 5,
        U32 = 6,
        S8B0 = 8,
        S8B1 = 9,
        S8B2 = 10,
        S8B3 = 11,
        S16H0 = 12,
        S16H1 = 13,
        S32 = 14,
    }

    enum VideoOp
    {
        Mrg16h = 0,
        Mrg16l = 1,
        Mrg8b0 = 2,
        Mrg8b2 = 3,
        Acc = 4,
        Min = 5,
        Max = 6,
    }

    enum VideoRed
    {
        Acc = 1,
    }

    enum LaneMask4
    {
        Z = 1,
        W = 2,
        Zw = 3,
        X = 4,
        Xz = 5,
        Xw = 6,
        Xzw = 7,
        Y = 8,
        Yz = 9,
        Yw = 10,
        Yzw = 11,
        Xy = 12,
        Xyz = 13,
        Xyw = 14,
        Xyzw = 15,
    }

    enum ASelect4
    {
        _0000 = 0,
        _1111 = 1,
        _2222 = 2,
        _3333 = 3,
        _3210 = 4,
        _5432 = 6,
        _6543 = 7,
        _3201 = 8,
        _3012 = 9,
        _0213 = 10,
        _3120 = 11,
        _1230 = 12,
        _2310 = 13,
    }

    enum BSelect4
    {
        _4444 = 0,
        _5555 = 1,
        _6666 = 2,
        _7777 = 3,
        _7654 = 4,
        _5432 = 6,
        _4321 = 7,
        _4567 = 8,
        _6745 = 9,
        _5476 = 10,
    }

    enum VideoScale
    {
        Shr7 = 1,
        Shr15 = 2,
    }

    enum VoteMode
    {
        All = 0,
        Any = 1,
        Eq = 2,
    }

    enum XmadCop
    {
        Cfull = 0,
        Clo = 1,
        Chi = 2,
        Csfu = 3,
        Cbcc = 4,
    }

    enum XmadCop2
    {
        Cfull = 0,
        Clo = 1,
        Chi = 2,
        Csfu = 3,
    }

    enum ImadspASelect
    {
        U32 = 0,
        S32 = 1,
        U24 = 2,
        S24 = 3,
        U16h0 = 4,
        S16h0 = 5,
        U16h1 = 6,
        S16h1 = 7,
    }

    enum ImadspBSelect
    {
        U24 = 0,
        S24 = 1,
        U16h0 = 2,
        S16h0 = 3,
    }

    readonly struct InstConditional
    {
        private readonly ulong _opcode;
        public InstConditional(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
    }

    readonly struct InstAl2p
    {
        private readonly ulong _opcode;
        public InstAl2p(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public AlSize AlSize => (AlSize)((_opcode >> 47) & 0x3);
        public bool Aio => (_opcode & 0x100000000) != 0;
        public int Imm11 => (int)((_opcode >> 20) & 0x7FF);
        public int DestPred => (int)((_opcode >> 44) & 0x7);
    }

    readonly struct InstAld
    {
        private readonly ulong _opcode;
        public InstAld(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm11 => (int)((_opcode >> 20) & 0x7FF);
        public bool P => (_opcode & 0x80000000) != 0;
        public bool O => (_opcode & 0x100000000) != 0;
        public AlSize AlSize => (AlSize)((_opcode >> 47) & 0x3);
        public bool Phys => !P && Imm11 == 0 && SrcA != RegisterConsts.RegisterZeroIndex;
    }

    readonly struct InstAst
    {
        private readonly ulong _opcode;
        public InstAst(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)(_opcode & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm11 => (int)((_opcode >> 20) & 0x7FF);
        public bool P => (_opcode & 0x80000000) != 0;
        public AlSize AlSize => (AlSize)((_opcode >> 47) & 0x3);
        public bool Phys => !P && Imm11 == 0 && SrcA != RegisterConsts.RegisterZeroIndex;
    }

    readonly struct InstAtom
    {
        private readonly ulong _opcode;
        public InstAtom(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm20 => (int)((_opcode >> 28) & 0xFFFFF);
        public AtomSize Size => (AtomSize)((_opcode >> 49) & 0x7);
        public AtomOp Op => (AtomOp)((_opcode >> 52) & 0xF);
        public bool E => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstAtomCas
    {
        private readonly ulong _opcode;
        public InstAtomCas(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int BcRz => (int)((_opcode >> 50) & 0x3);
        public bool E => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstAtoms
    {
        private readonly ulong _opcode;
        public InstAtoms(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm22 => (int)((_opcode >> 30) & 0x3FFFFF);
        public AtomsSize AtomsSize => (AtomsSize)((_opcode >> 28) & 0x3);
        public AtomOp AtomOp => (AtomOp)((_opcode >> 52) & 0xF);
    }

    readonly struct InstAtomsCas
    {
        private readonly ulong _opcode;
        public InstAtomsCas(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int AtomsBcRz => (int)((_opcode >> 28) & 0x3);
    }

    readonly struct InstB2r
    {
        private readonly ulong _opcode;
        public InstB2r(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int DestPred => (int)((_opcode >> 45) & 0x7);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public BarMode Mode => (BarMode)((_opcode >> 32) & 0x3);
    }

    readonly struct InstBar
    {
        private readonly ulong _opcode;
        public InstBar(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm12 => (int)((_opcode >> 20) & 0xFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public BarOp BarOp => (BarOp)((_opcode >> 32) & 0x7);
        public BarRedOp BarRedOp => (BarRedOp)((_opcode >> 35) & 0x3);
        public bool AFixBar => (_opcode & 0x100000000000) != 0;
        public bool BFixBar => (_opcode & 0x80000000000) != 0;
    }

    readonly struct InstBfeR
    {
        private readonly ulong _opcode;
        public InstBfeR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Brev => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstBfeI
    {
        private readonly ulong _opcode;
        public InstBfeI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Brev => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstBfeC
    {
        private readonly ulong _opcode;
        public InstBfeC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Brev => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstBfiR
    {
        private readonly ulong _opcode;
        public InstBfiR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstBfiI
    {
        private readonly ulong _opcode;
        public InstBfiI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstBfiC
    {
        private readonly ulong _opcode;
        public InstBfiC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstBfiRc
    {
        private readonly ulong _opcode;
        public InstBfiRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstBpt
    {
        private readonly ulong _opcode;
        public InstBpt(ulong opcode) => _opcode = opcode;
        public int Imm20 => (int)((_opcode >> 20) & 0xFFFFF);
        public Bpt Bpt => (Bpt)((_opcode >> 6) & 0x7);
    }

    readonly struct InstBra
    {
        private readonly ulong _opcode;
        public InstBra(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
        public bool Lmt => (_opcode & 0x40) != 0;
        public bool U => (_opcode & 0x80) != 0;
    }

    readonly struct InstBrk
    {
        private readonly ulong _opcode;
        public InstBrk(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
    }

    readonly struct InstBrx
    {
        private readonly ulong _opcode;
        public InstBrx(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
        public bool Lmt => (_opcode & 0x40) != 0;
    }

    readonly struct InstCal
    {
        private readonly ulong _opcode;
        public InstCal(ulong opcode) => _opcode = opcode;
        public bool Ca => (_opcode & 0x20) != 0;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Inc => (_opcode & 0x40) != 0;
    }

    readonly struct InstCctl
    {
        private readonly ulong _opcode;
        public InstCctl(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm30 => (int)((_opcode >> 22) & 0x3FFFFFFF);
        public bool E => (_opcode & 0x10000000000000) != 0;
        public CacheType Cache => (CacheType)((_opcode >> 4) & 0x7);
        public CctlOp CctlOp => (CctlOp)(_opcode & 0xF);
    }

    readonly struct InstCctll
    {
        private readonly ulong _opcode;
        public InstCctll(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm22 => (int)((_opcode >> 22) & 0x3FFFFF);
        public int Cache => (int)((_opcode >> 4) & 0x3);
        public CctlOp CctlOp => (CctlOp)(_opcode & 0xF);
    }

    readonly struct InstCctlt
    {
        private readonly ulong _opcode;
        public InstCctlt(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int TsIdx13 => (int)((_opcode >> 36) & 0x1FFF);
        public CctltOp CctltOp => (CctltOp)(_opcode & 0x3);
    }

    readonly struct InstCctltR
    {
        private readonly ulong _opcode;
        public InstCctltR(ulong opcode) => _opcode = opcode;
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public CctltOp CctltOp => (CctltOp)(_opcode & 0x3);
    }

    readonly struct InstCont
    {
        private readonly ulong _opcode;
        public InstCont(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
    }

    readonly struct InstCset
    {
        private readonly ulong _opcode;
        public InstCset(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 8) & 0x1F);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public bool BVal => (_opcode & 0x100000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
    }

    readonly struct InstCsetp
    {
        private readonly ulong _opcode;
        public InstCsetp(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 8) & 0x1F);
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
    }

    readonly struct InstCs2r
    {
        private readonly ulong _opcode;
        public InstCs2r(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public SReg SReg => (SReg)((_opcode >> 20) & 0xFF);
    }

    readonly struct InstDaddR
    {
        private readonly ulong _opcode;
        public InstDaddR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
    }

    readonly struct InstDaddI
    {
        private readonly ulong _opcode;
        public InstDaddI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
    }

    readonly struct InstDaddC
    {
        private readonly ulong _opcode;
        public InstDaddC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
    }

    readonly struct InstDepbar
    {
        private readonly ulong _opcode;
        public InstDepbar(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Le => (_opcode & 0x20000000) != 0;
        public int Sbid => (int)((_opcode >> 26) & 0x7);
        public int PendCnt => (int)((_opcode >> 20) & 0x3F);
        public int Imm6 => (int)(_opcode & 0x3F);
    }

    readonly struct InstDfmaR
    {
        private readonly ulong _opcode;
        public InstDfmaR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 50) & 0x3);
        public bool NegC => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstDfmaI
    {
        private readonly ulong _opcode;
        public InstDfmaI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 50) & 0x3);
        public bool NegC => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstDfmaC
    {
        private readonly ulong _opcode;
        public InstDfmaC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 50) & 0x3);
        public bool NegC => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstDfmaRc
    {
        private readonly ulong _opcode;
        public InstDfmaRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 50) & 0x3);
        public bool NegC => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstDmnmxR
    {
        private readonly ulong _opcode;
        public InstDmnmxR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstDmnmxI
    {
        private readonly ulong _opcode;
        public InstDmnmxI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstDmnmxC
    {
        private readonly ulong _opcode;
        public InstDmnmxC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstDmulR
    {
        private readonly ulong _opcode;
        public InstDmulR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public bool NegA => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstDmulI
    {
        private readonly ulong _opcode;
        public InstDmulI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public bool NegA => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstDmulC
    {
        private readonly ulong _opcode;
        public InstDmulC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public bool NegA => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstDsetR
    {
        private readonly ulong _opcode;
        public InstDsetR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsA => (_opcode & 0x40000000000000) != 0;
        public bool NegB => (_opcode & 0x20000000000000) != 0;
        public bool BVal => (_opcode & 0x10000000000000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstDsetI
    {
        private readonly ulong _opcode;
        public InstDsetI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsA => (_opcode & 0x40000000000000) != 0;
        public bool NegB => (_opcode & 0x20000000000000) != 0;
        public bool BVal => (_opcode & 0x10000000000000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstDsetC
    {
        private readonly ulong _opcode;
        public InstDsetC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsA => (_opcode & 0x40000000000000) != 0;
        public bool NegB => (_opcode & 0x20000000000000) != 0;
        public bool BVal => (_opcode & 0x10000000000000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstDsetpR
    {
        private readonly ulong _opcode;
        public InstDsetpR(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool AbsA => (_opcode & 0x80) != 0;
        public bool NegB => (_opcode & 0x40) != 0;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
    }

    readonly struct InstDsetpI
    {
        private readonly ulong _opcode;
        public InstDsetpI(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool AbsA => (_opcode & 0x80) != 0;
        public bool NegB => (_opcode & 0x40) != 0;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
    }

    readonly struct InstDsetpC
    {
        private readonly ulong _opcode;
        public InstDsetpC(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool AbsA => (_opcode & 0x80) != 0;
        public bool NegB => (_opcode & 0x40) != 0;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
    }

    readonly struct InstExit
    {
        private readonly ulong _opcode;
        public InstExit(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
        public bool KeepRefCnt => (_opcode & 0x20) != 0;
    }

    readonly struct InstF2fR
    {
        private readonly ulong _opcode;
        public InstF2fR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public DstFmt DstFmt => (DstFmt)((_opcode >> 8) & 0x3);
        public DstFmt SrcFmt => (DstFmt)((_opcode >> 10) & 0x3);
        public IntegerRound RoundMode => (IntegerRound)((int)((_opcode >> 40) & 0x4) | (int)((_opcode >> 39) & 0x3));
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstF2fI
    {
        private readonly ulong _opcode;
        public InstF2fI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public DstFmt DstFmt => (DstFmt)((_opcode >> 8) & 0x3);
        public DstFmt SrcFmt => (DstFmt)((_opcode >> 10) & 0x3);
        public IntegerRound RoundMode => (IntegerRound)((int)((_opcode >> 40) & 0x4) | (int)((_opcode >> 39) & 0x3));
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstF2fC
    {
        private readonly ulong _opcode;
        public InstF2fC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public DstFmt DstFmt => (DstFmt)((_opcode >> 8) & 0x3);
        public DstFmt SrcFmt => (DstFmt)((_opcode >> 10) & 0x3);
        public IntegerRound RoundMode => (IntegerRound)((int)((_opcode >> 40) & 0x4) | (int)((_opcode >> 39) & 0x3));
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstF2iR
    {
        private readonly ulong _opcode;
        public InstF2iR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public IDstFmt IDstFmt => (IDstFmt)((int)((_opcode >> 10) & 0x4) | (int)((_opcode >> 8) & 0x3));
        public DstFmt SrcFmt => (DstFmt)((_opcode >> 10) & 0x3);
        public RoundMode2 RoundMode => (RoundMode2)((_opcode >> 39) & 0x3);
    }

    readonly struct InstF2iI
    {
        private readonly ulong _opcode;
        public InstF2iI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public IDstFmt IDstFmt => (IDstFmt)((int)((_opcode >> 10) & 0x4) | (int)((_opcode >> 8) & 0x3));
        public DstFmt SrcFmt => (DstFmt)((_opcode >> 10) & 0x3);
        public RoundMode2 RoundMode => (RoundMode2)((_opcode >> 39) & 0x3);
    }

    readonly struct InstF2iC
    {
        private readonly ulong _opcode;
        public InstF2iC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public IDstFmt IDstFmt => (IDstFmt)((int)((_opcode >> 10) & 0x4) | (int)((_opcode >> 8) & 0x3));
        public DstFmt SrcFmt => (DstFmt)((_opcode >> 10) & 0x3);
        public RoundMode2 RoundMode => (RoundMode2)((_opcode >> 39) & 0x3);
    }

    readonly struct InstFaddR
    {
        private readonly ulong _opcode;
        public InstFaddR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
    }

    readonly struct InstFaddI
    {
        private readonly ulong _opcode;
        public InstFaddI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
    }

    readonly struct InstFaddC
    {
        private readonly ulong _opcode;
        public InstFaddC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
    }

    readonly struct InstFadd32i
    {
        private readonly ulong _opcode;
        public InstFadd32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool AbsB => (_opcode & 0x200000000000000) != 0;
        public bool NegA => (_opcode & 0x100000000000000) != 0;
        public bool Ftz => (_opcode & 0x80000000000000) != 0;
        public bool AbsA => (_opcode & 0x40000000000000) != 0;
        public bool NegB => (_opcode & 0x20000000000000) != 0;
    }

    readonly struct InstFchkR
    {
        private readonly ulong _opcode;
        public InstFchkR(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public ChkModeF ChkModeF => (ChkModeF)((_opcode >> 39) & 0x3F);
    }

    readonly struct InstFchkI
    {
        private readonly ulong _opcode;
        public InstFchkI(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public ChkModeF ChkModeF => (ChkModeF)((_opcode >> 39) & 0x3F);
    }

    readonly struct InstFchkC
    {
        private readonly ulong _opcode;
        public InstFchkC(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public ChkModeF ChkModeF => (ChkModeF)((_opcode >> 39) & 0x3F);
    }

    readonly struct InstFcmpR
    {
        private readonly ulong _opcode;
        public InstFcmpR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public bool Ftz => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstFcmpI
    {
        private readonly ulong _opcode;
        public InstFcmpI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public bool Ftz => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstFcmpC
    {
        private readonly ulong _opcode;
        public InstFcmpC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public bool Ftz => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstFcmpRc
    {
        private readonly ulong _opcode;
        public InstFcmpRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public bool Ftz => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstFfmaR
    {
        private readonly ulong _opcode;
        public InstFfmaR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool NegC => (_opcode & 0x2000000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 51) & 0x3);
        public Fmz Fmz => (Fmz)((_opcode >> 53) & 0x3);
    }

    readonly struct InstFfmaI
    {
        private readonly ulong _opcode;
        public InstFfmaI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool NegC => (_opcode & 0x2000000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 51) & 0x3);
        public Fmz Fmz => (Fmz)((_opcode >> 53) & 0x3);
    }

    readonly struct InstFfmaC
    {
        private readonly ulong _opcode;
        public InstFfmaC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool NegC => (_opcode & 0x2000000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 51) & 0x3);
        public Fmz Fmz => (Fmz)((_opcode >> 53) & 0x3);
    }

    readonly struct InstFfmaRc
    {
        private readonly ulong _opcode;
        public InstFfmaRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool NegC => (_opcode & 0x2000000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 51) & 0x3);
        public Fmz Fmz => (Fmz)((_opcode >> 53) & 0x3);
    }

    readonly struct InstFfma32i
    {
        private readonly ulong _opcode;
        public InstFfma32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm32 => (int)(_opcode >> 20);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegC => (_opcode & 0x200000000000000) != 0;
        public bool NegA => (_opcode & 0x100000000000000) != 0;
        public bool Sat => (_opcode & 0x80000000000000) != 0;
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 53) & 0x3);
    }

    readonly struct InstFloR
    {
        private readonly ulong _opcode;
        public InstFloR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstFloI
    {
        private readonly ulong _opcode;
        public InstFloI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstFloC
    {
        private readonly ulong _opcode;
        public InstFloC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstFmnmxR
    {
        private readonly ulong _opcode;
        public InstFmnmxR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstFmnmxI
    {
        private readonly ulong _opcode;
        public InstFmnmxI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstFmnmxC
    {
        private readonly ulong _opcode;
        public InstFmnmxC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstFmulR
    {
        private readonly ulong _opcode;
        public InstFmulR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public Fmz Fmz => (Fmz)((_opcode >> 44) & 0x3);
        public MultiplyScale Scale => (MultiplyScale)((_opcode >> 41) & 0x7);
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstFmulI
    {
        private readonly ulong _opcode;
        public InstFmulI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public Fmz Fmz => (Fmz)((_opcode >> 44) & 0x3);
        public MultiplyScale Scale => (MultiplyScale)((_opcode >> 41) & 0x7);
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstFmulC
    {
        private readonly ulong _opcode;
        public InstFmulC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public Fmz Fmz => (Fmz)((_opcode >> 44) & 0x3);
        public MultiplyScale Scale => (MultiplyScale)((_opcode >> 41) & 0x7);
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstFmul32i
    {
        private readonly ulong _opcode;
        public InstFmul32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool Sat => (_opcode & 0x80000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 53) & 0x3);
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
    }

    readonly struct InstFsetR
    {
        private readonly ulong _opcode;
        public InstFsetR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool NegB => (_opcode & 0x20000000000000) != 0;
        public bool AbsA => (_opcode & 0x40000000000000) != 0;
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool Ftz => (_opcode & 0x80000000000000) != 0;
        public bool BVal => (_opcode & 0x10000000000000) != 0;
    }

    readonly struct InstFsetC
    {
        private readonly ulong _opcode;
        public InstFsetC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool NegB => (_opcode & 0x20000000000000) != 0;
        public bool AbsA => (_opcode & 0x40000000000000) != 0;
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool Ftz => (_opcode & 0x80000000000000) != 0;
        public bool BVal => (_opcode & 0x10000000000000) != 0;
    }

    readonly struct InstFsetI
    {
        private readonly ulong _opcode;
        public InstFsetI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool NegB => (_opcode & 0x20000000000000) != 0;
        public bool AbsA => (_opcode & 0x40000000000000) != 0;
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool Ftz => (_opcode & 0x80000000000000) != 0;
        public bool BVal => (_opcode & 0x10000000000000) != 0;
    }

    readonly struct InstFsetpR
    {
        private readonly ulong _opcode;
        public InstFsetpR(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool NegB => (_opcode & 0x40) != 0;
        public bool AbsA => (_opcode & 0x80) != 0;
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool Ftz => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstFsetpI
    {
        private readonly ulong _opcode;
        public InstFsetpI(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool NegB => (_opcode & 0x40) != 0;
        public bool AbsA => (_opcode & 0x80) != 0;
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool Ftz => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstFsetpC
    {
        private readonly ulong _opcode;
        public InstFsetpC(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool NegB => (_opcode & 0x40) != 0;
        public bool AbsA => (_opcode & 0x80) != 0;
        public bool AbsB => (_opcode & 0x100000000000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool Ftz => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstFswzadd
    {
        private readonly ulong _opcode;
        public InstFswzadd(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Ftz => (_opcode & 0x100000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public bool Ndv => (_opcode & 0x4000000000) != 0;
        public int PnWord => (int)((_opcode >> 28) & 0xFF);
    }

    readonly struct InstGetcrsptr
    {
        private readonly ulong _opcode;
        public InstGetcrsptr(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
    }

    readonly struct InstGetlmembase
    {
        private readonly ulong _opcode;
        public InstGetlmembase(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
    }

    readonly struct InstHadd2R
    {
        private readonly ulong _opcode;
        public InstHadd2R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public OFmt OFmt => (OFmt)((_opcode >> 49) & 0x3);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public HalfSwizzle BSwizzle => (HalfSwizzle)((_opcode >> 28) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool NegB => (_opcode & 0x80000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public bool AbsB => (_opcode & 0x40000000) != 0;
        public bool Sat => (_opcode & 0x100000000) != 0;
        public bool Ftz => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstHadd2I
    {
        private readonly ulong _opcode;
        public InstHadd2I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int BimmH0 => (int)((_opcode >> 20) & 0x3FF);
        public int BimmH1 => (int)((_opcode >> 47) & 0x200) | (int)((_opcode >> 30) & 0x1FF);
        public OFmt OFmt => (OFmt)((_opcode >> 49) & 0x3);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public bool Sat => (_opcode & 0x10000000000000) != 0;
        public bool Ftz => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstHadd2C
    {
        private readonly ulong _opcode;
        public InstHadd2C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public OFmt OFmt => (OFmt)((_opcode >> 49) & 0x3);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool NegB => (_opcode & 0x100000000000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public bool AbsB => (_opcode & 0x40000000000000) != 0;
        public bool Sat => (_opcode & 0x10000000000000) != 0;
        public bool Ftz => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstHadd232i
    {
        private readonly ulong _opcode;
        public InstHadd232i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm => (int)(_opcode >> 20);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 53) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x100000000000000) != 0;
        public bool Sat => (_opcode & 0x10000000000000) != 0;
        public bool Ftz => (_opcode & 0x80000000000000) != 0;
    }

    readonly struct InstHfma2R
    {
        private readonly ulong _opcode;
        public InstHfma2R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public OFmt OFmt => (OFmt)((_opcode >> 49) & 0x3);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public HalfSwizzle BSwizzle => (HalfSwizzle)((_opcode >> 28) & 0x3);
        public HalfSwizzle CSwizzle => (HalfSwizzle)((_opcode >> 35) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000) != 0;
        public bool NegC => (_opcode & 0x40000000) != 0;
        public bool Sat => (_opcode & 0x100000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 37) & 0x3);
    }

    readonly struct InstHfma2I
    {
        private readonly ulong _opcode;
        public InstHfma2I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int BimmH0 => (int)((_opcode >> 20) & 0x3FF);
        public int BimmH1 => (int)((_opcode >> 47) & 0x200) | (int)((_opcode >> 30) & 0x1FF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public OFmt OFmt => (OFmt)((_opcode >> 49) & 0x3);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public HalfSwizzle CSwizzle => (HalfSwizzle)((_opcode >> 53) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegC => (_opcode & 0x8000000000000) != 0;
        public bool Sat => (_opcode & 0x10000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 57) & 0x3);
    }

    readonly struct InstHfma2C
    {
        private readonly ulong _opcode;
        public InstHfma2C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public OFmt OFmt => (OFmt)((_opcode >> 49) & 0x3);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public HalfSwizzle CSwizzle => (HalfSwizzle)((_opcode >> 53) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x100000000000000) != 0;
        public bool NegC => (_opcode & 0x8000000000000) != 0;
        public bool Sat => (_opcode & 0x10000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 57) & 0x3);
    }

    readonly struct InstHfma2Rc
    {
        private readonly ulong _opcode;
        public InstHfma2Rc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public OFmt OFmt => (OFmt)((_opcode >> 49) & 0x3);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public HalfSwizzle CSwizzle => (HalfSwizzle)((_opcode >> 53) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x100000000000000) != 0;
        public bool NegC => (_opcode & 0x8000000000000) != 0;
        public bool Sat => (_opcode & 0x10000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 57) & 0x3);
    }

    readonly struct InstHfma232i
    {
        private readonly ulong _opcode;
        public InstHfma232i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm => (int)(_opcode >> 20);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegC => (_opcode & 0x8000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 57) & 0x3);
    }

    readonly struct InstHmul2R
    {
        private readonly ulong _opcode;
        public InstHmul2R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public OFmt OFmt => (OFmt)((_opcode >> 49) & 0x3);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public HalfSwizzle BSwizzle => (HalfSwizzle)((_opcode >> 28) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public bool AbsB => (_opcode & 0x40000000) != 0;
        public bool Sat => (_opcode & 0x100000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 39) & 0x3);
    }

    readonly struct InstHmul2I
    {
        private readonly ulong _opcode;
        public InstHmul2I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int BimmH0 => (int)((_opcode >> 20) & 0x3FF);
        public int BimmH1 => (int)((_opcode >> 47) & 0x200) | (int)((_opcode >> 30) & 0x1FF);
        public OFmt OFmt => (OFmt)((_opcode >> 49) & 0x3);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public bool Sat => (_opcode & 0x10000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 39) & 0x3);
    }

    readonly struct InstHmul2C
    {
        private readonly ulong _opcode;
        public InstHmul2C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public OFmt OFmt => (OFmt)((_opcode >> 49) & 0x3);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public bool AbsB => (_opcode & 0x40000000000000) != 0;
        public bool Sat => (_opcode & 0x10000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 39) & 0x3);
    }

    readonly struct InstHmul232i
    {
        private readonly ulong _opcode;
        public InstHmul232i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm32 => (int)(_opcode >> 20);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 53) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Sat => (_opcode & 0x10000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 55) & 0x3);
    }

    readonly struct InstHset2R
    {
        private readonly ulong _opcode;
        public InstHset2R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public HalfSwizzle BSwizzle => (HalfSwizzle)((_opcode >> 28) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public bool NegB => (_opcode & 0x80000000) != 0;
        public bool AbsB => (_opcode & 0x40000000) != 0;
        public bool Bval => (_opcode & 0x2000000000000) != 0;
        public FComp Cmp => (FComp)((_opcode >> 35) & 0xF);
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public bool Ftz => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstHset2I
    {
        private readonly ulong _opcode;
        public InstHset2I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int BimmH0 => (int)((_opcode >> 20) & 0x3FF);
        public int BimmH1 => (int)((_opcode >> 47) & 0x200) | (int)((_opcode >> 30) & 0x1FF);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public bool Bval => (_opcode & 0x20000000000000) != 0;
        public FComp Cmp => (FComp)((_opcode >> 49) & 0xF);
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public bool Ftz => (_opcode & 0x40000000000000) != 0;
    }

    readonly struct InstHset2C
    {
        private readonly ulong _opcode;
        public InstHset2C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public bool NegB => (_opcode & 0x100000000000000) != 0;
        public bool Bval => (_opcode & 0x20000000000000) != 0;
        public FComp Cmp => (FComp)((_opcode >> 49) & 0xF);
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public bool Ftz => (_opcode & 0x40000000000000) != 0;
    }

    readonly struct InstHsetp2R
    {
        private readonly ulong _opcode;
        public InstHsetp2R(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool NegB => (_opcode & 0x80000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public bool AbsB => (_opcode & 0x40000000) != 0;
        public FComp FComp2 => (FComp)((_opcode >> 35) & 0xF);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool Ftz => (_opcode & 0x40) != 0;
        public bool HAnd => (_opcode & 0x2000000000000) != 0;
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public HalfSwizzle BSwizzle => (HalfSwizzle)((_opcode >> 28) & 0x3);
    }

    readonly struct InstHsetp2I
    {
        private readonly ulong _opcode;
        public InstHsetp2I(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int BimmH0 => (int)((_opcode >> 20) & 0x3FF);
        public int BimmH1 => (int)((_opcode >> 47) & 0x200) | (int)((_opcode >> 30) & 0x1FF);
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public FComp FComp => (FComp)((_opcode >> 49) & 0xF);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool Ftz => (_opcode & 0x40) != 0;
        public bool HAnd => (_opcode & 0x20000000000000) != 0;
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
    }

    readonly struct InstHsetp2C
    {
        private readonly ulong _opcode;
        public InstHsetp2C(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x80000000000) != 0;
        public bool NegB => (_opcode & 0x100000000000000) != 0;
        public bool AbsA => (_opcode & 0x100000000000) != 0;
        public bool AbsB => (_opcode & 0x40000000000000) != 0;
        public FComp FComp => (FComp)((_opcode >> 49) & 0xF);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool Ftz => (_opcode & 0x40) != 0;
        public bool HAnd => (_opcode & 0x20000000000000) != 0;
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
    }

    readonly struct InstI2fR
    {
        private readonly ulong _opcode;
        public InstI2fR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public ISrcFmt ISrcFmt => (ISrcFmt)((int)((_opcode >> 11) & 0x4) | (int)((_opcode >> 10) & 0x3));
        public DstFmt DstFmt => (DstFmt)((_opcode >> 8) & 0x3);
    }

    readonly struct InstI2fI
    {
        private readonly ulong _opcode;
        public InstI2fI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public ISrcFmt ISrcFmt => (ISrcFmt)((int)((_opcode >> 11) & 0x4) | (int)((_opcode >> 10) & 0x3));
        public DstFmt DstFmt => (DstFmt)((_opcode >> 8) & 0x3);
    }

    readonly struct InstI2fC
    {
        private readonly ulong _opcode;
        public InstI2fC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public ISrcFmt ISrcFmt => (ISrcFmt)((int)((_opcode >> 11) & 0x4) | (int)((_opcode >> 10) & 0x3));
        public DstFmt DstFmt => (DstFmt)((_opcode >> 8) & 0x3);
    }

    readonly struct InstI2iR
    {
        private readonly ulong _opcode;
        public InstI2iR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public ISrcDstFmt IDstFmt => (ISrcDstFmt)((int)((_opcode >> 10) & 0x4) | (int)((_opcode >> 8) & 0x3));
        public ISrcDstFmt ISrcFmt => (ISrcDstFmt)((int)((_opcode >> 11) & 0x4) | (int)((_opcode >> 10) & 0x3));
    }

    readonly struct InstI2iI
    {
        private readonly ulong _opcode;
        public InstI2iI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public ISrcDstFmt IDstFmt => (ISrcDstFmt)((int)((_opcode >> 10) & 0x4) | (int)((_opcode >> 8) & 0x3));
        public ISrcDstFmt ISrcFmt => (ISrcDstFmt)((int)((_opcode >> 11) & 0x4) | (int)((_opcode >> 10) & 0x3));
    }

    readonly struct InstI2iC
    {
        private readonly ulong _opcode;
        public InstI2iC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public ISrcDstFmt IDstFmt => (ISrcDstFmt)((int)((_opcode >> 10) & 0x4) | (int)((_opcode >> 8) & 0x3));
        public ISrcDstFmt ISrcFmt => (ISrcDstFmt)((int)((_opcode >> 11) & 0x4) | (int)((_opcode >> 10) & 0x3));
    }

    readonly struct InstIaddR
    {
        private readonly ulong _opcode;
        public InstIaddR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 48) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
    }

    readonly struct InstIaddI
    {
        private readonly ulong _opcode;
        public InstIaddI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 48) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
    }

    readonly struct InstIaddC
    {
        private readonly ulong _opcode;
        public InstIaddC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 48) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
    }

    readonly struct InstIadd32i
    {
        private readonly ulong _opcode;
        public InstIadd32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public AvgMode AvgMode => (AvgMode)((_opcode >> 55) & 0x3);
        public bool Sat => (_opcode & 0x40000000000000) != 0;
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
        public bool X => (_opcode & 0x20000000000000) != 0;
    }

    readonly struct InstIadd3R
    {
        private readonly ulong _opcode;
        public InstIadd3R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x8000000000000) != 0;
        public bool NegB => (_opcode & 0x4000000000000) != 0;
        public bool NegC => (_opcode & 0x2000000000000) != 0;
        public bool X => (_opcode & 0x1000000000000) != 0;
        public Lrs Lrs => (Lrs)((_opcode >> 37) & 0x3);
        public HalfSelect Apart => (HalfSelect)((_opcode >> 35) & 0x3);
        public HalfSelect Bpart => (HalfSelect)((_opcode >> 33) & 0x3);
        public HalfSelect Cpart => (HalfSelect)((_opcode >> 31) & 0x3);
    }

    readonly struct InstIadd3I
    {
        private readonly ulong _opcode;
        public InstIadd3I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x8000000000000) != 0;
        public bool NegB => (_opcode & 0x4000000000000) != 0;
        public bool NegC => (_opcode & 0x2000000000000) != 0;
        public bool X => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstIadd3C
    {
        private readonly ulong _opcode;
        public InstIadd3C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool NegA => (_opcode & 0x8000000000000) != 0;
        public bool NegB => (_opcode & 0x4000000000000) != 0;
        public bool NegC => (_opcode & 0x2000000000000) != 0;
        public bool X => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstIcmpR
    {
        private readonly ulong _opcode;
        public InstIcmpR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstIcmpI
    {
        private readonly ulong _opcode;
        public InstIcmpI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstIcmpC
    {
        private readonly ulong _opcode;
        public InstIcmpC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstIcmpRc
    {
        private readonly ulong _opcode;
        public InstIcmpRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstIde
    {
        private readonly ulong _opcode;
        public InstIde(ulong opcode) => _opcode = opcode;
        public int Imm16 => (int)((_opcode >> 20) & 0xFFFF);
        public bool Di => (_opcode & 0x20) != 0;
    }

    readonly struct InstIdpR
    {
        private readonly ulong _opcode;
        public InstIdpR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool IsHi => (_opcode & 0x4000000000000) != 0;
        public bool SrcASign => (_opcode & 0x2000000000000) != 0;
        public bool IsDp => (_opcode & 0x1000000000000) != 0;
        public bool SrcBSign => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstIdpC
    {
        private readonly ulong _opcode;
        public InstIdpC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool IsHi => (_opcode & 0x4000000000000) != 0;
        public bool SrcASign => (_opcode & 0x2000000000000) != 0;
        public bool IsDp => (_opcode & 0x1000000000000) != 0;
        public bool SrcBSign => (_opcode & 0x800000000000) != 0;
    }

    readonly struct InstImadR
    {
        private readonly ulong _opcode;
        public InstImadR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Hilo => (_opcode & 0x40000000000000) != 0;
        public bool BSigned => (_opcode & 0x20000000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 51) & 0x3);
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public bool X => (_opcode & 0x2000000000000) != 0;
        public bool ASigned => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstImadI
    {
        private readonly ulong _opcode;
        public InstImadI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Hilo => (_opcode & 0x40000000000000) != 0;
        public bool BSigned => (_opcode & 0x20000000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 51) & 0x3);
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public bool X => (_opcode & 0x2000000000000) != 0;
        public bool ASigned => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstImadC
    {
        private readonly ulong _opcode;
        public InstImadC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Hilo => (_opcode & 0x40000000000000) != 0;
        public bool BSigned => (_opcode & 0x20000000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 51) & 0x3);
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public bool X => (_opcode & 0x2000000000000) != 0;
        public bool ASigned => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstImadRc
    {
        private readonly ulong _opcode;
        public InstImadRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Hilo => (_opcode & 0x40000000000000) != 0;
        public bool BSigned => (_opcode & 0x20000000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 51) & 0x3);
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public bool X => (_opcode & 0x2000000000000) != 0;
        public bool ASigned => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstImad32i
    {
        private readonly ulong _opcode;
        public InstImad32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool BSigned => (_opcode & 0x200000000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 55) & 0x3);
        public bool ASigned => (_opcode & 0x40000000000000) != 0;
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
        public bool Hilo => (_opcode & 0x20000000000000) != 0;
    }

    readonly struct InstImadspR
    {
        private readonly ulong _opcode;
        public InstImadspR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ImadspASelect ASelect => (ImadspASelect)((_opcode >> 48) & 0x7);
        public ImadspBSelect BSelect => (ImadspBSelect)((_opcode >> 53) & 0x3);
        public ImadspASelect CSelect => (ImadspASelect)((int)((_opcode >> 50) & 0x6) | (int)((_opcode >> 48) & 0x1));
    }

    readonly struct InstImadspI
    {
        private readonly ulong _opcode;
        public InstImadspI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ImadspASelect ASelect => (ImadspASelect)((_opcode >> 48) & 0x7);
        public ImadspBSelect BSelect => (ImadspBSelect)((_opcode >> 53) & 0x3);
        public ImadspASelect CSelect => (ImadspASelect)((int)((_opcode >> 50) & 0x6) | (int)((_opcode >> 48) & 0x1));
    }

    readonly struct InstImadspC
    {
        private readonly ulong _opcode;
        public InstImadspC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ImadspASelect ASelect => (ImadspASelect)((_opcode >> 48) & 0x7);
        public ImadspBSelect BSelect => (ImadspBSelect)((_opcode >> 53) & 0x3);
        public ImadspASelect CSelect => (ImadspASelect)((int)((_opcode >> 50) & 0x6) | (int)((_opcode >> 48) & 0x1));
    }

    readonly struct InstImadspRc
    {
        private readonly ulong _opcode;
        public InstImadspRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ImadspASelect ASelect => (ImadspASelect)((_opcode >> 48) & 0x7);
        public ImadspBSelect BSelect => (ImadspBSelect)((_opcode >> 53) & 0x3);
        public ImadspASelect CSelect => (ImadspASelect)((int)((_opcode >> 50) & 0x6) | (int)((_opcode >> 48) & 0x1));
    }

    readonly struct InstImnmxR
    {
        private readonly ulong _opcode;
        public InstImnmxR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public XMode XMode => (XMode)((_opcode >> 43) & 0x3);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstImnmxI
    {
        private readonly ulong _opcode;
        public InstImnmxI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public XMode XMode => (XMode)((_opcode >> 43) & 0x3);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstImnmxC
    {
        private readonly ulong _opcode;
        public InstImnmxC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public XMode XMode => (XMode)((_opcode >> 43) & 0x3);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstImulR
    {
        private readonly ulong _opcode;
        public InstImulR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool ASigned => (_opcode & 0x10000000000) != 0;
        public bool BSigned => (_opcode & 0x20000000000) != 0;
        public bool Hilo => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstImulI
    {
        private readonly ulong _opcode;
        public InstImulI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool ASigned => (_opcode & 0x10000000000) != 0;
        public bool BSigned => (_opcode & 0x20000000000) != 0;
        public bool Hilo => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstImulC
    {
        private readonly ulong _opcode;
        public InstImulC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool ASigned => (_opcode & 0x10000000000) != 0;
        public bool BSigned => (_opcode & 0x20000000000) != 0;
        public bool Hilo => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstImul32i
    {
        private readonly ulong _opcode;
        public InstImul32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool ASigned => (_opcode & 0x40000000000000) != 0;
        public bool BSigned => (_opcode & 0x80000000000000) != 0;
        public bool Hilo => (_opcode & 0x20000000000000) != 0;
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
    }

    readonly struct InstIpa
    {
        private readonly ulong _opcode;
        public InstIpa(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IpaOp IpaOp => (IpaOp)((_opcode >> 54) & 0x3);
        public int Msi => (int)((_opcode >> 52) & 0x3);
        public bool Sat => (_opcode & 0x8000000000000) != 0;
        public bool Idx => (_opcode & 0x4000000000) != 0;
        public int Imm10 => (int)((_opcode >> 28) & 0x3FF);
        public int SrcPred => (int)((_opcode >> 47) & 0x7);
        public bool SrcPredInv => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstIsberd
    {
        private readonly ulong _opcode;
        public InstIsberd(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public AlSize AlSize => (AlSize)((_opcode >> 47) & 0x3);
        public IBase IBase => (IBase)((_opcode >> 33) & 0x3);
        public bool O => (_opcode & 0x100000000) != 0;
        public bool P => (_opcode & 0x80000000) != 0;
    }

    readonly struct InstIscaddR
    {
        private readonly ulong _opcode;
        public InstIscaddR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int Imm5 => (int)((_opcode >> 39) & 0x1F);
        public AvgMode AvgMode => (AvgMode)((_opcode >> 48) & 0x3);
    }

    readonly struct InstIscaddI
    {
        private readonly ulong _opcode;
        public InstIscaddI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int Imm5 => (int)((_opcode >> 39) & 0x1F);
        public AvgMode AvgMode => (AvgMode)((_opcode >> 48) & 0x3);
    }

    readonly struct InstIscaddC
    {
        private readonly ulong _opcode;
        public InstIscaddC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int Imm5 => (int)((_opcode >> 39) & 0x1F);
        public AvgMode AvgMode => (AvgMode)((_opcode >> 48) & 0x3);
    }

    readonly struct InstIscadd32i
    {
        private readonly ulong _opcode;
        public InstIscadd32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
        public int Imm5 => (int)((_opcode >> 53) & 0x1F);
    }

    readonly struct InstIsetR
    {
        private readonly ulong _opcode;
        public InstIsetR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public bool BVal => (_opcode & 0x100000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
    }

    readonly struct InstIsetI
    {
        private readonly ulong _opcode;
        public InstIsetI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public bool BVal => (_opcode & 0x100000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
    }

    readonly struct InstIsetC
    {
        private readonly ulong _opcode;
        public InstIsetC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public bool BVal => (_opcode & 0x100000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
    }

    readonly struct InstIsetpR
    {
        private readonly ulong _opcode;
        public InstIsetpR(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
    }

    readonly struct InstIsetpI
    {
        private readonly ulong _opcode;
        public InstIsetpI(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
    }

    readonly struct InstIsetpC
    {
        private readonly ulong _opcode;
        public InstIsetpC(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
    }

    readonly struct InstJcal
    {
        private readonly ulong _opcode;
        public InstJcal(ulong opcode) => _opcode = opcode;
        public int Imm32 => (int)(_opcode >> 20);
        public bool Ca => (_opcode & 0x20) != 0;
        public bool Inc => (_opcode & 0x40) != 0;
    }

    readonly struct InstJmp
    {
        private readonly ulong _opcode;
        public InstJmp(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
        public bool Ca => (_opcode & 0x20) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool Lmt => (_opcode & 0x40) != 0;
        public bool U => (_opcode & 0x80) != 0;
    }

    readonly struct InstJmx
    {
        private readonly ulong _opcode;
        public InstJmx(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
        public bool Ca => (_opcode & 0x20) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool Lmt => (_opcode & 0x40) != 0;
    }

    readonly struct InstKil
    {
        private readonly ulong _opcode;
        public InstKil(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
    }

    readonly struct InstLd
    {
        private readonly ulong _opcode;
        public InstLd(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 58) & 0x7);
        public CacheOpLd CacheOp => (CacheOpLd)((_opcode >> 56) & 0x3);
        public LsSize LsSize => (LsSize)((_opcode >> 53) & 0x7);
        public bool E => (_opcode & 0x10000000000000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
    }

    readonly struct InstLdc
    {
        private readonly ulong _opcode;
        public InstLdc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public AddressMode AddressMode => (AddressMode)((_opcode >> 44) & 0x3);
        public int CbufSlot => (int)((_opcode >> 36) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0xFFFF);
    }

    readonly struct InstLdg
    {
        private readonly ulong _opcode;
        public InstLdg(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize LsSize => (LsSize)((_opcode >> 48) & 0x7);
        public CacheOpLd CacheOp => (CacheOpLd)((_opcode >> 46) & 0x3);
        public bool E => (_opcode & 0x200000000000) != 0;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    readonly struct InstLdl
    {
        private readonly ulong _opcode;
        public InstLdl(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public CacheOp2 CacheOp => (CacheOp2)((_opcode >> 44) & 0x3);
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    readonly struct InstLds
    {
        private readonly ulong _opcode;
        public InstLds(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public bool U => (_opcode & 0x100000000000) != 0;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    readonly struct InstLeaR
    {
        private readonly ulong _opcode;
        public InstLeaR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x400000000000) != 0;
        public bool NegA => (_opcode & 0x200000000000) != 0;
        public int ImmU5 => (int)((_opcode >> 39) & 0x1F);
        public int DestPred => (int)((_opcode >> 48) & 0x7);
    }

    readonly struct InstLeaI
    {
        private readonly ulong _opcode;
        public InstLeaI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x400000000000) != 0;
        public bool NegA => (_opcode & 0x200000000000) != 0;
        public int ImmU5 => (int)((_opcode >> 39) & 0x1F);
        public int DestPred => (int)((_opcode >> 48) & 0x7);
    }

    readonly struct InstLeaC
    {
        private readonly ulong _opcode;
        public InstLeaC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x400000000000) != 0;
        public bool NegA => (_opcode & 0x200000000000) != 0;
        public int ImmU5 => (int)((_opcode >> 39) & 0x1F);
        public int DestPred => (int)((_opcode >> 48) & 0x7);
    }

    readonly struct InstLeaHiR
    {
        private readonly ulong _opcode;
        public InstLeaHiR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x4000000000) != 0;
        public bool NegA => (_opcode & 0x2000000000) != 0;
        public int ImmU5 => (int)((_opcode >> 28) & 0x1F);
        public int DestPred => (int)((_opcode >> 48) & 0x7);
    }

    readonly struct InstLeaHiC
    {
        private readonly ulong _opcode;
        public InstLeaHiC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x200000000000000) != 0;
        public bool NegA => (_opcode & 0x100000000000000) != 0;
        public int ImmU5 => (int)((_opcode >> 51) & 0x1F);
        public int DestPred => (int)((_opcode >> 48) & 0x7);
    }

    readonly struct InstLepc
    {
#pragma warning disable IDE0052 // Remove unread private member
        private readonly ulong _opcode;
#pragma warning restore IDE0052
        public InstLepc(ulong opcode) => _opcode = opcode;
    }

    readonly struct InstLongjmp
    {
        private readonly ulong _opcode;
        public InstLongjmp(ulong opcode) => _opcode = opcode;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
    }

    readonly struct InstLopR
    {
        private readonly ulong _opcode;
        public InstLopR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int DestPred => (int)((_opcode >> 48) & 0x7);
        public PredicateOp PredicateOp => (PredicateOp)((_opcode >> 44) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
        public LogicOp Lop => (LogicOp)((_opcode >> 41) & 0x3);
        public bool NegA => (_opcode & 0x8000000000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstLopI
    {
        private readonly ulong _opcode;
        public InstLopI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int DestPred => (int)((_opcode >> 48) & 0x7);
        public PredicateOp PredicateOp => (PredicateOp)((_opcode >> 44) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
        public LogicOp LogicOp => (LogicOp)((_opcode >> 41) & 0x3);
        public bool NegA => (_opcode & 0x8000000000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstLopC
    {
        private readonly ulong _opcode;
        public InstLopC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int DestPred => (int)((_opcode >> 48) & 0x7);
        public PredicateOp PredicateOp => (PredicateOp)((_opcode >> 44) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
        public LogicOp LogicOp => (LogicOp)((_opcode >> 41) & 0x3);
        public bool NegA => (_opcode & 0x8000000000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstLop3R
    {
        private readonly ulong _opcode;
        public InstLop3R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int DestPred => (int)((_opcode >> 48) & 0x7);
        public PredicateOp PredicateOp => (PredicateOp)((_opcode >> 36) & 0x3);
        public bool X => (_opcode & 0x4000000000) != 0;
        public int Imm => (int)((_opcode >> 28) & 0xFF);
    }

    readonly struct InstLop3I
    {
        private readonly ulong _opcode;
        public InstLop3I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x200000000000000) != 0;
        public int Imm => (int)((_opcode >> 48) & 0xFF);
    }

    readonly struct InstLop3C
    {
        private readonly ulong _opcode;
        public InstLop3C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x100000000000000) != 0;
        public int Imm => (int)((_opcode >> 48) & 0xFF);
    }

    readonly struct InstLop32i
    {
        private readonly ulong _opcode;
        public InstLop32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool X => (_opcode & 0x200000000000000) != 0;
        public LogicOp LogicOp => (LogicOp)((_opcode >> 53) & 0x3);
        public bool NegA => (_opcode & 0x80000000000000) != 0;
        public bool NegB => (_opcode & 0x100000000000000) != 0;
    }

    readonly struct InstMembar
    {
        private readonly ulong _opcode;
        public InstMembar(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Membar Membar => (Membar)((_opcode >> 8) & 0x3);
        public Ivall Ivall => (Ivall)(_opcode & 0x3);
    }

    readonly struct InstMovR
    {
        private readonly ulong _opcode;
        public InstMovR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int QuadMask => (int)((_opcode >> 39) & 0xF);
    }

    readonly struct InstMovI
    {
        private readonly ulong _opcode;
        public InstMovI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int QuadMask => (int)((_opcode >> 39) & 0xF);
    }

    readonly struct InstMovC
    {
        private readonly ulong _opcode;
        public InstMovC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int QuadMask => (int)((_opcode >> 39) & 0xF);
    }

    readonly struct InstMov32i
    {
        private readonly ulong _opcode;
        public InstMov32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Imm32 => (int)(_opcode >> 20);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int QuadMask => (int)((_opcode >> 12) & 0xF);
    }

    readonly struct InstMufu
    {
        private readonly ulong _opcode;
        public InstMufu(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public MufuOp MufuOp => (MufuOp)((_opcode >> 20) & 0xF);
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstNop
    {
        private readonly ulong _opcode;
        public InstNop(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm16 => (int)((_opcode >> 20) & 0xFFFF);
        public bool Trig => (_opcode & 0x2000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 8) & 0x1F);
    }

    readonly struct InstOutR
    {
        private readonly ulong _opcode;
        public InstOutR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public OutType OutType => (OutType)((_opcode >> 39) & 0x3);
    }

    readonly struct InstOutI
    {
        private readonly ulong _opcode;
        public InstOutI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public OutType OutType => (OutType)((_opcode >> 39) & 0x3);
    }

    readonly struct InstOutC
    {
        private readonly ulong _opcode;
        public InstOutC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public OutType OutType => (OutType)((_opcode >> 39) & 0x3);
    }

    readonly struct InstP2rR
    {
        private readonly ulong _opcode;
        public InstP2rR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstP2rI
    {
        private readonly ulong _opcode;
        public InstP2rI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstP2rC
    {
        private readonly ulong _opcode;
        public InstP2rC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstPbk
    {
        private readonly ulong _opcode;
        public InstPbk(ulong opcode) => _opcode = opcode;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
    }

    readonly struct InstPcnt
    {
        private readonly ulong _opcode;
        public InstPcnt(ulong opcode) => _opcode = opcode;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
    }

    readonly struct InstPexit
    {
        private readonly ulong _opcode;
        public InstPexit(ulong opcode) => _opcode = opcode;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    readonly struct InstPixld
    {
        private readonly ulong _opcode;
        public InstPixld(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int DestPred => (int)((_opcode >> 45) & 0x7);
        public PixMode PixMode => (PixMode)((_opcode >> 31) & 0x7);
        public int Imm8 => (int)((_opcode >> 20) & 0xFF);
    }

    readonly struct InstPlongjmp
    {
        private readonly ulong _opcode;
        public InstPlongjmp(ulong opcode) => _opcode = opcode;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
    }

    readonly struct InstPopcR
    {
        private readonly ulong _opcode;
        public InstPopcR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstPopcI
    {
        private readonly ulong _opcode;
        public InstPopcI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstPopcC
    {
        private readonly ulong _opcode;
        public InstPopcC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstPret
    {
        private readonly ulong _opcode;
        public InstPret(ulong opcode) => _opcode = opcode;
        public bool Ca => (_opcode & 0x20) != 0;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Inc => (_opcode & 0x40) != 0;
    }

    readonly struct InstPrmtR
    {
        private readonly ulong _opcode;
        public InstPrmtR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public PMode PMode => (PMode)((_opcode >> 48) & 0xF);
    }

    readonly struct InstPrmtI
    {
        private readonly ulong _opcode;
        public InstPrmtI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public PMode PMode => (PMode)((_opcode >> 48) & 0xF);
    }

    readonly struct InstPrmtC
    {
        private readonly ulong _opcode;
        public InstPrmtC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public PMode PMode => (PMode)((_opcode >> 48) & 0xF);
    }

    readonly struct InstPrmtRc
    {
        private readonly ulong _opcode;
        public InstPrmtRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public PMode PMode => (PMode)((_opcode >> 48) & 0xF);
    }

    readonly struct InstPset
    {
        private readonly ulong _opcode;
        public InstPset(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int Src2Pred => (int)((_opcode >> 12) & 0x7);
        public bool Src2PredInv => (_opcode & 0x8000) != 0;
        public int Src1Pred => (int)((_opcode >> 29) & 0x7);
        public bool Src1PredInv => (_opcode & 0x100000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp BoolOpAB => (BoolOp)((_opcode >> 24) & 0x3);
        public BoolOp BoolOpC => (BoolOp)((_opcode >> 45) & 0x3);
        public bool BVal => (_opcode & 0x100000000000) != 0;
    }

    readonly struct InstPsetp
    {
        private readonly ulong _opcode;
        public InstPsetp(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int DestPredInv => (int)(_opcode & 0x7);
        public int Src2Pred => (int)((_opcode >> 12) & 0x7);
        public bool Src2PredInv => (_opcode & 0x8000) != 0;
        public int Src1Pred => (int)((_opcode >> 29) & 0x7);
        public bool Src1PredInv => (_opcode & 0x100000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp BoolOpAB => (BoolOp)((_opcode >> 24) & 0x3);
        public BoolOp BoolOpC => (BoolOp)((_opcode >> 45) & 0x3);
    }

    readonly struct InstR2b
    {
        private readonly ulong _opcode;
        public InstR2b(ulong opcode) => _opcode = opcode;
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public BarMode Mode => (BarMode)((_opcode >> 32) & 0x3);
        public int Name => (int)((_opcode >> 28) & 0xF);
    }

    readonly struct InstR2pR
    {
        private readonly ulong _opcode;
        public InstR2pR(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstR2pI
    {
        private readonly ulong _opcode;
        public InstR2pI(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstR2pC
    {
        private readonly ulong _opcode;
        public InstR2pC(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    readonly struct InstRam
    {
#pragma warning disable IDE0052 // Remove unread private member
        private readonly ulong _opcode;
#pragma warning restore IDE0052
        public InstRam(ulong opcode) => _opcode = opcode;
    }

    readonly struct InstRed
    {
        private readonly ulong _opcode;
        public InstRed(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)(_opcode & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm20 => (int)((_opcode >> 28) & 0xFFFFF);
        public AtomSize RedSize => (AtomSize)((_opcode >> 20) & 0x7);
        public RedOp RedOp => (RedOp)((_opcode >> 23) & 0x7);
        public bool E => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstRet
    {
        private readonly ulong _opcode;
        public InstRet(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
    }

    readonly struct InstRroR
    {
        private readonly ulong _opcode;
        public InstRroR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool RroOp => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstRroI
    {
        private readonly ulong _opcode;
        public InstRroI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool RroOp => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstRroC
    {
        private readonly ulong _opcode;
        public InstRroC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool RroOp => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstRtt
    {
#pragma warning disable IDE0052 // Remove unread private member
        private readonly ulong _opcode;
#pragma warning restore IDE0052
        public InstRtt(ulong opcode) => _opcode = opcode;
    }

    readonly struct InstS2r
    {
        private readonly ulong _opcode;
        public InstS2r(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public SReg SReg => (SReg)((_opcode >> 20) & 0xFF);
    }

    readonly struct InstSam
    {
#pragma warning disable IDE0052 // Remove unread private member
        private readonly ulong _opcode;
#pragma warning restore IDE0052
        public InstSam(ulong opcode) => _opcode = opcode;
    }

    readonly struct InstSelR
    {
        private readonly ulong _opcode;
        public InstSelR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstSelI
    {
        private readonly ulong _opcode;
        public InstSelI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstSelC
    {
        private readonly ulong _opcode;
        public InstSelC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    readonly struct InstSetcrsptr
    {
        private readonly ulong _opcode;
        public InstSetcrsptr(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
    }

    readonly struct InstSetlmembase
    {
        private readonly ulong _opcode;
        public InstSetlmembase(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
    }

    readonly struct InstShfLR
    {
        private readonly ulong _opcode;
        public InstShfLR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool M => (_opcode & 0x4000000000000) != 0;
        public XModeShf XModeShf => (XModeShf)((_opcode >> 48) & 0x3);
        public MaxShift MaxShift => (MaxShift)((_opcode >> 37) & 0x3);
    }

    readonly struct InstShfRR
    {
        private readonly ulong _opcode;
        public InstShfRR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool M => (_opcode & 0x4000000000000) != 0;
        public XModeShf XModeShf => (XModeShf)((_opcode >> 48) & 0x3);
        public MaxShift MaxShift => (MaxShift)((_opcode >> 37) & 0x3);
    }

    readonly struct InstShfLI
    {
        private readonly ulong _opcode;
        public InstShfLI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool M => (_opcode & 0x4000000000000) != 0;
        public XModeShf XModeShf => (XModeShf)((_opcode >> 48) & 0x3);
        public MaxShift MaxShift => (MaxShift)((_opcode >> 37) & 0x3);
        public int Imm6 => (int)((_opcode >> 20) & 0x3F);
    }

    readonly struct InstShfRI
    {
        private readonly ulong _opcode;
        public InstShfRI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool M => (_opcode & 0x4000000000000) != 0;
        public XModeShf XModeShf => (XModeShf)((_opcode >> 48) & 0x3);
        public MaxShift MaxShift => (MaxShift)((_opcode >> 37) & 0x3);
        public int Imm6 => (int)((_opcode >> 20) & 0x3F);
    }

    readonly struct InstShfl
    {
        private readonly ulong _opcode;
        public InstShfl(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int SrcBImm => (int)((_opcode >> 20) & 0x1F);
        public int SrcCImm => (int)((_opcode >> 34) & 0x1FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ShflMode ShflMode => (ShflMode)((_opcode >> 30) & 0x3);
        public bool CFixShfl => (_opcode & 0x20000000) != 0;
        public bool BFixShfl => (_opcode & 0x10000000) != 0;
        public int DestPred => (int)((_opcode >> 48) & 0x7);
    }

    readonly struct InstShlR
    {
        private readonly ulong _opcode;
        public InstShlR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x80000000000) != 0;
        public bool M => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstShlI
    {
        private readonly ulong _opcode;
        public InstShlI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x80000000000) != 0;
        public bool M => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstShlC
    {
        private readonly ulong _opcode;
        public InstShlC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x80000000000) != 0;
        public bool M => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstShrR
    {
        private readonly ulong _opcode;
        public InstShrR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public XMode XMode => (XMode)((_opcode >> 43) & 0x3);
        public bool Brev => (_opcode & 0x10000000000) != 0;
        public bool M => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstShrI
    {
        private readonly ulong _opcode;
        public InstShrI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public XMode XMode => (XMode)((_opcode >> 43) & 0x3);
        public bool Brev => (_opcode & 0x10000000000) != 0;
        public bool M => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstShrC
    {
        private readonly ulong _opcode;
        public InstShrC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public XMode XMode => (XMode)((_opcode >> 43) & 0x3);
        public bool Brev => (_opcode & 0x10000000000) != 0;
        public bool M => (_opcode & 0x8000000000) != 0;
    }

    readonly struct InstSsy
    {
        private readonly ulong _opcode;
        public InstSsy(ulong opcode) => _opcode = opcode;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
    }

    readonly struct InstSt
    {
        private readonly ulong _opcode;
        public InstSt(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 58) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 56) & 0x3);
        public LsSize LsSize => (LsSize)((_opcode >> 53) & 0x7);
        public bool E => (_opcode & 0x10000000000000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
    }

    readonly struct InstStg
    {
        private readonly ulong _opcode;
        public InstStg(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 46) & 0x3);
        public bool E => (_opcode & 0x200000000000) != 0;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    readonly struct InstStl
    {
        private readonly ulong _opcode;
        public InstStl(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 44) & 0x3);
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    readonly struct InstStp
    {
        private readonly ulong _opcode;
        public InstStp(ulong opcode) => _opcode = opcode;
        public bool Wait => (_opcode & 0x80000000) != 0;
        public int Imm8 => (int)((_opcode >> 20) & 0xFF);
    }

    readonly struct InstSts
    {
        private readonly ulong _opcode;
        public InstSts(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    readonly struct InstSuatomB
    {
        private readonly ulong _opcode;
        public InstSuatomB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public SuatomSize Size => (SuatomSize)((_opcode >> 36) & 0x7);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public SuatomOp Op => (SuatomOp)((_opcode >> 29) & 0xF);
        public bool Ba => (_opcode & 0x10000000) != 0;
    }

    readonly struct InstSuatom
    {
        private readonly ulong _opcode;
        public InstSuatom(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public SuatomSize Size => (SuatomSize)((_opcode >> 51) & 0x7);
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public SuatomOp Op => (SuatomOp)((_opcode >> 29) & 0xF);
        public bool Ba => (_opcode & 0x10000000) != 0;
    }

    readonly struct InstSuatomB2
    {
        private readonly ulong _opcode;
        public InstSuatomB2(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int DestPred => (int)((_opcode >> 51) & 0x7);
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public SuatomSize Size => (SuatomSize)((_opcode >> 36) & 0x7);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public SuatomOp Op => (SuatomOp)((_opcode >> 29) & 0xF);
        public bool Ba => (_opcode & 0x10000000) != 0;
    }

    readonly struct InstSuatomCasB
    {
        private readonly ulong _opcode;
        public InstSuatomCasB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public SuatomSize Size => (SuatomSize)((_opcode >> 36) & 0x7);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public int DestPred => (int)((_opcode >> 30) & 0x7);
        public bool Ba => (_opcode & 0x10000000) != 0;
    }

    readonly struct InstSuatomCas
    {
        private readonly ulong _opcode;
        public InstSuatomCas(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public SuatomSize Size => (SuatomSize)((_opcode >> 51) & 0x7);
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public int DestPred => (int)((_opcode >> 30) & 0x7);
        public bool Ba => (_opcode & 0x10000000) != 0;
    }

    readonly struct InstSuldDB
    {
        private readonly ulong _opcode;
        public InstSuldDB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public int DestPred2 => (int)((_opcode >> 30) & 0x7);
        public CacheOpLd CacheOp => (CacheOpLd)((_opcode >> 24) & 0x3);
        public bool Ba => (_opcode & 0x800000) != 0;
        public SuSize Size => (SuSize)((_opcode >> 20) & 0x7);
    }

    readonly struct InstSuldD
    {
        private readonly ulong _opcode;
        public InstSuldD(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public int DestPred2 => (int)((_opcode >> 30) & 0x7);
        public CacheOpLd CacheOp => (CacheOpLd)((_opcode >> 24) & 0x3);
        public bool Ba => (_opcode & 0x800000) != 0;
        public SuSize Size => (SuSize)((_opcode >> 20) & 0x7);
    }

    readonly struct InstSuldB
    {
        private readonly ulong _opcode;
        public InstSuldB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public int DestPred2 => (int)((_opcode >> 30) & 0x7);
        public CacheOpLd CacheOp => (CacheOpLd)((_opcode >> 24) & 0x3);
        public SuRgba Rgba => (SuRgba)((_opcode >> 20) & 0xF);
    }

    readonly struct InstSuld
    {
        private readonly ulong _opcode;
        public InstSuld(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public int DestPred2 => (int)((_opcode >> 30) & 0x7);
        public CacheOpLd CacheOp => (CacheOpLd)((_opcode >> 24) & 0x3);
        public SuRgba Rgba => (SuRgba)((_opcode >> 20) & 0xF);
    }

    readonly struct InstSuredB
    {
        private readonly ulong _opcode;
        public InstSuredB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public RedOp Op => (RedOp)((_opcode >> 24) & 0x7);
        public bool Ba => (_opcode & 0x800000) != 0;
        public SuatomSize Size => (SuatomSize)((_opcode >> 20) & 0x7);
    }

    readonly struct InstSured
    {
        private readonly ulong _opcode;
        public InstSured(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public RedOp Op => (RedOp)((_opcode >> 24) & 0x7);
        public bool Ba => (_opcode & 0x800000) != 0;
        public SuatomSize Size => (SuatomSize)((_opcode >> 20) & 0x7);
    }

    readonly struct InstSustDB
    {
        private readonly ulong _opcode;
        public InstSustDB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 24) & 0x3);
        public bool Ba => (_opcode & 0x800000) != 0;
        public SuSize Size => (SuSize)((_opcode >> 20) & 0x7);
    }

    readonly struct InstSustD
    {
        private readonly ulong _opcode;
        public InstSustD(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 24) & 0x3);
        public bool Ba => (_opcode & 0x800000) != 0;
        public SuSize Size => (SuSize)((_opcode >> 20) & 0x7);
    }

    readonly struct InstSustB
    {
        private readonly ulong _opcode;
        public InstSustB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 24) & 0x3);
        public SuRgba Rgba => (SuRgba)((_opcode >> 20) & 0xF);
    }

    readonly struct InstSust
    {
        private readonly ulong _opcode;
        public InstSust(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 24) & 0x3);
        public SuRgba Rgba => (SuRgba)((_opcode >> 20) & 0xF);
    }

    readonly struct InstSync
    {
        private readonly ulong _opcode;
        public InstSync(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)(_opcode & 0x1F);
    }

    readonly struct InstTex
    {
        private readonly ulong _opcode;
        public InstTex(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Lc => (_opcode & 0x400000000000000) != 0;
        public int DestPred => (int)((_opcode >> 51) & 0x7);
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public Lod Lod => (Lod)((_opcode >> 55) & 0x7);
        public bool Aoffi => (_opcode & 0x40000000000000) != 0;
        public bool Dc => (_opcode & 0x4000000000000) != 0;
        public bool Ndv => (_opcode & 0x800000000) != 0;
        public TexDim Dim => (TexDim)((_opcode >> 28) & 0x7);
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
    }

    readonly struct InstTexB
    {
        private readonly ulong _opcode;
        public InstTexB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Lcb => (_opcode & 0x10000000000) != 0;
        public int DestPred => (int)((_opcode >> 51) & 0x7);
        public Lod Lodb => (Lod)((_opcode >> 37) & 0x7);
        public bool Aoffib => (_opcode & 0x1000000000) != 0;
        public bool Dc => (_opcode & 0x4000000000000) != 0;
        public bool Ndv => (_opcode & 0x800000000) != 0;
        public TexDim Dim => (TexDim)((_opcode >> 28) & 0x7);
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
    }

    readonly struct InstTexs
    {
        private readonly ulong _opcode;
        public InstTexs(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public TexsTarget Target => (TexsTarget)((_opcode >> 53) & 0xF);
        public int WMask => (int)((_opcode >> 50) & 0x7);
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int Dest2 => (int)((_opcode >> 28) & 0xFF);
    }

    readonly struct InstTld
    {
        private readonly ulong _opcode;
        public InstTld(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public bool Lod => (_opcode & 0x80000000000000) != 0;
        public bool Toff => (_opcode & 0x800000000) != 0;
        public bool Ms => (_opcode & 0x4000000000000) != 0;
        public bool Cl => (_opcode & 0x40000000000000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int DestPred => (int)((_opcode >> 51) & 0x7);
        public TexDim Dim => (TexDim)((_opcode >> 28) & 0x7);
    }

    readonly struct InstTldB
    {
        private readonly ulong _opcode;
        public InstTldB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public bool Lod => (_opcode & 0x80000000000000) != 0;
        public bool Toff => (_opcode & 0x800000000) != 0;
        public bool Ms => (_opcode & 0x4000000000000) != 0;
        public bool Cl => (_opcode & 0x40000000000000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int DestPred => (int)((_opcode >> 51) & 0x7);
        public TexDim Dim => (TexDim)((_opcode >> 28) & 0x7);
    }

    readonly struct InstTlds
    {
        private readonly ulong _opcode;
        public InstTlds(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public TldsTarget Target => (TldsTarget)((_opcode >> 53) & 0xF);
        public int WMask => (int)((_opcode >> 50) & 0x7);
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int Dest2 => (int)((_opcode >> 28) & 0xFF);
    }

    readonly struct InstTld4
    {
        private readonly ulong _opcode;
        public InstTld4(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Lc => (_opcode & 0x400000000000000) != 0;
        public int DestPred => (int)((_opcode >> 51) & 0x7);
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public TexComp TexComp => (TexComp)((_opcode >> 56) & 0x3);
        public TexOffset Toff => (TexOffset)((_opcode >> 54) & 0x3);
        public bool Dc => (_opcode & 0x4000000000000) != 0;
        public bool Ndv => (_opcode & 0x800000000) != 0;
        public TexDim Dim => (TexDim)((_opcode >> 28) & 0x7);
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
    }

    readonly struct InstTld4B
    {
        private readonly ulong _opcode;
        public InstTld4B(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Lc => (_opcode & 0x10000000000) != 0;
        public int DestPred => (int)((_opcode >> 51) & 0x7);
        public TexComp TexComp => (TexComp)((_opcode >> 38) & 0x3);
        public TexOffset Toff => (TexOffset)((_opcode >> 36) & 0x3);
        public bool Dc => (_opcode & 0x4000000000000) != 0;
        public bool Ndv => (_opcode & 0x800000000) != 0;
        public TexDim Dim => (TexDim)((_opcode >> 28) & 0x7);
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
    }

    readonly struct InstTld4s
    {
        private readonly ulong _opcode;
        public InstTld4s(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public TexComp TexComp => (TexComp)((_opcode >> 52) & 0x3);
        public bool Aoffi => (_opcode & 0x8000000000000) != 0;
        public bool Dc => (_opcode & 0x4000000000000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int Dest2 => (int)((_opcode >> 28) & 0xFF);
    }

    readonly struct InstTmml
    {
        private readonly ulong _opcode;
        public InstTmml(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public bool Ndv => (_opcode & 0x800000000) != 0;
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public TexDim Dim => (TexDim)((_opcode >> 28) & 0x7);
    }

    readonly struct InstTmmlB
    {
        private readonly ulong _opcode;
        public InstTmmlB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public bool Ndv => (_opcode & 0x800000000) != 0;
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public TexDim Dim => (TexDim)((_opcode >> 28) & 0x7);
    }

    readonly struct InstTxa
    {
        private readonly ulong _opcode;
        public InstTxa(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public bool Ndv => (_opcode & 0x800000000) != 0;
        public int WMask => (int)((_opcode >> 31) & 0xF);
    }

    readonly struct InstTxd
    {
        private readonly ulong _opcode;
        public InstTxd(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int DestPred => (int)((_opcode >> 51) & 0x7);
        public bool Lc => (_opcode & 0x4000000000000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public bool Toff => (_opcode & 0x800000000) != 0;
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public TexDim Dim => (TexDim)((_opcode >> 28) & 0x7);
    }

    readonly struct InstTxdB
    {
        private readonly ulong _opcode;
        public InstTxdB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int DestPred => (int)((_opcode >> 51) & 0x7);
        public bool Lc => (_opcode & 0x4000000000000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public bool Toff => (_opcode & 0x800000000) != 0;
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public TexDim Dim => (TexDim)((_opcode >> 28) & 0x7);
    }

    readonly struct InstTxq
    {
        private readonly ulong _opcode;
        public InstTxq(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public TexQuery TexQuery => (TexQuery)((_opcode >> 22) & 0x3F);
    }

    readonly struct InstTxqB
    {
        private readonly ulong _opcode;
        public InstTxqB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public TexQuery TexQuery => (TexQuery)((_opcode >> 22) & 0x3F);
    }

    readonly struct InstVabsdiff
    {
        private readonly ulong _opcode;
        public InstVabsdiff(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool DFormat => (_opcode & 0x40000000000000) != 0;
        public VectorSelect ASelect => (VectorSelect)((int)((_opcode >> 45) & 0x8) | (int)((_opcode >> 36) & 0x7));
        public VectorSelect BSelect => (VectorSelect)((int)((_opcode >> 46) & 0x8) | (int)((_opcode >> 28) & 0x7));
        public bool Sat => (_opcode & 0x80000000000000) != 0;
        public VideoOp VideoOp => (VideoOp)((_opcode >> 51) & 0x7);
        public bool BVideo => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstVabsdiff4
    {
        private readonly ulong _opcode;
        public InstVabsdiff4(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public VideoRed VRed => (VideoRed)((_opcode >> 53) & 0x3);
        public LaneMask4 LaneMask4 => (LaneMask4)((int)((_opcode >> 49) & 0xC) | (int)((_opcode >> 36) & 0x3));
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public bool SrcBFmt => (_opcode & 0x2000000000000) != 0;
        public bool SrcAFmt => (_opcode & 0x1000000000000) != 0;
        public bool DFormat => (_opcode & 0x4000000000) != 0;
        public ASelect4 Asel4 => (ASelect4)((_opcode >> 32) & 0xF);
        public BSelect4 Bsel4 => (BSelect4)((_opcode >> 28) & 0xF);
    }

    readonly struct InstVadd
    {
        private readonly ulong _opcode;
        public InstVadd(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm16 => (int)((_opcode >> 20) & 0xFFFF);
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 56) & 0x3);
        public bool DFormat => (_opcode & 0x40000000000000) != 0;
        public VectorSelect ASelect => (VectorSelect)((int)((_opcode >> 45) & 0x8) | (int)((_opcode >> 36) & 0x7));
        public VectorSelect BSelect => (VectorSelect)((int)((_opcode >> 46) & 0x8) | (int)((_opcode >> 28) & 0x7));
        public bool Sat => (_opcode & 0x80000000000000) != 0;
        public VideoOp VideoOp => (VideoOp)((_opcode >> 51) & 0x7);
        public bool BVideo => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstVmad
    {
        private readonly ulong _opcode;
        public InstVmad(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm16 => (int)((_opcode >> 20) & 0xFFFF);
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public VectorSelect ASelect => (VectorSelect)((int)((_opcode >> 45) & 0x8) | (int)((_opcode >> 36) & 0x7));
        public VectorSelect BSelect => (VectorSelect)((int)((_opcode >> 46) & 0x8) | (int)((_opcode >> 28) & 0x7));
        public bool Sat => (_opcode & 0x80000000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 53) & 0x3);
        public VideoScale VideoScale => (VideoScale)((_opcode >> 51) & 0x3);
        public bool BVideo => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstVmnmx
    {
        private readonly ulong _opcode;
        public InstVmnmx(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm16 => (int)((_opcode >> 20) & 0xFFFF);
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool DFormat => (_opcode & 0x40000000000000) != 0;
        public VectorSelect ASelect => (VectorSelect)((int)((_opcode >> 45) & 0x8) | (int)((_opcode >> 36) & 0x7));
        public VectorSelect BSelect => (VectorSelect)((int)((_opcode >> 46) & 0x8) | (int)((_opcode >> 28) & 0x7));
        public bool Sat => (_opcode & 0x80000000000000) != 0;
        public VideoOp VideoOp => (VideoOp)((_opcode >> 51) & 0x7);
        public bool Mn => (_opcode & 0x100000000000000) != 0;
        public bool BVideo => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstVote
    {
        private readonly ulong _opcode;
        public InstVote(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public VoteMode VoteMode => (VoteMode)((_opcode >> 48) & 0x3);
        public int VpDest => (int)((_opcode >> 45) & 0x7);
    }

    readonly struct InstVotevtg
    {
        private readonly ulong _opcode;
        public InstVotevtg(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public VoteMode VoteMode => (VoteMode)((_opcode >> 48) & 0x3);
        public int Imm28 => (int)((_opcode >> 20) & 0xFFFFFFF);
    }

    readonly struct InstVset
    {
        private readonly ulong _opcode;
        public InstVset(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public IComp VComp => (IComp)((_opcode >> 54) & 0x7);
        public VectorSelect ASelect => (VectorSelect)((int)((_opcode >> 45) & 0x8) | (int)((_opcode >> 36) & 0x7));
        public VectorSelect BSelect => (VectorSelect)((int)((_opcode >> 46) & 0x8) | (int)((_opcode >> 28) & 0x7));
        public VideoOp VideoOp => (VideoOp)((_opcode >> 51) & 0x7);
        public bool BVideo => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstVsetp
    {
        private readonly ulong _opcode;
        public InstVsetp(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm16 => (int)((_opcode >> 20) & 0xFFFF);
        public VectorSelect ASelect => (VectorSelect)((int)((_opcode >> 45) & 0x8) | (int)((_opcode >> 36) & 0x7));
        public VectorSelect BSelect => (VectorSelect)((int)((_opcode >> 46) & 0x8) | (int)((_opcode >> 28) & 0x7));
        public IComp VComp => (IComp)((int)((_opcode >> 45) & 0x4) | (int)((_opcode >> 43) & 0x3));
        public BoolOp BoolOp => (BoolOp)((_opcode >> 45) & 0x3);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)(_opcode & 0x7);
        public bool BVideo => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstVshl
    {
        private readonly ulong _opcode;
        public InstVshl(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Mv => (_opcode & 0x2000000000000) != 0;
        public bool DFormat => (_opcode & 0x40000000000000) != 0;
        public VectorSelect ASelect => (VectorSelect)((int)((_opcode >> 45) & 0x8) | (int)((_opcode >> 36) & 0x7));
        public VectorSelect BSelect => (VectorSelect)((_opcode >> 28) & 0x7);
        public bool Sat => (_opcode & 0x80000000000000) != 0;
        public VideoOp VideoOp => (VideoOp)((_opcode >> 51) & 0x7);
        public bool BVideo => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstVshr
    {
        private readonly ulong _opcode;
        public InstVshr(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Mv => (_opcode & 0x2000000000000) != 0;
        public bool DFormat => (_opcode & 0x40000000000000) != 0;
        public VectorSelect ASelect => (VectorSelect)((int)((_opcode >> 45) & 0x8) | (int)((_opcode >> 36) & 0x7));
        public VectorSelect BSelect => (VectorSelect)((_opcode >> 28) & 0x7);
        public bool Sat => (_opcode & 0x80000000000000) != 0;
        public VideoOp VideoOp => (VideoOp)((_opcode >> 51) & 0x7);
        public bool BVideo => (_opcode & 0x4000000000000) != 0;
    }

    readonly struct InstXmadR
    {
        private readonly ulong _opcode;
        public InstXmadR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool HiloA => (_opcode & 0x20000000000000) != 0;
        public XmadCop XmadCop => (XmadCop)((_opcode >> 50) & 0x7);
        public bool BSigned => (_opcode & 0x2000000000000) != 0;
        public bool ASigned => (_opcode & 0x1000000000000) != 0;
        public bool X => (_opcode & 0x4000000000) != 0;
        public bool Mrg => (_opcode & 0x2000000000) != 0;
        public bool Psl => (_opcode & 0x1000000000) != 0;
        public bool HiloB => (_opcode & 0x800000000) != 0;
    }

    readonly struct InstXmadI
    {
        private readonly ulong _opcode;
        public InstXmadI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int Imm16 => (int)((_opcode >> 20) & 0xFFFF);
        public bool HiloA => (_opcode & 0x20000000000000) != 0;
        public XmadCop XmadCop => (XmadCop)((_opcode >> 50) & 0x7);
        public bool BSigned => (_opcode & 0x2000000000000) != 0;
        public bool ASigned => (_opcode & 0x1000000000000) != 0;
        public bool X => (_opcode & 0x4000000000) != 0;
        public bool Mrg => (_opcode & 0x2000000000) != 0;
        public bool Psl => (_opcode & 0x1000000000) != 0;
    }

    readonly struct InstXmadC
    {
        private readonly ulong _opcode;
        public InstXmadC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Mrg => (_opcode & 0x100000000000000) != 0;
        public bool Psl => (_opcode & 0x80000000000000) != 0;
        public bool X => (_opcode & 0x40000000000000) != 0;
        public bool HiloA => (_opcode & 0x20000000000000) != 0;
        public bool HiloB => (_opcode & 0x10000000000000) != 0;
        public XmadCop2 XmadCop => (XmadCop2)((_opcode >> 50) & 0x3);
        public bool BSigned => (_opcode & 0x2000000000000) != 0;
        public bool ASigned => (_opcode & 0x1000000000000) != 0;
    }

    readonly struct InstXmadRc
    {
        private readonly ulong _opcode;
        public InstXmadRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)(_opcode & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x40000000000000) != 0;
        public bool HiloA => (_opcode & 0x20000000000000) != 0;
        public bool HiloB => (_opcode & 0x10000000000000) != 0;
        public XmadCop2 XmadCop => (XmadCop2)((_opcode >> 50) & 0x3);
        public bool BSigned => (_opcode & 0x2000000000000) != 0;
        public bool ASigned => (_opcode & 0x1000000000000) != 0;
    }
}
