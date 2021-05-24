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
        private void FlushUboDirty()
        {
            if (_ubFollowUpAddress != 0)
            {
                BufferManager.ForceDirty(_ubFollowUpAddress - _ubByteCount, _ubByteCount);

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
                FlushUboDirty();

                _ubByteCount = 0;
                _ubBeginCpuAddress = _context.MemoryManager.Translate(address);
            }

            _context.PhysicalMemory.WriteUntracked(_ubBeginCpuAddress + _ubByteCount, MemoryMarshal.Cast<int, byte>(MemoryMarshal.CreateSpan(ref argument, 1)));

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
                FlushUboDirty();

                _ubByteCount = 0;
                _ubBeginCpuAddress = _context.MemoryManager.Translate(address);
            }

            _context.PhysicalMemory.WriteUntracked(_ubBeginCpuAddress + _ubByteCount, MemoryMarshal.Cast<int, byte>(data));

            _ubFollowUpAddress = address + size;
            _ubByteCount += size;

            state.SetUniformBufferOffset(uniformBuffer.Offset + data.Length * 4);
        }
    }
}