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
        Texture2DLodLevelOffset = 0xc
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

    struct InstConditional
    {
        private ulong _opcode;
        public InstConditional(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
    }

    struct InstAl2p
    {
        private ulong _opcode;
        public InstAl2p(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public AlSize AlSize => (AlSize)((_opcode >> 47) & 0x3);
        public bool Aio => (_opcode & 0x100000000) != 0;
        public int Imm11 => (int)((_opcode >> 20) & 0x7FF);
        public int DestPred => (int)((_opcode >> 44) & 0x7);
    }

    struct InstAld
    {
        private ulong _opcode;
        public InstAld(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstAst
    {
        private ulong _opcode;
        public InstAst(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 0) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm11 => (int)((_opcode >> 20) & 0x7FF);
        public bool P => (_opcode & 0x80000000) != 0;
        public AlSize AlSize => (AlSize)((_opcode >> 47) & 0x3);
        public bool Phys => !P && Imm11 == 0 && SrcA != RegisterConsts.RegisterZeroIndex;
    }

    struct InstAtom
    {
        private ulong _opcode;
        public InstAtom(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm20 => (int)((_opcode >> 28) & 0xFFFFF);
        public AtomSize Size => (AtomSize)((_opcode >> 49) & 0x7);
        public AtomOp Op => (AtomOp)((_opcode >> 52) & 0xF);
        public bool E => (_opcode & 0x1000000000000) != 0;
    }

    struct InstAtomCas
    {
        private ulong _opcode;
        public InstAtomCas(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int BcRz => (int)((_opcode >> 50) & 0x3);
        public bool E => (_opcode & 0x1000000000000) != 0;
    }

    struct InstAtoms
    {
        private ulong _opcode;
        public InstAtoms(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm22 => (int)((_opcode >> 30) & 0x3FFFFF);
        public AtomsSize AtomsSize => (AtomsSize)((_opcode >> 28) & 0x3);
        public AtomOp AtomOp => (AtomOp)((_opcode >> 52) & 0xF);
    }

    struct InstAtomsCas
    {
        private ulong _opcode;
        public InstAtomsCas(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int AtomsBcRz => (int)((_opcode >> 28) & 0x3);
    }

    struct InstB2r
    {
        private ulong _opcode;
        public InstB2r(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int DestPred => (int)((_opcode >> 45) & 0x7);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public BarMode Mode => (BarMode)((_opcode >> 32) & 0x3);
    }

    struct InstBar
    {
        private ulong _opcode;
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

    struct InstBfeR
    {
        private ulong _opcode;
        public InstBfeR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Brev => (_opcode & 0x10000000000) != 0;
    }

    struct InstBfeI
    {
        private ulong _opcode;
        public InstBfeI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Brev => (_opcode & 0x10000000000) != 0;
    }

    struct InstBfeC
    {
        private ulong _opcode;
        public InstBfeC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Brev => (_opcode & 0x10000000000) != 0;
    }

    struct InstBfiR
    {
        private ulong _opcode;
        public InstBfiR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
    }

    struct InstBfiI
    {
        private ulong _opcode;
        public InstBfiI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
    }

    struct InstBfiC
    {
        private ulong _opcode;
        public InstBfiC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
    }

    struct InstBfiRc
    {
        private ulong _opcode;
        public InstBfiRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
    }

    struct InstBpt
    {
        private ulong _opcode;
        public InstBpt(ulong opcode) => _opcode = opcode;
        public int Imm20 => (int)((_opcode >> 20) & 0xFFFFF);
        public Bpt Bpt => (Bpt)((_opcode >> 6) & 0x7);
    }

    struct InstBra
    {
        private ulong _opcode;
        public InstBra(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
        public bool Lmt => (_opcode & 0x40) != 0;
        public bool U => (_opcode & 0x80) != 0;
    }

    struct InstBrk
    {
        private ulong _opcode;
        public InstBrk(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
    }

    struct InstBrx
    {
        private ulong _opcode;
        public InstBrx(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
        public bool Lmt => (_opcode & 0x40) != 0;
    }

    struct InstCal
    {
        private ulong _opcode;
        public InstCal(ulong opcode) => _opcode = opcode;
        public bool Ca => (_opcode & 0x20) != 0;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Inc => (_opcode & 0x40) != 0;
    }

    struct InstCctl
    {
        private ulong _opcode;
        public InstCctl(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm30 => (int)((_opcode >> 22) & 0x3FFFFFFF);
        public bool E => (_opcode & 0x10000000000000) != 0;
        public CacheType Cache => (CacheType)((_opcode >> 4) & 0x7);
        public CctlOp CctlOp => (CctlOp)((_opcode >> 0) & 0xF);
    }

    struct InstCctll
    {
        private ulong _opcode;
        public InstCctll(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm22 => (int)((_opcode >> 22) & 0x3FFFFF);
        public int Cache => (int)((_opcode >> 4) & 0x3);
        public CctlOp CctlOp => (CctlOp)((_opcode >> 0) & 0xF);
    }

    struct InstCctlt
    {
        private ulong _opcode;
        public InstCctlt(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int TsIdx13 => (int)((_opcode >> 36) & 0x1FFF);
        public CctltOp CctltOp => (CctltOp)((_opcode >> 0) & 0x3);
    }

    struct InstCctltR
    {
        private ulong _opcode;
        public InstCctltR(ulong opcode) => _opcode = opcode;
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public CctltOp CctltOp => (CctltOp)((_opcode >> 0) & 0x3);
    }

    struct InstCont
    {
        private ulong _opcode;
        public InstCont(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
    }

    struct InstCset
    {
        private ulong _opcode;
        public InstCset(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 8) & 0x1F);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public bool BVal => (_opcode & 0x100000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
    }

    struct InstCsetp
    {
        private ulong _opcode;
        public InstCsetp(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 8) & 0x1F);
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp Bop => (BoolOp)((_opcode >> 45) & 0x3);
    }

    struct InstCs2r
    {
        private ulong _opcode;
        public InstCs2r(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public SReg SReg => (SReg)((_opcode >> 20) & 0xFF);
    }

    struct InstDaddR
    {
        private ulong _opcode;
        public InstDaddR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDaddI
    {
        private ulong _opcode;
        public InstDaddI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDaddC
    {
        private ulong _opcode;
        public InstDaddC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDepbar
    {
        private ulong _opcode;
        public InstDepbar(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Le => (_opcode & 0x20000000) != 0;
        public int Sbid => (int)((_opcode >> 26) & 0x7);
        public int PendCnt => (int)((_opcode >> 20) & 0x3F);
        public int Imm6 => (int)((_opcode >> 0) & 0x3F);
    }

    struct InstDfmaR
    {
        private ulong _opcode;
        public InstDfmaR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDfmaI
    {
        private ulong _opcode;
        public InstDfmaI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDfmaC
    {
        private ulong _opcode;
        public InstDfmaC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDfmaRc
    {
        private ulong _opcode;
        public InstDfmaRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDmnmxR
    {
        private ulong _opcode;
        public InstDmnmxR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDmnmxI
    {
        private ulong _opcode;
        public InstDmnmxI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDmnmxC
    {
        private ulong _opcode;
        public InstDmnmxC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDmulR
    {
        private ulong _opcode;
        public InstDmulR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public bool NegA => (_opcode & 0x1000000000000) != 0;
    }

    struct InstDmulI
    {
        private ulong _opcode;
        public InstDmulI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public bool NegA => (_opcode & 0x1000000000000) != 0;
    }

    struct InstDmulC
    {
        private ulong _opcode;
        public InstDmulC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public RoundMode RoundMode => (RoundMode)((_opcode >> 39) & 0x3);
        public bool NegA => (_opcode & 0x1000000000000) != 0;
    }

    struct InstDsetR
    {
        private ulong _opcode;
        public InstDsetR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDsetI
    {
        private ulong _opcode;
        public InstDsetI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDsetC
    {
        private ulong _opcode;
        public InstDsetC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstDsetpR
    {
        private ulong _opcode;
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
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
    }

    struct InstDsetpI
    {
        private ulong _opcode;
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
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
    }

    struct InstDsetpC
    {
        private ulong _opcode;
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
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
    }

    struct InstExit
    {
        private ulong _opcode;
        public InstExit(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
        public bool KeepRefCnt => (_opcode & 0x20) != 0;
    }

    struct InstF2fR
    {
        private ulong _opcode;
        public InstF2fR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstF2fI
    {
        private ulong _opcode;
        public InstF2fI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstF2fC
    {
        private ulong _opcode;
        public InstF2fC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstF2iR
    {
        private ulong _opcode;
        public InstF2iR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstF2iI
    {
        private ulong _opcode;
        public InstF2iI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstF2iC
    {
        private ulong _opcode;
        public InstF2iC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFaddR
    {
        private ulong _opcode;
        public InstFaddR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFaddI
    {
        private ulong _opcode;
        public InstFaddI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFaddC
    {
        private ulong _opcode;
        public InstFaddC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFadd32i
    {
        private ulong _opcode;
        public InstFadd32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFchkR
    {
        private ulong _opcode;
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

    struct InstFchkI
    {
        private ulong _opcode;
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

    struct InstFchkC
    {
        private ulong _opcode;
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

    struct InstFcmpR
    {
        private ulong _opcode;
        public InstFcmpR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public bool Ftz => (_opcode & 0x800000000000) != 0;
    }

    struct InstFcmpI
    {
        private ulong _opcode;
        public InstFcmpI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public bool Ftz => (_opcode & 0x800000000000) != 0;
    }

    struct InstFcmpC
    {
        private ulong _opcode;
        public InstFcmpC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public bool Ftz => (_opcode & 0x800000000000) != 0;
    }

    struct InstFcmpRc
    {
        private ulong _opcode;
        public InstFcmpRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public FComp FComp => (FComp)((_opcode >> 48) & 0xF);
        public bool Ftz => (_opcode & 0x800000000000) != 0;
    }

    struct InstFfmaR
    {
        private ulong _opcode;
        public InstFfmaR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFfmaI
    {
        private ulong _opcode;
        public InstFfmaI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFfmaC
    {
        private ulong _opcode;
        public InstFfmaC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFfmaRc
    {
        private ulong _opcode;
        public InstFfmaRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFfma32i
    {
        private ulong _opcode;
        public InstFfma32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFloR
    {
        private ulong _opcode;
        public InstFloR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    struct InstFloI
    {
        private ulong _opcode;
        public InstFloI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    struct InstFloC
    {
        private ulong _opcode;
        public InstFloC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Signed => (_opcode & 0x1000000000000) != 0;
        public bool Sh => (_opcode & 0x20000000000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    struct InstFmnmxR
    {
        private ulong _opcode;
        public InstFmnmxR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFmnmxI
    {
        private ulong _opcode;
        public InstFmnmxI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFmnmxC
    {
        private ulong _opcode;
        public InstFmnmxC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFmulR
    {
        private ulong _opcode;
        public InstFmulR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFmulI
    {
        private ulong _opcode;
        public InstFmulI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFmulC
    {
        private ulong _opcode;
        public InstFmulC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFmul32i
    {
        private ulong _opcode;
        public InstFmul32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool Sat => (_opcode & 0x80000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 53) & 0x3);
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
    }

    struct InstFsetR
    {
        private ulong _opcode;
        public InstFsetR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFsetC
    {
        private ulong _opcode;
        public InstFsetC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFsetI
    {
        private ulong _opcode;
        public InstFsetI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstFsetpR
    {
        private ulong _opcode;
        public InstFsetpR(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
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

    struct InstFsetpI
    {
        private ulong _opcode;
        public InstFsetpI(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
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

    struct InstFsetpC
    {
        private ulong _opcode;
        public InstFsetpC(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
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

    struct InstFswzadd
    {
        private ulong _opcode;
        public InstFswzadd(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstGetcrsptr
    {
        private ulong _opcode;
        public InstGetcrsptr(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
    }

    struct InstGetlmembase
    {
        private ulong _opcode;
        public InstGetlmembase(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
    }

    struct InstHadd2R
    {
        private ulong _opcode;
        public InstHadd2R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHadd2I
    {
        private ulong _opcode;
        public InstHadd2I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHadd2C
    {
        private ulong _opcode;
        public InstHadd2C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHadd232i
    {
        private ulong _opcode;
        public InstHadd232i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm => (int)(_opcode >> 20);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 53) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegA => (_opcode & 0x100000000000000) != 0;
        public bool Sat => (_opcode & 0x10000000000000) != 0;
        public bool Ftz => (_opcode & 0x80000000000000) != 0;
    }

    struct InstHfma2R
    {
        private ulong _opcode;
        public InstHfma2R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHfma2I
    {
        private ulong _opcode;
        public InstHfma2I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHfma2C
    {
        private ulong _opcode;
        public InstHfma2C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHfma2Rc
    {
        private ulong _opcode;
        public InstHfma2Rc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHfma232i
    {
        private ulong _opcode;
        public InstHfma232i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm => (int)(_opcode >> 20);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 47) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegC => (_opcode & 0x8000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 57) & 0x3);
    }

    struct InstHmul2R
    {
        private ulong _opcode;
        public InstHmul2R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHmul2I
    {
        private ulong _opcode;
        public InstHmul2I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHmul2C
    {
        private ulong _opcode;
        public InstHmul2C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHmul232i
    {
        private ulong _opcode;
        public InstHmul232i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm32 => (int)(_opcode >> 20);
        public HalfSwizzle ASwizzle => (HalfSwizzle)((_opcode >> 53) & 0x3);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Sat => (_opcode & 0x10000000000000) != 0;
        public Fmz Fmz => (Fmz)((_opcode >> 55) & 0x3);
    }

    struct InstHset2R
    {
        private ulong _opcode;
        public InstHset2R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHset2I
    {
        private ulong _opcode;
        public InstHset2I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHset2C
    {
        private ulong _opcode;
        public InstHset2C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstHsetp2R
    {
        private ulong _opcode;
        public InstHsetp2R(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
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

    struct InstHsetp2I
    {
        private ulong _opcode;
        public InstHsetp2I(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
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

    struct InstHsetp2C
    {
        private ulong _opcode;
        public InstHsetp2C(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
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

    struct InstI2fR
    {
        private ulong _opcode;
        public InstI2fR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstI2fI
    {
        private ulong _opcode;
        public InstI2fI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstI2fC
    {
        private ulong _opcode;
        public InstI2fC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstI2iR
    {
        private ulong _opcode;
        public InstI2iR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstI2iI
    {
        private ulong _opcode;
        public InstI2iI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstI2iC
    {
        private ulong _opcode;
        public InstI2iC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstIaddR
    {
        private ulong _opcode;
        public InstIaddR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 48) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
    }

    struct InstIaddI
    {
        private ulong _opcode;
        public InstIaddI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
        public AvgMode AvgMode => (AvgMode)((_opcode >> 48) & 0x3);
        public bool X => (_opcode & 0x80000000000) != 0;
    }

    struct InstIaddC
    {
        private ulong _opcode;
        public InstIaddC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstIadd32i
    {
        private ulong _opcode;
        public InstIadd32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public AvgMode AvgMode => (AvgMode)((_opcode >> 55) & 0x3);
        public bool Sat => (_opcode & 0x40000000000000) != 0;
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
        public bool X => (_opcode & 0x20000000000000) != 0;
    }

    struct InstIadd3R
    {
        private ulong _opcode;
        public InstIadd3R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstIadd3I
    {
        private ulong _opcode;
        public InstIadd3I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstIadd3C
    {
        private ulong _opcode;
        public InstIadd3C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstIcmpR
    {
        private ulong _opcode;
        public InstIcmpR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
    }

    struct InstIcmpI
    {
        private ulong _opcode;
        public InstIcmpI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
    }

    struct InstIcmpC
    {
        private ulong _opcode;
        public InstIcmpC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
    }

    struct InstIcmpRc
    {
        private ulong _opcode;
        public InstIcmpRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public IComp IComp => (IComp)((_opcode >> 49) & 0x7);
        public bool Signed => (_opcode & 0x1000000000000) != 0;
    }

    struct InstIde
    {
        private ulong _opcode;
        public InstIde(ulong opcode) => _opcode = opcode;
        public int Imm16 => (int)((_opcode >> 20) & 0xFFFF);
        public bool Di => (_opcode & 0x20) != 0;
    }

    struct InstIdpR
    {
        private ulong _opcode;
        public InstIdpR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstIdpC
    {
        private ulong _opcode;
        public InstIdpC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImadR
    {
        private ulong _opcode;
        public InstImadR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImadI
    {
        private ulong _opcode;
        public InstImadI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImadC
    {
        private ulong _opcode;
        public InstImadC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImadRc
    {
        private ulong _opcode;
        public InstImadRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImad32i
    {
        private ulong _opcode;
        public InstImad32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImadspR
    {
        private ulong _opcode;
        public InstImadspR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ImadspASelect ASelect => (ImadspASelect)((_opcode >> 48) & 0x7);
        public ImadspBSelect BSelect => (ImadspBSelect)((_opcode >> 53) & 0x3);
        public ImadspASelect CSelect => (ImadspASelect)((int)((_opcode >> 50) & 0x6) | (int)((_opcode >> 48) & 0x1));
    }

    struct InstImadspI
    {
        private ulong _opcode;
        public InstImadspI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ImadspASelect ASelect => (ImadspASelect)((_opcode >> 48) & 0x7);
        public ImadspBSelect BSelect => (ImadspBSelect)((_opcode >> 53) & 0x3);
        public ImadspASelect CSelect => (ImadspASelect)((int)((_opcode >> 50) & 0x6) | (int)((_opcode >> 48) & 0x1));
    }

    struct InstImadspC
    {
        private ulong _opcode;
        public InstImadspC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImadspRc
    {
        private ulong _opcode;
        public InstImadspRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImnmxR
    {
        private ulong _opcode;
        public InstImnmxR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImnmxI
    {
        private ulong _opcode;
        public InstImnmxI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImnmxC
    {
        private ulong _opcode;
        public InstImnmxC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImulR
    {
        private ulong _opcode;
        public InstImulR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool ASigned => (_opcode & 0x10000000000) != 0;
        public bool BSigned => (_opcode & 0x20000000000) != 0;
        public bool Hilo => (_opcode & 0x8000000000) != 0;
    }

    struct InstImulI
    {
        private ulong _opcode;
        public InstImulI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool ASigned => (_opcode & 0x10000000000) != 0;
        public bool BSigned => (_opcode & 0x20000000000) != 0;
        public bool Hilo => (_opcode & 0x8000000000) != 0;
    }

    struct InstImulC
    {
        private ulong _opcode;
        public InstImulC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstImul32i
    {
        private ulong _opcode;
        public InstImul32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool ASigned => (_opcode & 0x40000000000000) != 0;
        public bool BSigned => (_opcode & 0x80000000000000) != 0;
        public bool Hilo => (_opcode & 0x20000000000000) != 0;
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
    }

    struct InstIpa
    {
        private ulong _opcode;
        public InstIpa(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstIsberd
    {
        private ulong _opcode;
        public InstIsberd(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public AlSize AlSize => (AlSize)((_opcode >> 47) & 0x3);
        public IBase IBase => (IBase)((_opcode >> 33) & 0x3);
        public bool O => (_opcode & 0x100000000) != 0;
        public bool P => (_opcode & 0x80000000) != 0;
    }

    struct InstIscaddR
    {
        private ulong _opcode;
        public InstIscaddR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int Imm5 => (int)((_opcode >> 39) & 0x1F);
        public AvgMode AvgMode => (AvgMode)((_opcode >> 48) & 0x3);
    }

    struct InstIscaddI
    {
        private ulong _opcode;
        public InstIscaddI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int Imm5 => (int)((_opcode >> 39) & 0x1F);
        public AvgMode AvgMode => (AvgMode)((_opcode >> 48) & 0x3);
    }

    struct InstIscaddC
    {
        private ulong _opcode;
        public InstIscaddC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public int Imm5 => (int)((_opcode >> 39) & 0x1F);
        public AvgMode AvgMode => (AvgMode)((_opcode >> 48) & 0x3);
    }

    struct InstIscadd32i
    {
        private ulong _opcode;
        public InstIscadd32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool WriteCC => (_opcode & 0x10000000000000) != 0;
        public int Imm5 => (int)((_opcode >> 53) & 0x1F);
    }

    struct InstIsetR
    {
        private ulong _opcode;
        public InstIsetR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstIsetI
    {
        private ulong _opcode;
        public InstIsetI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstIsetC
    {
        private ulong _opcode;
        public InstIsetC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstIsetpR
    {
        private ulong _opcode;
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
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
    }

    struct InstIsetpI
    {
        private ulong _opcode;
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
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
    }

    struct InstIsetpC
    {
        private ulong _opcode;
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
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
    }

    struct InstJcal
    {
        private ulong _opcode;
        public InstJcal(ulong opcode) => _opcode = opcode;
        public int Imm32 => (int)(_opcode >> 20);
        public bool Ca => (_opcode & 0x20) != 0;
        public bool Inc => (_opcode & 0x40) != 0;
    }

    struct InstJmp
    {
        private ulong _opcode;
        public InstJmp(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
        public bool Ca => (_opcode & 0x20) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool Lmt => (_opcode & 0x40) != 0;
        public bool U => (_opcode & 0x80) != 0;
    }

    struct InstJmx
    {
        private ulong _opcode;
        public InstJmx(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
        public bool Ca => (_opcode & 0x20) != 0;
        public int Imm32 => (int)(_opcode >> 20);
        public bool Lmt => (_opcode & 0x40) != 0;
    }

    struct InstKil
    {
        private ulong _opcode;
        public InstKil(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
    }

    struct InstLd
    {
        private ulong _opcode;
        public InstLd(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 58) & 0x7);
        public CacheOpLd CacheOp => (CacheOpLd)((_opcode >> 56) & 0x3);
        public LsSize LsSize => (LsSize)((_opcode >> 53) & 0x7);
        public bool E => (_opcode & 0x10000000000000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
    }

    struct InstLdc
    {
        private ulong _opcode;
        public InstLdc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public AddressMode AddressMode => (AddressMode)((_opcode >> 44) & 0x3);
        public int CbufSlot => (int)((_opcode >> 36) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0xFFFF);
    }

    struct InstLdg
    {
        private ulong _opcode;
        public InstLdg(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize LsSize => (LsSize)((_opcode >> 48) & 0x7);
        public CacheOpLd CacheOp => (CacheOpLd)((_opcode >> 46) & 0x3);
        public bool E => (_opcode & 0x200000000000) != 0;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    struct InstLdl
    {
        private ulong _opcode;
        public InstLdl(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public CacheOp2 CacheOp => (CacheOp2)((_opcode >> 44) & 0x3);
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    struct InstLds
    {
        private ulong _opcode;
        public InstLds(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public bool U => (_opcode & 0x100000000000) != 0;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    struct InstLeaR
    {
        private ulong _opcode;
        public InstLeaR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstLeaI
    {
        private ulong _opcode;
        public InstLeaI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstLeaC
    {
        private ulong _opcode;
        public InstLeaC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstLeaHiR
    {
        private ulong _opcode;
        public InstLeaHiR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstLeaHiC
    {
        private ulong _opcode;
        public InstLeaHiC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstLepc
    {
        private ulong _opcode;
        public InstLepc(ulong opcode) => _opcode = opcode;
    }

    struct InstLongjmp
    {
        private ulong _opcode;
        public InstLongjmp(ulong opcode) => _opcode = opcode;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
    }

    struct InstLopR
    {
        private ulong _opcode;
        public InstLopR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstLopI
    {
        private ulong _opcode;
        public InstLopI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstLopC
    {
        private ulong _opcode;
        public InstLopC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstLop3R
    {
        private ulong _opcode;
        public InstLop3R(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstLop3I
    {
        private ulong _opcode;
        public InstLop3I(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x200000000000000) != 0;
        public int Imm => (int)((_opcode >> 48) & 0xFF);
    }

    struct InstLop3C
    {
        private ulong _opcode;
        public InstLop3C(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstLop32i
    {
        private ulong _opcode;
        public InstLop32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstMembar
    {
        private ulong _opcode;
        public InstMembar(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Membar Membar => (Membar)((_opcode >> 8) & 0x3);
        public Ivall Ivall => (Ivall)((_opcode >> 0) & 0x3);
    }

    struct InstMovR
    {
        private ulong _opcode;
        public InstMovR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int QuadMask => (int)((_opcode >> 39) & 0xF);
    }

    struct InstMovI
    {
        private ulong _opcode;
        public InstMovI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int QuadMask => (int)((_opcode >> 39) & 0xF);
    }

    struct InstMovC
    {
        private ulong _opcode;
        public InstMovC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int QuadMask => (int)((_opcode >> 39) & 0xF);
    }

    struct InstMov32i
    {
        private ulong _opcode;
        public InstMov32i(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int Imm32 => (int)(_opcode >> 20);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int QuadMask => (int)((_opcode >> 12) & 0xF);
    }

    struct InstMufu
    {
        private ulong _opcode;
        public InstMufu(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public MufuOp MufuOp => (MufuOp)((_opcode >> 20) & 0xF);
        public bool AbsA => (_opcode & 0x400000000000) != 0;
        public bool NegA => (_opcode & 0x1000000000000) != 0;
        public bool Sat => (_opcode & 0x4000000000000) != 0;
    }

    struct InstNop
    {
        private ulong _opcode;
        public InstNop(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm16 => (int)((_opcode >> 20) & 0xFFFF);
        public bool Trig => (_opcode & 0x2000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 8) & 0x1F);
    }

    struct InstOutR
    {
        private ulong _opcode;
        public InstOutR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public OutType OutType => (OutType)((_opcode >> 39) & 0x3);
    }

    struct InstOutI
    {
        private ulong _opcode;
        public InstOutI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public OutType OutType => (OutType)((_opcode >> 39) & 0x3);
    }

    struct InstOutC
    {
        private ulong _opcode;
        public InstOutC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public OutType OutType => (OutType)((_opcode >> 39) & 0x3);
    }

    struct InstP2rR
    {
        private ulong _opcode;
        public InstP2rR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    struct InstP2rI
    {
        private ulong _opcode;
        public InstP2rI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    struct InstP2rC
    {
        private ulong _opcode;
        public InstP2rC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    struct InstPbk
    {
        private ulong _opcode;
        public InstPbk(ulong opcode) => _opcode = opcode;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
    }

    struct InstPcnt
    {
        private ulong _opcode;
        public InstPcnt(ulong opcode) => _opcode = opcode;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
    }

    struct InstPexit
    {
        private ulong _opcode;
        public InstPexit(ulong opcode) => _opcode = opcode;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    struct InstPixld
    {
        private ulong _opcode;
        public InstPixld(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int DestPred => (int)((_opcode >> 45) & 0x7);
        public PixMode PixMode => (PixMode)((_opcode >> 31) & 0x7);
        public int Imm8 => (int)((_opcode >> 20) & 0xFF);
    }

    struct InstPlongjmp
    {
        private ulong _opcode;
        public InstPlongjmp(ulong opcode) => _opcode = opcode;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
    }

    struct InstPopcR
    {
        private ulong _opcode;
        public InstPopcR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    struct InstPopcI
    {
        private ulong _opcode;
        public InstPopcI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    struct InstPopcC
    {
        private ulong _opcode;
        public InstPopcC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool NegB => (_opcode & 0x10000000000) != 0;
    }

    struct InstPret
    {
        private ulong _opcode;
        public InstPret(ulong opcode) => _opcode = opcode;
        public bool Ca => (_opcode & 0x20) != 0;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Inc => (_opcode & 0x40) != 0;
    }

    struct InstPrmtR
    {
        private ulong _opcode;
        public InstPrmtR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public PMode PMode => (PMode)((_opcode >> 48) & 0xF);
    }

    struct InstPrmtI
    {
        private ulong _opcode;
        public InstPrmtI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public PMode PMode => (PMode)((_opcode >> 48) & 0xF);
    }

    struct InstPrmtC
    {
        private ulong _opcode;
        public InstPrmtC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public PMode PMode => (PMode)((_opcode >> 48) & 0xF);
    }

    struct InstPrmtRc
    {
        private ulong _opcode;
        public InstPrmtRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public PMode PMode => (PMode)((_opcode >> 48) & 0xF);
    }

    struct InstPset
    {
        private ulong _opcode;
        public InstPset(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstPsetp
    {
        private ulong _opcode;
        public InstPsetp(ulong opcode) => _opcode = opcode;
        public int DestPred => (int)((_opcode >> 3) & 0x7);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
        public int Src2Pred => (int)((_opcode >> 12) & 0x7);
        public bool Src2PredInv => (_opcode & 0x8000) != 0;
        public int Src1Pred => (int)((_opcode >> 29) & 0x7);
        public bool Src1PredInv => (_opcode & 0x100000000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public BoolOp BoolOpAB => (BoolOp)((_opcode >> 24) & 0x3);
        public BoolOp BoolOpC => (BoolOp)((_opcode >> 45) & 0x3);
    }

    struct InstR2b
    {
        private ulong _opcode;
        public InstR2b(ulong opcode) => _opcode = opcode;
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public BarMode Mode => (BarMode)((_opcode >> 32) & 0x3);
        public int Name => (int)((_opcode >> 28) & 0xF);
    }

    struct InstR2pR
    {
        private ulong _opcode;
        public InstR2pR(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    struct InstR2pI
    {
        private ulong _opcode;
        public InstR2pI(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    struct InstR2pC
    {
        private ulong _opcode;
        public InstR2pC(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public ByteSel ByteSel => (ByteSel)((_opcode >> 41) & 0x3);
        public bool Ccpr => (_opcode & 0x10000000000) != 0;
    }

    struct InstRam
    {
        private ulong _opcode;
        public InstRam(ulong opcode) => _opcode = opcode;
    }

    struct InstRed
    {
        private ulong _opcode;
        public InstRed(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 0) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int Imm20 => (int)((_opcode >> 28) & 0xFFFFF);
        public AtomSize RedSize => (AtomSize)((_opcode >> 20) & 0x7);
        public RedOp RedOp => (RedOp)((_opcode >> 23) & 0x7);
        public bool E => (_opcode & 0x1000000000000) != 0;
    }

    struct InstRet
    {
        private ulong _opcode;
        public InstRet(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
    }

    struct InstRroR
    {
        private ulong _opcode;
        public InstRroR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool RroOp => (_opcode & 0x8000000000) != 0;
    }

    struct InstRroI
    {
        private ulong _opcode;
        public InstRroI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool RroOp => (_opcode & 0x8000000000) != 0;
    }

    struct InstRroC
    {
        private ulong _opcode;
        public InstRroC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool AbsB => (_opcode & 0x2000000000000) != 0;
        public bool NegB => (_opcode & 0x200000000000) != 0;
        public bool RroOp => (_opcode & 0x8000000000) != 0;
    }

    struct InstRtt
    {
        private ulong _opcode;
        public InstRtt(ulong opcode) => _opcode = opcode;
    }

    struct InstS2r
    {
        private ulong _opcode;
        public InstS2r(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public SReg SReg => (SReg)((_opcode >> 20) & 0xFF);
    }

    struct InstSam
    {
        private ulong _opcode;
        public InstSam(ulong opcode) => _opcode = opcode;
    }

    struct InstSelR
    {
        private ulong _opcode;
        public InstSelR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    struct InstSelI
    {
        private ulong _opcode;
        public InstSelI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    struct InstSelC
    {
        private ulong _opcode;
        public InstSelC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
    }

    struct InstSetcrsptr
    {
        private ulong _opcode;
        public InstSetcrsptr(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
    }

    struct InstSetlmembase
    {
        private ulong _opcode;
        public InstSetlmembase(ulong opcode) => _opcode = opcode;
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
    }

    struct InstShfLR
    {
        private ulong _opcode;
        public InstShfLR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstShfRR
    {
        private ulong _opcode;
        public InstShfRR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstShfLI
    {
        private ulong _opcode;
        public InstShfLI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstShfRI
    {
        private ulong _opcode;
        public InstShfRI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstShfl
    {
        private ulong _opcode;
        public InstShfl(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstShlR
    {
        private ulong _opcode;
        public InstShlR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x80000000000) != 0;
        public bool M => (_opcode & 0x8000000000) != 0;
    }

    struct InstShlI
    {
        private ulong _opcode;
        public InstShlI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Imm20 => (int)((_opcode >> 37) & 0x80000) | (int)((_opcode >> 20) & 0x7FFFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x80000000000) != 0;
        public bool M => (_opcode & 0x8000000000) != 0;
    }

    struct InstShlC
    {
        private ulong _opcode;
        public InstShlC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int CbufSlot => (int)((_opcode >> 34) & 0x1F);
        public int CbufOffset => (int)((_opcode >> 20) & 0x3FFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool WriteCC => (_opcode & 0x800000000000) != 0;
        public bool X => (_opcode & 0x80000000000) != 0;
        public bool M => (_opcode & 0x8000000000) != 0;
    }

    struct InstShrR
    {
        private ulong _opcode;
        public InstShrR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstShrI
    {
        private ulong _opcode;
        public InstShrI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstShrC
    {
        private ulong _opcode;
        public InstShrC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSsy
    {
        private ulong _opcode;
        public InstSsy(ulong opcode) => _opcode = opcode;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
        public bool Ca => (_opcode & 0x20) != 0;
    }

    struct InstSt
    {
        private ulong _opcode;
        public InstSt(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 58) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 56) & 0x3);
        public LsSize LsSize => (LsSize)((_opcode >> 53) & 0x7);
        public bool E => (_opcode & 0x10000000000000) != 0;
        public int Imm32 => (int)(_opcode >> 20);
    }

    struct InstStg
    {
        private ulong _opcode;
        public InstStg(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 46) & 0x3);
        public bool E => (_opcode & 0x200000000000) != 0;
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    struct InstStl
    {
        private ulong _opcode;
        public InstStl(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 44) & 0x3);
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    struct InstStp
    {
        private ulong _opcode;
        public InstStp(ulong opcode) => _opcode = opcode;
        public bool Wait => (_opcode & 0x80000000) != 0;
        public int Imm8 => (int)((_opcode >> 20) & 0xFF);
    }

    struct InstSts
    {
        private ulong _opcode;
        public InstSts(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public LsSize2 LsSize => (LsSize2)((_opcode >> 48) & 0x7);
        public int Imm24 => (int)((_opcode >> 20) & 0xFFFFFF);
    }

    struct InstSuatomB
    {
        private ulong _opcode;
        public InstSuatomB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSuatom
    {
        private ulong _opcode;
        public InstSuatom(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSuatomB2
    {
        private ulong _opcode;
        public InstSuatomB2(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSuatomCasB
    {
        private ulong _opcode;
        public InstSuatomCasB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSuatomCas
    {
        private ulong _opcode;
        public InstSuatomCas(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSuldDB
    {
        private ulong _opcode;
        public InstSuldDB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSuldD
    {
        private ulong _opcode;
        public InstSuldD(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSuldB
    {
        private ulong _opcode;
        public InstSuldB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSuld
    {
        private ulong _opcode;
        public InstSuld(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSuredB
    {
        private ulong _opcode;
        public InstSuredB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSured
    {
        private ulong _opcode;
        public InstSured(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSustDB
    {
        private ulong _opcode;
        public InstSustDB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSustD
    {
        private ulong _opcode;
        public InstSustD(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstSustB
    {
        private ulong _opcode;
        public InstSustB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcC => (int)((_opcode >> 39) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 24) & 0x3);
        public SuRgba Rgba => (SuRgba)((_opcode >> 20) & 0xF);
    }

    struct InstSust
    {
        private ulong _opcode;
        public InstSust(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Clamp Clamp => (Clamp)((_opcode >> 49) & 0x3);
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public SuDim Dim => (SuDim)((_opcode >> 33) & 0x7);
        public CacheOpSt CacheOp => (CacheOpSt)((_opcode >> 24) & 0x3);
        public SuRgba Rgba => (SuRgba)((_opcode >> 20) & 0xF);
    }

    struct InstSync
    {
        private ulong _opcode;
        public InstSync(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public Ccc Ccc => (Ccc)((_opcode >> 0) & 0x1F);
    }

    struct InstTex
    {
        private ulong _opcode;
        public InstTex(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTexB
    {
        private ulong _opcode;
        public InstTexB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTexs
    {
        private ulong _opcode;
        public InstTexs(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTld
    {
        private ulong _opcode;
        public InstTld(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTldB
    {
        private ulong _opcode;
        public InstTldB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTlds
    {
        private ulong _opcode;
        public InstTlds(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTld4
    {
        private ulong _opcode;
        public InstTld4(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTld4B
    {
        private ulong _opcode;
        public InstTld4B(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTld4s
    {
        private ulong _opcode;
        public InstTld4s(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTmml
    {
        private ulong _opcode;
        public InstTmml(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTmmlB
    {
        private ulong _opcode;
        public InstTmmlB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int SrcB => (int)((_opcode >> 20) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public bool Ndv => (_opcode & 0x800000000) != 0;
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public TexDim Dim => (TexDim)((_opcode >> 28) & 0x7);
    }

    struct InstTxa
    {
        private ulong _opcode;
        public InstTxa(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public bool Ndv => (_opcode & 0x800000000) != 0;
        public int WMask => (int)((_opcode >> 31) & 0xF);
    }

    struct InstTxd
    {
        private ulong _opcode;
        public InstTxd(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTxdB
    {
        private ulong _opcode;
        public InstTxdB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstTxq
    {
        private ulong _opcode;
        public InstTxq(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int TidB => (int)((_opcode >> 36) & 0x1FFF);
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public TexQuery TexQuery => (TexQuery)((_opcode >> 22) & 0x3F);
    }

    struct InstTxqB
    {
        private ulong _opcode;
        public InstTxqB(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int SrcA => (int)((_opcode >> 8) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public bool Nodep => (_opcode & 0x2000000000000) != 0;
        public int WMask => (int)((_opcode >> 31) & 0xF);
        public TexQuery TexQuery => (TexQuery)((_opcode >> 22) & 0x3F);
    }

    struct InstVabsdiff
    {
        private ulong _opcode;
        public InstVabsdiff(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstVabsdiff4
    {
        private ulong _opcode;
        public InstVabsdiff4(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstVadd
    {
        private ulong _opcode;
        public InstVadd(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstVmad
    {
        private ulong _opcode;
        public InstVmad(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstVmnmx
    {
        private ulong _opcode;
        public InstVmnmx(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstVote
    {
        private ulong _opcode;
        public InstVote(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public int SrcPred => (int)((_opcode >> 39) & 0x7);
        public bool SrcPredInv => (_opcode & 0x40000000000) != 0;
        public VoteMode VoteMode => (VoteMode)((_opcode >> 48) & 0x3);
        public int VpDest => (int)((_opcode >> 45) & 0x7);
    }

    struct InstVotevtg
    {
        private ulong _opcode;
        public InstVotevtg(ulong opcode) => _opcode = opcode;
        public int Pred => (int)((_opcode >> 16) & 0x7);
        public bool PredInv => (_opcode & 0x80000) != 0;
        public VoteMode VoteMode => (VoteMode)((_opcode >> 48) & 0x3);
        public int Imm28 => (int)((_opcode >> 20) & 0xFFFFFFF);
    }

    struct InstVset
    {
        private ulong _opcode;
        public InstVset(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstVsetp
    {
        private ulong _opcode;
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
        public int DestPredInv => (int)((_opcode >> 0) & 0x7);
        public bool BVideo => (_opcode & 0x4000000000000) != 0;
    }

    struct InstVshl
    {
        private ulong _opcode;
        public InstVshl(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstVshr
    {
        private ulong _opcode;
        public InstVshr(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstXmadR
    {
        private ulong _opcode;
        public InstXmadR(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstXmadI
    {
        private ulong _opcode;
        public InstXmadI(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstXmadC
    {
        private ulong _opcode;
        public InstXmadC(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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

    struct InstXmadRc
    {
        private ulong _opcode;
        public InstXmadRc(ulong opcode) => _opcode = opcode;
        public int Dest => (int)((_opcode >> 0) & 0xFF);
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