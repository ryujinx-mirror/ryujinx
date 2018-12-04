using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.SystemState;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LibHac;
using Ryujinx.HLE.FileSystem;

namespace Ryujinx.HLE.HOS.Services.Set
{
    class ISystemSettingsServer : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ISystemSettingsServer()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 3,  GetFirmwareVersion  },
                { 4,  GetFirmwareVersion2  },
                { 23, GetColorSetId        },
                { 24, SetColorSetId        },
                { 38, GetSettingsItemValue }
            };
        }

        // GetFirmwareVersion() -> buffer<nn::settings::system::FirmwareVersion, 0x1a, 0x100>
        public static long GetFirmwareVersion(ServiceCtx context)
        {
            return GetFirmwareVersion2(context);
        }

        // GetFirmwareVersion2() -> buffer<nn::settings::system::FirmwareVersion, 0x1a, 0x100>
        public static long GetFirmwareVersion2(ServiceCtx context)
        {
            long replyPos  = context.Request.RecvListBuff[0].Position;
            long replySize = context.Request.RecvListBuff[0].Size;

            byte[] firmwareData = GetFirmwareData(context.Device);

            if (firmwareData != null)
            {
                context.Memory.WriteBytes(replyPos, firmwareData);

                return 0;
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

            //http://switchbrew.org/index.php?title=System_Version_Title
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

                context.Memory.WriteBytes(replyPos, ms.ToArray());
            }

            return 0;
        }

        // GetColorSetId() -> i32
        public static long GetColorSetId(ServiceCtx context)
        {
            context.ResponseData.Write((int)context.Device.System.State.ThemeColor);

            return 0;
        }

        // GetColorSetId() -> i32
        public static long SetColorSetId(ServiceCtx context)
        {
            int colorSetId = context.RequestData.ReadInt32();

            context.Device.System.State.ThemeColor = (ColorSet)colorSetId;

            return 0;
        }

        // GetSettingsItemValue(buffer<nn::settings::SettingsName, 0x19, 0x48>, buffer<nn::settings::SettingsItemKey, 0x19, 0x48>) -> (u64, buffer<unknown, 6, 0>)
        public static long GetSettingsItemValue(ServiceCtx context)
        {
            long classPos  = context.Request.PtrBuff[0].Position;
            long classSize = context.Request.PtrBuff[0].Size;

            long namePos  = context.Request.PtrBuff[1].Position;
            long nameSize = context.Request.PtrBuff[1].Size;

            long replyPos  = context.Request.ReceiveBuff[0].Position;
            long replySize = context.Request.ReceiveBuff[0].Size;

            byte[] Class = context.Memory.ReadBytes(classPos, classSize);
            byte[] name  = context.Memory.ReadBytes(namePos, nameSize);

            string askedSetting = Encoding.ASCII.GetString(Class).Trim('\0') + "!" + Encoding.ASCII.GetString(name).Trim('\0');

            NxSettings.Settings.TryGetValue(askedSetting, out object nxSetting);

            if (nxSetting != null)
            {
                byte[] settingBuffer = new byte[replySize];

                if (nxSetting is string stringValue)
                {
                    if (stringValue.Length + 1 > replySize)
                    {
                        Logger.PrintError(LogClass.ServiceSet, $"{askedSetting} String value size is too big!");
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

                context.Memory.WriteBytes(replyPos, settingBuffer);

                Logger.PrintDebug(LogClass.ServiceSet, $"{askedSetting} set value: {nxSetting} as {nxSetting.GetType()}");
            }
            else
            {
                Logger.PrintError(LogClass.ServiceSet, $"{askedSetting} not found!");
            }

            return 0;
        }

        public static byte[] GetFirmwareData(Switch device)
        {
            byte[] data        = null;
            long   titleId     = 0x0100000000000809;
            string contentPath = device.System.ContentManager.GetInstalledContentPath(titleId, StorageId.NandSystem, ContentType.Data);

            if(string.IsNullOrWhiteSpace(contentPath))
            {
                return null;
            }

            string     firmwareTitlePath = device.FileSystem.SwitchPathToSystemPath(contentPath);
            FileStream firmwareStream    = File.Open(firmwareTitlePath, FileMode.Open, FileAccess.Read);
            Nca        firmwareContent   = new Nca(device.System.KeySet, firmwareStream, false);
            Stream     romFsStream       = firmwareContent.OpenSection(0, false, device.System.FsIntegrityCheckLevel);

            if(romFsStream == null)
            {
                return null;
            }

            Romfs firmwareRomFs = new Romfs(romFsStream);

            using(MemoryStream memoryStream = new MemoryStream())
            {
                using (Stream firmwareFile = firmwareRomFs.OpenFile("/file"))
                {
                    firmwareFile.CopyTo(memoryStream);
                }

                data = memoryStream.ToArray();
            }

            firmwareContent.Dispose();
            firmwareStream.Dispose();

            return data;
        }
    }
}
