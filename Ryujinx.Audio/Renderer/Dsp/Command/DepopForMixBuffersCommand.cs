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
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class DepopForMixBuffersCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.DepopForMixBuffers;

        public ulong EstimatedProcessingTime { get; set; }

        public uint MixBufferOffset { get; }

        public uint MixBufferCount { get; }

        public float Decay { get; }

        public Memory<float> DepopBuffer { get; }

        public DepopForMixBuffersCommand(Memory<float> depopBuffer, uint bufferOffset, uint mixBufferCount, int nodeId, uint sampleRate)
        {
            Enabled = true;
            NodeId = nodeId;
            MixBufferOffset = bufferOffset;
            MixBufferCount = mixBufferCount;
            DepopBuffer = depopBuffer;

            if (sampleRate == 48000)
            {
                Decay = 0.962189f;
            }
            else // if (sampleRate == 32000)
            {
                Decay = 0.943695f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe float ProcessDepopMix(float* buffer, float depopValue, uint sampleCount)
        {
            if (depopValue < 0)
            {
                depopValue = -depopValue;

                for (int i = 0; i < sampleCount; i++)
                {
                    depopValue = FloatingPointHelper.MultiplyRoundDown(Decay, depopValue);

                    buffer[i] -= depopValue;
                }

                return -depopValue;
            }
            else
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    depopValue = FloatingPointHelper.MultiplyRoundDown(Decay, depopValue);

                    buffer[i] += depopValue;
                }

                return depopValue;
            }
        }

        public void Process(CommandList context)
        {
            Span<float> depopBuffer = DepopBuffer.Span;

            uint bufferCount = Math.Min(MixBufferOffset + MixBufferCount, context.BufferCount);

            for (int i = (int)MixBufferOffset; i < bufferCount; i++)
            {
                float depopValue = depopBuffer[i];
                if (depopValue != 0)
                {
                    unsafe
                    {
                        float* buffer = (float*)context.GetBufferPointer(i);

                        depopBuffer[i] = ProcessDepopMix(buffer, depopValue, context.SampleCount);
                    }
                }
            }
        }
    }
}
