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
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISystemSettingsServer()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 3,  GetFirmwareVersion  },
                { 4,  GetFirmwareVersion2  },
                { 23, GetColorSetId        },
                { 24, SetColorSetId        },
                { 38, GetSettingsItemValue }
            };
        }

        // GetFirmwareVersion() -> buffer<nn::settings::system::FirmwareVersion, 0x1a, 0x100>
        public static long GetFirmwareVersion(ServiceCtx Context)
        {
            return GetFirmwareVersion2(Context);
        }

        // GetFirmwareVersion2() -> buffer<nn::settings::system::FirmwareVersion, 0x1a, 0x100>
        public static long GetFirmwareVersion2(ServiceCtx Context)
        {
            long ReplyPos  = Context.Request.RecvListBuff[0].Position;
            long ReplySize = Context.Request.RecvListBuff[0].Size;

            byte[] FirmwareData = GetFirmwareData(Context.Device);

            if (FirmwareData != null)
            {
                Context.Memory.WriteBytes(ReplyPos, FirmwareData);

                return 0;
            }

            const byte MajorFWVersion = 0x03;
            const byte MinorFWVersion = 0x00;
            const byte MicroFWVersion = 0x00;
            const byte Unknown        = 0x00; //Build?

            const int RevisionNumber = 0x0A;

            const string Platform   = "NX";
            const string UnknownHex = "7fbde2b0bba4d14107bf836e4643043d9f6c8e47";
            const string Version    = "3.0.0";
            const string Build      = "NintendoSDK Firmware for NX 3.0.0-10.0";

            //http://switchbrew.org/index.php?title=System_Version_Title
            using (MemoryStream MS = new MemoryStream(0x100))
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                Writer.Write(MajorFWVersion);
                Writer.Write(MinorFWVersion);
                Writer.Write(MicroFWVersion);
                Writer.Write(Unknown);

                Writer.Write(RevisionNumber);

                Writer.Write(Encoding.ASCII.GetBytes(Platform));

                MS.Seek(0x28, SeekOrigin.Begin);

                Writer.Write(Encoding.ASCII.GetBytes(UnknownHex));

                MS.Seek(0x68, SeekOrigin.Begin);

                Writer.Write(Encoding.ASCII.GetBytes(Version));

                MS.Seek(0x80, SeekOrigin.Begin);

                Writer.Write(Encoding.ASCII.GetBytes(Build));

                Context.Memory.WriteBytes(ReplyPos, MS.ToArray());
            }

            return 0;
        }

        // GetColorSetId() -> i32
        public static long GetColorSetId(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)Context.Device.System.State.ThemeColor);

            return 0;
        }

        // GetColorSetId() -> i32
        public static long SetColorSetId(ServiceCtx Context)
        {
            int ColorSetId = Context.RequestData.ReadInt32();

            Context.Device.System.State.ThemeColor = (ColorSet)ColorSetId;

            return 0;
        }

        // GetSettingsItemValue(buffer<nn::settings::SettingsName, 0x19, 0x48>, buffer<nn::settings::SettingsItemKey, 0x19, 0x48>) -> (u64, buffer<unknown, 6, 0>)
        public static long GetSettingsItemValue(ServiceCtx Context)
        {
            long ClassPos  = Context.Request.PtrBuff[0].Position;
            long ClassSize = Context.Request.PtrBuff[0].Size;

            long NamePos  = Context.Request.PtrBuff[1].Position;
            long NameSize = Context.Request.PtrBuff[1].Size;

            long ReplyPos  = Context.Request.ReceiveBuff[0].Position;
            long ReplySize = Context.Request.ReceiveBuff[0].Size;

            byte[] Class = Context.Memory.ReadBytes(ClassPos, ClassSize);
            byte[] Name  = Context.Memory.ReadBytes(NamePos, NameSize);

            string AskedSetting = Encoding.ASCII.GetString(Class).Trim('\0') + "!" + Encoding.ASCII.GetString(Name).Trim('\0');

            NxSettings.Settings.TryGetValue(AskedSetting, out object NxSetting);

            if (NxSetting != null)
            {
                byte[] SettingBuffer = new byte[ReplySize];

                if (NxSetting is string StringValue)
                {
                    if (StringValue.Length + 1 > ReplySize)
                    {
                        Logger.PrintError(LogClass.ServiceSet, $"{AskedSetting} String value size is too big!");
                    }
                    else
                    {
                        SettingBuffer = Encoding.ASCII.GetBytes(StringValue + "\0");
                    }
                }

                if (NxSetting is int IntValue)
                {
                    SettingBuffer = BitConverter.GetBytes(IntValue);
                }
                else if (NxSetting is bool BoolValue)
                {
                    SettingBuffer[0] = BoolValue ? (byte)1 : (byte)0;
                }
                else
                {
                    throw new NotImplementedException(NxSetting.GetType().Name);
                }

                Context.Memory.WriteBytes(ReplyPos, SettingBuffer);

                Logger.PrintDebug(LogClass.ServiceSet, $"{AskedSetting} set value: {NxSetting} as {NxSetting.GetType()}");
            }
            else
            {
                Logger.PrintError(LogClass.ServiceSet, $"{AskedSetting} not found!");
            }

            return 0;
        }

        public static byte[] GetFirmwareData(Switch Device)
        {
            byte[] Data        = null;
            long   TitleId     = 0x0100000000000809;
            string ContentPath = Device.System.ContentManager.GetInstalledContentPath(TitleId, StorageId.NandSystem, ContentType.Data);

            if(string.IsNullOrWhiteSpace(ContentPath))
            {
                return null;
            }

            string     FirmwareTitlePath = Device.FileSystem.SwitchPathToSystemPath(ContentPath);
            FileStream FirmwareStream    = File.Open(FirmwareTitlePath, FileMode.Open, FileAccess.Read);
            Nca        FirmwareContent   = new Nca(Device.System.KeySet, FirmwareStream, false);
            Stream     RomFsStream       = FirmwareContent.OpenSection(0, false, Device.System.FsIntegrityCheckLevel);

            if(RomFsStream == null)
            {
                return null;
            }

            Romfs FirmwareRomFs = new Romfs(RomFsStream);

            using(MemoryStream MemoryStream = new MemoryStream())
            {
                using (Stream FirmwareFile = FirmwareRomFs.OpenFile("/file"))
                {
                    FirmwareFile.CopyTo(MemoryStream);
                }

                Data = MemoryStream.ToArray();
            }

            FirmwareContent.Dispose();
            FirmwareStream.Dispose();

            return Data;
        }
    }
}
