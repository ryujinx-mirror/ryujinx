using Ryujinx.Graphics.Gpu.State;

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

            _context.MemoryAccessor.Write(uniformBuffer.Address.Pack() + (uint)uniformBuffer.Offset, argument);

            state.SetUniformBufferOffset(uniformBuffer.Offset + 4);

            _context.AdvanceSequence();
        }
    }
}