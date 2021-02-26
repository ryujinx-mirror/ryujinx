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

        private IAudioInManager _impl;

        public AudioInManagerServer(ServiceCtx context) : this(context, new AudioInManager(context.Device.System.AudioInputManager)) { }

        public AudioInManagerServer(ServiceCtx context, IAudioInManager impl) : base(context.Device.System.AudOutServer)
        {
            _impl = impl;
        }

        [Command(0)]
        // ListAudioIns() -> (u32, buffer<bytes, 6>)
        public ResultCode ListAudioIns(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioIns(false);

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
                MemoryHelper.FillWithZeros(context.Memory, position + buffer.Length, AudioInNameSize - buffer.Length);

                position += AudioInNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [Command(1)]
        // OpenAudioIn(AudioInInputConfiguration input_config, nn::applet::AppletResourceUserId, pid, handle<copy, process>, buffer<bytes, 5> name)
        // -> (u32 sample_rate, u32 channel_count, u32 pcm_format, u32, object<nn::audio::detail::IAudioIn>, buffer<bytes, 6> name)
        public ResultCode OpenAudioIn(ServiceCtx context)
        {
            AudioInputConfiguration inputConfiguration = context.RequestData.ReadStruct<AudioInputConfiguration>();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            long deviceNameInputPosition = context.Request.SendBuff[0].Position;
            long deviceNameInputSize = context.Request.SendBuff[0].Size;

            long deviceNameOutputPosition = context.Request.ReceiveBuff[0].Position;
            long deviceNameOutputSize = context.Request.ReceiveBuff[0].Size;

            uint processHandle = (uint)context.Request.HandleDesc.ToCopy[0];

            string inputDeviceName = MemoryHelper.ReadAsciiString(context.Memory, deviceNameInputPosition, deviceNameInputSize);

            ResultCode resultCode = _impl.OpenAudioIn(context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioIn obj, inputDeviceName, ref inputConfiguration, appletResourceUserId, processHandle);

            if (resultCode == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(outputConfiguration);

                byte[] outputDeviceNameRaw = Encoding.ASCII.GetBytes(outputDeviceName);

                context.Memory.Write((ulong)deviceNameOutputPosition, outputDeviceNameRaw);
                MemoryHelper.FillWithZeros(context.Memory, deviceNameOutputPosition + outputDeviceNameRaw.Length, AudioInNameSize - outputDeviceNameRaw.Length);

                MakeObject(context, new AudioInServer(obj));
            }

            return resultCode;
        }

        [Command(2)] // 3.0.0+
        // ListAudioInsAuto() -> (u32, buffer<bytes, 0x22>)
        public ResultCode ListAudioInsAuto(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioIns(false);

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
                MemoryHelper.FillWithZeros(context.Memory, position + buffer.Length, AudioInNameSize - buffer.Length);

                position += AudioInNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [Command(3)] // 3.0.0+
        // OpenAudioInAuto(AudioInInputConfiguration input_config, nn::applet::AppletResourceUserId, pid, handle<copy, process>, buffer<bytes, 0x21>)
        // -> (u32 sample_rate, u32 channel_count, u32 pcm_format, u32, object<nn::audio::detail::IAudioIn>, buffer<bytes, 0x22> name)
        public ResultCode OpenAudioInAuto(ServiceCtx context)
        {
            AudioInputConfiguration inputConfiguration = context.RequestData.ReadStruct<AudioInputConfiguration>();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            (long deviceNameInputPosition, long deviceNameInputSize) = context.Request.GetBufferType0x21();
            (long deviceNameOutputPosition, long deviceNameOutputSize) = context.Request.GetBufferType0x22();

            uint processHandle = (uint)context.Request.HandleDesc.ToCopy[0];

            string inputDeviceName = MemoryHelper.ReadAsciiString(context.Memory, deviceNameInputPosition, deviceNameInputSize);

            ResultCode resultCode = _impl.OpenAudioIn(context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioIn obj, inputDeviceName, ref inputConfiguration, appletResourceUserId, processHandle);

            if (resultCode == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(outputConfiguration);

                byte[] outputDeviceNameRaw = Encoding.ASCII.GetBytes(outputDeviceName);

                context.Memory.Write((ulong)deviceNameOutputPosition, outputDeviceNameRaw);
                MemoryHelper.FillWithZeros(context.Memory, deviceNameOutputPosition + outputDeviceNameRaw.Length, AudioInNameSize - outputDeviceNameRaw.Length);

                MakeObject(context, new AudioInServer(obj));
            }

            return resultCode;
        }

        [Command(4)] // 3.0.0+
        // ListAudioInsAutoFiltered() -> (u32, buffer<bytes, 0x22>)
        public ResultCode ListAudioInsAutoFiltered(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioIns(true);

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
                MemoryHelper.FillWithZeros(context.Memory, position + buffer.Length, AudioInNameSize - buffer.Length);

                position += AudioInNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [Command(5)] // 5.0.0+
        // OpenAudioInProtocolSpecified(b64 protocol_specified_related, AudioInInputConfiguration input_config, nn::applet::AppletResourceUserId, pid, handle<copy, process>, buffer<bytes, 5> name)
        // -> (u32 sample_rate, u32 channel_count, u32 pcm_format, u32, object<nn::audio::detail::IAudioIn>, buffer<bytes, 6> name)
        public ResultCode OpenAudioInProtocolSpecified(ServiceCtx context)
        {
            // NOTE: We always assume that only the default device will be plugged (we never report any USB Audio Class type devices).
            bool protocolSpecifiedRelated = context.RequestData.ReadUInt64() == 1;

            AudioInputConfiguration inputConfiguration = context.RequestData.ReadStruct<AudioInputConfiguration>();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            long deviceNameInputPosition = context.Request.SendBuff[0].Position;
            long deviceNameInputSize = context.Request.SendBuff[0].Size;

            long deviceNameOutputPosition = context.Request.ReceiveBuff[0].Position;
            long deviceNameOutputSize = context.Request.ReceiveBuff[0].Size;

            uint processHandle = (uint)context.Request.HandleDesc.ToCopy[0];

            string inputDeviceName = MemoryHelper.ReadAsciiString(context.Memory, deviceNameInputPosition, deviceNameInputSize);

            ResultCode resultCode = _impl.OpenAudioIn(context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioIn obj, inputDeviceName, ref inputConfiguration, appletResourceUserId, processHandle);

            if (resultCode == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(outputConfiguration);

                byte[] outputDeviceNameRaw = Encoding.ASCII.GetBytes(outputDeviceName);

                context.Memory.Write((ulong)deviceNameOutputPosition, outputDeviceNameRaw);
                MemoryHelper.FillWithZeros(context.Memory, deviceNameOutputPosition + outputDeviceNameRaw.Length, AudioInNameSize - outputDeviceNameRaw.Length);

                MakeObject(context, new AudioInServer(obj));
            }

            return resultCode;
        }
    }
}
