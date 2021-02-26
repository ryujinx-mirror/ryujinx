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

using Ryujinx.Audio.Renderer.Parameter.Sink;
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using System;
using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class CircularBufferSinkCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.CircularBufferSink;

        public ulong EstimatedProcessingTime { get; set; }

        public ushort[] Input { get; }
        public uint InputCount { get; }

        public ulong CircularBuffer { get; }
        public ulong CircularBufferSize { get; }
        public ulong CurrentOffset { get; }

        public CircularBufferSinkCommand(uint bufferOffset, ref CircularBufferParameter parameter, ref AddressInfo circularBufferAddressInfo, uint currentOffset, int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            Input = new ushort[Constants.ChannelCountMax];
            InputCount = parameter.InputCount;

            for (int i = 0; i < InputCount; i++)
            {
                Input[i] = (ushort)(bufferOffset + parameter.Input[i]);
            }

            CircularBuffer = circularBufferAddressInfo.GetReference(true);
            CircularBufferSize = parameter.BufferSize;
            CurrentOffset = currentOffset;

            Debug.Assert(CircularBuffer != 0);
        }

        public void Process(CommandList context)
        {
            const int targetChannelCount = 2;

            ulong currentOffset = CurrentOffset;

            if (CircularBufferSize > 0)
            {
                for (int i = 0; i < InputCount; i++)
                {
                    ReadOnlySpan<float> inputBuffer = context.GetBuffer(Input[i]);

                    ulong targetOffset = CircularBuffer + currentOffset;

                    for (int y = 0; y < context.SampleCount; y++)
                    {
                        context.MemoryManager.Write(targetOffset + (ulong)y * targetChannelCount, PcmHelper.Saturate(inputBuffer[y]));
                    }

                    currentOffset += context.SampleCount * targetChannelCount;

                    if (currentOffset >= CircularBufferSize)
                    {
                        currentOffset = 0;
                    }
                }
            }
        }
    }
}
