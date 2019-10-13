using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private void UniformBufferUpdate(int argument)
        {
            UniformBufferState uniformBuffer = _context.State.GetUniformBufferState();

            _context.MemoryAccessor.Write(uniformBuffer.Address.Pack() + (uint)uniformBuffer.Offset, argument);

            _context.State.SetUniformBufferOffset(uniformBuffer.Offset + 4);

            _context.AdvanceSequence();
        }
    }
}