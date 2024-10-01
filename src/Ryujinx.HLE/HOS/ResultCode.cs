namespace Ryujinx.HLE.HOS
{
    public enum ResultCode
    {
        OsModuleId = 3,
        ErrorCodeShift = 9,

        Success = 0,

        NotAllocated = (1023 << ErrorCodeShift) | OsModuleId,
    }
}
