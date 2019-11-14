using Ryujinx.Audio;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    [Service("audren:u")]
    class IAudioRendererManager : IpcService
    {
        public IAudioRendererManager(ServiceCtx context) { }

        [Command(0)]
        // OpenAudioRenderer(nn::audio::detail::AudioRendererParameterInternal, u64, nn::applet::AppletResourceUserId, pid, handle<copy>, handle<copy>)
        // -> object<nn::audio::detail::IAudioRenderer>
        public ResultCode OpenAudioRenderer(ServiceCtx context)
        {
            IAalOutput audioOut = context.Device.AudioOut;

            AudioRendererParameter Params = GetAudioRendererParameter(context);

            MakeObject(context, new IAudioRenderer(
                context.Device.System,
                context.Memory,
                audioOut,
                Params));

            return ResultCode.Success;
        }

        [Command(1)]
        // GetWorkBufferSize(nn::audio::detail::AudioRendererParameterInternal) -> u64
        public ResultCode GetAudioRendererWorkBufferSize(ServiceCtx context)
        {
            AudioRendererParameter parameters = GetAudioRendererParameter(context);

            if (AudioRendererCommon.CheckValidRevision(parameters))
            {
                BehaviorInfo behaviorInfo = new BehaviorInfo();

                behaviorInfo.SetUserLibRevision(parameters.Revision);

                long size;

                int totalMixCount = parameters.SubMixCount + 1;

                size = BitUtils.AlignUp(parameters.MixBufferCount * 4, AudioRendererConsts.BufferAlignment) +
                       parameters.SubMixCount * 0x400 +
                       totalMixCount          * 0x940 +
                       parameters.VoiceCount  * 0x3F0 +
                       BitUtils.AlignUp(totalMixCount * 8, 16) +
                       BitUtils.AlignUp(parameters.VoiceCount * 8, 16) +
                       BitUtils.AlignUp(((parameters.SinkCount + parameters.SubMixCount) * 0x3C0 + parameters.SampleCount * 4) *
                                         (parameters.MixBufferCount + 6), AudioRendererConsts.BufferAlignment) +
                       (parameters.SinkCount + parameters.SubMixCount) * 0x2C0 +
                       (parameters.EffectCount + parameters.VoiceCount * 4) * 0x30 + 
                       0x50;

                if (behaviorInfo.IsSplitterSupported())
                {
                    size += BitUtils.AlignUp(NodeStates.GetWorkBufferSize(totalMixCount) + EdgeMatrix.GetWorkBufferSize(totalMixCount), 16);
                }

                size = parameters.SinkCount                            * 0x170 +
                       (parameters.SinkCount + parameters.SubMixCount) * 0x280 +
                       parameters.EffectCount                          * 0x4C0 +
                       ((size + SplitterContext.CalcWorkBufferSize(behaviorInfo, parameters) + 0x30 * parameters.EffectCount + (4 * parameters.VoiceCount) + 0x8F) & ~0x3FL) +
                       ((parameters.VoiceCount << 8) | 0x40);

                if (parameters.PerformanceManagerCount >= 1)
                {
                    size += (PerformanceManager.GetRequiredBufferSizeForPerformanceMetricsPerFrame(behaviorInfo, parameters) * 
                            (parameters.PerformanceManagerCount + 1) + 0xFF) & ~0x3FL;
                }

                if (behaviorInfo.IsVariadicCommandBufferSizeSupported())
                {
                    size += CommandGenerator.CalculateCommandBufferSize(parameters) + 0x7E;
                }
                else
                {
                    size += 0x1807E;
                }

                size = BitUtils.AlignUp(size, 0x1000);

                context.ResponseData.Write(size);

                Logger.PrintDebug(LogClass.ServiceAudio, $"WorkBufferSize is 0x{size:x16}.");

                return ResultCode.Success;
            }
            else
            {
                context.ResponseData.Write(0L);

                Logger.PrintWarning(LogClass.ServiceAudio, $"Library Revision REV{AudioRendererCommon.GetRevisionVersion(parameters.Revision)} is not supported!");

                return ResultCode.UnsupportedRevision;
            }
        }

        private AudioRendererParameter GetAudioRendererParameter(ServiceCtx context)
        {
            AudioRendererParameter Params = new AudioRendererParameter
            {
                SampleRate                   = context.RequestData.ReadInt32(),
                SampleCount                  = context.RequestData.ReadInt32(),
                MixBufferCount               = context.RequestData.ReadInt32(),
                SubMixCount                  = context.RequestData.ReadInt32(),
                VoiceCount                   = context.RequestData.ReadInt32(),
                SinkCount                    = context.RequestData.ReadInt32(),
                EffectCount                  = context.RequestData.ReadInt32(),
                PerformanceManagerCount      = context.RequestData.ReadInt32(),
                VoiceDropEnable              = context.RequestData.ReadInt32(),
                SplitterCount                = context.RequestData.ReadInt32(),
                SplitterDestinationDataCount = context.RequestData.ReadInt32(),
                Unknown2C                    = context.RequestData.ReadInt32(),
                Revision                     = context.RequestData.ReadInt32()
            };

            return Params;
        }

        [Command(2)]
        // GetAudioDeviceService(nn::applet::AppletResourceUserId) -> object<nn::audio::detail::IAudioDevice>
        public ResultCode GetAudioDeviceService(ServiceCtx context)
        {
            long appletResourceUserId = context.RequestData.ReadInt64();

            MakeObject(context, new IAudioDevice(context.Device.System));

            return ResultCode.Success;
        }

        [Command(4)] // 4.0.0+
        // GetAudioDeviceServiceWithRevisionInfo(u32 revision_info, nn::applet::AppletResourceUserId) -> object<nn::audio::detail::IAudioDevice>
        public ResultCode GetAudioDeviceServiceWithRevisionInfo(ServiceCtx context)
        {
            int  revisionInfo         = context.RequestData.ReadInt32();
            long appletResourceUserId = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceAudio, new { appletResourceUserId, revisionInfo });

            return GetAudioDeviceService(context);
        }
    }
}
