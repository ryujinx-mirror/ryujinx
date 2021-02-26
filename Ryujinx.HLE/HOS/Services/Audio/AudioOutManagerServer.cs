using Ryujinx.Audio.Common;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Services.Audio.AudioOut;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    [Service("audout:u")]
    class AudioOutManagerServer : IpcService
    {
        private const int AudioOutNameSize = 0x100;

        private IAudioOutManager _impl;

        public AudioOutManagerServer(ServiceCtx context) : this(context, new AudioOutManager(context.Device.System.AudioOutputManager)) { }

        public AudioOutManagerServer(ServiceCtx context, IAudioOutManager impl) : base(context.Device.System.AudOutServer)
        {
            _impl = impl;
        }

        [Command(0)]
        // ListAudioOuts() -> (u32, buffer<bytes, 6>)
        public ResultCode ListAudioOuts(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioOuts();

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
                MemoryHelper.FillWithZeros(context.Memory, position + buffer.Length, AudioOutNameSize - buffer.Length);

                position += AudioOutNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [Command(1)]
        // OpenAudioOut(AudioOutInputConfiguration input_config, nn::applet::AppletResourceUserId, pid, handle<copy, process> process_handle, buffer<bytes, 5> name_in)
        // -> (AudioOutInputConfiguration output_config, object<nn::audio::detail::IAudioOut>, buffer<bytes, 6> name_out)
        public ResultCode OpenAudioOut(ServiceCtx context)
        {
            AudioInputConfiguration inputConfiguration = context.RequestData.ReadStruct<AudioInputConfiguration>();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            long deviceNameInputPosition = context.Request.SendBuff[0].Position;
            long deviceNameInputSize = context.Request.SendBuff[0].Size;

            long deviceNameOutputPosition = context.Request.ReceiveBuff[0].Position;
            long deviceNameOutputSize = context.Request.ReceiveBuff[0].Size;

            uint processHandle = (uint)context.Request.HandleDesc.ToCopy[0];

            string inputDeviceName = MemoryHelper.ReadAsciiString(context.Memory, deviceNameInputPosition, deviceNameInputSize);

            ResultCode resultCode = _impl.OpenAudioOut(context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioOut obj, inputDeviceName, ref inputConfiguration, appletResourceUserId, processHandle);

            if (resultCode == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(outputConfiguration);

                byte[] outputDeviceNameRaw = Encoding.ASCII.GetBytes(outputDeviceName);

                context.Memory.Write((ulong)deviceNameOutputPosition, outputDeviceNameRaw);
                MemoryHelper.FillWithZeros(context.Memory, deviceNameOutputPosition + outputDeviceNameRaw.Length, AudioOutNameSize - outputDeviceNameRaw.Length);

                MakeObject(context, new AudioOutServer(obj));
            }

            return resultCode;
        }

        [Command(2)] // 3.0.0+
        // ListAudioOutsAuto() -> (u32, buffer<bytes, 0x22>)
        public ResultCode ListAudioOutsAuto(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioOuts();

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
                MemoryHelper.FillWithZeros(context.Memory, position + buffer.Length, AudioOutNameSize - buffer.Length);

                position += AudioOutNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [Command(3)] // 3.0.0+
        // OpenAudioOut(AudioOutInputConfiguration input_config, nn::applet::AppletResourceUserId, pid, handle<copy, process> process_handle, buffer<bytes, 0x21> name_in)
        // -> (AudioOutInputConfiguration output_config, object<nn::audio::detail::IAudioOut>, buffer<bytes, 0x22> name_out)
        public ResultCode OpenAudioOutAuto(ServiceCtx context)
        {
            AudioInputConfiguration inputConfiguration = context.RequestData.ReadStruct<AudioInputConfiguration>();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            (long deviceNameInputPosition, long deviceNameInputSize) = context.Request.GetBufferType0x21();
            (long deviceNameOutputPosition, long deviceNameOutputSize) = context.Request.GetBufferType0x22();

            uint processHandle = (uint)context.Request.HandleDesc.ToCopy[0];

            string inputDeviceName = MemoryHelper.ReadAsciiString(context.Memory, deviceNameInputPosition, deviceNameInputSize);

            ResultCode resultCode = _impl.OpenAudioOut(context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioOut obj, inputDeviceName, ref inputConfiguration, appletResourceUserId, processHandle);

            if (resultCode == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(outputConfiguration);

                byte[] outputDeviceNameRaw = Encoding.ASCII.GetBytes(outputDeviceName);

                context.Memory.Write((ulong)deviceNameOutputPosition, outputDeviceNameRaw);
                MemoryHelper.FillWithZeros(context.Memory, deviceNameOutputPosition + outputDeviceNameRaw.Length, AudioOutNameSize - outputDeviceNameRaw.Length);

                MakeObject(context, new AudioOutServer(obj));
            }

            return resultCode;
        }
    }
}
