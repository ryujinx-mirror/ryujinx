using Ryujinx.HLE.HOS.Services.Audio.HardwareOpusDecoderManager;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    [Service("hwopus")]
    class IHardwareOpusDecoderManager : IpcService
    {
        public IHardwareOpusDecoderManager(ServiceCtx context) { }

        [Command(0)]
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

        [Command(1)]
        // GetWorkBufferSize(bytes<8, 4>) -> u32
        public ResultCode GetWorkBufferSize(ServiceCtx context)
        {
            // Note: The sample rate is ignored because it is fixed to 48KHz.
            int sampleRate    = context.RequestData.ReadInt32();
            int channelsCount = context.RequestData.ReadInt32();

            context.ResponseData.Write(GetOpusDecoderSize(channelsCount));

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