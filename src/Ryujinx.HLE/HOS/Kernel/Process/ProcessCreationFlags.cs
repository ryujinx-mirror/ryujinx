using System;
using System.Diagnostics.CodeAnalysis;

namespace Ryujinx.HLE.HOS.Kernel.Process
{
    [Flags]
    [SuppressMessage("Design", "CA1069: Enums values should not be duplicated")]
    enum ProcessCreationFlags
    {
        Is64Bit = 1 << 0,

        AddressSpaceShift = 1,
        AddressSpace32Bit = 0 << AddressSpaceShift,
        AddressSpace64BitDeprecated = 1 << AddressSpaceShift,
        AddressSpace32BitWithoutAlias = 2 << AddressSpaceShift,
        AddressSpace64Bit = 3 << AddressSpaceShift,
        AddressSpaceMask = 7 << AddressSpaceShift,

        EnableDebug = 1 << 4,
        EnableAslr = 1 << 5,
        IsApplication = 1 << 6,
        DeprecatedUseSecureMemory = 1 << 7,

        PoolPartitionShift = 7,
        PoolPartitionApplication = 0 << PoolPartitionShift,
        PoolPartitionApplet = 1 << PoolPartitionShift,
        PoolPartitionSystem = 2 << PoolPartitionShift,
        PoolPartitionSystemNonSecure = 3 << PoolPartitionShift,
        PoolPartitionMask = 0xf << PoolPartitionShift,

        OptimizeMemoryAllocation = 1 << 11,
        DisableDeviceAddressSpaceMerge = 1 << 12,
        EnableAliasRegionExtraSize = 1 << 13,

        All =
            Is64Bit |
            AddressSpaceMask |
            EnableDebug |
            EnableAslr |
            IsApplication |
            DeprecatedUseSecureMemory |
            PoolPartitionMask |
            OptimizeMemoryAllocation |
            DisableDeviceAddressSpaceMerge |
            EnableAliasRegionExtraSize,
    }
}
