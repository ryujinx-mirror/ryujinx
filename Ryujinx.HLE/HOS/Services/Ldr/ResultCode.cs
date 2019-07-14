namespace Ryujinx.HLE.HOS.Services.Ldr
{
    enum ResultCode
    {
        ModuleId       = 9,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidMemoryState = (51 << ErrorCodeShift) | ModuleId,
        InvalidNro         = (52 << ErrorCodeShift) | ModuleId,
        InvalidNrr         = (53 << ErrorCodeShift) | ModuleId,
        MaxNro             = (55 << ErrorCodeShift) | ModuleId,
        MaxNrr             = (56 << ErrorCodeShift) | ModuleId,
        NroAlreadyLoaded   = (57 << ErrorCodeShift) | ModuleId,
        NroHashNotPresent  = (54 << ErrorCodeShift) | ModuleId,
        UnalignedAddress   = (81 << ErrorCodeShift) | ModuleId,
        BadSize            = (82 << ErrorCodeShift) | ModuleId,
        BadNroAddress      = (84 << ErrorCodeShift) | ModuleId,
        BadNrrAddress      = (85 << ErrorCodeShift) | ModuleId,
        BadInitialization  = (87 << ErrorCodeShift) | ModuleId
    }
}