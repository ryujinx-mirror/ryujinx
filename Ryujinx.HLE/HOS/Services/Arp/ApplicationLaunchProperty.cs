using Ryujinx.HLE.FileSystem;

namespace Ryujinx.HLE.HOS.Services.Arp
{
    class ApplicationLaunchProperty
    {
        public ulong TitleId;
        public int   Version;
        public byte  BaseGameStorageId;
        public byte  UpdateGameStorageId;
#pragma warning disable CS0649
        public short Padding;
#pragma warning restore CS0649

        public static ApplicationLaunchProperty Default
        {
            get
            {
                return new ApplicationLaunchProperty
                {
                    TitleId             = 0x00,
                    Version             = 0x00,
                    BaseGameStorageId   = (byte)StorageId.NandSystem,
                    UpdateGameStorageId = (byte)StorageId.None
                };
            }
        }

        public static ApplicationLaunchProperty GetByPid(ServiceCtx context)
        {
            // TODO: Handle ApplicationLaunchProperty as array when pid will be supported and return the right item.
            //       For now we can hardcode values, and fix it after GetApplicationLaunchProperty is implemented.

            return new ApplicationLaunchProperty
            {
                TitleId             = context.Device.Application.TitleId,
                Version             = 0x00,
                BaseGameStorageId   = (byte)StorageId.NandSystem,
                UpdateGameStorageId = (byte)StorageId.None
            };
        }
    }
}