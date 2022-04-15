using Ryujinx.HLE.HOS.Services.Audio.Types;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Audio.HardwareOpusDecoderManager
{
    class IHardwareOpusDecoder : IpcService
    {
        private readonly IDecoder _decoder;
        private readonly OpusDecoderFlags _flags;

        public IHardwareOpusDecoder(int sampleRate, int channelsCount, OpusDecoderFlags flags)
        {
            _decoder = new Decoder(sampleRate, channelsCount);
            _flags = flags;
        }

        public IHardwareOpusDecoder(int sampleRate, int channelsCount, int streams, int coupledStreams, OpusDecoderFlags flags, byte[] mapping)
        {
            _decoder = new MultiSampleDecoder(sampleRate, channelsCount, streams, coupledStreams, mapping);
            _flags = flags;
        }

        [CommandHipc(0)]
        // DecodeInterleavedOld(buffer<unknown, 5>) -> (u32, u32, buffer<unknown, 6>)
        public ResultCode DecodeInterleavedOld(ServiceCtx context)
        {
            return DecodeInterleavedInternal(context, OpusDecoderFlags.None, reset: false, withPerf: false);
        }

        [CommandHipc(2)]
        // DecodeInterleavedForMultiStreamOld(buffer<unknown, 5>) -> (u32, u32, buffer<unknown, 6>)
        public ResultCode DecodeInterleavedForMultiStreamOld(ServiceCtx context)
        {
            return DecodeInterleavedInternal(context, OpusDecoderFlags.None, reset: false, withPerf: false);
        }

        [CommandHipc(4)] // 6.0.0+
        // DecodeInterleavedWithPerfOld(buffer<unknown, 5>) -> (u32, u32, u64, buffer<unknown, 0x46>)
        public ResultCode DecodeInterleavedWithPerfOld(ServiceCtx context)
        {
            return DecodeInterleavedInternal(context, OpusDecoderFlags.None, reset: false, withPerf: true);
        }

        [CommandHipc(5)] // 6.0.0+
        // DecodeInterleavedForMultiStreamWithPerfOld(buffer<unknown, 5>) -> (u32, u32, u64, buffer<unknown, 0x46>)
        public ResultCode DecodeInterleavedForMultiStreamWithPerfOld(ServiceCtx context)
        {
            return DecodeInterleavedInternal(context, OpusDecoderFlags.None, reset: false, withPerf: true);
        }

        [CommandHipc(6)] // 6.0.0+
        // DecodeInterleavedWithPerfAndResetOld(bool reset, buffer<unknown, 5>) -> (u32, u32, u64, buffer<unknown, 0x46>)
        public ResultCode DecodeInterleavedWithPerfAndResetOld(ServiceCtx context)
        {
            bool reset = context.RequestData.ReadBoolean();

            return DecodeInterleavedInternal(context, OpusDecoderFlags.None, reset, withPerf: true);
        }

        [CommandHipc(7)] // 6.0.0+
        // DecodeInterleavedForMultiStreamWithPerfAndResetOld(bool reset, buffer<unknown, 5>) -> (u32, u32, u64, buffer<unknown, 0x46>)
        public ResultCode DecodeInterleavedForMultiStreamWithPerfAndResetOld(ServiceCtx context)
        {
            bool reset = context.RequestData.ReadBoolean();

            return DecodeInterleavedInternal(context, OpusDecoderFlags.None, reset, withPerf: true);
        }

        [CommandHipc(8)] // 7.0.0+
        // DecodeInterleaved(bool reset, buffer<unknown, 0x45>) -> (u32, u32, u64, buffer<unknown, 0x46>)
        public ResultCode DecodeInterleaved(ServiceCtx context)
        {
            bool reset = context.RequestData.ReadBoolean();

            return DecodeInterleavedInternal(context, _flags, reset, withPerf: true);
        }

        [CommandHipc(9)] // 7.0.0+
        // DecodeInterleavedForMultiStream(bool reset, buffer<unknown, 0x45>) -> (u32, u32, u64, buffer<unknown, 0x46>)
        public ResultCode DecodeInterleavedForMultiStream(ServiceCtx context)
        {
            bool reset = context.RequestData.ReadBoolean();

            return DecodeInterleavedInternal(context, _flags, reset, withPerf: true);
        }

        private ResultCode DecodeInterleavedInternal(ServiceCtx context, OpusDecoderFlags flags, bool reset, bool withPerf)
        {
            ulong inPosition     = context.Request.SendBuff[0].Position;
            ulong inSize         = context.Request.SendBuff[0].Size;
            ulong outputPosition = context.Request.ReceiveBuff[0].Position;
            ulong outputSize     = context.Request.ReceiveBuff[0].Size;

            ReadOnlySpan<byte> input = context.Memory.GetSpan(inPosition, (int)inSize);

            ResultCode result = _decoder.DecodeInterleaved(reset, input, out short[] outPcmData, outputSize, out uint outConsumed, out int outSamples);

            if (result == ResultCode.Success)
            {
                context.Memory.Write(outputPosition, MemoryMarshal.Cast<short, byte>(outPcmData.AsSpan()));

                context.ResponseData.Write(outConsumed);
                context.ResponseData.Write(outSamples);

                if (withPerf)
                {
                    // This is the time the DSP took to process the request, TODO: fill this.
                    context.ResponseData.Write(0UL);
                }
            }

            return result;
        }
    }
}