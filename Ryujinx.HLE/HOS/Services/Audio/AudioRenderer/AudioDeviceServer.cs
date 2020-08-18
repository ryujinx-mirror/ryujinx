using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRenderer
{
    class AudioDeviceServer : IpcService
    {
        private const int AudioDeviceNameSize = 0x100;

        private IAudioDevice _impl;

        public AudioDeviceServer(IAudioDevice impl)
        {
            _impl = impl;
        }

        [Command(0)]
        // ListAudioDeviceName() -> (u32, buffer<bytes, 6>)
        public ResultCode ListAudioDeviceName(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioDeviceName();

            long position = context.Request.ReceiveBuff[0].Position;
            long size = context.Request.ReceiveBuff[0].Size;

            long basePosition = position;

            int count = 0;

            foreach (string name in deviceNames)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(name);

                if ((position - basePosition) + buffer.Length > size)
                {
                    Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");

                    break;
                }

                context.Memory.Write((ulong)position, buffer);
                MemoryHelper.FillWithZeros(context.Memory, position + buffer.Length, AudioDeviceNameSize - buffer.Length);

                position += AudioDeviceNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [Command(1)]
        // SetAudioDeviceOutputVolume(f32 volume, buffer<bytes, 5> name)
        public ResultCode SetAudioDeviceOutputVolume(ServiceCtx context)
        {
            float volume = context.RequestData.ReadSingle();

            long position = context.Request.SendBuff[0].Position;
            long size = context.Request.SendBuff[0].Size;

            string deviceName = MemoryHelper.ReadAsciiString(context.Memory, position, size);

            return _impl.SetAudioDeviceOutputVolume(deviceName, volume);
        }

        [Command(2)]
        // GetAudioDeviceOutputVolume(buffer<bytes, 5> name) -> f32 volume
        public ResultCode GetAudioDeviceOutputVolume(ServiceCtx context)
        {
            long position = context.Request.SendBuff[0].Position;
            long size = context.Request.SendBuff[0].Size;

            string deviceName = MemoryHelper.ReadAsciiString(context.Memory, position, size);

            ResultCode result = _impl.GetAudioDeviceOutputVolume(deviceName, out float volume);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(volume);
            }

            return result;
        }

        [Command(3)]
        // GetActiveAudioDeviceName() -> buffer<bytes, 6>
        public ResultCode GetActiveAudioDeviceName(ServiceCtx context)
        {
            string name = _impl.GetActiveAudioDeviceName();

            long position = context.Request.ReceiveBuff[0].Position;
            long size = context.Request.ReceiveBuff[0].Size;

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
            KEvent deviceSystemEvent = _impl.QueryAudioDeviceSystemEvent();

            if (context.Process.HandleTable.GenerateHandle(deviceSystemEvent.ReadableEvent, out int handle) != KernelResult.Success)
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
            context.ResponseData.Write(_impl.GetActiveChannelCount());

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [Command(6)] // 3.0.0+
        // ListAudioDeviceNameAuto() -> (u32, buffer<bytes, 0x22>)
        public ResultCode ListAudioDeviceNameAuto(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioDeviceName();

            (long position, long size) = context.Request.GetBufferType0x22();

            long basePosition = position;

            int count = 0;

            foreach (string name in deviceNames)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(name);

                if ((position - basePosition) + buffer.Length > size)
                {
                    Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");

                    break;
                }

                context.Memory.Write((ulong)position, buffer);
                MemoryHelper.FillWithZeros(context.Memory, position + buffer.Length, AudioDeviceNameSize - buffer.Length);

                position += AudioDeviceNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [Command(7)] // 3.0.0+
        // SetAudioDeviceOutputVolumeAuto(f32 volume, buffer<bytes, 0x21> name)
        public ResultCode SetAudioDeviceOutputVolumeAuto(ServiceCtx context)
        {
            float volume = context.RequestData.ReadSingle();

            (long position, long size) = context.Request.GetBufferType0x21();

            string deviceName = MemoryHelper.ReadAsciiString(context.Memory, position, size);

            return _impl.SetAudioDeviceOutputVolume(deviceName, volume);
        }

        [Command(8)] // 3.0.0+
        // GetAudioDeviceOutputVolumeAuto(buffer<bytes, 0x21> name) -> f32
        public ResultCode GetAudioDeviceOutputVolumeAuto(ServiceCtx context)
        {
            (long position, long size) = context.Request.GetBufferType0x21();

            string deviceName = MemoryHelper.ReadAsciiString(context.Memory, position, size);

            ResultCode result = _impl.GetAudioDeviceOutputVolume(deviceName, out float volume);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(volume);
            }

            return ResultCode.Success;
        }

        [Command(10)] // 3.0.0+
        // GetActiveAudioDeviceNameAuto() -> buffer<bytes, 0x22>
        public ResultCode GetActiveAudioDeviceNameAuto(ServiceCtx context)
        {
            string name = _impl.GetActiveAudioDeviceName();

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

        [Command(11)] // 3.0.0+
        // QueryAudioDeviceInputEvent() -> handle<copy, event>
        public ResultCode QueryAudioDeviceInputEvent(ServiceCtx context)
        {
            KEvent deviceInputEvent = _impl.QueryAudioDeviceInputEvent();

            if (context.Process.HandleTable.GenerateHandle(deviceInputEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [Command(12)] // 3.0.0+
        // QueryAudioDeviceOutputEvent() -> handle<copy, event>
        public ResultCode QueryAudioDeviceOutputEvent(ServiceCtx context)
        {
            KEvent deviceOutputEvent = _impl.QueryAudioDeviceOutputEvent();

            if (context.Process.HandleTable.GenerateHandle(deviceOutputEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [Command(13)]
        // GetAudioSystemMasterVolumeSetting(buffer<bytes, 5> name) -> f32
        public ResultCode GetAudioSystemMasterVolumeSetting(ServiceCtx context)
        {
            long position = context.Request.SendBuff[0].Position;
            long size = context.Request.SendBuff[0].Size;

            string deviceName = MemoryHelper.ReadAsciiString(context.Memory, position, size);

            ResultCode result = _impl.GetAudioSystemMasterVolumeSetting(deviceName, out float systemMasterVolume);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(systemMasterVolume);
            }

            return result;
        }
    }
}
