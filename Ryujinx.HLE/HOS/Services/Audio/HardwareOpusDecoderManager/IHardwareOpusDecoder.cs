using Concentus;
using Concentus.Enums;
using Concentus.Structs;
using Ryujinx.HLE.HOS.Services.Audio.Types;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Audio.HardwareOpusDecoderManager
{
    class IHardwareOpusDecoder : IpcService
    {
        private int  _sampleRate;
        private int  _channelsCount;
        private bool _reset;

        private OpusDecoder _decoder;

        public IHardwareOpusDecoder(int sampleRate, int channelsCount)
        {
            _sampleRate    = sampleRate;
            _channelsCount = channelsCount;
            _reset         = false;

            _decoder = new OpusDecoder(sampleRate, channelsCount);
        }

        private ResultCode GetPacketNumSamples(out int numSamples, byte[] packet)
        {
            int result = OpusPacketInfo.GetNumSamples(_decoder, packet, 0, packet.Length);

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

        private ResultCode DecodeInterleavedInternal(BinaryReader input, out short[] outPcmData, long outputSize, out uint outConsumed, out int outSamples)
        {
            outPcmData  = null;
            outConsumed = 0;
            outSamples  = 0;

            long streamSize = input.BaseStream.Length;

            if (streamSize < Marshal.SizeOf<OpusPacketHeader>())
            {
                return ResultCode.OpusInvalidInput;
            }

            OpusPacketHeader header = OpusPacketHeader.FromStream(input);

            uint totalSize = header.length + (uint)Marshal.SizeOf<OpusPacketHeader>();

            if (totalSize > streamSize)
            {
                return ResultCode.OpusInvalidInput;
            }

            byte[] opusData = input.ReadBytes((int)header.length);

            ResultCode result = GetPacketNumSamples(out int numSamples, opusData);

            if (result == ResultCode.Success)
            {
                if ((uint)numSamples * (uint)_channelsCount * sizeof(short) > outputSize)
                {
                    return ResultCode.OpusInvalidInput;
                }

                outPcmData = new short[numSamples * _channelsCount];

                if (_reset)
                {
                    _reset = false;

                    _decoder.ResetState();
                }

                try
                {
                    outSamples  = _decoder.Decode(opusData, 0, opusData.Length, outPcmData, 0, outPcmData.Length / _channelsCount);
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

        [Command(0)]
        // DecodeInterleaved(buffer<unknown, 5>) -> (u32, u32, buffer<unknown, 6>)
        public ResultCode DecodeInterleavedOriginal(ServiceCtx context)
        {
            ResultCode result;

            long inPosition     = context.Request.SendBuff[0].Position;
            long inSize         = context.Request.SendBuff[0].Size;
            long outputPosition = context.Request.ReceiveBuff[0].Position;
            long outputSize     = context.Request.ReceiveBuff[0].Size;

            byte[] buffer = new byte[inSize];

            context.Memory.Read((ulong)inPosition, buffer);

            using (BinaryReader inputStream = new BinaryReader(new MemoryStream(buffer)))
            {
                result = DecodeInterleavedInternal(inputStream, out short[] outPcmData, outputSize, out uint outConsumed, out int outSamples);

                if (result == ResultCode.Success)
                {
                    byte[] pcmDataBytes = new byte[outPcmData.Length * sizeof(short)];
                    Buffer.BlockCopy(outPcmData, 0, pcmDataBytes, 0, pcmDataBytes.Length);
                    context.Memory.Write((ulong)outputPosition, pcmDataBytes);

                    context.ResponseData.Write(outConsumed);
                    context.ResponseData.Write(outSamples);
                }
            }

            return result;
        }

        [Command(4)] // 6.0.0+
        // DecodeInterleavedWithPerfOld(buffer<unknown, 5>) -> (u32, u32, u64, buffer<unknown, 0x46>)
        public ResultCode DecodeInterleavedWithPerfOld(ServiceCtx context)
        {
            ResultCode result;

            long inPosition     = context.Request.SendBuff[0].Position;
            long inSize         = context.Request.SendBuff[0].Size;
            long outputPosition = context.Request.ReceiveBuff[0].Position;
            long outputSize     = context.Request.ReceiveBuff[0].Size;

            byte[] buffer = new byte[inSize];

            context.Memory.Read((ulong)inPosition, buffer);

            using (BinaryReader inputStream = new BinaryReader(new MemoryStream(buffer)))
            {
                result = DecodeInterleavedInternal(inputStream, out short[] outPcmData, outputSize, out uint outConsumed, out int outSamples);

                if (result == ResultCode.Success)
                {
                    byte[] pcmDataBytes = new byte[outPcmData.Length * sizeof(short)];
                    Buffer.BlockCopy(outPcmData, 0, pcmDataBytes, 0, pcmDataBytes.Length);
                    context.Memory.Write((ulong)outputPosition, pcmDataBytes);

                    context.ResponseData.Write(outConsumed);
                    context.ResponseData.Write(outSamples);

                    // This is the time the DSP took to process the request, TODO: fill this.
                    context.ResponseData.Write(0);
                }
            }

            return result;
        }

        [Command(6)] // 6.0.0+
        // DecodeInterleavedOld(bool reset, buffer<unknown, 5>) -> (u32, u32, u64, buffer<unknown, 0x46>)
        public ResultCode DecodeInterleavedOld(ServiceCtx context)
        {
            ResultCode result;

            _reset = context.RequestData.ReadBoolean();

            long inPosition     = context.Request.SendBuff[0].Position;
            long inSize         = context.Request.SendBuff[0].Size;
            long outputPosition = context.Request.ReceiveBuff[0].Position;
            long outputSize     = context.Request.ReceiveBuff[0].Size;

            byte[] buffer = new byte[inSize];

            context.Memory.Read((ulong)inPosition, buffer);

            using (BinaryReader inputStream = new BinaryReader(new MemoryStream(buffer)))
            {
                result = DecodeInterleavedInternal(inputStream, out short[] outPcmData, outputSize, out uint outConsumed, out int outSamples);

                if (result == ResultCode.Success)
                {
                    byte[] pcmDataBytes = new byte[outPcmData.Length * sizeof(short)];
                    Buffer.BlockCopy(outPcmData, 0, pcmDataBytes, 0, pcmDataBytes.Length);
                    context.Memory.Write((ulong)outputPosition, pcmDataBytes);

                    context.ResponseData.Write(outConsumed);
                    context.ResponseData.Write(outSamples);

                    // This is the time the DSP took to process the request, TODO: fill this.
                    context.ResponseData.Write(0);
                }
            }

            return result;
        }

        [Command(8)] // 7.0.0+
        // DecodeInterleaved(bool reset, buffer<unknown, 0x45>) -> (u32, u32, u64, buffer<unknown, 0x46>)
        public ResultCode DecodeInterleaved(ServiceCtx context)
        {
            ResultCode result;

            _reset = context.RequestData.ReadBoolean();

            long inPosition     = context.Request.SendBuff[0].Position;
            long inSize         = context.Request.SendBuff[0].Size;
            long outputPosition = context.Request.ReceiveBuff[0].Position;
            long outputSize     = context.Request.ReceiveBuff[0].Size;

            byte[] buffer = new byte[inSize];

            context.Memory.Read((ulong)inPosition, buffer);

            using (BinaryReader inputStream = new BinaryReader(new MemoryStream(buffer)))
            {
                result = DecodeInterleavedInternal(inputStream, out short[] outPcmData, outputSize, out uint outConsumed, out int outSamples);

                if (result == ResultCode.Success)
                {
                    byte[] pcmDataBytes = new byte[outPcmData.Length * sizeof(short)];
                    Buffer.BlockCopy(outPcmData, 0, pcmDataBytes, 0, pcmDataBytes.Length);
                    context.Memory.Write((ulong)outputPosition, pcmDataBytes);

                    context.ResponseData.Write(outConsumed);
                    context.ResponseData.Write(outSamples);

                    // This is the time the DSP took to process the request, TODO: fill this.
                    context.ResponseData.Write(0);
                }
            }

            return result;
        }
    }
}