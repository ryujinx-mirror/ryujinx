using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class DownMixSurroundToStereoCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.DownMixSurroundToStereo;

        public uint EstimatedProcessingTime { get; set; }

        public ushort[] InputBufferIndices { get; }
        public ushort[] OutputBufferIndices { get; }

        public float[] Coefficients { get; }

        public DownMixSurroundToStereoCommand(uint bufferOffset, Span<byte> inputBufferOffset, Span<byte> outputBufferOffset, float[] downMixParameter, int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            InputBufferIndices = new ushort[Constants.VoiceChannelCountMax];
            OutputBufferIndices = new ushort[Constants.VoiceChannelCountMax];

            for (int i = 0; i < Constants.VoiceChannelCountMax; i++)
            {
                InputBufferIndices[i] = (ushort)(bufferOffset + inputBufferOffset[i]);
                OutputBufferIndices[i] = (ushort)(bufferOffset + outputBufferOffset[i]);
            }

            Coefficients = downMixParameter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float DownMixSurroundToStereo(ReadOnlySpan<float> coefficients, float back, float lfe, float center, float front)
        {
            return FloatingPointHelper.RoundUp(coefficients[3] * back + coefficients[2] * lfe + coefficients[1] * center + coefficients[0] * front);
        }

        public void Process(CommandList context)
        {
            ReadOnlySpan<float> frontLeft = context.GetBuffer(InputBufferIndices[0]);
            ReadOnlySpan<float> frontRight = context.GetBuffer(InputBufferIndices[1]);
            ReadOnlySpan<float> frontCenter = context.GetBuffer(InputBufferIndices[2]);
            ReadOnlySpan<float> lowFrequency = context.GetBuffer(InputBufferIndices[3]);
            ReadOnlySpan<float> backLeft = context.GetBuffer(InputBufferIndices[4]);
            ReadOnlySpan<float> backRight = context.GetBuffer(InputBufferIndices[5]);

            Span<float> stereoLeft = context.GetBuffer(OutputBufferIndices[0]);
            Span<float> stereoRight = context.GetBuffer(OutputBufferIndices[1]);

            for (int i = 0; i < context.SampleCount; i++)
            {
                stereoLeft[i] = DownMixSurroundToStereo(Coefficients, backLeft[i], lowFrequency[i], frontCenter[i], frontLeft[i]);
                stereoRight[i] = DownMixSurroundToStereo(Coefficients, backRight[i], lowFrequency[i], frontCenter[i], frontRight[i]);
            }

            context.ClearBuffer(OutputBufferIndices[2]);
            context.ClearBuffer(OutputBufferIndices[3]);
            context.ClearBuffer(OutputBufferIndices[4]);
            context.ClearBuffer(OutputBufferIndices[5]);
        }
    }
}
