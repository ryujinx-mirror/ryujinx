using Concentus;
using Concentus.Enums;
using Concentus.Structs;
using Ryujinx.HLE.HOS.Services.Audio.Types;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.HLE.HOS.Services.Audio.HardwareOpusDecoderManager
{
    static class DecoderCommon
    {
        private static ResultCode GetPacketNumSamples(this IDecoder decoder, out int numSamples, byte[] packet)
        {
            int result = OpusPacketInfo.GetNumSamples(packet, 0, packet.Length, decoder.SampleRate);

            numSamples = result;

            if (result == OpusError.OPUS_INVALID_PACKET)
            {
                return ResultCode.OpusInvalidInput;
            }
            else if (result == OpusError.OPUS_BAD_ARG)
            {
                return ResultCode.OpusInvalidInput;
            }

            return ResultCode.Success;
        }

        public static ResultCode DecodeInterleaved(
            this IDecoder decoder,
            bool reset,
            ReadOnlySpan<byte> input,
            out short[] outPcmData,
            ulong outputSize,
            out uint outConsumed,
            out int outSamples)
        {
            outPcmData = null;
            outConsumed = 0;
            outSamples = 0;

            int streamSize = input.Length;

            if (streamSize < Unsafe.SizeOf<OpusPacketHeader>())
            {
                return ResultCode.OpusInvalidInput;
            }

            OpusPacketHeader header = OpusPacketHeader.FromSpan(input);
            int headerSize = Unsafe.SizeOf<OpusPacketHeader>();
            uint totalSize = header.length + (uint)headerSize;

            if (totalSize > streamSize)
            {
                return ResultCode.OpusInvalidInput;
            }

            byte[] opusData = input.Slice(headerSize, (int)header.length).ToArray();

            ResultCode result = decoder.GetPacketNumSamples(out int numSamples, opusData);

            if (result == ResultCode.Success)
            {
                if ((uint)numSamples * (uint)decoder.ChannelsCount * sizeof(short) > outputSize)
                {
                    return ResultCode.OpusInvalidInput;
                }

                outPcmData = new short[numSamples * decoder.ChannelsCount];

                if (reset)
                {
                    decoder.ResetState();
                }

                try
                {
                    outSamples = decoder.Decode(opusData, 0, opusData.Length, outPcmData, 0, outPcmData.Length / decoder.ChannelsCount);
                    outConsumed = totalSize;
                }
                catch (OpusException)
                {
                    // TODO: as OpusException doesn't provide us the exact error code, this is kind of inaccurate in some cases...
                    return ResultCode.OpusInvalidInput;
                }
            }

            return ResultCode.Success;
        }
    }
}