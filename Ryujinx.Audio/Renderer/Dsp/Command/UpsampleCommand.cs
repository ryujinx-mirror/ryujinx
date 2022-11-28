using Ryujinx.Audio.Renderer.Server.Upsampler;
using System;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class UpsampleCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.Upsample;

        public uint EstimatedProcessingTime { get; set; }

        public uint BufferCount { get; }
        public uint InputBufferIndex { get; }
        public uint InputSampleCount { get; }
        public uint InputSampleRate { get; }

        public UpsamplerState UpsamplerInfo { get; }

        public Memory<float> OutBuffer { get; }

        public UpsampleCommand(uint bufferOffset, UpsamplerState info, uint inputCount, Span<byte> inputBufferOffset, uint bufferCount, uint sampleCount, uint sampleRate, int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            InputBufferIndex = 0;
            OutBuffer = info.OutputBuffer;
            BufferCount = bufferCount;
            InputSampleCount = sampleCount;
            InputSampleRate = sampleRate;
            info.SourceSampleCount = inputCount;
            info.InputBufferIndices = new ushort[inputCount];

            for (int i = 0; i < inputCount; i++)
            {
                info.InputBufferIndices[i] = (ushort)(bufferOffset + inputBufferOffset[i]);
            }

            UpsamplerInfo = info;
        }

        private Span<float> GetBuffer(int index, int sampleCount)
        {
            return UpsamplerInfo.OutputBuffer.Span.Slice(index * sampleCount, sampleCount);
        }

        public void Process(CommandList context)
        {
            float ratio = (float)InputSampleRate / Constants.TargetSampleRate;

            uint bufferCount = Math.Min(BufferCount, UpsamplerInfo.SourceSampleCount);

            for (int i = 0; i < bufferCount; i++)
            {
                Span<float> inputBuffer = context.GetBuffer(UpsamplerInfo.InputBufferIndices[i]);
                Span<float> outputBuffer = GetBuffer(UpsamplerInfo.InputBufferIndices[i], (int)UpsamplerInfo.SampleCount);

                float fraction = 0.0f;

                ResamplerHelper.ResampleForUpsampler(outputBuffer, inputBuffer, ratio, ref fraction, (int)(InputSampleCount / ratio));
            }
        }
    }
}