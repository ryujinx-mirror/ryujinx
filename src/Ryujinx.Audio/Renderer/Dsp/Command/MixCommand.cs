using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class MixCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.Mix;

        public uint EstimatedProcessingTime { get; set; }

        public ushort InputBufferIndex { get; }
        public ushort OutputBufferIndex { get; }

        public float Volume { get; }

        public MixCommand(uint inputBufferIndex, uint outputBufferIndex, int nodeId, float volume)
        {
            Enabled = true;
            NodeId = nodeId;

            InputBufferIndex = (ushort)inputBufferIndex;
            OutputBufferIndex = (ushort)outputBufferIndex;

            Volume = volume;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMixAvx(Span<float> outputMix, ReadOnlySpan<float> inputMix)
        {
            Vector256<float> volumeVec = Vector256.Create(Volume);

            ReadOnlySpan<Vector256<float>> inputVec = MemoryMarshal.Cast<float, Vector256<float>>(inputMix);
            Span<Vector256<float>> outputVec = MemoryMarshal.Cast<float, Vector256<float>>(outputMix);

            int sisdStart = inputVec.Length * 8;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = Avx.Add(outputVec[i], Avx.Ceiling(Avx.Multiply(inputVec[i], volumeVec)));
            }

            for (int i = sisdStart; i < inputMix.Length; i++)
            {
                outputMix[i] += FloatingPointHelper.MultiplyRoundUp(inputMix[i], Volume);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMixSse41(Span<float> outputMix, ReadOnlySpan<float> inputMix)
        {
            Vector128<float> volumeVec = Vector128.Create(Volume);

            ReadOnlySpan<Vector128<float>> inputVec = MemoryMarshal.Cast<float, Vector128<float>>(inputMix);
            Span<Vector128<float>> outputVec = MemoryMarshal.Cast<float, Vector128<float>>(outputMix);

            int sisdStart = inputVec.Length * 4;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = Sse.Add(outputVec[i], Sse41.Ceiling(Sse.Multiply(inputVec[i], volumeVec)));
            }

            for (int i = sisdStart; i < inputMix.Length; i++)
            {
                outputMix[i] += FloatingPointHelper.MultiplyRoundUp(inputMix[i], Volume);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMixAdvSimd(Span<float> outputMix, ReadOnlySpan<float> inputMix)
        {
            Vector128<float> volumeVec = Vector128.Create(Volume);

            ReadOnlySpan<Vector128<float>> inputVec = MemoryMarshal.Cast<float, Vector128<float>>(inputMix);
            Span<Vector128<float>> outputVec = MemoryMarshal.Cast<float, Vector128<float>>(outputMix);

            int sisdStart = inputVec.Length * 4;

            for (int i = 0; i < inputVec.Length; i++)
            {
                outputVec[i] = AdvSimd.Add(outputVec[i], AdvSimd.Ceiling(AdvSimd.Multiply(inputVec[i], volumeVec)));
            }

            for (int i = sisdStart; i < inputMix.Length; i++)
            {
                outputMix[i] += FloatingPointHelper.MultiplyRoundUp(inputMix[i], Volume);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMixSlowPath(Span<float> outputMix, ReadOnlySpan<float> inputMix)
        {
            for (int i = 0; i < inputMix.Length; i++)
            {
                outputMix[i] += FloatingPointHelper.MultiplyRoundUp(inputMix[i], Volume);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMix(Span<float> outputMix, ReadOnlySpan<float> inputMix)
        {
            if (Avx.IsSupported)
            {
                ProcessMixAvx(outputMix, inputMix);
            }
            else if (Sse41.IsSupported)
            {
                ProcessMixSse41(outputMix, inputMix);
            }
            else if (AdvSimd.IsSupported)
            {
                ProcessMixAdvSimd(outputMix, inputMix);
            }
            else
            {
                ProcessMixSlowPath(outputMix, inputMix);
            }
        }

        public void Process(CommandList context)
        {
            ReadOnlySpan<float> inputBuffer = context.GetBuffer(InputBufferIndex);
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            ProcessMix(outputBuffer, inputBuffer);
        }
    }
}
