namespace Ryujinx.HLE.HOS.Kernel.Common
{
    enum KernelResult
    {
        Success            = 0,
        InvalidCapability  = 0x1c01,
        ThreadNotStarted   = 0x7201,
        ThreadTerminating  = 0x7601,
        InvalidSize        = 0xca01,
        InvalidAddress     = 0xcc01,
        OutOfResource      = 0xce01,
        OutOfMemory        = 0xd001,
        HandleTableFull    = 0xd201,
        InvalidMemState    = 0xd401,
        InvalidPermission  = 0xd801,
        InvalidMemRange    = 0xdc01,
        InvalidPriority    = 0xe001,
        InvalidCpuCore     = 0xe201,
        InvalidHandle      = 0xe401,
        UserCopyFailed     = 0xe601,
        InvalidCombination = 0xe801,
        TimedOut           = 0xea01,
        Cancelled          = 0xec01,
        MaximumExceeded    = 0xee01,
        InvalidEnumValue   = 0xf001,
        NotFound           = 0xf201,
        InvalidThread      = 0xf401,
        InvalidState       = 0xfa01,
        ReservedValue      = 0xfc01,
        ResLimitExceeded   = 0x10801
    }
}