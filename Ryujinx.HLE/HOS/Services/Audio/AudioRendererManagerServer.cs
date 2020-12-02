using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Server;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Services.Audio.AudioRenderer;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    [Service("audren:u")]
    class AudioRendererManagerServer : IpcService
    {
        private const int InitialRevision = ('R' << 0) | ('E' << 8) | ('V' << 16) | ('1' << 24);

        private IAudioRendererManager _impl;

        public AudioRendererManagerServer(ServiceCtx context) : this(context, new AudioRendererManager(context.Device.System.AudioRendererManager, context.Device.System.AudioDeviceSessionRegistry)) { }

        public AudioRendererManagerServer(ServiceCtx context, IAudioRendererManager impl) : base(context.Device.System.AudRenServer)
        {
            _impl = impl;
        }

        [Command(0)]
        // OpenAudioRenderer(nn::audio::detail::AudioRendererParameterInternal parameter, u64 workBufferSize, nn::applet::AppletResourceUserId appletResourceId, pid, handle<copy> workBuffer, handle<copy> processHandle)
        // -> object<nn::audio::detail::IAudioRenderer>
        public ResultCode OpenAudioRenderer(ServiceCtx context)
        {
            AudioRendererConfiguration parameter = context.RequestData.ReadStruct<AudioRendererConfiguration>();
            ulong workBufferSize = context.RequestData.ReadUInt64();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            int transferMemoryHandle = context.Request.HandleDesc.ToCopy[0];
            KTransferMemory workBufferTransferMemory = context.Process.HandleTable.GetObject<KTransferMemory>(transferMemoryHandle);
            uint processHandle = (uint)context.Request.HandleDesc.ToCopy[1];

            ResultCode result = _impl.OpenAudioRenderer(context, out IAudioRenderer renderer, ref parameter, workBufferSize, appletResourceUserId, workBufferTransferMemory, processHandle);

            if (result == ResultCode.Success)
            {
                MakeObject(context, new AudioRendererServer(renderer));
            }

            context.Device.System.KernelContext.Syscall.CloseHandle(transferMemoryHandle);
            context.Device.System.KernelContext.Syscall.CloseHandle((int)processHandle);

            return result;
        }

        [Command(1)]
        // GetWorkBufferSize(nn::audio::detail::AudioRendererParameterInternal parameter) -> u64 workBufferSize
        public ResultCode GetAudioRendererWorkBufferSize(ServiceCtx context)
        {
            AudioRendererConfiguration parameter = context.RequestData.ReadStruct<AudioRendererConfiguration>();

            if (BehaviourContext.CheckValidRevision(parameter.Revision))
            {
                ulong size = _impl.GetWorkBufferSize(ref parameter);

                context.ResponseData.Write(size);

                Logger.Debug?.Print(LogClass.ServiceAudio, $"WorkBufferSize is 0x{size:x16}.");

                return ResultCode.Success;
            }
            else
            {
                context.ResponseData.Write(0L);

                Logger.Warning?.Print(LogClass.ServiceAudio, $"Library Revision REV{BehaviourContext.GetRevisionNumber(parameter.Revision)} is not supported!");

                return ResultCode.UnsupportedRevision;
            }
        }

        [Command(2)]
        // GetAudioDeviceService(nn::applet::AppletResourceUserId) -> object<nn::audio::detail::IAudioDevice>
        public ResultCode GetAudioDeviceService(ServiceCtx context)
        {
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            ResultCode result = _impl.GetAudioDeviceServiceWithRevisionInfo(context, out IAudioDevice device, InitialRevision, appletResourceUserId);

            if (result == ResultCode.Success)
            {
                MakeObject(context, new AudioDeviceServer(device));
            }

            return result;
        }

        [Command(4)] // 4.0.0+
        // GetAudioDeviceServiceWithRevisionInfo(s32 revision, nn::applet::AppletResourceUserId appletResourceId) -> object<nn::audio::detail::IAudioDevice>
        public ResultCode GetAudioDeviceServiceWithRevisionInfo(ServiceCtx context)
        {
            int revision = context.RequestData.ReadInt32();
            ulong appletResourceUserId = context.RequestData.ReadUInt64();

            ResultCode result = _impl.GetAudioDeviceServiceWithRevisionInfo(context, out IAudioDevice device, revision, appletResourceUserId);

            if (result == ResultCode.Success)
            {
                MakeObject(context, new AudioDeviceServer(device));
            }

            return result;
        }
    }
}
