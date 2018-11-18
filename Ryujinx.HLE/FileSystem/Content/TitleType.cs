namespace Ryujinx.HLE.FileSystem.Content
{
    enum TitleType
    {
        SystemPrograms     = 0x01,
        SystemDataArchive  = 0x02,
        SystemUpdate       = 0x03,
        FirmwarePackageA   = 0x04,
        FirmwarePackageB   = 0x05,
        RegularApplication = 0x80,
        Update             = 0x81,
        AddOnContent       = 0x82,
        DeltaTitle         = 0x83
    }
}
