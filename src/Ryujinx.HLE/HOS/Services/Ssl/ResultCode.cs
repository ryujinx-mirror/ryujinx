namespace Ryujinx.HLE.HOS.Services.Ssl
{
    public enum ResultCode
    {
        OsModuleId = 123,
        ErrorCodeShift = 9,

        Success = 0,
        NoSocket = (103 << ErrorCodeShift) | OsModuleId,
        InvalidSocket = (106 << ErrorCodeShift) | OsModuleId,
        InvalidCertBufSize = (112 << ErrorCodeShift) | OsModuleId,
        InvalidOption = (126 << ErrorCodeShift) | OsModuleId,
        CertBufferTooSmall = (202 << ErrorCodeShift) | OsModuleId,
        AlreadyInUse = (203 << ErrorCodeShift) | OsModuleId,
        WouldBlock = (204 << ErrorCodeShift) | OsModuleId,
        Timeout = (205 << ErrorCodeShift) | OsModuleId,
        ConnectionReset = (209 << ErrorCodeShift) | OsModuleId,
        ConnectionAbort = (210 << ErrorCodeShift) | OsModuleId,
    }
}
