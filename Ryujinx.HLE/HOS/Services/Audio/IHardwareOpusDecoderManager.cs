using Ryujinx.Common;
using Ryujinx.HLE.HOS.Services.Audio.HardwareOpusDecoderManager;
using Ryujinx.HLE.HOS.Services.Audio.Types;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    [Service("hwopus")]
    class IHardwareOpusDecoderManager : IpcService
    {
        public IHardwareOpusDecoderManager(ServiceCtx context) { }

        [CommandHipc(0)]
        // Initialize(bytes<8, 4>, u32, handle<copy>) -> object<nn::codec::detail::IHardwareOpusDecoder>
        public ResultCode Initialize(ServiceCtx context)
        {
            int sampleRate    = context.RequestData.ReadInt32();
            int channelsCount = context.RequestData.ReadInt32();

            MakeObject(context, new IHardwareOpusDecoder(sampleRate, channelsCount));

            // Close transfer memory immediately as we don't use it.
            context.Device.System.KernelContext.Syscall.CloseHandle(context.Request.HandleDesc.ToCopy[0]);

            return ResultCode.Success;
        }

        [CommandHipc(1)]
        // GetWorkBufferSize(bytes<8, 4>) -> u32
        public ResultCode GetWorkBufferSize(ServiceCtx context)
        {
            // NOTE: The sample rate is ignored because it is fixed to 48KHz.
            int sampleRate    = context.RequestData.ReadInt32();
            int channelsCount = context.RequestData.ReadInt32();

            context.ResponseData.Write(GetOpusDecoderSize(channelsCount));

            return ResultCode.Success;
        }

        [CommandHipc(4)] // 12.0.0+
        // InitializeEx(OpusParametersEx, u32, handle<copy>) -> object<nn::codec::detail::IHardwareOpusDecoder>
        public ResultCode InitializeEx(ServiceCtx context)
        {
            OpusParametersEx parameters = context.RequestData.ReadStruct<OpusParametersEx>();

            // UseLargeFrameSize can be ignored due to not relying on fixed size buffers for storing the decoded result.
            MakeObject(context, new IHardwareOpusDecoder(parameters.SampleRate, parameters.ChannelCount));

            // Close transfer memory immediately as we don't use it.
            context.Device.System.KernelContext.Syscall.CloseHandle(context.Request.HandleDesc.ToCopy[0]);

            return ResultCode.Success;
        }

        [CommandHipc(5)] // 12.0.0+
        // GetWorkBufferSizeEx(OpusParametersEx) -> u32
        public ResultCode GetWorkBufferSizeEx(ServiceCtx context)
        {
            OpusParametersEx parameters = context.RequestData.ReadStruct<OpusParametersEx>();

            // NOTE: The sample rate is ignored because it is fixed to 48KHz.
            context.ResponseData.Write(GetOpusDecoderSize(parameters.ChannelCount));

            return ResultCode.Success;
        }

        private static int GetOpusDecoderSize(int channelsCount)
        {
            const int silkDecoderSize = 0x2198;

            if (channelsCount < 1 || channelsCount > 2)
            {
                return 0;
            }

            int celtDecoderSize = GetCeltDecoderSize(channelsCount);

            int opusDecoderSize = (channelsCount * 0x800 + 0x4807) & -0x800 | 0x50;

            return opusDecoderSize + silkDecoderSize + celtDecoderSize;
        }

        private static int GetCeltDecoderSize(int channelsCount)
        {
            const int decodeBufferSize = 0x2030;
            const int celtDecoderSize  = 0x58;
            const int celtSigSize      = 0x4;
            const int overlap          = 120;
            const int eBandsCount      = 21;

            return (decodeBufferSize + overlap * 4) * channelsCount +
                    eBandsCount * 16 +
                    celtDecoderSize +
                    celtSigSize;
        }
    }
}