using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Handles;
using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.HLE.OsHle.Services.Aud
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
                { 0,  ListAudioDeviceName            },
                { 1,  SetAudioDeviceOutputVolume     },
                { 3,  GetActiveAudioDeviceName       },
                { 4,  QueryAudioDeviceSystemEvent    },
                { 5,  GetActiveChannelCount          },
                { 6,  ListAudioDeviceNameAuto        },
                { 7,  SetAudioDeviceOutputVolumeAuto },
                { 8,  GetAudioDeviceOutputVolumeAuto },
                { 10, GetActiveAudioDeviceNameAuto   },
                { 11, QueryAudioDeviceInputEvent     },
                { 12, QueryAudioDeviceOutputEvent    }
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

                Context.Memory.WriteBytes(Position, Buffer);

                Position += Buffer.Length;
            }

            return 0;
        }

        public long SetAudioDeviceOutputVolume(ServiceCtx Context)
        {
            float Volume = Context.RequestData.ReadSingle();

            long Position = Context.Request.SendBuff[0].Position;
            long Size     = Context.Request.SendBuff[0].Size;

            byte[] DeviceNameBuffer = Context.Memory.ReadBytes(Position, Size);

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
                Context.Memory.WriteBytes(Position, DeviceNameBuffer);
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

        public long ListAudioDeviceNameAuto(ServiceCtx Context)
        {
            string[] DeviceNames = SystemStateMgr.AudioOutputs;

            Context.ResponseData.Write(DeviceNames.Length);

            (long Position, long Size) = Context.Request.GetBufferType0x22();

            long BasePosition = Position;

            foreach (string Name in DeviceNames)
            {
                byte[] Buffer = Encoding.UTF8.GetBytes(Name + '\0');

                if ((Position - BasePosition) + Buffer.Length > Size)
                {
                    Context.Ns.Log.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");

                    break;
                }

                Context.Memory.WriteBytes(Position, Buffer);

                Position += Buffer.Length;
            }

            return 0;
        }

        public long SetAudioDeviceOutputVolumeAuto(ServiceCtx Context)
        {
            float Volume = Context.RequestData.ReadSingle();

            (long Position, long Size) = Context.Request.GetBufferType0x21();

            byte[] DeviceNameBuffer = Context.Memory.ReadBytes(Position, Size);

            string DeviceName = Encoding.UTF8.GetString(DeviceNameBuffer);

            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long GetAudioDeviceOutputVolumeAuto(ServiceCtx Context)
        {
            Context.ResponseData.Write(1f);

            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long GetActiveAudioDeviceNameAuto(ServiceCtx Context)
        {
            string Name = Context.Ns.Os.SystemState.ActiveAudioOutput;

            (long Position, long Size) = Context.Request.GetBufferType0x22();

            byte[] DeviceNameBuffer = Encoding.UTF8.GetBytes(Name + '\0');

            if ((ulong)DeviceNameBuffer.Length <= (ulong)Size)
            {
                Context.Memory.WriteBytes(Position, DeviceNameBuffer);
            }
            else
            {
                Context.Ns.Log.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");
            }

            return 0;
        }

        public long QueryAudioDeviceInputEvent(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(SystemEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long QueryAudioDeviceOutputEvent(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(SystemEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }
    }
}
