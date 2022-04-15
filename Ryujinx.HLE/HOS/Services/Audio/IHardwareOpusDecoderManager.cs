using Ryujinx.Common;
using Ryujinx.HLE.HOS.Services.Audio.HardwareOpusDecoderManager;
using Ryujinx.HLE.HOS.Services.Audio.Types;
using System.Runtime.InteropServices;

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

            MakeObject(context, new IHardwareOpusDecoder(sampleRate, channelsCount, OpusDecoderFlags.None));

            // Close transfer memory immediately as we don't use it.
            context.Device.System.KernelContext.Syscall.CloseHandle(context.Request.HandleDesc.ToCopy[0]);

            return ResultCode.Success;
        }

        [CommandHipc(1)]
        // GetWorkBufferSize(bytes<8, 4>) -> u32
        public ResultCode GetWorkBufferSize(ServiceCtx context)
        {
            int sampleRate    = context.RequestData.ReadInt32();
            int channelsCount = context.RequestData.ReadInt32();

            int opusDecoderSize = GetOpusDecoderSize(channelsCount);

            int frameSize = BitUtils.AlignUp(channelsCount * 1920 / (48000 / sampleRate), 64);
            int totalSize = opusDecoderSize + 1536 + frameSize;

            context.ResponseData.Write(totalSize);

            return ResultCode.Success;
        }

        [CommandHipc(2)] // 3.0.0+
        // InitializeForMultiStream(u32, handle<copy>, buffer<unknown<0x110>, 0x19>) -> object<nn::codec::detail::IHardwareOpusDecoder>
        public ResultCode InitializeForMultiStream(ServiceCtx context)
        {
            ulong parametersAddress = context.Request.PtrBuff[0].Position;

            OpusMultiStreamParameters parameters = context.Memory.Read<OpusMultiStreamParameters>(parametersAddress);

            MakeObject(context, new IHardwareOpusDecoder(parameters.SampleRate, parameters.ChannelsCount, OpusDecoderFlags.None));

            // Close transfer memory immediately as we don't use it.
            context.Device.System.KernelContext.Syscall.CloseHandle(context.Request.HandleDesc.ToCopy[0]);

            return ResultCode.Success;
        }

        [CommandHipc(3)] // 3.0.0+
        // GetWorkBufferSizeForMultiStream(buffer<unknown<0x110>, 0x19>) -> u32
        public ResultCode GetWorkBufferSizeForMultiStream(ServiceCtx context)
        {
            ulong parametersAddress = context.Request.PtrBuff[0].Position;

            OpusMultiStreamParameters parameters = context.Memory.Read<OpusMultiStreamParameters>(parametersAddress);

            int opusDecoderSize = GetOpusMultistreamDecoderSize(parameters.NumberOfStreams, parameters.NumberOfStereoStreams);

            int streamSize = BitUtils.AlignUp(parameters.NumberOfStreams * 1500, 64);
            int frameSize = BitUtils.AlignUp(parameters.ChannelsCount * 1920 / (48000 / parameters.SampleRate), 64);
            int totalSize = opusDecoderSize + streamSize + frameSize;

            context.ResponseData.Write(totalSize);

            return ResultCode.Success;
        }

        [CommandHipc(4)] // 12.0.0+
        // InitializeEx(OpusParametersEx, u32, handle<copy>) -> object<nn::codec::detail::IHardwareOpusDecoder>
        public ResultCode InitializeEx(ServiceCtx context)
        {
            OpusParametersEx parameters = context.RequestData.ReadStruct<OpusParametersEx>();

            // UseLargeFrameSize can be ignored due to not relying on fixed size buffers for storing the decoded result.
            MakeObject(context, new IHardwareOpusDecoder(parameters.SampleRate, parameters.ChannelsCount, parameters.Flags));

            // Close transfer memory immediately as we don't use it.
            context.Device.System.KernelContext.Syscall.CloseHandle(context.Request.HandleDesc.ToCopy[0]);

            return ResultCode.Success;
        }

        [CommandHipc(5)] // 12.0.0+
        // GetWorkBufferSizeEx(OpusParametersEx) -> u32
        public ResultCode GetWorkBufferSizeEx(ServiceCtx context)
        {
            OpusParametersEx parameters = context.RequestData.ReadStruct<OpusParametersEx>();

            int opusDecoderSize = GetOpusDecoderSize(parameters.ChannelsCount);

            int frameSizeMono48KHz = parameters.Flags.HasFlag(OpusDecoderFlags.LargeFrameSize) ? 5760 : 1920;
            int frameSize = BitUtils.AlignUp(parameters.ChannelsCount * frameSizeMono48KHz / (48000 / parameters.SampleRate), 64);
            int totalSize = opusDecoderSize + 1536 + frameSize;

            context.ResponseData.Write(totalSize);

            return ResultCode.Success;
        }

        [CommandHipc(6)] // 12.0.0+
        // InitializeForMultiStreamEx(u32, handle<copy>, buffer<unknown<0x118>, 0x19>) -> object<nn::codec::detail::IHardwareOpusDecoder>
        public ResultCode InitializeForMultiStreamEx(ServiceCtx context)
        {
            ulong parametersAddress = context.Request.PtrBuff[0].Position;

            OpusMultiStreamParametersEx parameters = context.Memory.Read<OpusMultiStreamParametersEx>(parametersAddress);

            byte[] mappings = MemoryMarshal.Cast<uint, byte>(parameters.ChannelMappings.ToSpan()).ToArray();

            // UseLargeFrameSize can be ignored due to not relying on fixed size buffers for storing the decoded result.
            MakeObject(context, new IHardwareOpusDecoder(
                parameters.SampleRate,
                parameters.ChannelsCount,
                parameters.NumberOfStreams,
                parameters.NumberOfStereoStreams,
                parameters.Flags,
                mappings));

            // Close transfer memory immediately as we don't use it.
            context.Device.System.KernelContext.Syscall.CloseHandle(context.Request.HandleDesc.ToCopy[0]);

            return ResultCode.Success;
        }

        [CommandHipc(7)] // 12.0.0+
        // GetWorkBufferSizeForMultiStreamEx(buffer<unknown<0x118>, 0x19>) -> u32
        public ResultCode GetWorkBufferSizeForMultiStreamEx(ServiceCtx context)
        {
            ulong parametersAddress = context.Request.PtrBuff[0].Position;

            OpusMultiStreamParametersEx parameters = context.Memory.Read<OpusMultiStreamParametersEx>(parametersAddress);

            int opusDecoderSize = GetOpusMultistreamDecoderSize(parameters.NumberOfStreams, parameters.NumberOfStereoStreams);

            int frameSizeMono48KHz = parameters.Flags.HasFlag(OpusDecoderFlags.LargeFrameSize) ? 5760 : 1920;
            int streamSize = BitUtils.AlignUp(parameters.NumberOfStreams * 1500, 64);
            int frameSize = BitUtils.AlignUp(parameters.ChannelsCount * frameSizeMono48KHz / (48000 / parameters.SampleRate), 64);
            int totalSize = opusDecoderSize + streamSize + frameSize;

            context.ResponseData.Write(totalSize);

            return ResultCode.Success;
        }

        private static int GetOpusMultistreamDecoderSize(int streams, int coupledStreams)
        {
            if (streams < 1 || coupledStreams > streams || coupledStreams < 0)
            {
                return 0;
            }

            int coupledSize = GetOpusDecoderSize(2);
            int monoSize = GetOpusDecoderSize(1);

            return Align4(monoSize - GetOpusDecoderAllocSize(1)) * (streams - coupledStreams) +
                Align4(coupledSize - GetOpusDecoderAllocSize(2)) * coupledStreams + 0xb90c;
        }

        private static int Align4(int value)
        {
            return BitUtils.AlignUp(value, 4);
        }

        private static int GetOpusDecoderSize(int channelsCount)
        {
            const int SilkDecoderSize = 0x2160;

            if (channelsCount < 1 || channelsCount > 2)
            {
                return 0;
            }

            int celtDecoderSize = GetCeltDecoderSize(channelsCount);
            int opusDecoderSize = GetOpusDecoderAllocSize(channelsCount) | 0x4c;

            return opusDecoderSize + SilkDecoderSize + celtDecoderSize;
        }

        private static int GetOpusDecoderAllocSize(int channelsCount)
        {
            return (channelsCount * 0x800 + 0x4803) & -0x800;
        }

        private static int GetCeltDecoderSize(int channelsCount)
        {
            const int DecodeBufferSize = 0x2030;
            const int Overlap          = 120;
            const int EBandsCount      = 21;

            return (DecodeBufferSize + Overlap * 4) * channelsCount + EBandsCount * 16 + 0x50;
        }
    }
}