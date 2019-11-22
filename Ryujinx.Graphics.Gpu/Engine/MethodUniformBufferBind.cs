using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private void UniformBufferBindVertex(GpuState state, int argument)
        {
            UniformBufferBind(state, argument, ShaderType.Vertex);
        }

        private void UniformBufferBindTessControl(GpuState state, int argument)
        {
            UniformBufferBind(state, argument, ShaderType.TessellationControl);
        }

        private void UniformBufferBindTessEvaluation(GpuState state, int argument)
        {
            UniformBufferBind(state, argument, ShaderType.TessellationEvaluation);
        }

        private void UniformBufferBindGeometry(GpuState state, int argument)
        {
            UniformBufferBind(state, argument, ShaderType.Geometry);
        }

        private void UniformBufferBindFragment(GpuState state, int argument)
        {
            UniformBufferBind(state, argument, ShaderType.Fragment);
        }

        private void UniformBufferBind(GpuState state, int argument, ShaderType type)
        {
            bool enable = (argument & 1) != 0;

            int index = (argument >> 4) & 0x1f;

            if (enable)
            {
                var uniformBuffer = state.Get<UniformBufferState>(MethodOffset.UniformBufferState);

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