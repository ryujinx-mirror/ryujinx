using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.State;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        // State associated with direct uniform buffer updates.
        // This state is used to attempt to batch together consecutive updates.
        private ulong _ubBeginCpuAddress = 0;
        private ulong _ubFollowUpAddress = 0;
        private ulong _ubByteCount = 0;

        /// <summary>
        /// Flushes any queued ubo updates.
        /// </summary>
        /// <param name="memoryManager">GPU memory manager where the uniform buffer is mapped</param>
        private void FlushUboDirty(MemoryManager memoryManager)
        {
            if (_ubFollowUpAddress != 0)
            {
                memoryManager.Physical.BufferCache.ForceDirty(memoryManager, _ubFollowUpAddress - _ubByteCount, _ubByteCount);

                _ubFollowUpAddress = 0;
            }
        }

        /// <summary>
        /// Updates the uniform buffer data with inline data.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">New uniform buffer data word</param>
        private void UniformBufferUpdate(GpuState state, int argument)
        {
            var uniformBuffer = state.Get<UniformBufferState>(MethodOffset.UniformBufferState);

            ulong address = uniformBuffer.Address.Pack() + (uint)uniformBuffer.Offset;

            if (_ubFollowUpAddress != address)
            {
                FlushUboDirty(state.Channel.MemoryManager);

                _ubByteCount = 0;
                _ubBeginCpuAddress = state.Channel.MemoryManager.Translate(address);
            }

            var byteData = MemoryMarshal.Cast<int, byte>(MemoryMarshal.CreateSpan(ref argument, 1));
            state.Channel.MemoryManager.Physical.WriteUntracked(_ubBeginCpuAddress + _ubByteCount, byteData);

            _ubFollowUpAddress = address + 4;
            _ubByteCount += 4;

            state.SetUniformBufferOffset(uniformBuffer.Offset + 4);
        }

        /// <summary>
        /// Updates the uniform buffer data with inline data.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="data">Data to be written to the uniform buffer</param>
        public void UniformBufferUpdate(GpuState state, ReadOnlySpan<int> data)
        {
            var uniformBuffer = state.Get<UniformBufferState>(MethodOffset.UniformBufferState);

            ulong address = uniformBuffer.Address.Pack() + (uint)uniformBuffer.Offset;

            ulong size = (ulong)data.Length * 4;

            if (_ubFollowUpAddress != address)
            {
                FlushUboDirty(state.Channel.MemoryManager);

                _ubByteCount = 0;
                _ubBeginCpuAddress = state.Channel.MemoryManager.Translate(address);
            }

            var byteData = MemoryMarshal.Cast<int, byte>(data);
            state.Channel.MemoryManager.Physical.WriteUntracked(_ubBeginCpuAddress + _ubByteCount, byteData);

            _ubFollowUpAddress = address + size;
            _ubByteCount += size;

            state.SetUniformBufferOffset(uniformBuffer.Offset + data.Length * 4);
        }
    }
}