using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using Ryujinx.Core.Settings;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ryujinx.Core.OsHle.Services.Set
{
    class ISystemSettingsServer : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISystemSettingsServer()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {  4, GetFirmwareVersion2 },
                { 23, GetColorSetId       },
                { 24, SetColorSetId       }
            };
        }

        public static long GetFirmwareVersion2(ServiceCtx Context)
        {
            long ReplyPos  = Context.Request.RecvListBuff[0].Position;
            long ReplySize = Context.Request.RecvListBuff[0].Size;

            byte MajorFWVersion = 0x03;
            byte MinorFWVersion = 0x00;
            byte MicroFWVersion = 0x00;
            byte Unknown        = 0x00; //Build?

            int RevisionNumber  = 0x0A;

            string Platform     = "NX";
            string UnknownHex   = "7fbde2b0bba4d14107bf836e4643043d9f6c8e47";
            string Version      = "3.0.0";
            string Build        = "NintendoSDK Firmware for NX 3.0.0-10.0";

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

                AMemoryHelper.WriteBytes(Context.Memory, ReplyPos, MS.ToArray());
            }

            return 0;
        }

        public static long GetColorSetId(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)Context.Ns.Settings.ThemeColor);

            return 0;
        }

        public static long SetColorSetId(ServiceCtx Context)
        {
            int ColorSetId = Context.RequestData.ReadInt32();

            Context.Ns.Settings.ThemeColor = (ColorSet)ColorSetId;
            return 0;
        }
    }
}