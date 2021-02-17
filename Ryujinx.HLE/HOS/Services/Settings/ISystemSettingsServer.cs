using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.SystemState;
using System;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Settings
{
    [Service("set:sys")]
    class ISystemSettingsServer : IpcService
    {
        public ISystemSettingsServer(ServiceCtx context) { }

        [Command(3)]
        // GetFirmwareVersion() -> buffer<nn::settings::system::FirmwareVersion, 0x1a, 0x100>
        public ResultCode GetFirmwareVersion(ServiceCtx context)
        {
            return GetFirmwareVersion2(context);
        }

        [Command(4)]
        // GetFirmwareVersion2() -> buffer<nn::settings::system::FirmwareVersion, 0x1a, 0x100>
        public ResultCode GetFirmwareVersion2(ServiceCtx context)
        {
            long replyPos  = context.Request.RecvListBuff[0].Position;

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize(0x100L);

            byte[] firmwareData = GetFirmwareData(context.Device);

            if (firmwareData != null)
            {
                context.Memory.Write((ulong)replyPos, firmwareData);

                return ResultCode.Success;
            }

            const byte majorFwVersion = 0x03;
            const byte minorFwVersion = 0x00;
            const byte microFwVersion = 0x00;
            const byte unknown        = 0x00; //Build?

            const int revisionNumber = 0x0A;

            const string platform   = "NX";
            const string unknownHex = "7fbde2b0bba4d14107bf836e4643043d9f6c8e47";
            const string version    = "3.0.0";
            const string build      = "NintendoSDK Firmware for NX 3.0.0-10.0";

            // http://switchbrew.org/index.php?title=System_Version_Title
            using (MemoryStream ms = new MemoryStream(0x100))
            {
                BinaryWriter writer = new BinaryWriter(ms);

                writer.Write(majorFwVersion);
                writer.Write(minorFwVersion);
                writer.Write(microFwVersion);
                writer.Write(unknown);

                writer.Write(revisionNumber);

                writer.Write(Encoding.ASCII.GetBytes(platform));

                ms.Seek(0x28, SeekOrigin.Begin);

                writer.Write(Encoding.ASCII.GetBytes(unknownHex));

                ms.Seek(0x68, SeekOrigin.Begin);

                writer.Write(Encoding.ASCII.GetBytes(version));

                ms.Seek(0x80, SeekOrigin.Begin);

                writer.Write(Encoding.ASCII.GetBytes(build));

                context.Memory.Write((ulong)replyPos, ms.ToArray());
            }

            return ResultCode.Success;
        }

        [Command(23)]
        // GetColorSetId() -> i32
        public ResultCode GetColorSetId(ServiceCtx context)
        {
            context.ResponseData.Write((int)context.Device.System.State.ThemeColor);

            return ResultCode.Success;
        }

        [Command(24)]
        // GetColorSetId() -> i32
        public ResultCode SetColorSetId(ServiceCtx context)
        {
            int colorSetId = context.RequestData.ReadInt32();

            context.Device.System.State.ThemeColor = (ColorSet)colorSetId;

            return ResultCode.Success;
        }

        [Command(37)]
        // GetSettingsItemValueSize(buffer<nn::settings::SettingsName, 0x19>, buffer<nn::settings::SettingsItemKey, 0x19>) -> u64
        public ResultCode GetSettingsItemValueSize(ServiceCtx context)
        {
            long classPos  = context.Request.PtrBuff[0].Position;
            long classSize = context.Request.PtrBuff[0].Size;

            long namePos  = context.Request.PtrBuff[1].Position;
            long nameSize = context.Request.PtrBuff[1].Size;

            byte[] classBuffer = new byte[classSize];

            context.Memory.Read((ulong)classPos, classBuffer);

            byte[] nameBuffer = new byte[nameSize];

            context.Memory.Read((ulong)namePos, nameBuffer);

            string askedSetting = Encoding.ASCII.GetString(classBuffer).Trim('\0') + "!" + Encoding.ASCII.GetString(nameBuffer).Trim('\0');

            NxSettings.Settings.TryGetValue(askedSetting, out object nxSetting);

            if (nxSetting != null)
            {
                ulong settingSize;

                if (nxSetting is string stringValue)
                {
                    settingSize = (ulong)stringValue.Length + 1;
                }
                else if (nxSetting is int)
                {
                    settingSize = sizeof(int);
                }
                else if (nxSetting is bool)
                {
                    settingSize = 1;
                }
                else
                {
                    throw new NotImplementedException(nxSetting.GetType().Name);
                }

                context.ResponseData.Write(settingSize);
            }

            return ResultCode.Success;
        }

        [Command(38)]
        // GetSettingsItemValue(buffer<nn::settings::SettingsName, 0x19, 0x48>, buffer<nn::settings::SettingsItemKey, 0x19, 0x48>) -> (u64, buffer<unknown, 6, 0>)
        public ResultCode GetSettingsItemValue(ServiceCtx context)
        {
            long classPos  = context.Request.PtrBuff[0].Position;
            long classSize = context.Request.PtrBuff[0].Size;

            long namePos  = context.Request.PtrBuff[1].Position;
            long nameSize = context.Request.PtrBuff[1].Size;

            long replyPos  = context.Request.ReceiveBuff[0].Position;
            long replySize = context.Request.ReceiveBuff[0].Size;

            byte[] classBuffer = new byte[classSize];

            context.Memory.Read((ulong)classPos, classBuffer);

            byte[] nameBuffer = new byte[nameSize];

            context.Memory.Read((ulong)namePos, nameBuffer);

            string askedSetting = Encoding.ASCII.GetString(classBuffer).Trim('\0') + "!" + Encoding.ASCII.GetString(nameBuffer).Trim('\0');

            NxSettings.Settings.TryGetValue(askedSetting, out object nxSetting);

            if (nxSetting != null)
            {
                byte[] settingBuffer = new byte[replySize];

                if (nxSetting is string stringValue)
                {
                    if (stringValue.Length + 1 > replySize)
                    {
                        Logger.Error?.Print(LogClass.ServiceSet, $"{askedSetting} String value size is too big!");
                    }
                    else
                    {
                        settingBuffer = Encoding.ASCII.GetBytes(stringValue + "\0");
                    }
                }

                if (nxSetting is int intValue)
                {
                    settingBuffer = BitConverter.GetBytes(intValue);
                }
                else if (nxSetting is bool boolValue)
                {
                    settingBuffer[0] = boolValue ? (byte)1 : (byte)0;
                }
                else
                {
                    throw new NotImplementedException(nxSetting.GetType().Name);
                }

                context.Memory.Write((ulong)replyPos, settingBuffer);

                Logger.Debug?.Print(LogClass.ServiceSet, $"{askedSetting} set value: {nxSetting} as {nxSetting.GetType()}");
            }
            else
            {
                Logger.Error?.Print(LogClass.ServiceSet, $"{askedSetting} not found!");
            }

            return ResultCode.Success;
        }

       [Command(60)]
        // IsUserSystemClockAutomaticCorrectionEnabled() -> bool
        public ResultCode IsUserSystemClockAutomaticCorrectionEnabled(ServiceCtx context)
        {
            // NOTE: When set to true, is automatically synced with the internet.
            context.ResponseData.Write(true);

            Logger.Stub?.PrintStub(LogClass.ServiceSet, "Stubbed");

            return ResultCode.Success;
        }

        public byte[] GetFirmwareData(Switch device)
        {
            long   titleId     = 0x0100000000000809;
            string contentPath = device.System.ContentManager.GetInstalledContentPath(titleId, StorageId.NandSystem, NcaContentType.Data);

            if (string.IsNullOrWhiteSpace(contentPath))
            {
                return null;
            }

            string firmwareTitlePath = device.FileSystem.SwitchPathToSystemPath(contentPath);

            using(IStorage firmwareStorage = new LocalStorage(firmwareTitlePath, FileAccess.Read))
            {
                Nca firmwareContent = new Nca(device.System.KeySet, firmwareStorage);

                if (!firmwareContent.CanOpenSection(NcaSectionType.Data))
                {
                    return null;
                }

                IFileSystem firmwareRomFs = firmwareContent.OpenFileSystem(NcaSectionType.Data, device.System.FsIntegrityCheckLevel);

                Result result = firmwareRomFs.OpenFile(out IFile firmwareFile, "/file".ToU8Span(), OpenMode.Read);
                if (result.IsFailure())
                {
                    return null;
                }

                result = firmwareFile.GetSize(out long fileSize);
                if (result.IsFailure())
                {
                    return null;
                }

                byte[] data = new byte[fileSize];

                result = firmwareFile.Read(out _, 0, data);
                if (result.IsFailure())
                {
                    return null;
                }

                return data;
            }
        }
    }
}
