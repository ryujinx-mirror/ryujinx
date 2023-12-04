// This file was auto-generated from NVIDIA official Maxwell definitions.

using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Gpu.Engine.Twod
{
    /// <summary>
    /// Notify type.
    /// </summary>
    enum NotifyType
    {
        WriteOnly = 0,
        WriteThenAwaken = 1,
    }

    /// <summary>
    /// Format of the destination texture.
    /// </summary>
    enum SetDstFormatV
    {
        A8r8g8b8 = 207,
        A8rl8gl8bl8 = 208,
        A2r10g10b10 = 223,
        A8b8g8r8 = 213,
        A8bl8gl8rl8 = 214,
        A2b10g10r10 = 209,
        X8r8g8b8 = 230,
        X8rl8gl8bl8 = 231,
        X8b8g8r8 = 249,
        X8bl8gl8rl8 = 250,
        R5g6b5 = 232,
        A1r5g5b5 = 233,
        X1r5g5b5 = 248,
        Y8 = 243,
        Y16 = 238,
        Y32 = 255,
        Z1r5g5b5 = 251,
        O1r5g5b5 = 252,
        Z8r8g8b8 = 253,
        O8r8g8b8 = 254,
        Y18x8 = 28,
        Rf16 = 242,
        Rf32 = 229,
        Rf32Gf32 = 203,
        Rf16Gf16Bf16Af16 = 202,
        Rf16Gf16Bf16X16 = 206,
        Rf32Gf32Bf32Af32 = 192,
        Rf32Gf32Bf32X32 = 195,
        R16G16B16A16 = 198,
        Rn16Gn16Bn16An16 = 199,
        Bf10gf11rf11 = 224,
        An8bn8gn8rn8 = 215,
        Rf16Gf16 = 222,
        R16G16 = 218,
        Rn16Gn16 = 219,
        G8r8 = 234,
        Gn8rn8 = 235,
        Rn16 = 239,
        Rn8 = 244,
        A8 = 247,
    }

    /// <summary>
    /// Memory layout of the destination texture.
    /// </summary>
    enum SetDstMemoryLayoutV
    {
        Blocklinear = 0,
        Pitch = 1,
    }

    /// <summary>
    /// Height in GOBs of the destination texture.
    /// </summary>
    enum SetDstBlockSizeHeight
    {
        OneGob = 0,
        TwoGobs = 1,
        FourGobs = 2,
        EightGobs = 3,
        SixteenGobs = 4,
        ThirtytwoGobs = 5,
    }

    /// <summary>
    /// Depth in GOBs of the destination texture.
    /// </summary>
    enum SetDstBlockSizeDepth
    {
        OneGob = 0,
        TwoGobs = 1,
        FourGobs = 2,
        EightGobs = 3,
        SixteenGobs = 4,
        ThirtytwoGobs = 5,
    }

    /// <summary>
    /// Format of the source texture.
    /// </summary>
    enum SetSrcFormatV
    {
        A8r8g8b8 = 207,
        A8rl8gl8bl8 = 208,
        A2r10g10b10 = 223,
        A8b8g8r8 = 213,
        A8bl8gl8rl8 = 214,
        A2b10g10r10 = 209,
        X8r8g8b8 = 230,
        X8rl8gl8bl8 = 231,
        X8b8g8r8 = 249,
        X8bl8gl8rl8 = 250,
        R5g6b5 = 232,
        A1r5g5b5 = 233,
        X1r5g5b5 = 248,
        Y8 = 243,
        Ay8 = 29,
        Y16 = 238,
        Y32 = 255,
        Z1r5g5b5 = 251,
        O1r5g5b5 = 252,
        Z8r8g8b8 = 253,
        O8r8g8b8 = 254,
        Y18x8 = 28,
        Rf16 = 242,
        Rf32 = 229,
        Rf32Gf32 = 203,
        Rf16Gf16Bf16Af16 = 202,
        Rf16Gf16Bf16X16 = 206,
        Rf32Gf32Bf32Af32 = 192,
        Rf32Gf32Bf32X32 = 195,
        R16G16B16A16 = 198,
        Rn16Gn16Bn16An16 = 199,
        Bf10gf11rf11 = 224,
        An8bn8gn8rn8 = 215,
        Rf16Gf16 = 222,
        R16G16 = 218,
        Rn16Gn16 = 219,
        G8r8 = 234,
        Gn8rn8 = 235,
        Rn16 = 239,
        Rn8 = 244,
        A8 = 247,
    }

    /// <summary>
    /// Memory layout of the source texture.
    /// </summary>
    enum SetSrcMemoryLayoutV
    {
        Blocklinear = 0,
        Pitch = 1,
    }

    /// <summary>
    /// Height in GOBs of the source texture.
    /// </summary>
    enum SetSrcBlockSizeHeight
    {
        OneGob = 0,
        TwoGobs = 1,
        FourGobs = 2,
        EightGobs = 3,
        SixteenGobs = 4,
        ThirtytwoGobs = 5,
    }

    /// <summary>
    /// Depth in GOBs of the source texture.
    /// </summary>
    enum SetSrcBlockSizeDepth
    {
        OneGob = 0,
        TwoGobs = 1,
        FourGobs = 2,
        EightGobs = 3,
        SixteenGobs = 4,
        ThirtytwoGobs = 5,
    }

    /// <summary>
    /// Texture data caches to invalidate.
    /// </summary>
    enum TwodInvalidateTextureDataCacheV
    {
        L1Only = 0,
        L2Only = 1,
        L1AndL2 = 2,
    }

    /// <summary>
    /// Sector promotion parameters.
    /// </summary>
    enum SetPixelsFromMemorySectorPromotionV
    {
        NoPromotion = 0,
        PromoteTo2V = 1,
        PromoteTo2H = 2,
        PromoteTo4 = 3,
    }

    /// <summary>
    /// Number of processing clusters.
    /// </summary>
    enum SetNumProcessingClustersV
    {
        All = 0,
        One = 1,
    }

    /// <summary>
    /// Color key format.
    /// </summary>
    enum SetColorKeyFormatV
    {
        A16r5g6b5 = 0,
        A1r5g5b5 = 1,
        A8r8g8b8 = 2,
        A2r10g10b10 = 3,
        Y8 = 4,
        Y16 = 5,
        Y32 = 6,
    }

    /// <summary>
    /// Color blit operation.
    /// </summary>
    enum SetOperationV
    {
        SrccopyAnd = 0,
        RopAnd = 1,
        BlendAnd = 2,
        Srccopy = 3,
        Rop = 4,
        SrccopyPremult = 5,
        BlendPremult = 6,
    }

    /// <summary>
    /// Texture pattern selection.
    /// </summary>
    enum SetPatternSelectV
    {
        Monochrome8x8 = 0,
        Monochrome64x1 = 1,
        Monochrome1x64 = 2,
        Color = 3,
    }

    /// <summary>
    /// Render enable override mode.
    /// </summary>
    enum SetRenderEnableOverrideMode
    {
        UseRenderEnable = 0,
        AlwaysRender = 1,
        NeverRender = 2,
    }

    /// <summary>
    /// Pixels from memory horizontal direction.
    /// </summary>
    enum SetPixelsFromMemoryDirectionHorizontal
    {
        HwDecides = 0,
        LeftToRight = 1,
        RightToLeft = 2,
    }

    /// <summary>
    /// Pixels from memory vertical direction.
    /// </summary>
    enum SetPixelsFromMemoryDirectionVertical
    {
        HwDecides = 0,
        TopToBottom = 1,
        BottomToTop = 2,
    }

    /// <summary>
    /// Color format of the monochrome pattern.
    /// </summary>
    enum SetMonochromePatternColorFormatV
    {
        A8x8r5g6b5 = 0,
        A1r5g5b5 = 1,
        A8r8g8b8 = 2,
        A8y8 = 3,
        A8x8y16 = 4,
        Y32 = 5,
        ByteExpand = 6,
    }

    /// <summary>
    /// Format of the monochrome pattern.
    /// </summary>
    enum SetMonochromePatternFormatV
    {
        Cga6M1 = 0,
        LeM1 = 1,
    }

    /// <summary>
    /// DMA semaphore reduction operation.
    /// </summary>
    enum MmeDmaReductionReductionOp
    {
        RedAdd = 0,
        RedMin = 1,
        RedMax = 2,
        RedInc = 3,
        RedDec = 4,
        RedAnd = 5,
        RedOr = 6,
        RedXor = 7,
    }

    /// <summary>
    /// DMA semaphore reduction format.
    /// </summary>
    enum MmeDmaReductionReductionFormat
    {
        Unsigned = 0,
        Signed = 1,
    }

    /// <summary>
    /// DMA semaphore reduction size.
    /// </summary>
    enum MmeDmaReductionReductionSize
    {
        FourBytes = 0,
        EightBytes = 1,
    }

    /// <summary>
    /// Data FIFO size.
    /// </summary>
    enum SetMmeDataFifoConfigFifoSize
    {
        Size0kb = 0,
        Size4kb = 1,
        Size8kb = 2,
        Size12kb = 3,
        Size16kb = 4,
    }

    /// <summary>
    /// Render solid primitive mode.
    /// </summary>
    enum RenderSolidPrimModeV
    {
        Points = 0,
        Lines = 1,
        Polyline = 2,
        Triangles = 3,
        Rects = 4,
    }

    /// <summary>
    /// Render solid primitive color format.
    /// </summary>
    enum SetRenderSolidPrimColorFormatV
    {
        Rf32Gf32Bf32Af32 = 192,
        Rf16Gf16Bf16Af16 = 202,
        Rf32Gf32 = 203,
        A8r8g8b8 = 207,
        A2r10g10b10 = 223,
        A8b8g8r8 = 213,
        A2b10g10r10 = 209,
        X8r8g8b8 = 230,
        X8b8g8r8 = 249,
        R5g6b5 = 232,
        A1r5g5b5 = 233,
        X1r5g5b5 = 248,
        Y8 = 243,
        Y16 = 238,
        Y32 = 255,
        Z1r5g5b5 = 251,
        O1r5g5b5 = 252,
        Z8r8g8b8 = 253,
        O8r8g8b8 = 254,
    }

    /// <summary>
    /// Pixels from CPU data type.
    /// </summary>
    enum SetPixelsFromCpuDataTypeV
    {
        Color = 0,
        Index = 1,
    }

    /// <summary>
    /// Pixels from CPU color format.
    /// </summary>
    enum SetPixelsFromCpuColorFormatV
    {
        A8r8g8b8 = 207,
        A2r10g10b10 = 223,
        A8b8g8r8 = 213,
        A2b10g10r10 = 209,
        X8r8g8b8 = 230,
        X8b8g8r8 = 249,
        R5g6b5 = 232,
        A1r5g5b5 = 233,
        X1r5g5b5 = 248,
        Y8 = 243,
        Y16 = 238,
        Y32 = 255,
        Z1r5g5b5 = 251,
        O1r5g5b5 = 252,
        Z8r8g8b8 = 253,
        O8r8g8b8 = 254,
    }

    /// <summary>
    /// Pixels from CPU palette index format.
    /// </summary>
    enum SetPixelsFromCpuIndexFormatV
    {
        I1 = 0,
        I4 = 1,
        I8 = 2,
    }

    /// <summary>
    /// Pixels from CPU monochrome format.
    /// </summary>
    enum SetPixelsFromCpuMonoFormatV
    {
        Cga6M1 = 0,
        LeM1 = 1,
    }

    /// <summary>
    /// Pixels from CPU wrap mode.
    /// </summary>
    enum SetPixelsFromCpuWrapV
    {
        WrapPixel = 0,
        WrapByte = 1,
        WrapDword = 2,
    }

    /// <summary>
    /// Pixels from CPU monochrome opacity.
    /// </summary>
    enum SetPixelsFromCpuMonoOpacityV
    {
        Transparent = 0,
        Opaque = 1,
    }

    /// <summary>
    /// Pixels from memory block shape.
    /// </summary>
    enum SetPixelsFromMemoryBlockShapeV
    {
        Auto = 0,
        Shape8x8 = 1,
        Shape16x4 = 2,
    }

    /// <summary>
    /// Pixels from memory origin.
    /// </summary>
    enum SetPixelsFromMemorySampleModeOrigin
    {
        Center = 0,
        Corner = 1,
    }

    /// <summary>
    /// Pixels from memory filter mode.
    /// </summary>
    enum SetPixelsFromMemorySampleModeFilter
    {
        Point = 0,
        Bilinear = 1,
    }

    /// <summary>
    /// Render solid primitive point coordinates.
    /// </summary>
    struct RenderSolidPrimPoint
    {
#pragma warning disable CS0649 // Field is never assigned to
        public uint SetX;
        public uint Y;
#pragma warning restore CS0649
    }

    /// <summary>
    /// 2D class state.
    /// </summary>
    unsafe struct TwodClassState : IShadowState
    {
#pragma warning disable CS0649 // Field is never assigned to
        public uint SetObject;
        public readonly int SetObjectClassId => (int)(SetObject & 0xFFFF);
        public readonly int SetObjectEngineId => (int)((SetObject >> 16) & 0x1F);
        public fixed uint Reserved04[63];
        public uint NoOperation;
        public uint SetNotifyA;
        public readonly int SetNotifyAAddressUpper => (int)(SetNotifyA & 0x1FFFFFF);
        public uint SetNotifyB;
        public uint Notify;
        public readonly NotifyType NotifyType => (NotifyType)(Notify);
        public uint WaitForIdle;
        public uint LoadMmeInstructionRamPointer;
        public uint LoadMmeInstructionRam;
        public uint LoadMmeStartAddressRamPointer;
        public uint LoadMmeStartAddressRam;
        public uint SetMmeShadowRamControl;
        public readonly SetMmeShadowRamControlMode SetMmeShadowRamControlMode => (SetMmeShadowRamControlMode)(SetMmeShadowRamControl & 0x3);
        public fixed uint Reserved128[2];
        public uint SetGlobalRenderEnableA;
        public readonly int SetGlobalRenderEnableAOffsetUpper => (int)(SetGlobalRenderEnableA & 0xFF);
        public uint SetGlobalRenderEnableB;
        public uint SetGlobalRenderEnableC;
        public readonly int SetGlobalRenderEnableCMode => (int)(SetGlobalRenderEnableC & 0x7);
        public uint SendGoIdle;
        public uint PmTrigger;
        public fixed uint Reserved144[3];
        public uint SetInstrumentationMethodHeader;
        public uint SetInstrumentationMethodData;
        public fixed uint Reserved158[37];
        public uint SetMmeSwitchState;
        public readonly bool SetMmeSwitchStateValid => (SetMmeSwitchState & 0x1) != 0;
        public readonly int SetMmeSwitchStateSaveMacro => (int)((SetMmeSwitchState >> 4) & 0xFF);
        public readonly int SetMmeSwitchStateRestoreMacro => (int)((SetMmeSwitchState >> 12) & 0xFF);
        public fixed uint Reserved1F0[4];
        public uint SetDstFormat;
        public readonly SetDstFormatV SetDstFormatV => (SetDstFormatV)(SetDstFormat & 0xFF);
        public uint SetDstMemoryLayout;
        public readonly SetDstMemoryLayoutV SetDstMemoryLayoutV => (SetDstMemoryLayoutV)(SetDstMemoryLayout & 0x1);
        public uint SetDstBlockSize;
        public readonly SetDstBlockSizeHeight SetDstBlockSizeHeight => (SetDstBlockSizeHeight)((SetDstBlockSize >> 4) & 0x7);
        public readonly SetDstBlockSizeDepth SetDstBlockSizeDepth => (SetDstBlockSizeDepth)((SetDstBlockSize >> 8) & 0x7);
        public uint SetDstDepth;
        public uint SetDstLayer;
        public uint SetDstPitch;
        public uint SetDstWidth;
        public uint SetDstHeight;
        public uint SetDstOffsetUpper;
        public readonly int SetDstOffsetUpperV => (int)(SetDstOffsetUpper & 0xFF);
        public uint SetDstOffsetLower;
        public uint FlushAndInvalidateRopMiniCache;
        public readonly bool FlushAndInvalidateRopMiniCacheV => (FlushAndInvalidateRopMiniCache & 0x1) != 0;
        public uint SetSpareNoop06;
        public uint SetSrcFormat;
        public readonly SetSrcFormatV SetSrcFormatV => (SetSrcFormatV)(SetSrcFormat & 0xFF);
        public uint SetSrcMemoryLayout;
        public readonly SetSrcMemoryLayoutV SetSrcMemoryLayoutV => (SetSrcMemoryLayoutV)(SetSrcMemoryLayout & 0x1);
        public uint SetSrcBlockSize;
        public readonly SetSrcBlockSizeHeight SetSrcBlockSizeHeight => (SetSrcBlockSizeHeight)((SetSrcBlockSize >> 4) & 0x7);
        public readonly SetSrcBlockSizeDepth SetSrcBlockSizeDepth => (SetSrcBlockSizeDepth)((SetSrcBlockSize >> 8) & 0x7);
        public uint SetSrcDepth;
        public uint TwodInvalidateTextureDataCache;
        public readonly TwodInvalidateTextureDataCacheV TwodInvalidateTextureDataCacheV => (TwodInvalidateTextureDataCacheV)(TwodInvalidateTextureDataCache & 0x3);
        public uint SetSrcPitch;
        public uint SetSrcWidth;
        public uint SetSrcHeight;
        public uint SetSrcOffsetUpper;
        public readonly int SetSrcOffsetUpperV => (int)(SetSrcOffsetUpper & 0xFF);
        public uint SetSrcOffsetLower;
        public uint SetPixelsFromMemorySectorPromotion;
        public readonly SetPixelsFromMemorySectorPromotionV SetPixelsFromMemorySectorPromotionV => (SetPixelsFromMemorySectorPromotionV)(SetPixelsFromMemorySectorPromotion & 0x3);
        public uint SetSpareNoop12;
        public uint SetNumProcessingClusters;
        public readonly SetNumProcessingClustersV SetNumProcessingClustersV => (SetNumProcessingClustersV)(SetNumProcessingClusters & 0x1);
        public uint SetRenderEnableA;
        public readonly int SetRenderEnableAOffsetUpper => (int)(SetRenderEnableA & 0xFF);
        public uint SetRenderEnableB;
        public uint SetRenderEnableC;
        public readonly int SetRenderEnableCMode => (int)(SetRenderEnableC & 0x7);
        public uint SetSpareNoop08;
        public uint SetSpareNoop01;
        public uint SetSpareNoop11;
        public uint SetSpareNoop07;
        public uint SetClipX0;
        public uint SetClipY0;
        public uint SetClipWidth;
        public uint SetClipHeight;
        public uint SetClipEnable;
        public readonly bool SetClipEnableV => (SetClipEnable & 0x1) != 0;
        public uint SetColorKeyFormat;
        public readonly SetColorKeyFormatV SetColorKeyFormatV => (SetColorKeyFormatV)(SetColorKeyFormat & 0x7);
        public uint SetColorKey;
        public uint SetColorKeyEnable;
        public readonly bool SetColorKeyEnableV => (SetColorKeyEnable & 0x1) != 0;
        public uint SetRop;
        public readonly int SetRopV => (int)(SetRop & 0xFF);
        public uint SetBeta1;
        public uint SetBeta4;
        public readonly int SetBeta4B => (int)(SetBeta4 & 0xFF);
        public readonly int SetBeta4G => (int)((SetBeta4 >> 8) & 0xFF);
        public readonly int SetBeta4R => (int)((SetBeta4 >> 16) & 0xFF);
        public readonly int SetBeta4A => (int)((SetBeta4 >> 24) & 0xFF);
        public uint SetOperation;
        public readonly SetOperationV SetOperationV => (SetOperationV)(SetOperation & 0x7);
        public uint SetPatternOffset;
        public readonly int SetPatternOffsetX => (int)(SetPatternOffset & 0x3F);
        public readonly int SetPatternOffsetY => (int)((SetPatternOffset >> 8) & 0x3F);
        public uint SetPatternSelect;
        public readonly SetPatternSelectV SetPatternSelectV => (SetPatternSelectV)(SetPatternSelect & 0x3);
        public uint SetDstColorRenderToZetaSurface;
        public readonly bool SetDstColorRenderToZetaSurfaceV => (SetDstColorRenderToZetaSurface & 0x1) != 0;
        public uint SetSpareNoop04;
        public uint SetSpareNoop15;
        public uint SetSpareNoop13;
        public uint SetSpareNoop03;
        public uint SetSpareNoop14;
        public uint SetSpareNoop02;
        public uint SetCompression;
        public readonly bool SetCompressionEnable => (SetCompression & 0x1) != 0;
        public uint SetSpareNoop09;
        public uint SetRenderEnableOverride;
        public readonly SetRenderEnableOverrideMode SetRenderEnableOverrideMode => (SetRenderEnableOverrideMode)(SetRenderEnableOverride & 0x3);
        public uint SetPixelsFromMemoryDirection;
        public readonly SetPixelsFromMemoryDirectionHorizontal SetPixelsFromMemoryDirectionHorizontal => (SetPixelsFromMemoryDirectionHorizontal)(SetPixelsFromMemoryDirection & 0x3);
        public readonly SetPixelsFromMemoryDirectionVertical SetPixelsFromMemoryDirectionVertical => (SetPixelsFromMemoryDirectionVertical)((SetPixelsFromMemoryDirection >> 4) & 0x3);
        public uint SetSpareNoop10;
        public uint SetMonochromePatternColorFormat;
        public readonly SetMonochromePatternColorFormatV SetMonochromePatternColorFormatV => (SetMonochromePatternColorFormatV)(SetMonochromePatternColorFormat & 0x7);
        public uint SetMonochromePatternFormat;
        public readonly SetMonochromePatternFormatV SetMonochromePatternFormatV => (SetMonochromePatternFormatV)(SetMonochromePatternFormat & 0x1);
        public uint SetMonochromePatternColor0;
        public uint SetMonochromePatternColor1;
        public uint SetMonochromePattern0;
        public uint SetMonochromePattern1;
        public Array64<uint> ColorPatternX8r8g8b8;
        public int ColorPatternX8r8g8b8B0(int i) => (int)((ColorPatternX8r8g8b8[i] >> 0) & 0xFF);
        public int ColorPatternX8r8g8b8G0(int i) => (int)((ColorPatternX8r8g8b8[i] >> 8) & 0xFF);
        public int ColorPatternX8r8g8b8R0(int i) => (int)((ColorPatternX8r8g8b8[i] >> 16) & 0xFF);
        public int ColorPatternX8r8g8b8Ignore0(int i) => (int)((ColorPatternX8r8g8b8[i] >> 24) & 0xFF);
        public Array32<uint> ColorPatternR5g6b5;
        public int ColorPatternR5g6b5B0(int i) => (int)((ColorPatternR5g6b5[i] >> 0) & 0x1F);
        public int ColorPatternR5g6b5G0(int i) => (int)((ColorPatternR5g6b5[i] >> 5) & 0x3F);
        public int ColorPatternR5g6b5R0(int i) => (int)((ColorPatternR5g6b5[i] >> 11) & 0x1F);
        public int ColorPatternR5g6b5B1(int i) => (int)((ColorPatternR5g6b5[i] >> 16) & 0x1F);
        public int ColorPatternR5g6b5G1(int i) => (int)((ColorPatternR5g6b5[i] >> 21) & 0x3F);
        public int ColorPatternR5g6b5R1(int i) => (int)((ColorPatternR5g6b5[i] >> 27) & 0x1F);
        public Array32<uint> ColorPatternX1r5g5b5;
        public int ColorPatternX1r5g5b5B0(int i) => (int)((ColorPatternX1r5g5b5[i] >> 0) & 0x1F);
        public int ColorPatternX1r5g5b5G0(int i) => (int)((ColorPatternX1r5g5b5[i] >> 5) & 0x1F);
        public int ColorPatternX1r5g5b5R0(int i) => (int)((ColorPatternX1r5g5b5[i] >> 10) & 0x1F);
        public bool ColorPatternX1r5g5b5Ignore0(int i) => (ColorPatternX1r5g5b5[i] & 0x8000) != 0;
        public int ColorPatternX1r5g5b5B1(int i) => (int)((ColorPatternX1r5g5b5[i] >> 16) & 0x1F);
        public int ColorPatternX1r5g5b5G1(int i) => (int)((ColorPatternX1r5g5b5[i] >> 21) & 0x1F);
        public int ColorPatternX1r5g5b5R1(int i) => (int)((ColorPatternX1r5g5b5[i] >> 26) & 0x1F);
        public bool ColorPatternX1r5g5b5Ignore1(int i) => (ColorPatternX1r5g5b5[i] & 0x80000000) != 0;
        public Array16<uint> ColorPatternY8;
        public int ColorPatternY8Y0(int i) => (int)((ColorPatternY8[i] >> 0) & 0xFF);
        public int ColorPatternY8Y1(int i) => (int)((ColorPatternY8[i] >> 8) & 0xFF);
        public int ColorPatternY8Y2(int i) => (int)((ColorPatternY8[i] >> 16) & 0xFF);
        public int ColorPatternY8Y3(int i) => (int)((ColorPatternY8[i] >> 24) & 0xFF);
        public uint SetRenderSolidPrimColor0;
        public uint SetRenderSolidPrimColor1;
        public uint SetRenderSolidPrimColor2;
        public uint SetRenderSolidPrimColor3;
        public uint SetMmeMemAddressA;
        public readonly int SetMmeMemAddressAUpper => (int)(SetMmeMemAddressA & 0x1FFFFFF);
        public uint SetMmeMemAddressB;
        public uint SetMmeDataRamAddress;
        public uint MmeDmaRead;
        public uint MmeDmaReadFifoed;
        public uint MmeDmaWrite;
        public uint MmeDmaReduction;
        public readonly MmeDmaReductionReductionOp MmeDmaReductionReductionOp => (MmeDmaReductionReductionOp)(MmeDmaReduction & 0x7);
        public readonly MmeDmaReductionReductionFormat MmeDmaReductionReductionFormat => (MmeDmaReductionReductionFormat)((MmeDmaReduction >> 4) & 0x3);
        public readonly MmeDmaReductionReductionSize MmeDmaReductionReductionSize => (MmeDmaReductionReductionSize)((MmeDmaReduction >> 8) & 0x1);
        public uint MmeDmaSysmembar;
        public readonly bool MmeDmaSysmembarV => (MmeDmaSysmembar & 0x1) != 0;
        public uint MmeDmaSync;
        public uint SetMmeDataFifoConfig;
        public readonly SetMmeDataFifoConfigFifoSize SetMmeDataFifoConfigFifoSize => (SetMmeDataFifoConfigFifoSize)(SetMmeDataFifoConfig & 0x7);
        public fixed uint Reserved578[2];
        public uint RenderSolidPrimMode;
        public readonly RenderSolidPrimModeV RenderSolidPrimModeV => (RenderSolidPrimModeV)(RenderSolidPrimMode & 0x7);
        public uint SetRenderSolidPrimColorFormat;
        public readonly SetRenderSolidPrimColorFormatV SetRenderSolidPrimColorFormatV => (SetRenderSolidPrimColorFormatV)(SetRenderSolidPrimColorFormat & 0xFF);
        public uint SetRenderSolidPrimColor;
        public uint SetRenderSolidLineTieBreakBits;
        public readonly bool SetRenderSolidLineTieBreakBitsXmajXincYinc => (SetRenderSolidLineTieBreakBits & 0x1) != 0;
        public readonly bool SetRenderSolidLineTieBreakBitsXmajXdecYinc => (SetRenderSolidLineTieBreakBits & 0x10) != 0;
        public readonly bool SetRenderSolidLineTieBreakBitsYmajXincYinc => (SetRenderSolidLineTieBreakBits & 0x100) != 0;
        public readonly bool SetRenderSolidLineTieBreakBitsYmajXdecYinc => (SetRenderSolidLineTieBreakBits & 0x1000) != 0;
        public fixed uint Reserved590[20];
        public uint RenderSolidPrimPointXY;
        public readonly int RenderSolidPrimPointXYX => (int)(RenderSolidPrimPointXY & 0xFFFF);
        public readonly int RenderSolidPrimPointXYY => (int)((RenderSolidPrimPointXY >> 16) & 0xFFFF);
        public fixed uint Reserved5E4[7];
        public Array64<RenderSolidPrimPoint> RenderSolidPrimPoint;
        public uint SetPixelsFromCpuDataType;
        public readonly SetPixelsFromCpuDataTypeV SetPixelsFromCpuDataTypeV => (SetPixelsFromCpuDataTypeV)(SetPixelsFromCpuDataType & 0x1);
        public uint SetPixelsFromCpuColorFormat;
        public readonly SetPixelsFromCpuColorFormatV SetPixelsFromCpuColorFormatV => (SetPixelsFromCpuColorFormatV)(SetPixelsFromCpuColorFormat & 0xFF);
        public uint SetPixelsFromCpuIndexFormat;
        public readonly SetPixelsFromCpuIndexFormatV SetPixelsFromCpuIndexFormatV => (SetPixelsFromCpuIndexFormatV)(SetPixelsFromCpuIndexFormat & 0x3);
        public uint SetPixelsFromCpuMonoFormat;
        public readonly SetPixelsFromCpuMonoFormatV SetPixelsFromCpuMonoFormatV => (SetPixelsFromCpuMonoFormatV)(SetPixelsFromCpuMonoFormat & 0x1);
        public uint SetPixelsFromCpuWrap;
        public readonly SetPixelsFromCpuWrapV SetPixelsFromCpuWrapV => (SetPixelsFromCpuWrapV)(SetPixelsFromCpuWrap & 0x3);
        public uint SetPixelsFromCpuColor0;
        public uint SetPixelsFromCpuColor1;
        public uint SetPixelsFromCpuMonoOpacity;
        public readonly SetPixelsFromCpuMonoOpacityV SetPixelsFromCpuMonoOpacityV => (SetPixelsFromCpuMonoOpacityV)(SetPixelsFromCpuMonoOpacity & 0x1);
        public fixed uint Reserved820[6];
        public uint SetPixelsFromCpuSrcWidth;
        public uint SetPixelsFromCpuSrcHeight;
        public uint SetPixelsFromCpuDxDuFrac;
        public uint SetPixelsFromCpuDxDuInt;
        public uint SetPixelsFromCpuDyDvFrac;
        public uint SetPixelsFromCpuDyDvInt;
        public uint SetPixelsFromCpuDstX0Frac;
        public uint SetPixelsFromCpuDstX0Int;
        public uint SetPixelsFromCpuDstY0Frac;
        public uint SetPixelsFromCpuDstY0Int;
        public uint PixelsFromCpuData;
        public fixed uint Reserved864[3];
        public uint SetBigEndianControl;
        public readonly bool SetBigEndianControlX32Swap1 => (SetBigEndianControl & 0x1) != 0;
        public readonly bool SetBigEndianControlX32Swap4 => (SetBigEndianControl & 0x2) != 0;
        public readonly bool SetBigEndianControlX32Swap8 => (SetBigEndianControl & 0x4) != 0;
        public readonly bool SetBigEndianControlX32Swap16 => (SetBigEndianControl & 0x8) != 0;
        public readonly bool SetBigEndianControlX16Swap1 => (SetBigEndianControl & 0x10) != 0;
        public readonly bool SetBigEndianControlX16Swap4 => (SetBigEndianControl & 0x20) != 0;
        public readonly bool SetBigEndianControlX16Swap8 => (SetBigEndianControl & 0x40) != 0;
        public readonly bool SetBigEndianControlX16Swap16 => (SetBigEndianControl & 0x80) != 0;
        public readonly bool SetBigEndianControlX8Swap1 => (SetBigEndianControl & 0x100) != 0;
        public readonly bool SetBigEndianControlX8Swap4 => (SetBigEndianControl & 0x200) != 0;
        public readonly bool SetBigEndianControlX8Swap8 => (SetBigEndianControl & 0x400) != 0;
        public readonly bool SetBigEndianControlX8Swap16 => (SetBigEndianControl & 0x800) != 0;
        public readonly bool SetBigEndianControlI1X8Cga6Swap1 => (SetBigEndianControl & 0x1000) != 0;
        public readonly bool SetBigEndianControlI1X8Cga6Swap4 => (SetBigEndianControl & 0x2000) != 0;
        public readonly bool SetBigEndianControlI1X8Cga6Swap8 => (SetBigEndianControl & 0x4000) != 0;
        public readonly bool SetBigEndianControlI1X8Cga6Swap16 => (SetBigEndianControl & 0x8000) != 0;
        public readonly bool SetBigEndianControlI1X8LeSwap1 => (SetBigEndianControl & 0x10000) != 0;
        public readonly bool SetBigEndianControlI1X8LeSwap4 => (SetBigEndianControl & 0x20000) != 0;
        public readonly bool SetBigEndianControlI1X8LeSwap8 => (SetBigEndianControl & 0x40000) != 0;
        public readonly bool SetBigEndianControlI1X8LeSwap16 => (SetBigEndianControl & 0x80000) != 0;
        public readonly bool SetBigEndianControlI4Swap1 => (SetBigEndianControl & 0x100000) != 0;
        public readonly bool SetBigEndianControlI4Swap4 => (SetBigEndianControl & 0x200000) != 0;
        public readonly bool SetBigEndianControlI4Swap8 => (SetBigEndianControl & 0x400000) != 0;
        public readonly bool SetBigEndianControlI4Swap16 => (SetBigEndianControl & 0x800000) != 0;
        public readonly bool SetBigEndianControlI8Swap1 => (SetBigEndianControl & 0x1000000) != 0;
        public readonly bool SetBigEndianControlI8Swap4 => (SetBigEndianControl & 0x2000000) != 0;
        public readonly bool SetBigEndianControlI8Swap8 => (SetBigEndianControl & 0x4000000) != 0;
        public readonly bool SetBigEndianControlI8Swap16 => (SetBigEndianControl & 0x8000000) != 0;
        public readonly bool SetBigEndianControlOverride => (SetBigEndianControl & 0x10000000) != 0;
        public fixed uint Reserved874[3];
        public uint SetPixelsFromMemoryBlockShape;
        public readonly SetPixelsFromMemoryBlockShapeV SetPixelsFromMemoryBlockShapeV => (SetPixelsFromMemoryBlockShapeV)(SetPixelsFromMemoryBlockShape & 0x7);
        public uint SetPixelsFromMemoryCorralSize;
        public readonly int SetPixelsFromMemoryCorralSizeV => (int)(SetPixelsFromMemoryCorralSize & 0x3FF);
        public uint SetPixelsFromMemorySafeOverlap;
        public readonly bool SetPixelsFromMemorySafeOverlapV => (SetPixelsFromMemorySafeOverlap & 0x1) != 0;
        public uint SetPixelsFromMemorySampleMode;
        public readonly SetPixelsFromMemorySampleModeOrigin SetPixelsFromMemorySampleModeOrigin => (SetPixelsFromMemorySampleModeOrigin)(SetPixelsFromMemorySampleMode & 0x1);
        public readonly SetPixelsFromMemorySampleModeFilter SetPixelsFromMemorySampleModeFilter => (SetPixelsFromMemorySampleModeFilter)((SetPixelsFromMemorySampleMode >> 4) & 0x1);
        public fixed uint Reserved890[8];
        public uint SetPixelsFromMemoryDstX0;
        public uint SetPixelsFromMemoryDstY0;
        public uint SetPixelsFromMemoryDstWidth;
        public uint SetPixelsFromMemoryDstHeight;
        public uint SetPixelsFromMemoryDuDxFrac;
        public uint SetPixelsFromMemoryDuDxInt;
        public uint SetPixelsFromMemoryDvDyFrac;
        public uint SetPixelsFromMemoryDvDyInt;
        public uint SetPixelsFromMemorySrcX0Frac;
        public uint SetPixelsFromMemorySrcX0Int;
        public uint SetPixelsFromMemorySrcY0Frac;
        public uint PixelsFromMemorySrcY0Int;
        public uint SetFalcon00;
        public uint SetFalcon01;
        public uint SetFalcon02;
        public uint SetFalcon03;
        public uint SetFalcon04;
        public uint SetFalcon05;
        public uint SetFalcon06;
        public uint SetFalcon07;
        public uint SetFalcon08;
        public uint SetFalcon09;
        public uint SetFalcon10;
        public uint SetFalcon11;
        public uint SetFalcon12;
        public uint SetFalcon13;
        public uint SetFalcon14;
        public uint SetFalcon15;
        public uint SetFalcon16;
        public uint SetFalcon17;
        public uint SetFalcon18;
        public uint SetFalcon19;
        public uint SetFalcon20;
        public uint SetFalcon21;
        public uint SetFalcon22;
        public uint SetFalcon23;
        public uint SetFalcon24;
        public uint SetFalcon25;
        public uint SetFalcon26;
        public uint SetFalcon27;
        public uint SetFalcon28;
        public uint SetFalcon29;
        public uint SetFalcon30;
        public uint SetFalcon31;
        public fixed uint Reserved960[291];
        public uint MmeDmaWriteMethodBarrier;
        public readonly bool MmeDmaWriteMethodBarrierV => (MmeDmaWriteMethodBarrier & 0x1) != 0;
        public fixed uint ReservedDF0[2436];
        public Array256<uint> SetMmeShadowScratch;
#pragma warning restore CS0649
    }
}
