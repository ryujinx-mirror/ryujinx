//
// Copyright (c) 2019-2021 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class DownMixSurroundToStereoCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.DownMixSurroundToStereo;

        public ulong EstimatedProcessingTime { get; set; }

        public ushort[] InputBufferIndices { get; }
        public ushort[] OutputBufferIndices { get; }

        public float[] Coefficients { get; }

        public DownMixSurroundToStereoCommand(uint bufferOffset, Span<byte> inputBufferOffset, Span<byte> outputBufferOffset, ReadOnlySpan<float> downMixParameter, int nodeId)
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

            Coefficients = downMixParameter.ToArray();
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
            Span<float> unused2 = context.GetBuffer(OutputBufferIndices[2]);
            Span<float> unused3 = context.GetBuffer(OutputBufferIndices[3]);
            Span<float> unused4 = context.GetBuffer(OutputBufferIndices[4]);
            Span<float> unused5 = context.GetBuffer(OutputBufferIndices[5]);

            for (int i = 0; i < context.SampleCount; i++)
            {
                stereoLeft[i] = DownMixSurroundToStereo(Coefficients, backLeft[i], lowFrequency[i], frontCenter[i], frontLeft[i]);
                stereoRight[i] = DownMixSurroundToStereo(Coefficients, backRight[i], lowFrequency[i], frontCenter[i], frontRight[i]);
            }

            unused2.Fill(0);
            unused3.Fill(0);
            unused4.Fill(0);
            unused5.Fill(0);
        }
    }
}
