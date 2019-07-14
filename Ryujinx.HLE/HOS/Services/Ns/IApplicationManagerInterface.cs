using LibHac;
using System;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    [Service("ns:am")]
    class IApplicationManagerInterface : IpcService
    {
        public IApplicationManagerInterface(ServiceCtx context) { }

        [Command(400)]
        // GetApplicationControlData(unknown<0x10>) -> (unknown<4>, buffer<unknown, 6>)
        public ResultCode GetApplicationControlData(ServiceCtx context)
        {
            long position = context.Request.ReceiveBuff[0].Position;

            Nacp nacp = context.Device.System.ControlData;

            for (int i = 0; i < 0x10; i++)
            {
                NacpDescription description = nacp.Descriptions[i];

                byte[] titleData     = new byte[0x200];
                byte[] developerData = new byte[0x100];

                if (description !=null && description.Title != null)
                {
                    byte[] titleDescriptionData = Encoding.ASCII.GetBytes(description.Title);
                    Buffer.BlockCopy(titleDescriptionData, 0, titleData, 0, titleDescriptionData.Length);

                }

                if (description != null && description.Developer != null)
                {
                    byte[] developerDescriptionData = Encoding.ASCII.GetBytes(description.Developer);
                    Buffer.BlockCopy(developerDescriptionData, 0, developerData, 0, developerDescriptionData.Length);
                }

                context.Memory.WriteBytes(position, titleData);
                context.Memory.WriteBytes(position + 0x200, developerData);

                position += i * 0x300;
            }

            byte[] isbn = new byte[0x25];

            if (nacp.Isbn != null)
            {
                byte[] isbnData = Encoding.ASCII.GetBytes(nacp.Isbn);
                Buffer.BlockCopy(isbnData, 0, isbn, 0, isbnData.Length);
            }

            context.Memory.WriteBytes(position, isbn);
            position += isbn.Length;

            context.Memory.WriteByte(position++, nacp.StartupUserAccount);
            context.Memory.WriteByte(position++, nacp.UserAccountSwitchLock);
            context.Memory.WriteByte(position++, nacp.AocRegistrationType);

            context.Memory.WriteInt32(position, nacp.AttributeFlag);
            position += 4;

            context.Memory.WriteUInt32(position, nacp.SupportedLanguageFlag);
            position += 4;

            context.Memory.WriteUInt32(position, nacp.ParentalControlFlag);
            position += 4;

            context.Memory.WriteByte(position++, nacp.Screenshot);
            context.Memory.WriteByte(position++, nacp.VideoCapture);
            context.Memory.WriteByte(position++, nacp.DataLossConfirmation);
            context.Memory.WriteByte(position++, nacp.PlayLogPolicy);

            context.Memory.WriteUInt64(position, nacp.PresenceGroupId);
            position += 8;

            for (int i = 0; i < nacp.RatingAge.Length; i++)
            {
                context.Memory.WriteSByte(position++, nacp.RatingAge[i]);
            }

            byte[] displayVersion = new byte[0x10];

            if (nacp.DisplayVersion != null)
            {
                byte[] displayVersionData = Encoding.ASCII.GetBytes(nacp.DisplayVersion);
                Buffer.BlockCopy(displayVersionData, 0, displayVersion, 0, displayVersionData.Length);
            }

            context.Memory.WriteBytes(position, displayVersion);
            position += displayVersion.Length;

            context.Memory.WriteUInt64(position, nacp.AddOnContentBaseId);
            position += 8;

            context.Memory.WriteUInt64(position, nacp.SaveDataOwnerId);
            position += 8;

            context.Memory.WriteInt64(position, nacp.UserAccountSaveDataSize);
            position += 8;

            context.Memory.WriteInt64(position, nacp.UserAccountSaveDataJournalSize);
            position += 8;

            context.Memory.WriteInt64(position, nacp.DeviceSaveDataSize);
            position += 8;

            context.Memory.WriteInt64(position, nacp.DeviceSaveDataJournalSize);
            position += 8;

            context.Memory.WriteInt64(position, nacp.BcatDeliveryCacheStorageSize);
            position += 8;

            byte[] applicationErrorCodeCategory = new byte[0x8];

            if (nacp.ApplicationErrorCodeCategory != null)
            {
                byte[] applicationErrorCodeCategoryData = Encoding.ASCII.GetBytes(nacp.ApplicationErrorCodeCategory);
                Buffer.BlockCopy(applicationErrorCodeCategoryData, 0, applicationErrorCodeCategoryData, 0, applicationErrorCodeCategoryData.Length);
            }

            context.Memory.WriteBytes(position, applicationErrorCodeCategory);
            position += applicationErrorCodeCategory.Length;

            for (int i = 0; i < nacp.LocalCommunicationId.Length; i++)
            {
                context.Memory.WriteUInt64(position, nacp.LocalCommunicationId[i]);
                position += 8;
            }

            context.Memory.WriteByte(position++, nacp.LogoType);
            context.Memory.WriteByte(position++, nacp.LogoHandling);
            context.Memory.WriteByte(position++, nacp.RuntimeAddOnContentInstall);

            byte[] reserved000 = new byte[0x3];
            context.Memory.WriteBytes(position, reserved000);
            position += reserved000.Length;

            context.Memory.WriteByte(position++, nacp.CrashReport);
            context.Memory.WriteByte(position++, nacp.Hdcp);
            context.Memory.WriteUInt64(position, nacp.SeedForPseudoDeviceId);
            position += 8;

            byte[] bcatPassphrase = new byte[65];
            if (nacp.BcatPassphrase != null)
            {
                byte[] bcatPassphraseData = Encoding.ASCII.GetBytes(nacp.BcatPassphrase);
                Buffer.BlockCopy(bcatPassphraseData, 0, bcatPassphrase, 0, bcatPassphraseData.Length);
            }

            context.Memory.WriteBytes(position, bcatPassphrase);
            position += bcatPassphrase.Length;

            context.Memory.WriteByte(position++, nacp.Reserved01);

            byte[] reserved02 = new byte[0x6];
            context.Memory.WriteBytes(position, reserved02);
            position += reserved02.Length;

            context.Memory.WriteInt64(position, nacp.UserAccountSaveDataSizeMax);
            position += 8;

            context.Memory.WriteInt64(position, nacp.UserAccountSaveDataJournalSizeMax);
            position += 8;

            context.Memory.WriteInt64(position, nacp.DeviceSaveDataSizeMax);
            position += 8;

            context.Memory.WriteInt64(position, nacp.DeviceSaveDataJournalSizeMax);
            position += 8;

            context.Memory.WriteInt64(position, nacp.TemporaryStorageSize);
            position += 8;

            context.Memory.WriteInt64(position, nacp.CacheStorageSize);
            position += 8;

            context.Memory.WriteInt64(position, nacp.CacheStorageJournalSize);
            position += 8;

            context.Memory.WriteInt64(position, nacp.CacheStorageDataAndJournalSizeMax);
            position += 8;

            context.Memory.WriteInt16(position, nacp.CacheStorageIndex);
            position += 2;

            byte[] reserved03 = new byte[0x6];
            context.Memory.WriteBytes(position, reserved03);
            position += reserved03.Length;

            for (int i = 0; i < 16; i++)
            {
                ulong value = 0;

                if (nacp.PlayLogQueryableApplicationId.Count > i)
                {
                    value = nacp.PlayLogQueryableApplicationId[i];
                }

                context.Memory.WriteUInt64(position, value);
                position += 8;
            }

            context.Memory.WriteByte(position++, nacp.PlayLogQueryCapability);
            context.Memory.WriteByte(position++, nacp.RepairFlag);
            context.Memory.WriteByte(position++, nacp.ProgramIndex);

            return ResultCode.Success;
        }
    }
}