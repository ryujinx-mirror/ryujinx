using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.Core.OsHle.Services.Aud
{
    class IAudioDeviceService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent SystemEvent;

        public IAudioDeviceService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, ListAudioDeviceName         },
                { 1, SetAudioDeviceOutputVolume  },
                { 4, QueryAudioDeviceSystemEvent },
                { 5, GetActiveChannelCount       }
            };

            SystemEvent = new KEvent();

            //TODO: We shouldn't be signaling this here.
            SystemEvent.WaitEvent.Set();
        }

        public long ListAudioDeviceName(ServiceCtx Context)
        {
            string[] Names = new string[] { "FIXME" };

            Context.ResponseData.Write(Names.Length);

            long Position = Context.Request.ReceiveBuff[0].Position;
            long Size     = Context.Request.ReceiveBuff[0].Size;

            long BasePosition = Position;

            foreach (string Name in Names)
            {
                byte[] Buffer = Encoding.ASCII.GetBytes(Name + '\0');

                if ((Position - BasePosition) + Buffer.Length > Size)
                {
                    break;
                }

                AMemoryHelper.WriteBytes(Context.Memory, Position, Buffer);

                Position += Buffer.Length;
            }

            return 0;
        }

        public long SetAudioDeviceOutputVolume(ServiceCtx Context)
        {
            float Volume = Context.RequestData.ReadSingle();

            long Position = Context.Request.SendBuff[0].Position;
            long Size     = Context.Request.SendBuff[0].Size;

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position, Size);

            Logging.Stub(LogClass.ServiceAudio, $"Volume = {Volume}, Position = {Position}, Size = {Size}");

            return 0;
        }

        public long QueryAudioDeviceSystemEvent(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(SystemEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Logging.Stub(LogClass.ServiceAudio, "Stubbed");

            return 0;
        }

        public long GetActiveChannelCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(2);

            Logging.Stub(LogClass.ServiceAudio, "Stubbed");

            return 0;
        }
    }
}
