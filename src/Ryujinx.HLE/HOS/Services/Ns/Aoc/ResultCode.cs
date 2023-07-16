namespace Ryujinx.HLE.HOS.Services.Ns.Aoc
{
    enum ResultCode
    {
        ModuleId = 166,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidBufferSize = (200 << ErrorCodeShift) | ModuleId,
        InvalidPid = (300 << ErrorCodeShift) | ModuleId,
    }
}
