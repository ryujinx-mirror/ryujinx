namespace Ryujinx.HLE.HOS.Services.Mii
{
    public enum ResultCode
    {
        ModuleId = 126,
        ErrorCodeShift = 9,

        Success = 0,

        InvalidArgument = (1 << ErrorCodeShift) | ModuleId,
        BufferTooSmall = (2 << ErrorCodeShift) | ModuleId,
        NotUpdated = (3 << ErrorCodeShift) | ModuleId,
        NotFound = (4 << ErrorCodeShift) | ModuleId,
        DatabaseFull = (5 << ErrorCodeShift) | ModuleId,
        InvalidDatabaseSignatureValue = (67 << ErrorCodeShift) | ModuleId,
        InvalidDatabaseEntryCount = (69 << ErrorCodeShift) | ModuleId,
        InvalidCharInfo = (100 << ErrorCodeShift) | ModuleId,
        InvalidCrc = (101 << ErrorCodeShift) | ModuleId,
        InvalidDeviceCrc = (102 << ErrorCodeShift) | ModuleId,
        InvalidDatabaseMagic = (103 << ErrorCodeShift) | ModuleId,
        InvalidDatabaseVersion = (104 << ErrorCodeShift) | ModuleId,
        InvalidDatabaseSize = (105 << ErrorCodeShift) | ModuleId,
        InvalidCreateId = (106 << ErrorCodeShift) | ModuleId,
        InvalidCoreData = (108 << ErrorCodeShift) | ModuleId,
        InvalidStoreData = (109 << ErrorCodeShift) | ModuleId,
        InvalidOperationOnSpecialMii = (202 << ErrorCodeShift) | ModuleId,
        PermissionDenied = (203 << ErrorCodeShift) | ModuleId,
        TestModeNotEnabled = (204 << ErrorCodeShift) | ModuleId,
    }
}
