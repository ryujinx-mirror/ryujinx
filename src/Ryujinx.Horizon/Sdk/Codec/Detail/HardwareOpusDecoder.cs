using Concentus;
using Concentus.Enums;
using Concentus.Structs;
using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Codec.Detail
{
    partial class HardwareOpusDecoder : IHardwareOpusDecoder, IDisposable
    {
        static HardwareOpusDecoder()
        {
            OpusCodecFactory.AttemptToUseNativeLibrary = false;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OpusPacketHeader
        {
            public uint Length;
            public uint FinalRange;

            public static OpusPacketHeader FromSpan(ReadOnlySpan<byte> data)
            {
                return new()
                {
                    Length = BinaryPrimitives.ReadUInt32BigEndian(data),
                    FinalRange = BinaryPrimitives.ReadUInt32BigEndian(data[sizeof(uint)..]),
                };
            }
        }

        private interface IDecoder : IDisposable
        {
            int SampleRate { get; }
            int ChannelsCount { get; }

            int Decode(ReadOnlySpan<byte> inData, Span<short> outPcm, int frameSize);
            void ResetState();
        }

        private class Decoder : IDecoder
        {
            private readonly IOpusDecoder _decoder;

            public int SampleRate => _decoder.SampleRate;
            public int ChannelsCount => _decoder.NumChannels;

            public Decoder(int sampleRate, int channelsCount)
            {
                _decoder = OpusCodecFactory.CreateDecoder(sampleRate, channelsCount);
            }

            public int Decode(ReadOnlySpan<byte> inData, Span<short> outPcm, int frameSize)
            {
                return _decoder.Decode(inData, outPcm, frameSize);
            }

            public void ResetState()
            {
                _decoder.ResetState();
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _decoder?.Dispose();
                }
            }
        }

        private class MultiSampleDecoder : IDecoder
        {
            private readonly IOpusMultiStreamDecoder _decoder;

            public int SampleRate => _decoder.SampleRate;
            public int ChannelsCount => _decoder.NumChannels;

            public MultiSampleDecoder(int sampleRate, int channelsCount, int streams, int coupledStreams, byte[] mapping)
            {
                _decoder = OpusCodecFactory.CreateMultiStreamDecoder(sampleRate, channelsCount, streams, coupledStreams, mapping);
            }

            public int Decode(ReadOnlySpan<byte> inData, Span<short> outPcm, int frameSize)
            {
                return _decoder.DecodeMultistream(inData, outPcm, frameSize, false);
            }

            public void ResetState()
            {
                _decoder.ResetState();
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _decoder?.Dispose();
                }
            }
        }

        private readonly IDecoder _decoder;
        private int _workBufferHandle;

        private HardwareOpusDecoder(int workBufferHandle)
        {
            _workBufferHandle = workBufferHandle;
        }

        public HardwareOpusDecoder(int sampleRate, int channelsCount, int workBufferHandle) : this(workBufferHandle)
        {
            _decoder = new Decoder(sampleRate, channelsCount);
        }

        public HardwareOpusDecoder(int sampleRate, int channelsCount, int streams, int coupledStreams, byte[] mapping, int workBufferHandle) : this(workBufferHandle)
        {
            _decoder = new MultiSampleDecoder(sampleRate, channelsCount, streams, coupledStreams, mapping);
        }

        [CmifCommand(0)]
        public Result DecodeInterleavedOld(
            out int outConsumed,
            out int outSamples,
            [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias)] Span<byte> output,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<byte> input)
        {
            return DecodeInterleavedInternal(out outConsumed, out outSamples, out _, output, input, reset: false, withPerf: false);
        }

        [CmifCommand(1)]
        public Result SetContext(ReadOnlySpan<byte> context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return Result.Success;
        }

        [CmifCommand(2)] // 3.0.0+
        public Result DecodeInterleavedForMultiStreamOld(
            out int outConsumed,
            out int outSamples,
            [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias)] Span<byte> output,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<byte> input)
        {
            return DecodeInterleavedInternal(out outConsumed, out outSamples, out _, output, input, reset: false, withPerf: false);
        }

        [CmifCommand(3)] // 3.0.0+
        public Result SetContextForMultiStream(ReadOnlySpan<byte> arg0)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return Result.Success;
        }

        [CmifCommand(4)] // 4.0.0+
        public Result DecodeInterleavedWithPerfOld(
            out int outConsumed,
            out long timeTaken,
            out int outSamples,
            [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias | HipcBufferFlags.MapTransferAllowsNonSecure)] Span<byte> output,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<byte> input)
        {
            return DecodeInterleavedInternal(out outConsumed, out outSamples, out timeTaken, output, input, reset: false, withPerf: true);
        }

        [CmifCommand(5)] // 4.0.0+
        public Result DecodeInterleavedForMultiStreamWithPerfOld(
            out int outConsumed,
            out long timeTaken,
            out int outSamples,
            [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias | HipcBufferFlags.MapTransferAllowsNonSecure)] Span<byte> output,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<byte> input)
        {
            return DecodeInterleavedInternal(out outConsumed, out outSamples, out timeTaken, output, input, reset: false, withPerf: true);
        }

        [CmifCommand(6)] // 6.0.0+
        public Result DecodeInterleavedWithPerfAndResetOld(
            out int outConsumed,
            out long timeTaken,
            out int outSamples,
            [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias | HipcBufferFlags.MapTransferAllowsNonSecure)] Span<byte> output,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<byte> input,
            bool reset)
        {
            return DecodeInterleavedInternal(out outConsumed, out outSamples, out timeTaken, output, input, reset, withPerf: true);
        }

        [CmifCommand(7)] // 6.0.0+
        public Result DecodeInterleavedForMultiStreamWithPerfAndResetOld(
            out int outConsumed,
            out long timeTaken,
            out int outSamples,
            [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias | HipcBufferFlags.MapTransferAllowsNonSecure)] Span<byte> output,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<byte> input,
            bool reset)
        {
            return DecodeInterleavedInternal(out outConsumed, out outSamples, out timeTaken, output, input, reset, withPerf: true);
        }

        [CmifCommand(8)] // 7.0.0+
        public Result DecodeInterleaved(
            out int outConsumed,
            out long timeTaken,
            out int outSamples,
            [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias | HipcBufferFlags.MapTransferAllowsNonSecure)] Span<byte> output,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias | HipcBufferFlags.MapTransferAllowsNonSecure)] ReadOnlySpan<byte> input,
            bool reset)
        {
            return DecodeInterleavedInternal(out outConsumed, out outSamples, out timeTaken, output, input, reset, withPerf: true);
        }

        [CmifCommand(9)] // 7.0.0+
        public Result DecodeInterleavedForMultiStream(
            out int outConsumed,
            out long timeTaken,
            out int outSamples,
            [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias | HipcBufferFlags.MapTransferAllowsNonSecure)] Span<byte> output,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias | HipcBufferFlags.MapTransferAllowsNonSecure)] ReadOnlySpan<byte> input,
            bool reset)
        {
            return DecodeInterleavedInternal(out outConsumed, out outSamples, out timeTaken, output, input, reset, withPerf: true);
        }

        private Result DecodeInterleavedInternal(
            out int outConsumed,
            out int outSamples,
            out long timeTaken,
            Span<byte> output,
            ReadOnlySpan<byte> input,
            bool reset,
            bool withPerf)
        {
            timeTaken = 0;

            Span<short> outPcmSpace = MemoryMarshal.Cast<byte, short>(output);
            Result result = DecodeInterleaved(_decoder, reset, input, outPcmSpace, output.Length, out outConsumed, out outSamples);

            if (withPerf)
            {
                // This is the time the DSP took to process the request, TODO: fill this.
                timeTaken = 0;
            }

            return result;
        }

        private static Result GetPacketNumSamples(IDecoder decoder, out int numSamples, ReadOnlySpan<byte> packet)
        {
            int result = OpusPacketInfo.GetNumSamples(packet, decoder.SampleRate);

            numSamples = result;

            if (result == OpusError.OPUS_INVALID_PACKET)
            {
                return CodecResult.OpusInvalidPacket;
            }
            else if (result == OpusError.OPUS_BAD_ARG)
            {
                return CodecResult.OpusBadArg;
            }

            return Result.Success;
        }

        private static Result DecodeInterleaved(
            IDecoder decoder,
            bool reset,
            ReadOnlySpan<byte> input,
            Span<short> outPcmData,
            int outputSize,
            out int outConsumed,
            out int outSamples)
        {
            outConsumed = 0;
            outSamples = 0;

            int streamSize = input.Length;

            if (streamSize < Unsafe.SizeOf<OpusPacketHeader>())
            {
                return CodecResult.InvalidLength;
            }

            OpusPacketHeader header = OpusPacketHeader.FromSpan(input);
            int headerSize = Unsafe.SizeOf<OpusPacketHeader>();
            uint totalSize = header.Length + (uint)headerSize;

            if (totalSize > streamSize)
            {
                return CodecResult.InvalidLength;
            }

            ReadOnlySpan<byte> opusData = input.Slice(headerSize, (int)header.Length);

            Result result = GetPacketNumSamples(decoder, out int numSamples, opusData);

            if (result.IsSuccess)
            {
                if ((uint)numSamples * (uint)decoder.ChannelsCount * sizeof(short) > outputSize)
                {
                    return CodecResult.InvalidLength;
                }

                if (reset)
                {
                    decoder.ResetState();
                }

                try
                {
                    outSamples = decoder.Decode(opusData, outPcmData, numSamples);
                    outConsumed = (int)totalSize;
                }
                catch (OpusException e)
                {
                    switch (e.OpusErrorCode)
                    {
                        case OpusError.OPUS_BUFFER_TOO_SMALL:
                            return CodecResult.InvalidLength;
                        case OpusError.OPUS_BAD_ARG:
                            return CodecResult.OpusBadArg;
                        case OpusError.OPUS_INVALID_PACKET:
                            return CodecResult.OpusInvalidPacket;
                        default:
                            return CodecResult.InvalidLength;
                    }
                }
            }

            return Result.Success;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_workBufferHandle != 0)
                {
                    HorizonStatic.Syscall.CloseHandle(_workBufferHandle);

                    _workBufferHandle = 0;
                }

                _decoder?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
