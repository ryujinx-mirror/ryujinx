using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.Core.OsHle.IpcServices.Aud
{
    class IAudioDevice : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IAudioDevice()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, ListAudioDeviceName        },
                { 1, SetAudioDeviceOutputVolume },
            };
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

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position, (int)Size);

            return 0;
        }
    }
}