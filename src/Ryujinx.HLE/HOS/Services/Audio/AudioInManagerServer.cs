using Ryujinx.Audio.Common;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Services.Audio.AudioIn;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    [Service("audin:u")]
    class AudioInManagerServer : IpcService
    {
        private const int AudioInNameSize = 0x100;

        private readonly IAudioInManager _impl;

        public AudioInManagerServer(ServiceCtx context) : this(context, new AudioInManager(context.Device.System.AudioInputManager)) { }

        public AudioInManagerServer(ServiceCtx context, IAudioInManager impl) : base(context.Device.System.AudOutServer)
        {
            _impl = impl;
        }

        [CommandCmif(0)]
        // ListAudioIns() -> (u32, buffer<bytes, 6>)
        public ResultCode ListAudioIns(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioIns(false);

            ulong position = context.Request.ReceiveBuff[0].Position;
            ulong size = context.Request.ReceiveBuff[0].Size;

            ulong basePosition = position;

            int count = 0;

            foreach (string name in deviceNames)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(name);

                if ((position - basePosition) + (ulong)buffer.Length > size)
                {
                    Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");

                    break;
                }

                context.Memory.Write(position, buffer);
                MemoryHelper.FillWithZeros(context.Memory, position + (ulong)buffer.Length, AudioInNameSize - buffer.Length);

                position += AudioInNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // OpenAudioIn(AudioInInputConfiguration input_config, nn::applet::AppletResourceUserId, pid, handle<copy, process>, buffer<bytes, 5> name)
        // -> (u32 sample_rate, u32 channel_count, u32 pcm_format, u32, object<nn::audio::detail::IAudioIn>, buffer<bytes, 6> name)
        public ResultCode OpenAudioIn(ServiceCtx context)
        {
            AudioInputConfiguration inputConfiguration = context.RequestData.ReadStruct<AudioInputConfiguration>();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            ulong deviceNameInputPosition = context.Request.SendBuff[0].Position;
            ulong deviceNameInputSize = context.Request.SendBuff[0].Size;

            ulong deviceNameOutputPosition = context.Request.ReceiveBuff[0].Position;
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            ulong deviceNameOutputSize = context.Request.ReceiveBuff[0].Size;
#pragma warning restore IDE0059

            uint processHandle = (uint)context.Request.HandleDesc.ToCopy[0];

            string inputDeviceName = MemoryHelper.ReadAsciiString(context.Memory, deviceNameInputPosition, (long)deviceNameInputSize);

            ResultCode resultCode = _impl.OpenAudioIn(context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioIn obj, inputDeviceName, ref inputConfiguration, appletResourceUserId, processHandle);

            if (resultCode == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(outputConfiguration);

                byte[] outputDeviceNameRaw = Encoding.ASCII.GetBytes(outputDeviceName);

                context.Memory.Write(deviceNameOutputPosition, outputDeviceNameRaw);
                MemoryHelper.FillWithZeros(context.Memory, deviceNameOutputPosition + (ulong)outputDeviceNameRaw.Length, AudioInNameSize - outputDeviceNameRaw.Length);

                MakeObject(context, new AudioInServer(obj));
            }

            return resultCode;
        }

        [CommandCmif(2)] // 3.0.0+
        // ListAudioInsAuto() -> (u32, buffer<bytes, 0x22>)
        public ResultCode ListAudioInsAuto(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioIns(false);

            (ulong position, ulong size) = context.Request.GetBufferType0x22();

            ulong basePosition = position;

            int count = 0;

            foreach (string name in deviceNames)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(name);

                if ((position - basePosition) + (ulong)buffer.Length > size)
                {
                    Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");

                    break;
                }

                context.Memory.Write(position, buffer);
                MemoryHelper.FillWithZeros(context.Memory, position + (ulong)buffer.Length, AudioInNameSize - buffer.Length);

                position += AudioInNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [CommandCmif(3)] // 3.0.0+
        // OpenAudioInAuto(AudioInInputConfiguration input_config, nn::applet::AppletResourceUserId, pid, handle<copy, process>, buffer<bytes, 0x21>)
        // -> (u32 sample_rate, u32 channel_count, u32 pcm_format, u32, object<nn::audio::detail::IAudioIn>, buffer<bytes, 0x22> name)
        public ResultCode OpenAudioInAuto(ServiceCtx context)
        {
            AudioInputConfiguration inputConfiguration = context.RequestData.ReadStruct<AudioInputConfiguration>();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            (ulong deviceNameInputPosition, ulong deviceNameInputSize) = context.Request.GetBufferType0x21();
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            (ulong deviceNameOutputPosition, ulong deviceNameOutputSize) = context.Request.GetBufferType0x22();
#pragma warning restore IDE0059

            uint processHandle = (uint)context.Request.HandleDesc.ToCopy[0];

            string inputDeviceName = MemoryHelper.ReadAsciiString(context.Memory, deviceNameInputPosition, (long)deviceNameInputSize);

            ResultCode resultCode = _impl.OpenAudioIn(context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioIn obj, inputDeviceName, ref inputConfiguration, appletResourceUserId, processHandle);

            if (resultCode == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(outputConfiguration);

                byte[] outputDeviceNameRaw = Encoding.ASCII.GetBytes(outputDeviceName);

                context.Memory.Write(deviceNameOutputPosition, outputDeviceNameRaw);
                MemoryHelper.FillWithZeros(context.Memory, deviceNameOutputPosition + (ulong)outputDeviceNameRaw.Length, AudioInNameSize - outputDeviceNameRaw.Length);

                MakeObject(context, new AudioInServer(obj));
            }

            return resultCode;
        }

        [CommandCmif(4)] // 3.0.0+
        // ListAudioInsAutoFiltered() -> (u32, buffer<bytes, 0x22>)
        public ResultCode ListAudioInsAutoFiltered(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioIns(true);

            (ulong position, ulong size) = context.Request.GetBufferType0x22();

            ulong basePosition = position;

            int count = 0;

            foreach (string name in deviceNames)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(name);

                if ((position - basePosition) + (ulong)buffer.Length > size)
                {
                    Logger.Error?.Print(LogClass.ServiceAudio, $"Output buffer size {size} too small!");

                    break;
                }

                context.Memory.Write(position, buffer);
                MemoryHelper.FillWithZeros(context.Memory, position + (ulong)buffer.Length, AudioInNameSize - buffer.Length);

                position += AudioInNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [CommandCmif(5)] // 5.0.0+
        // OpenAudioInProtocolSpecified(b64 protocol_specified_related, AudioInInputConfiguration input_config, nn::applet::AppletResourceUserId, pid, handle<copy, process>, buffer<bytes, 5> name)
        // -> (u32 sample_rate, u32 channel_count, u32 pcm_format, u32, object<nn::audio::detail::IAudioIn>, buffer<bytes, 6> name)
        public ResultCode OpenAudioInProtocolSpecified(ServiceCtx context)
        {
            // NOTE: We always assume that only the default device will be plugged (we never report any USB Audio Class type devices).
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            bool protocolSpecifiedRelated = context.RequestData.ReadUInt64() == 1;
#pragma warning restore IDE0059

            AudioInputConfiguration inputConfiguration = context.RequestData.ReadStruct<AudioInputConfiguration>();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            ulong deviceNameInputPosition = context.Request.SendBuff[0].Position;
            ulong deviceNameInputSize = context.Request.SendBuff[0].Size;

            ulong deviceNameOutputPosition = context.Request.ReceiveBuff[0].Position;
#pragma warning disable IDE0051, IDE0059 // Remove unused private member
            ulong deviceNameOutputSize = context.Request.ReceiveBuff[0].Size;
#pragma warning restore IDE0051, IDE0059

            uint processHandle = (uint)context.Request.HandleDesc.ToCopy[0];

            string inputDeviceName = MemoryHelper.ReadAsciiString(context.Memory, deviceNameInputPosition, (long)deviceNameInputSize);

            ResultCode resultCode = _impl.OpenAudioIn(context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioIn obj, inputDeviceName, ref inputConfiguration, appletResourceUserId, processHandle);

            if (resultCode == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(outputConfiguration);

                byte[] outputDeviceNameRaw = Encoding.ASCII.GetBytes(outputDeviceName);

                context.Memory.Write(deviceNameOutputPosition, outputDeviceNameRaw);
                MemoryHelper.FillWithZeros(context.Memory, deviceNameOutputPosition + (ulong)outputDeviceNameRaw.Length, AudioInNameSize - outputDeviceNameRaw.Length);

                MakeObject(context, new AudioInServer(obj));
            }

            return resultCode;
        }
    }
}
