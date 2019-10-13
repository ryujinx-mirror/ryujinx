using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    class ComputePipeline : IComputePipeline
    {
        private Renderer _renderer;

        private Program _program;

        public ComputePipeline(Renderer renderer)
        {
            _renderer = renderer;
        }

        public void Dispatch(int groupsX, int groupsY, int groupsZ)
        {
            BindProgram();

            GL.DispatchCompute(groupsX, groupsY, groupsZ);

            UnbindProgram();
        }

        public void SetProgram(IProgram program)
        {
            _program = (Program)program;
        }

        public void SetStorageBuffer(int index, BufferRange buffer)
        {
            BindProgram();

            BindBuffer(index, buffer, isStorage: true);

            UnbindProgram();
        }

        public void SetUniformBuffer(int index, BufferRange buffer)
        {
            BindProgram();

            BindBuffer(index, buffer, isStorage: false);

            UnbindProgram();
        }

        private void BindBuffer(int index, BufferRange buffer, bool isStorage)
        {
            int bindingPoint = isStorage
                ? _program.GetStorageBufferBindingPoint(ShaderStage.Compute, index)
                : _program.GetUniformBufferBindingPoint(ShaderStage.Compute, index);

            if (bindingPoint == -1)
            {
                return;
            }

            BufferRangeTarget target = isStorage
                ? BufferRangeTarget.ShaderStorageBuffer
                : BufferRangeTarget.UniformBuffer;

            if (buffer.Buffer == null)
            {
                GL.BindBufferRange(target, bindingPoint, 0, IntPtr.Zero, 0);

                return;
            }

            int bufferHandle = ((Buffer)buffer.Buffer).Handle;

            IntPtr bufferOffset = (IntPtr)buffer.Offset;

            GL.BindBufferRange(
                target,
                bindingPoint,
                bufferHandle,
                bufferOffset,
                buffer.Size);
        }

        private void BindProgram()
        {
            _program.Bind();
        }

        private void UnbindProgram()
        {
            ((GraphicsPipeline)_renderer.GraphicsPipeline).RebindProgram();
        }
    }
}
