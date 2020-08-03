using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.SystemState;
using System;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    class IAudioDevice : IpcService
    {
        private KEvent _systemEvent;

        public IAudioDevice(Horizon system)
        {
            _systemEvent = new KEvent(system.KernelContext);

            // TODO: We shouldn't be signaling this here.
            _systemEvent.ReadableEvent.Signal();
        }

        [Command(0)]
        // ListAudioDeviceName() -> (u32, buffer<bytes, 6>)
        public ResultCode ListAudioDeviceName(ServiceCtx context)
        {
            string[] deviceNames = SystemStateMgr.AudioOutputs;

            context.ResponseData.Write(deviceNames.Length);

            long position = context.Request.ReceiveBuff[0].Position;
            long size     = context.Request.ReceiveBuff[0].Size;

            long basePosition = position;

            foreach (string name in deviceNames)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(name + "\0");

                if ((position - basePosition) + buffer.Length > size)
                {
                    Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");

                    break;
                }

                context.Memory.Write((ulong)position, buffer);

                position += buffer.Length;
            }

            return ResultCode.Success;
        }

        [Command(1)]
        // SetAudioDeviceOutputVolume(u32, buffer<bytes, 5>)
        public ResultCode SetAudioDeviceOutputVolume(ServiceCtx context)
        {
            float volume = context.RequestData.ReadSingle();

            long position = context.Request.SendBuff[0].Position;
            long size     = context.Request.SendBuff[0].Size;

            byte[] deviceNameBuffer = new byte[size];
            
            context.Memory.Read((ulong)position, deviceNameBuffer);

            string deviceName = Encoding.ASCII.GetString(deviceNameBuffer);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [Command(3)]
        // GetActiveAudioDeviceName() -> buffer<bytes, 6>
        public ResultCode GetActiveAudioDeviceName(ServiceCtx context)
        {
            string name = context.Device.System.State.ActiveAudioOutput;

            long position = context.Request.ReceiveBuff[0].Position;
            long size     = context.Request.ReceiveBuff[0].Size;

            byte[] deviceNameBuffer = Encoding.ASCII.GetBytes(name + "\0");

            if ((ulong)deviceNameBuffer.Length <= (ulong)size)
            {
                context.Memory.Write((ulong)position, deviceNameBuffer);
            }
            else
            {
                Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");
            }

            return ResultCode.Success;
        }

        [Command(4)]
        // QueryAudioDeviceSystemEvent() -> handle<copy, event>
        public ResultCode QueryAudioDeviceSystemEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_systemEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [Command(5)]
        // GetActiveChannelCount() -> u32
        public ResultCode GetActiveChannelCount(ServiceCtx context)
        {
            context.ResponseData.Write(2);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [Command(6)]
        // ListAudioDeviceNameAuto() -> (u32, buffer<bytes, 0x22>)
        public ResultCode ListAudioDeviceNameAuto(ServiceCtx context)
        {
            string[] deviceNames = SystemStateMgr.AudioOutputs;

            context.ResponseData.Write(deviceNames.Length);

            (long position, long size) = context.Request.GetBufferType0x22();

            long basePosition = position;

            foreach (string name in deviceNames)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(name + '\0');

                if ((position - basePosition) + buffer.Length > size)
                {
                    Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");

                    break;
                }

                context.Memory.Write((ulong)position, buffer);

                position += buffer.Length;
            }

            return ResultCode.Success;
        }

        [Command(7)]
        // SetAudioDeviceOutputVolumeAuto(u32, buffer<bytes, 0x21>)
        public ResultCode SetAudioDeviceOutputVolumeAuto(ServiceCtx context)
        {
            float volume = context.RequestData.ReadSingle();

            (long position, long size) = context.Request.GetBufferType0x21();

            byte[] deviceNameBuffer = new byte[size];

            context.Memory.Read((ulong)position, deviceNameBuffer);

            string deviceName = Encoding.UTF8.GetString(deviceNameBuffer);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [Command(8)]
        // GetAudioDeviceOutputVolumeAuto(buffer<bytes, 0x21>) -> u32
        public ResultCode GetAudioDeviceOutputVolumeAuto(ServiceCtx context)
        {
            context.ResponseData.Write(1f);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [Command(10)]
        // GetActiveAudioDeviceNameAuto() -> buffer<bytes, 0x22>
        public ResultCode GetActiveAudioDeviceNameAuto(ServiceCtx context)
        {
            string name = context.Device.System.State.ActiveAudioOutput;

            (long position, long size) = context.Request.GetBufferType0x22();

            byte[] deviceNameBuffer = Encoding.UTF8.GetBytes(name + '\0');

            if ((ulong)deviceNameBuffer.Length <= (ulong)size)
            {
                context.Memory.Write((ulong)position, deviceNameBuffer);
            }
            else
            {
                Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");
            }

            return ResultCode.Success;
        }

        [Command(11)]
        // QueryAudioDeviceInputEvent() -> handle<copy, event>
        public ResultCode QueryAudioDeviceInputEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_systemEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [Command(12)]
        // QueryAudioDeviceOutputEvent() -> handle<copy, event>
        public ResultCode QueryAudioDeviceOutputEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_systemEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }
    }
}