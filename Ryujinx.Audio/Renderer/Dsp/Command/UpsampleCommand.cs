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

using Ryujinx.Audio.Renderer.Server.Upsampler;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class UpsampleCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.Upsample;

        public ulong EstimatedProcessingTime { get; set; }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
