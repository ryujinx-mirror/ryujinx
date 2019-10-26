using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private void UniformBufferBindVertex(int argument)
        {
            UniformBufferBind(argument, ShaderType.Vertex);
        }

        private void UniformBufferBindTessControl(int argument)
        {
            UniformBufferBind(argument, ShaderType.TessellationControl);
        }

        private void UniformBufferBindTessEvaluation(int argument)
        {
            UniformBufferBind(argument, ShaderType.TessellationEvaluation);
        }

        private void UniformBufferBindGeometry(int argument)
        {
            UniformBufferBind(argument, ShaderType.Geometry);
        }

        private void UniformBufferBindFragment(int argument)
        {
            UniformBufferBind(argument, ShaderType.Fragment);
        }

        private void UniformBufferBind(int argument, ShaderType type)
        {
            bool enable = (argument & 1) != 0;

            int index = (argument >> 4) & 0x1f;

            if (enable)
            {
                var uniformBuffer = _context.State.Get<UniformBufferState>(MethodOffset.UniformBufferState);

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