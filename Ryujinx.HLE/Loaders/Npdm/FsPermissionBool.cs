namespace Ryujinx.HLE.Loaders.Npdm
{
    enum FsPermissionBool : ulong
    {
        BisCache                  = 0x8000000000000080,
        EraseMmc                  = 0x8000000000000080,
        GameCardCertificate       = 0x8000000000000010,
        GameCardIdSet             = 0x8000000000000010,
        GameCardDriver            = 0x8000000000000200,
        GameCardAsic              = 0x8000000000000200,
        SaveDataCreate            = 0x8000000000002020,
        SaveDataDelete0           = 0x8000000000000060,
        SystemSaveDataCreate0     = 0x8000000000000028,
        SystemSaveDataCreate1     = 0x8000000000000020,
        SaveDataDelete1           = 0x8000000000004028,
        SaveDataIterators0        = 0x8000000000000060,
        SaveDataIterators1        = 0x8000000000004020,
        SaveThumbnails            = 0x8000000000020000,
        PosixTime                 = 0x8000000000000400,
        SaveDataExtraData         = 0x8000000000004060,
        GlobalMode                = 0x8000000000080000,
        SpeedEmulation            = 0x8000000000080000,
        NULL                      = 0,
        PaddingFiles              = 0xC000000000800000,
        SaveData_Debug            = 0xC000000001000000,
        SaveData_SystemManagement = 0xC000000002000000,
        Unknown0x16               = 0x8000000004000000,
        Unknown0x17               = 0x8000000008000000,
        Unknown0x18               = 0x8000000010000000,
        Unknown0x19               = 0x8000000000000800,
        Unknown0x1A               = 0x8000000000004020
    }
}
