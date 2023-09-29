using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Memory;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Ryujinx.Audio.Renderer.Dsp.State.AuxiliaryBufferHeader;
using CpuAddress = System.UInt64;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class AuxiliaryBufferCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.AuxiliaryBuffer;

        public uint EstimatedProcessingTime { get; set; }

        public uint InputBufferIndex { get; }
        public uint OutputBufferIndex { get; }

        public AuxiliaryBufferAddresses BufferInfo { get; }

        public CpuAddress InputBuffer { get; }
        public CpuAddress OutputBuffer { get; }
        public uint CountMax { get; }
        public uint UpdateCount { get; }
        public uint WriteOffset { get; }

        public bool IsEffectEnabled { get; }

        public AuxiliaryBufferCommand(
            uint bufferOffset,
            byte inputBufferOffset,
            byte outputBufferOffset,
            ref AuxiliaryBufferAddresses sendBufferInfo,
            bool isEnabled,
            uint countMax,
            CpuAddress outputBuffer,
            CpuAddress inputBuffer,
            uint updateCount,
            uint writeOffset,
            int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;
            InputBufferIndex = bufferOffset + inputBufferOffset;
            OutputBufferIndex = bufferOffset + outputBufferOffset;
            BufferInfo = sendBufferInfo;
            InputBuffer = inputBuffer;
            OutputBuffer = outputBuffer;
            CountMax = countMax;
            UpdateCount = updateCount;
            WriteOffset = writeOffset;
            IsEffectEnabled = isEnabled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint Read(IVirtualMemoryManager memoryManager, ulong bufferAddress, uint countMax, Span<int> outBuffer, uint count, uint readOffset, uint updateCount)
        {
            if (countMax == 0 || bufferAddress == 0)
            {
                return 0;
            }

            uint targetReadOffset = readOffset + AuxiliaryBufferInfo.GetReadOffset(memoryManager, BufferInfo.ReturnBufferInfo);

            if (targetReadOffset > countMax)
            {
                return 0;
            }

            uint remaining = count;

            uint outBufferOffset = 0;

            while (remaining != 0)
            {
                uint countToWrite = Math.Min(countMax - targetReadOffset, remaining);

                memoryManager.Read(bufferAddress + targetReadOffset * sizeof(int), MemoryMarshal.Cast<int, byte>(outBuffer.Slice((int)outBufferOffset, (int)countToWrite)));

                targetReadOffset = (targetReadOffset + countToWrite) % countMax;
                remaining -= countToWrite;
                outBufferOffset += countToWrite;
            }

            if (updateCount != 0)
            {
                uint newReadOffset = (AuxiliaryBufferInfo.GetReadOffset(memoryManager, BufferInfo.ReturnBufferInfo) + updateCount) % countMax;

                AuxiliaryBufferInfo.SetReadOffset(memoryManager, BufferInfo.ReturnBufferInfo, newReadOffset);
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint Write(IVirtualMemoryManager memoryManager, ulong outBufferAddress, uint countMax, ReadOnlySpan<int> buffer, uint count, uint writeOffset, uint updateCount)
        {
            if (countMax == 0 || outBufferAddress == 0)
            {
                return 0;
            }

            uint targetWriteOffset = writeOffset + AuxiliaryBufferInfo.GetWriteOffset(memoryManager, BufferInfo.SendBufferInfo);

            if (targetWriteOffset > countMax)
            {
                return 0;
            }

            uint remaining = count;

            uint inBufferOffset = 0;

            while (remaining != 0)
            {
                uint countToWrite = Math.Min(countMax - targetWriteOffset, remaining);

                memoryManager.Write(outBufferAddress + targetWriteOffset * sizeof(int), MemoryMarshal.Cast<int, byte>(buffer.Slice((int)inBufferOffset, (int)countToWrite)));

                targetWriteOffset = (targetWriteOffset + countToWrite) % countMax;
                remaining -= countToWrite;
                inBufferOffset += countToWrite;
            }

            if (updateCount != 0)
            {
                uint newWriteOffset = (AuxiliaryBufferInfo.GetWriteOffset(memoryManager, BufferInfo.SendBufferInfo) + updateCount) % countMax;

                AuxiliaryBufferInfo.SetWriteOffset(memoryManager, BufferInfo.SendBufferInfo, newWriteOffset);
            }

            return count;
        }

        public void Process(CommandList context)
        {
            Span<float> inputBuffer = context.GetBuffer((int)InputBufferIndex);
            Span<float> outputBuffer = context.GetBuffer((int)OutputBufferIndex);

            if (IsEffectEnabled)
            {
                Span<int> inputBufferInt = MemoryMarshal.Cast<float, int>(inputBuffer);
                Span<int> outputBufferInt = MemoryMarshal.Cast<float, int>(outputBuffer);

                // Convert input data to the target format for user (int)
                DataSourceHelper.ToInt(inputBufferInt, inputBuffer, inputBuffer.Length);

                // Send the input to the user
                Write(context.MemoryManager, OutputBuffer, CountMax, inputBufferInt, context.SampleCount, WriteOffset, UpdateCount);

                // Convert back to float just in case it's reused
                DataSourceHelper.ToFloat(inputBuffer, inputBufferInt, inputBuffer.Length);

                // Retrieve the input from user
                uint readResult = Read(context.MemoryManager, InputBuffer, CountMax, outputBufferInt, context.SampleCount, WriteOffset, UpdateCount);

                // Convert the outputBuffer back to the target format of the renderer (float)
                DataSourceHelper.ToFloat(outputBuffer, outputBufferInt, outputBuffer.Length);

                if (readResult != context.SampleCount)
                {
                    outputBuffer[(int)readResult..(int)context.SampleCount].Clear();
                }
            }
            else
            {
                AuxiliaryBufferInfo.Reset(context.MemoryManager, BufferInfo.SendBufferInfo);
                AuxiliaryBufferInfo.Reset(context.MemoryManager, BufferInfo.ReturnBufferInfo);

                if (InputBufferIndex != OutputBufferIndex)
                {
                    inputBuffer.CopyTo(outputBuffer);
                }
            }
        }
    }
}
