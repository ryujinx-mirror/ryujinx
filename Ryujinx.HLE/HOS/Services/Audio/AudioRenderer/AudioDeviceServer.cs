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

        [CommandHipc(0)]
        // ListAudioDeviceName() -> (u32, buffer<bytes, 6>)
        public ResultCode ListAudioDeviceName(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioDeviceName();

            ulong position = context.Request.ReceiveBuff[0].Position;
            ulong size = context.Request.ReceiveBuff[0].Size;

            ulong basePosition = position;

            int count = 0;

            foreach (string name in deviceNames)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(name);

                if ((position - basePosition) + (ulong)buffer.Length > size)
                {
                    break;
                }

                context.Memory.Write(position, buffer);
                MemoryHelper.FillWithZeros(context.Memory, position + (ulong)buffer.Length, AudioDeviceNameSize - buffer.Length);

                position += AudioDeviceNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [CommandHipc(1)]
        // SetAudioDeviceOutputVolume(f32 volume, buffer<bytes, 5> name)
        public ResultCode SetAudioDeviceOutputVolume(ServiceCtx context)
        {
            float volume = context.RequestData.ReadSingle();

            ulong position = context.Request.SendBuff[0].Position;
            ulong size = context.Request.SendBuff[0].Size;

            string deviceName = MemoryHelper.ReadAsciiString(context.Memory, position, (long)size);

            return _impl.SetAudioDeviceOutputVolume(deviceName, volume);
        }

        [CommandHipc(2)]
        // GetAudioDeviceOutputVolume(buffer<bytes, 5> name) -> f32 volume
        public ResultCode GetAudioDeviceOutputVolume(ServiceCtx context)
        {
            ulong position = context.Request.SendBuff[0].Position;
            ulong size = context.Request.SendBuff[0].Size;

            string deviceName = MemoryHelper.ReadAsciiString(context.Memory, position, (long)size);

            ResultCode result = _impl.GetAudioDeviceOutputVolume(deviceName, out float volume);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(volume);
            }

            return result;
        }

        [CommandHipc(3)]
        // GetActiveAudioDeviceName() -> buffer<bytes, 6>
        public ResultCode GetActiveAudioDeviceName(ServiceCtx context)
        {
            string name = _impl.GetActiveAudioDeviceName();

            ulong position = context.Request.ReceiveBuff[0].Position;
            ulong size = context.Request.ReceiveBuff[0].Size;

            byte[] deviceNameBuffer = Encoding.ASCII.GetBytes(name + "\0");

            if ((ulong)deviceNameBuffer.Length <= size)
            {
                context.Memory.Write(position, deviceNameBuffer);
            }
            else
            {
                Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");
            }

            return ResultCode.Success;
        }

        [CommandHipc(4)]
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

        [CommandHipc(5)]
        // GetActiveChannelCount() -> u32
        public ResultCode GetActiveChannelCount(ServiceCtx context)
        {
            context.ResponseData.Write(_impl.GetActiveChannelCount());

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandHipc(6)] // 3.0.0+
        // ListAudioDeviceNameAuto() -> (u32, buffer<bytes, 0x22>)
        public ResultCode ListAudioDeviceNameAuto(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioDeviceName();

            (ulong position, ulong size) = context.Request.GetBufferType0x22();

            ulong basePosition = position;

            int count = 0;

            foreach (string name in deviceNames)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(name);

                if ((position - basePosition) + (ulong)buffer.Length > size)
                {
                    break;
                }

                context.Memory.Write(position, buffer);
                MemoryHelper.FillWithZeros(context.Memory, position + (ulong)buffer.Length, AudioDeviceNameSize - buffer.Length);

                position += AudioDeviceNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [CommandHipc(7)] // 3.0.0+
        // SetAudioDeviceOutputVolumeAuto(f32 volume, buffer<bytes, 0x21> name)
        public ResultCode SetAudioDeviceOutputVolumeAuto(ServiceCtx context)
        {
            float volume = context.RequestData.ReadSingle();

            (ulong position, ulong size) = context.Request.GetBufferType0x21();

            string deviceName = MemoryHelper.ReadAsciiString(context.Memory, position, (long)size);

            return _impl.SetAudioDeviceOutputVolume(deviceName, volume);
        }

        [CommandHipc(8)] // 3.0.0+
        // GetAudioDeviceOutputVolumeAuto(buffer<bytes, 0x21> name) -> f32
        public ResultCode GetAudioDeviceOutputVolumeAuto(ServiceCtx context)
        {
            (ulong position, ulong size) = context.Request.GetBufferType0x21();

            string deviceName = MemoryHelper.ReadAsciiString(context.Memory, position, (long)size);

            ResultCode result = _impl.GetAudioDeviceOutputVolume(deviceName, out float volume);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(volume);
            }

            return ResultCode.Success;
        }

        [CommandHipc(10)] // 3.0.0+
        // GetActiveAudioDeviceNameAuto() -> buffer<bytes, 0x22>
        public ResultCode GetActiveAudioDeviceNameAuto(ServiceCtx context)
        {
            string name = _impl.GetActiveAudioDeviceName();

            (ulong position, ulong size) = context.Request.GetBufferType0x22();

            byte[] deviceNameBuffer = Encoding.UTF8.GetBytes(name + '\0');

            if ((ulong)deviceNameBuffer.Length <= size)
            {
                context.Memory.Write(position, deviceNameBuffer);
            }
            else
            {
                Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");
            }

            return ResultCode.Success;
        }

        [CommandHipc(11)] // 3.0.0+
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

        [CommandHipc(12)] // 3.0.0+
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

        [CommandHipc(13)] // 13.0.0+
        // GetActiveAudioOutputDeviceName() -> buffer<bytes, 6>
        public ResultCode GetActiveAudioOutputDeviceName(ServiceCtx context)
        {
            string name = _impl.GetActiveAudioOutputDeviceName();

            ulong position = context.Request.ReceiveBuff[0].Position;
            ulong size = context.Request.ReceiveBuff[0].Size;

            byte[] deviceNameBuffer = Encoding.ASCII.GetBytes(name + "\0");

            if ((ulong)deviceNameBuffer.Length <= size)
            {
                context.Memory.Write(position, deviceNameBuffer);
            }
            else
            {
                Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");
            }

            return ResultCode.Success;
        }

        [CommandHipc(14)] // 13.0.0+
        // ListAudioOutputDeviceName() -> (u32, buffer<bytes, 6>)
        public ResultCode ListAudioOutputDeviceName(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioOutputDeviceName();

            ulong position = context.Request.ReceiveBuff[0].Position;
            ulong size = context.Request.ReceiveBuff[0].Size;

            ulong basePosition = position;

            int count = 0;

            foreach (string name in deviceNames)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(name);

                if ((position - basePosition) + (ulong)buffer.Length > size)
                {
                    break;
                }

                context.Memory.Write(position, buffer);
                MemoryHelper.FillWithZeros(context.Memory, position + (ulong)buffer.Length, AudioDeviceNameSize - buffer.Length);

                position += AudioDeviceNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }
    }
}
