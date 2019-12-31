using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        /// <summary>
        /// Binds a uniform buffer for the vertex shader stage.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void UniformBufferBindVertex(GpuState state, int argument)
        {
            UniformBufferBind(state, argument, ShaderType.Vertex);
        }

        /// <summary>
        /// Binds a uniform buffer for the tessellation control shader stage.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void UniformBufferBindTessControl(GpuState state, int argument)
        {
            UniformBufferBind(state, argument, ShaderType.TessellationControl);
        }

        /// <summary>
        /// Binds a uniform buffer for the tessellation evaluation shader stage.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void UniformBufferBindTessEvaluation(GpuState state, int argument)
        {
            UniformBufferBind(state, argument, ShaderType.TessellationEvaluation);
        }

        /// <summary>
        /// Binds a uniform buffer for the geometry shader stage.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void UniformBufferBindGeometry(GpuState state, int argument)
        {
            UniformBufferBind(state, argument, ShaderType.Geometry);
        }

        /// <summary>
        /// Binds a uniform buffer for the fragment shader stage.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void UniformBufferBindFragment(GpuState state, int argument)
        {
            UniformBufferBind(state, argument, ShaderType.Fragment);
        }

        /// <summary>
        ///Binds a uniform buffer for the specified shader stage.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        /// <param name="type">Shader stage that will access the uniform buffer</param>
        private void UniformBufferBind(GpuState state, int argument, ShaderType type)
        {
            bool enable = (argument & 1) != 0;

            int index = (argument >> 4) & 0x1f;

            if (enable)
            {
                var uniformBuffer = state.Get<UniformBufferState>(MethodOffset.UniformBufferState);

                ulong address = uniformBuffer.Address.Pack();

                BufferManager.SetGraphicsUniformBuffer((int)type, index, address, (uint)uniformBuffer.Size);
            }
            else
            {
                BufferManager.SetGraphicsUniformBuffer((int)type, index, 0, 0);
            }
        }
    }
}