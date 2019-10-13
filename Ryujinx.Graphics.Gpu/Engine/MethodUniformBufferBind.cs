using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private void UniformBufferBind0(int argument)
        {
            UniformBufferBind(argument, ShaderType.Vertex);
        }

        private void UniformBufferBind1(int argument)
        {
            UniformBufferBind(argument, ShaderType.TessellationControl);
        }

        private void UniformBufferBind2(int argument)
        {
            UniformBufferBind(argument, ShaderType.TessellationEvaluation);
        }

        private void UniformBufferBind3(int argument)
        {
            UniformBufferBind(argument, ShaderType.Geometry);
        }

        private void UniformBufferBind4(int argument)
        {
            UniformBufferBind(argument, ShaderType.Fragment);
        }

        private void UniformBufferBind(int argument, ShaderType type)
        {
            bool enable = (argument & 1) != 0;

            int index = (argument >> 4) & 0x1f;

            if (enable)
            {
                UniformBufferState uniformBuffer = _context.State.GetUniformBufferState();

                ulong address = uniformBuffer.Address.Pack();

                _bufferManager.SetGraphicsUniformBuffer((int)type, index, address, (uint)uniformBuffer.Size);
            }
            else
            {
                _bufferManager.SetGraphicsUniformBuffer((int)type, index, 0, 0);
            }
        }
    }
}