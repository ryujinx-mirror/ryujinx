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

        [CommandHipc(0)]
        // ListAudioOuts() -> (u32, buffer<bytes, 6>)
        public ResultCode ListAudioOuts(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioOuts();

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
                MemoryHelper.FillWithZeros(context.Memory, position + (ulong)buffer.Length, AudioOutNameSize - buffer.Length);

                position += AudioOutNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [CommandHipc(1)]
        // OpenAudioOut(AudioOutInputConfiguration input_config, nn::applet::AppletResourceUserId, pid, handle<copy, process> process_handle, buffer<bytes, 5> name_in)
        // -> (AudioOutInputConfiguration output_config, object<nn::audio::detail::IAudioOut>, buffer<bytes, 6> name_out)
        public ResultCode OpenAudioOut(ServiceCtx context)
        {
            AudioInputConfiguration inputConfiguration = context.RequestData.ReadStruct<AudioInputConfiguration>();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            ulong deviceNameInputPosition = context.Request.SendBuff[0].Position;
            ulong deviceNameInputSize = context.Request.SendBuff[0].Size;

            ulong deviceNameOutputPosition = context.Request.ReceiveBuff[0].Position;
            ulong deviceNameOutputSize = context.Request.ReceiveBuff[0].Size;

            uint processHandle = (uint)context.Request.HandleDesc.ToCopy[0];

            string inputDeviceName = MemoryHelper.ReadAsciiString(context.Memory, deviceNameInputPosition, (long)deviceNameInputSize);

            ResultCode resultCode = _impl.OpenAudioOut(context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioOut obj, inputDeviceName, ref inputConfiguration, appletResourceUserId, processHandle, context.Device.Configuration.AudioVolume);

            if (resultCode == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(outputConfiguration);

                byte[] outputDeviceNameRaw = Encoding.ASCII.GetBytes(outputDeviceName);

                context.Memory.Write(deviceNameOutputPosition, outputDeviceNameRaw);
                MemoryHelper.FillWithZeros(context.Memory, deviceNameOutputPosition + (ulong)outputDeviceNameRaw.Length, AudioOutNameSize - outputDeviceNameRaw.Length);

                MakeObject(context, new AudioOutServer(obj));
            }

            return resultCode;
        }

        [CommandHipc(2)] // 3.0.0+
        // ListAudioOutsAuto() -> (u32, buffer<bytes, 0x22>)
        public ResultCode ListAudioOutsAuto(ServiceCtx context)
        {
            string[] deviceNames = _impl.ListAudioOuts();

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
                MemoryHelper.FillWithZeros(context.Memory, position + (ulong)buffer.Length, AudioOutNameSize - buffer.Length);

                position += AudioOutNameSize;
                count++;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [CommandHipc(3)] // 3.0.0+
        // OpenAudioOut(AudioOutInputConfiguration input_config, nn::applet::AppletResourceUserId, pid, handle<copy, process> process_handle, buffer<bytes, 0x21> name_in)
        // -> (AudioOutInputConfiguration output_config, object<nn::audio::detail::IAudioOut>, buffer<bytes, 0x22> name_out)
        public ResultCode OpenAudioOutAuto(ServiceCtx context)
        {
            AudioInputConfiguration inputConfiguration = context.RequestData.ReadStruct<AudioInputConfiguration>();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            (ulong deviceNameInputPosition, ulong deviceNameInputSize) = context.Request.GetBufferType0x21();
            (ulong deviceNameOutputPosition, ulong deviceNameOutputSize) = context.Request.GetBufferType0x22();

            uint processHandle = (uint)context.Request.HandleDesc.ToCopy[0];

            string inputDeviceName = MemoryHelper.ReadAsciiString(context.Memory, deviceNameInputPosition, (long)deviceNameInputSize);

            ResultCode resultCode = _impl.OpenAudioOut(context, out string outputDeviceName, out AudioOutputConfiguration outputConfiguration, out IAudioOut obj, inputDeviceName, ref inputConfiguration, appletResourceUserId, processHandle, context.Device.Configuration.AudioVolume);

            if (resultCode == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(outputConfiguration);

                byte[] outputDeviceNameRaw = Encoding.ASCII.GetBytes(outputDeviceName);

                context.Memory.Write(deviceNameOutputPosition, outputDeviceNameRaw);
                MemoryHelper.FillWithZeros(context.Memory, deviceNameOutputPosition + (ulong)outputDeviceNameRaw.Length, AudioOutNameSize - outputDeviceNameRaw.Length);

                MakeObject(context, new AudioOutServer(obj));
            }

            return resultCode;
        }
    }
}
