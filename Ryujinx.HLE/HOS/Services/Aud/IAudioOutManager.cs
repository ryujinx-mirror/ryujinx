using ARMeilleure.Memory;
using Ryujinx.Audio;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Aud.AudioOut;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Aud
{
    [Service("audout:u")]
    class IAudioOutManager : IpcService
    {
        private const string DefaultAudioOutput   = "DeviceOut";
        private const int    DefaultSampleRate    = 48000;
        private const int    DefaultChannelsCount = 2;

        public IAudioOutManager(ServiceCtx context) { }

        [Command(0)]
        // ListAudioOuts() -> (u32 count, buffer<bytes, 6>)
        public ResultCode ListAudioOuts(ServiceCtx context)
        {
            return ListAudioOutsImpl(
                context,
                context.Request.ReceiveBuff[0].Position,
                context.Request.ReceiveBuff[0].Size);
        }

        [Command(1)]
        // OpenAudioOut(u32 sample_rate, u16 unused, u16 channel_count, nn::applet::AppletResourceUserId, pid, handle<copy, process>, buffer<bytes, 5> name_in)
        // -> (u32 sample_rate, u32 channel_count, u32 pcm_format, u32, object<nn::audio::detail::IAudioOut>, buffer<bytes, 6> name_out)
        public ResultCode OpenAudioOut(ServiceCtx context)
        {
            return OpenAudioOutImpl(
                context,
                context.Request.SendBuff[0].Position,
                context.Request.SendBuff[0].Size,
                context.Request.ReceiveBuff[0].Position,
                context.Request.ReceiveBuff[0].Size);
        }

        [Command(2)] // 3.0.0+
        // ListAudioOutsAuto() -> (u32 count, buffer<bytes, 0x22>)
        public ResultCode ListAudioOutsAuto(ServiceCtx context)
        {
            (long recvPosition, long recvSize) = context.Request.GetBufferType0x22();

            return ListAudioOutsImpl(context, recvPosition, recvSize);
        }

        [Command(3)] // 3.0.0+
        // OpenAudioOutAuto(u32 sample_rate, u16 unused, u16 channel_count, nn::applet::AppletResourceUserId, pid, handle<copy, process>, buffer<bytes, 0x21>)
        // -> (u32 sample_rate, u32 channel_count, u32 pcm_format, u32, object<nn::audio::detail::IAudioOut>, buffer<bytes, 0x22> name_out)
        public ResultCode OpenAudioOutAuto(ServiceCtx context)
        {
            (long sendPosition, long sendSize) = context.Request.GetBufferType0x21();
            (long recvPosition, long recvSize) = context.Request.GetBufferType0x22();

            return OpenAudioOutImpl(
                context,
                sendPosition,
                sendSize,
                recvPosition,
                recvSize);
        }

        private ResultCode ListAudioOutsImpl(ServiceCtx context, long position, long size)
        {
            int nameCount = 0;

            byte[] deviceNameBuffer = Encoding.ASCII.GetBytes(DefaultAudioOutput + "\0");

            if ((ulong)deviceNameBuffer.Length <= (ulong)size)
            {
                context.Memory.WriteBytes(position, deviceNameBuffer);

                nameCount++;
            }
            else
            {
                Logger.PrintError(LogClass.ServiceAudio, $"Output buffer size {size} too small!");
            }

            context.ResponseData.Write(nameCount);

            return ResultCode.Success;
        }

        private ResultCode OpenAudioOutImpl(ServiceCtx context, long sendPosition, long sendSize, long receivePosition, long receiveSize)
        {
            string deviceName = MemoryHelper.ReadAsciiString(
                context.Memory,
                sendPosition,
                sendSize);

            if (deviceName == string.Empty)
            {
                deviceName = DefaultAudioOutput;
            }

            if (deviceName != DefaultAudioOutput)
            {
                Logger.PrintWarning(LogClass.Audio, "Invalid device name!");

                return ResultCode.DeviceNotFound;
            }

            byte[] deviceNameBuffer = Encoding.ASCII.GetBytes(deviceName + "\0");

            if ((ulong)deviceNameBuffer.Length <= (ulong)receiveSize)
            {
                context.Memory.WriteBytes(receivePosition, deviceNameBuffer);
            }
            else
            {
                Logger.PrintError(LogClass.ServiceAudio, $"Output buffer size {receiveSize} too small!");
            }

            int sampleRate = context.RequestData.ReadInt32();
            int channels   = context.RequestData.ReadInt32();

            if (sampleRate == 0)
            {
                sampleRate = DefaultSampleRate;
            }

            if (sampleRate != DefaultSampleRate)
            {
                Logger.PrintWarning(LogClass.Audio, "Invalid sample rate!");

                return ResultCode.UnsupportedSampleRate;
            }

            channels = (ushort)channels;

            if (channels == 0)
            {
                channels = DefaultChannelsCount;
            }

            KEvent releaseEvent = new KEvent(context.Device.System);

            ReleaseCallback callback = () =>
            {
                releaseEvent.ReadableEvent.Signal();
            };

            IAalOutput audioOut = context.Device.AudioOut;

            int track = audioOut.OpenTrack(sampleRate, channels, callback);

            MakeObject(context, new IAudioOut(audioOut, releaseEvent, track));

            context.ResponseData.Write(sampleRate);
            context.ResponseData.Write(channels);
            context.ResponseData.Write((int)SampleFormat.PcmInt16);
            context.ResponseData.Write((int)PlaybackState.Stopped);

            return ResultCode.Success;
        }
    }
}