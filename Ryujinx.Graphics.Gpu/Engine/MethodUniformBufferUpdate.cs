using Ryujinx.Graphics.Gpu.State;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        /// <summary>
        /// Updates the uniform buffer data with inline data.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">New uniform buffer data word</param>
        private void UniformBufferUpdate(GpuState state, int argument)
        {
            var uniformBuffer = state.Get<UniformBufferState>(MethodOffset.UniformBufferState);

            _context.MemoryManager.Write(uniformBuffer.Address.Pack() + (uint)uniformBuffer.Offset, argument);

            state.SetUniformBufferOffset(uniformBuffer.Offset + 4);

            _context.AdvanceSequence();
        }

        /// <summary>
        /// Updates the uniform buffer data with inline data.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="data">Data to be written to the uniform buffer</param>
        public void UniformBufferUpdate(GpuState state, ReadOnlySpan<int> data)
        {
            var uniformBuffer = state.Get<UniformBufferState>(MethodOffset.UniformBufferState);

            _context.MemoryManager.Write(uniformBuffer.Address.Pack() + (uint)uniformBuffer.Offset, MemoryMarshal.Cast<int, byte>(data));

            state.SetUniformBufferOffset(uniformBuffer.Offset + data.Length * 4);

            _context.AdvanceSequence();
        }
    }
}