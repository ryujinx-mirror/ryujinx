using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.SystemState;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Aud
{
    class IAudioDevice : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent SystemEvent;

        public IAudioDevice(Horizon System)
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

            SystemEvent = new KEvent(System);

            //TODO: We shouldn't be signaling this here.
            SystemEvent.ReadableEvent.Signal();
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
                    Logger.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");

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

            Logger.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long GetActiveAudioDeviceName(ServiceCtx Context)
        {
            string Name = Context.Device.System.State.ActiveAudioOutput;

            long Position = Context.Request.ReceiveBuff[0].Position;
            long Size     = Context.Request.ReceiveBuff[0].Size;

            byte[] DeviceNameBuffer = Encoding.ASCII.GetBytes(Name + "\0");

            if ((ulong)DeviceNameBuffer.Length <= (ulong)Size)
            {
                Context.Memory.WriteBytes(Position, DeviceNameBuffer);
            }
            else
            {
                Logger.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");
            }

            return 0;
        }

        public long QueryAudioDeviceSystemEvent(ServiceCtx Context)
        {
            if (Context.Process.HandleTable.GenerateHandle(SystemEvent.ReadableEvent, out int Handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Logger.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long GetActiveChannelCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(2);

            Logger.PrintStub(LogClass.ServiceAudio, "Stubbed.");

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
                    Logger.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");

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

            Logger.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long GetAudioDeviceOutputVolumeAuto(ServiceCtx Context)
        {
            Context.ResponseData.Write(1f);

            Logger.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long GetActiveAudioDeviceNameAuto(ServiceCtx Context)
        {
            string Name = Context.Device.System.State.ActiveAudioOutput;

            (long Position, long Size) = Context.Request.GetBufferType0x22();

            byte[] DeviceNameBuffer = Encoding.UTF8.GetBytes(Name + '\0');

            if ((ulong)DeviceNameBuffer.Length <= (ulong)Size)
            {
                Context.Memory.WriteBytes(Position, DeviceNameBuffer);
            }
            else
            {
                Logger.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");
            }

            return 0;
        }

        public long QueryAudioDeviceInputEvent(ServiceCtx Context)
        {
            if (Context.Process.HandleTable.GenerateHandle(SystemEvent.ReadableEvent, out int Handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Logger.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long QueryAudioDeviceOutputEvent(ServiceCtx Context)
        {
            if (Context.Process.HandleTable.GenerateHandle(SystemEvent.ReadableEvent, out int Handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Logger.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }
    }
}
