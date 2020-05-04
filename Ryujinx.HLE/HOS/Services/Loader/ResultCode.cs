namespace Ryujinx.HLE.HOS.Services.Loader
{
    enum ResultCode
    {
        ModuleId       = 9,
        ErrorCodeShift = 9,

        Success = 0,

        ArgsTooLong                                   = (1   << ErrorCodeShift) | ModuleId,
        MaximumProcessesLoaded                        = (2   << ErrorCodeShift) | ModuleId,
        NPDMTooBig                                    = (3   << ErrorCodeShift) | ModuleId,
        InvalidNPDM                                   = (4   << ErrorCodeShift) | ModuleId,
        InvalidNSO                                    = (5   << ErrorCodeShift) | ModuleId,
        InvalidPath                                   = (6   << ErrorCodeShift) | ModuleId,
        AlreadyRegistered                             = (7   << ErrorCodeShift) | ModuleId,
        TitleNotFound                                 = (8   << ErrorCodeShift) | ModuleId,
        ACI0TitleIdNotMatchingRangeInACID             = (9   << ErrorCodeShift) | ModuleId,
        InvalidVersionInNPDM                          = (10  << ErrorCodeShift) | ModuleId,
        InsufficientAddressSpace                      = (51  << ErrorCodeShift) | ModuleId,
        InsufficientNRO                               = (52  << ErrorCodeShift) | ModuleId,
        InvalidNRR                                    = (53  << ErrorCodeShift) | ModuleId,
        InvalidSignature                              = (54  << ErrorCodeShift) | ModuleId,
        InsufficientNRORegistrations                  = (55  << ErrorCodeShift) | ModuleId,
        InsufficientNRRRegistrations                  = (56  << ErrorCodeShift) | ModuleId,
        NROAlreadyLoaded                              = (57  << ErrorCodeShift) | ModuleId,
        UnalignedNRRAddress                           = (81  << ErrorCodeShift) | ModuleId,
        BadNRRSize                                    = (82  << ErrorCodeShift) | ModuleId,
        NRRNotLoaded                                  = (84  << ErrorCodeShift) | ModuleId,
        BadNRRAddress                                 = (85  << ErrorCodeShift) | ModuleId,
        BadInitialization                             = (87  << ErrorCodeShift) | ModuleId,
        UnknownACI0Descriptor                         = (100 << ErrorCodeShift) | ModuleId,
        ACI0NotMatchingKernelFlagsDescriptor          = (103 << ErrorCodeShift) | ModuleId,
        ACI0NotMatchingSyscallMaskDescriptor          = (104 << ErrorCodeShift) | ModuleId,
        ACI0NotMatchingMapIoOrNormalRangeDescriptor   = (106 << ErrorCodeShift) | ModuleId,
        ACI0NotMatchingMapNormalPageDescriptor        = (107 << ErrorCodeShift) | ModuleId,
        ACI0NotMatchingInterruptPairDescriptor        = (111 << ErrorCodeShift) | ModuleId,
        ACI0NotMatchingApplicationTypeDescriptor      = (113 << ErrorCodeShift) | ModuleId,
        ACI0NotMatchingKernelReleaseVersionDescriptor = (114 << ErrorCodeShift) | ModuleId,
        ACI0NotMatchingHandleTableSizeDescriptor      = (115 << ErrorCodeShift) | ModuleId,
        ACI0NotMatchingDebugFlagsDescriptor           = (116 << ErrorCodeShift) | ModuleId
    }
}
