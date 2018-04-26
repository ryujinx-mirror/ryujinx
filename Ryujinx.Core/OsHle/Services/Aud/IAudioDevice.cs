using ChocolArm64.Memory;
using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.Core.OsHle.Services.Aud
{
    class IAudioDevice : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent SystemEvent;

        public IAudioDevice()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, ListAudioDeviceName         },
                { 1, SetAudioDeviceOutputVolume  },
                { 3, GetActiveAudioDeviceName    },
                { 4, QueryAudioDeviceSystemEvent },
                { 5, GetActiveChannelCount       }
            };

            SystemEvent = new KEvent();

            //TODO: We shouldn't be signaling this here.
            SystemEvent.WaitEvent.Set();
        }

        public long ListAudioDeviceName(ServiceCtx Context)
        {
            string[] DeviceNames = SystemStateMgr.AudioOutputs;

            Context.ResponseData.Write(DeviceNames.Length);

            long Position = Context.Request.ReceiveBuff[0].Position;
            long Size     = Context.Request.ReceiveBuff[0].Size;

            long BasePosition = Position;

            foreach (string Name in DeviceNames)
            {
                byte[] Buffer = Encoding.ASCII.GetBytes(Name + "\0");

                if ((Position - BasePosition) + Buffer.Length > Size)
                {
                    Context.Ns.Log.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");

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

            byte[] DeviceNameBuffer = AMemoryHelper.ReadBytes(Context.Memory, Position, Size);

            string DeviceName = Encoding.ASCII.GetString(DeviceNameBuffer);

            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long GetActiveAudioDeviceName(ServiceCtx Context)
        {
            string Name = Context.Ns.Os.SystemState.ActiveAudioOutput;

            long Position = Context.Request.ReceiveBuff[0].Position;
            long Size     = Context.Request.ReceiveBuff[0].Size;

            byte[] DeviceNameBuffer = Encoding.ASCII.GetBytes(Name + "\0");

            if ((ulong)DeviceNameBuffer.Length <= (ulong)Size)
            {
                AMemoryHelper.WriteBytes(Context.Memory, Position, DeviceNameBuffer);
            }
            else
            {
                Context.Ns.Log.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");
            }

            return 0;
        }

        public long QueryAudioDeviceSystemEvent(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(SystemEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long GetActiveChannelCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(2);

            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }
    }
}
