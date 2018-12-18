using ChocolArm64.Memory;
using Ryujinx.Audio;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Aud.AudioOut;
using System.Collections.Generic;
using System.Text;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Aud
{
    class IAudioOutManager : IpcService
    {
        private const string DefaultAudioOutput = "DeviceOut";

        private const int DefaultSampleRate = 48000;

        private const int DefaultChannelsCount = 2;

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IAudioOutManager()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, ListAudioOuts     },
                { 1, OpenAudioOut      },
                { 2, ListAudioOutsAuto },
                { 3, OpenAudioOutAuto  }
            };
        }

        public long ListAudioOuts(ServiceCtx context)
        {
            return ListAudioOutsImpl(
                context,
                context.Request.ReceiveBuff[0].Position,
                context.Request.ReceiveBuff[0].Size);
        }

        public long OpenAudioOut(ServiceCtx context)
        {
            return OpenAudioOutImpl(
                context,
                context.Request.SendBuff[0].Position,
                context.Request.SendBuff[0].Size,
                context.Request.ReceiveBuff[0].Position,
                context.Request.ReceiveBuff[0].Size);
        }

        public long ListAudioOutsAuto(ServiceCtx context)
        {
            (long recvPosition, long recvSize) = context.Request.GetBufferType0x22();

            return ListAudioOutsImpl(context, recvPosition, recvSize);
        }

        public long OpenAudioOutAuto(ServiceCtx context)
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

        private long ListAudioOutsImpl(ServiceCtx context, long position, long size)
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

            return 0;
        }

        private long OpenAudioOutImpl(ServiceCtx context, long sendPosition, long sendSize, long receivePosition, long receiveSize)
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

                return MakeError(ErrorModule.Audio, AudErr.DeviceNotFound);
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

                return MakeError(ErrorModule.Audio, AudErr.UnsupportedSampleRate);
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

            return 0;
        }
    }
}
