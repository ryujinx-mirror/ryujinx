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
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class VolumeCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.Volume;

        public ulong EstimatedProcessingTime { get; set; }

        public ushort InputBufferIndex { get; }
        public ushort OutputBufferIndex { get; }

        public float Volume { get; }

        public VolumeCommand(float volume, uint bufferIndex, int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            InputBufferIndex = (ushort)bufferIndex;
            OutputBufferIndex = (ushort)bufferIndex;

            Volume = volume;
        }

        private void ProcessVolumeAvx(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer)
        {
            Vector256<float> volumeVec = Vector256.Create(Volume);

            ReadOnlySpan<Vector256<float>> inputVec = MemoryMarshal.Cast<float, Vector256<float>>(inputBuffer);
            Span<Vector256<float>> outputVec = MemoryMarshal.Cast<float, Vector256<float>>(outputBuffer);

            int sisdStart = inputVec.Length * 8;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = Avx.Ceiling(Avx.Multiply(inputVec[i], volumeVec));
            }

            for (int i = sisdStart; i < inputBuffer.Length; i++)
            {
                outputBuffer[i] = FloatingPointHelper.MultiplyRoundUp(inputBuffer[i], Volume);
            }
        }

        private void ProcessVolumeSse41(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer)
        {
            Vector128<float> volumeVec = Vector128.Create(Volume);

            ReadOnlySpan<Vector128<float>> inputVec = MemoryMarshal.Cast<float, Vector128<float>>(inputBuffer);
            Span<Vector128<float>> outputVec = MemoryMarshal.Cast<float, Vector128<float>>(outputBuffer);

            int sisdStart = inputVec.Length * 4;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = Sse41.Ceiling(Sse.Multiply(inputVec[i], volumeVec));
            }

            for (int i = sisdStart; i < inputBuffer.Length; i++)
            {
                outputBuffer[i] = FloatingPointHelper.MultiplyRoundUp(inputBuffer[i], Volume);
            }
        }

        private void ProcessVolumeAdvSimd(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer)
        {
            Vector128<float> volumeVec = Vector128.Create(Volume);

            ReadOnlySpan<Vector128<float>> inputVec = MemoryMarshal.Cast<float, Vector128<float>>(inputBuffer);
            Span<Vector128<float>> outputVec = MemoryMarshal.Cast<float, Vector128<float>>(outputBuffer);

            int sisdStart = inputVec.Length * 4;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = AdvSimd.Ceiling(AdvSimd.Multiply(inputVec[i], volumeVec));
            }

            for (int i = sisdStart; i < inputBuffer.Length; i++)
            {
                outputBuffer[i] = FloatingPointHelper.MultiplyRoundUp(inputBuffer[i], Volume);
            }
        }

        private void ProcessVolume(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer)
        {
            if (Avx.IsSupported)
            {
                ProcessVolumeAvx(outputBuffer, inputBuffer);
            }
            else if (Sse41.IsSupported)
            {
                ProcessVolumeSse41(outputBuffer, inputBuffer);
            }
            else if (AdvSimd.IsSupported)
            {
                ProcessVolumeAdvSimd(outputBuffer, inputBuffer);
            }
            else
            {
                ProcessVolumeSlowPath(outputBuffer, inputBuffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessVolumeSlowPath(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer)
        {
            for (int i = 0; i < outputBuffer.Length; i++)
            {
                outputBuffer[i] = FloatingPointHelper.MultiplyRoundUp(inputBuffer[i], Volume);
            }
        }

        public void Process(CommandList context)
        {
            ReadOnlySpan<float> inputBuffer = context.GetBuffer(InputBufferIndex);
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            ProcessVolume(outputBuffer, inputBuffer);
        }
    }
}
