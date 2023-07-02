using Ryujinx.Graphics.Gpu.Engine.Types;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Gpu.Engine.Compute
{
    /// <summary>
    /// Type of the dependent Queue Meta Data.
    /// </summary>
    enum DependentQmdType
    {
        Queue,
        Grid,
    }

    /// <summary>
    /// Type of the release memory barrier.
    /// </summary>
    enum ReleaseMembarType
    {
        FeNone,
        FeSysmembar,
    }

    /// <summary>
    /// Type of the CWD memory barrier.
    /// </summary>
    enum CwdMembarType
    {
        L1None,
        L1Sysmembar,
        L1Membar,
    }

    /// <summary>
    /// NaN behavior of 32-bits float operations on the shader.
    /// </summary>
    enum Fp32NanBehavior
    {
        Legacy,
        Fp64Compatible,
    }

    /// <summary>
    /// NaN behavior of 32-bits float to integer conversion on the shader.
    /// </summary>
    enum Fp32F2iNanBehavior
    {
        PassZero,
        PassIndefinite,
    }

    /// <summary>
    /// Limit of calls.
    /// </summary>
    enum ApiVisibleCallLimit
    {
        _32,
        NoCheck,
    }

    /// <summary>
    /// Shared memory bank mapping mode.
    /// </summary>
    enum SharedMemoryBankMapping
    {
        FourBytesPerBank,
        EightBytesPerBank,
    }

    /// <summary>
    /// Denormal behavior of 32-bits float narrowing instructions.
    /// </summary>
    enum Fp32NarrowInstruction
    {
        KeepDenorms,
        FlushDenorms,
    }

    /// <summary>
    /// Configuration of the L1 cache.
    /// </summary>
    enum L1Configuration
    {
        DirectlyAddressableMemorySize16kb,
        DirectlyAddressableMemorySize32kb,
        DirectlyAddressableMemorySize48kb,
    }

    /// <summary>
    /// Reduction operation.
    /// </summary>
    enum ReductionOp
    {
        RedAdd,
        RedMin,
        RedMax,
        RedInc,
        RedDec,
        RedAnd,
        RedOr,
        RedXor,
    }

    /// <summary>
    /// Reduction format.
    /// </summary>
    enum ReductionFormat
    {
        Unsigned32,
        Signed32,
    }

    /// <summary>
    /// Size of a structure in words.
    /// </summary>
    enum StructureSize
    {
        FourWords,
        OneWord,
    }

    /// <summary>
    /// Compute Queue Meta Data.
    /// </summary>
    unsafe struct ComputeQmd
    {
        private fixed int _words[64];

        public readonly int OuterPut => BitRange(30, 0);
        public readonly bool OuterOverflow => Bit(31);
        public readonly int OuterGet => BitRange(62, 32);
        public readonly bool OuterStickyOverflow => Bit(63);
        public readonly int InnerGet => BitRange(94, 64);
        public readonly bool InnerOverflow => Bit(95);
        public readonly int InnerPut => BitRange(126, 96);
        public readonly bool InnerStickyOverflow => Bit(127);
        public readonly int QmdReservedAA => BitRange(159, 128);
        public readonly int DependentQmdPointer => BitRange(191, 160);
        public readonly int QmdGroupId => BitRange(197, 192);
        public readonly bool SmGlobalCachingEnable => Bit(198);
        public readonly bool RunCtaInOneSmPartition => Bit(199);
        public readonly bool IsQueue => Bit(200);
        public readonly bool AddToHeadOfQmdGroupLinkedList => Bit(201);
        public readonly bool SemaphoreReleaseEnable0 => Bit(202);
        public readonly bool SemaphoreReleaseEnable1 => Bit(203);
        public readonly bool RequireSchedulingPcas => Bit(204);
        public readonly bool DependentQmdScheduleEnable => Bit(205);
        public readonly DependentQmdType DependentQmdType => (DependentQmdType)BitRange(206, 206);
        public readonly bool DependentQmdFieldCopy => Bit(207);
        public readonly int QmdReservedB => BitRange(223, 208);
        public readonly int CircularQueueSize => BitRange(248, 224);
        public readonly bool QmdReservedC => Bit(249);
        public readonly bool InvalidateTextureHeaderCache => Bit(250);
        public readonly bool InvalidateTextureSamplerCache => Bit(251);
        public readonly bool InvalidateTextureDataCache => Bit(252);
        public readonly bool InvalidateShaderDataCache => Bit(253);
        public readonly bool InvalidateInstructionCache => Bit(254);
        public readonly bool InvalidateShaderConstantCache => Bit(255);
        public readonly int ProgramOffset => BitRange(287, 256);
        public readonly int CircularQueueAddrLower => BitRange(319, 288);
        public readonly int CircularQueueAddrUpper => BitRange(327, 320);
        public readonly int QmdReservedD => BitRange(335, 328);
        public readonly int CircularQueueEntrySize => BitRange(351, 336);
        public readonly int CwdReferenceCountId => BitRange(357, 352);
        public readonly int CwdReferenceCountDeltaMinusOne => BitRange(365, 358);
        public readonly ReleaseMembarType ReleaseMembarType => (ReleaseMembarType)BitRange(366, 366);
        public readonly bool CwdReferenceCountIncrEnable => Bit(367);
        public readonly CwdMembarType CwdMembarType => (CwdMembarType)BitRange(369, 368);
        public readonly bool SequentiallyRunCtas => Bit(370);
        public readonly bool CwdReferenceCountDecrEnable => Bit(371);
        public readonly bool Throttled => Bit(372);
        public readonly Fp32NanBehavior Fp32NanBehavior => (Fp32NanBehavior)BitRange(376, 376);
        public readonly Fp32F2iNanBehavior Fp32F2iNanBehavior => (Fp32F2iNanBehavior)BitRange(377, 377);
        public readonly ApiVisibleCallLimit ApiVisibleCallLimit => (ApiVisibleCallLimit)BitRange(378, 378);
        public readonly SharedMemoryBankMapping SharedMemoryBankMapping => (SharedMemoryBankMapping)BitRange(379, 379);
        public readonly SamplerIndex SamplerIndex => (SamplerIndex)BitRange(382, 382);
        public readonly Fp32NarrowInstruction Fp32NarrowInstruction => (Fp32NarrowInstruction)BitRange(383, 383);
        public readonly int CtaRasterWidth => BitRange(415, 384);
        public readonly int CtaRasterHeight => BitRange(431, 416);
        public readonly int CtaRasterDepth => BitRange(447, 432);
        public readonly int CtaRasterWidthResume => BitRange(479, 448);
        public readonly int CtaRasterHeightResume => BitRange(495, 480);
        public readonly int CtaRasterDepthResume => BitRange(511, 496);
        public readonly int QueueEntriesPerCtaMinusOne => BitRange(518, 512);
        public readonly int CoalesceWaitingPeriod => BitRange(529, 522);
        public readonly int SharedMemorySize => BitRange(561, 544);
        public readonly int QmdReservedG => BitRange(575, 562);
        public readonly int QmdVersion => BitRange(579, 576);
        public readonly int QmdMajorVersion => BitRange(583, 580);
        public readonly int QmdReservedH => BitRange(591, 584);
        public readonly int CtaThreadDimension0 => BitRange(607, 592);
        public readonly int CtaThreadDimension1 => BitRange(623, 608);
        public readonly int CtaThreadDimension2 => BitRange(639, 624);
        public readonly bool ConstantBufferValid(int i) => Bit(640 + i * 1);
        public readonly int QmdReservedI => BitRange(668, 648);
        public readonly L1Configuration L1Configuration => (L1Configuration)BitRange(671, 669);
        public readonly int SmDisableMaskLower => BitRange(703, 672);
        public readonly int SmDisableMaskUpper => BitRange(735, 704);
        public readonly int Release0AddressLower => BitRange(767, 736);
        public readonly int Release0AddressUpper => BitRange(775, 768);
        public readonly int QmdReservedJ => BitRange(783, 776);
        public readonly ReductionOp Release0ReductionOp => (ReductionOp)BitRange(790, 788);
        public readonly bool QmdReservedK => Bit(791);
        public readonly ReductionFormat Release0ReductionFormat => (ReductionFormat)BitRange(793, 792);
        public readonly bool Release0ReductionEnable => Bit(794);
        public readonly StructureSize Release0StructureSize => (StructureSize)BitRange(799, 799);
        public readonly int Release0Payload => BitRange(831, 800);
        public readonly int Release1AddressLower => BitRange(863, 832);
        public readonly int Release1AddressUpper => BitRange(871, 864);
        public readonly int QmdReservedL => BitRange(879, 872);
        public readonly ReductionOp Release1ReductionOp => (ReductionOp)BitRange(886, 884);
        public readonly bool QmdReservedM => Bit(887);
        public readonly ReductionFormat Release1ReductionFormat => (ReductionFormat)BitRange(889, 888);
        public readonly bool Release1ReductionEnable => Bit(890);
        public readonly StructureSize Release1StructureSize => (StructureSize)BitRange(895, 895);
        public readonly int Release1Payload => BitRange(927, 896);
        public readonly int ConstantBufferAddrLower(int i) => BitRange(959 + i * 64, 928 + i * 64);
        public readonly int ConstantBufferAddrUpper(int i) => BitRange(967 + i * 64, 960 + i * 64);
        public readonly int ConstantBufferReservedAddr(int i) => BitRange(973 + i * 64, 968 + i * 64);
        public readonly bool ConstantBufferInvalidate(int i) => Bit(974 + i * 64);
        public readonly int ConstantBufferSize(int i) => BitRange(991 + i * 64, 975 + i * 64);
        public readonly int ShaderLocalMemoryLowSize => BitRange(1463, 1440);
        public readonly int QmdReservedN => BitRange(1466, 1464);
        public readonly int BarrierCount => BitRange(1471, 1467);
        public readonly int ShaderLocalMemoryHighSize => BitRange(1495, 1472);
        public readonly int RegisterCount => BitRange(1503, 1496);
        public readonly int ShaderLocalMemoryCrsSize => BitRange(1527, 1504);
        public readonly int SassVersion => BitRange(1535, 1528);
        public readonly int HwOnlyInnerGet => BitRange(1566, 1536);
        public readonly bool HwOnlyRequireSchedulingPcas => Bit(1567);
        public readonly int HwOnlyInnerPut => BitRange(1598, 1568);
        public readonly bool HwOnlyScgType => Bit(1599);
        public readonly int HwOnlySpanListHeadIndex => BitRange(1629, 1600);
        public readonly bool QmdReservedQ => Bit(1630);
        public readonly bool HwOnlySpanListHeadIndexValid => Bit(1631);
        public readonly int HwOnlySkedNextQmdPointer => BitRange(1663, 1632);
        public readonly int QmdSpareE => BitRange(1695, 1664);
        public readonly int QmdSpareF => BitRange(1727, 1696);
        public readonly int QmdSpareG => BitRange(1759, 1728);
        public readonly int QmdSpareH => BitRange(1791, 1760);
        public readonly int QmdSpareI => BitRange(1823, 1792);
        public readonly int QmdSpareJ => BitRange(1855, 1824);
        public readonly int QmdSpareK => BitRange(1887, 1856);
        public readonly int QmdSpareL => BitRange(1919, 1888);
        public readonly int QmdSpareM => BitRange(1951, 1920);
        public readonly int QmdSpareN => BitRange(1983, 1952);
        public readonly int DebugIdUpper => BitRange(2015, 1984);
        public readonly int DebugIdLower => BitRange(2047, 2016);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool Bit(int bit)
        {
            if ((uint)bit >= 64 * 32)
            {
                throw new ArgumentOutOfRangeException(nameof(bit));
            }

            return (_words[bit >> 5] & (1 << (bit & 31))) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int BitRange(int upper, int lower)
        {
            if ((uint)lower >= 64 * 32)
            {
                throw new ArgumentOutOfRangeException(nameof(lower));
            }

            int mask = (int)(uint.MaxValue >> (32 - (upper - lower + 1)));

            return (_words[lower >> 5] >> (lower & 31)) & mask;
        }
    }
}
