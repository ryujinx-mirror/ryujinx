using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private void UniformBufferUpdate(GpuState state, int argument)
        {
            var uniformBuffer = state.Get<UniformBufferState>(MethodOffset.UniformBufferState);

            _context.MemoryAccessor.Write(uniformBuffer.Address.Pack() + (uint)uniformBuffer.Offset, argument);

            state.SetUniformBufferOffset(uniformBuffer.Offset + 4);

            _context.AdvanceSequence();
        }
    }
}