using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private void UniformBufferUpdate(int argument)
        {
            var uniformBuffer = _context.State.Get<UniformBufferState>(MethodOffset.UniformBufferState);

            _context.MemoryAccessor.Write(uniformBuffer.Address.Pack() + (uint)uniformBuffer.Offset, argument);

            _context.State.SetUniformBufferOffset(uniformBuffer.Offset + 4);

            _context.AdvanceSequence();
        }
    }
}