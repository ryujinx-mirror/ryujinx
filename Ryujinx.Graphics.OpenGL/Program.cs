using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using OpenTK.Graphics.OpenGL;

namespace Ryujinx.Graphics.OpenGL
{
    class Program : IProgram
    {
        private const int StageShift   = 5;
        private const int SbStageShift = 4;

        public int Handle { get; private set; }

        public bool IsLinked { get; private set; }

        private int[] _ubBindingPoints;
        private int[] _sbBindingPoints;
        private int[] _textureUnits;

        public Program(IShader[] shaders)
        {
            _ubBindingPoints = new int[32 * 6];
            _sbBindingPoints = new int[16 * 6];
            _textureUnits    = new int[32 * 6];

            for (int index = 0; index < _ubBindingPoints.Length; index++)
            {
                _ubBindingPoints[index] = -1;
            }

            for (int index = 0; index < _sbBindingPoints.Length; index++)
            {
                _sbBindingPoints[index] = -1;
            }

            for (int index = 0; index < _textureUnits.Length; index++)
            {
                _textureUnits[index] = -1;
            }

            Handle = GL.CreateProgram();

            for (int index = 0; index < shaders.Length; index++)
            {
                int shaderHandle = ((Shader)shaders[index]).Handle;

                GL.AttachShader(Handle, shaderHandle);
            }

            GL.LinkProgram(Handle);

            CheckProgramLink();

            Bind();

            int extraBlockindex = GL.GetUniformBlockIndex(Handle, "Extra");

            if (extraBlockindex >= 0)
            {
                GL.UniformBlockBinding(Handle, extraBlockindex, 0);
            }

            int ubBindingPoint = 1;
            int sbBindingPoint = 0;
            int textureUnit    = 0;

            for (int index = 0; index < shaders.Length; index++)
            {
                Shader shader = (Shader)shaders[index];

                foreach (BufferDescriptor descriptor in shader.Info.CBuffers)
                {
                    int location = GL.GetUniformBlockIndex(Handle, descriptor.Name);

                    if (location < 0)
                    {
                        continue;
                    }

                    GL.UniformBlockBinding(Handle, location, ubBindingPoint);

                    int bpIndex = (int)shader.Stage << StageShift | descriptor.Slot;

                    _ubBindingPoints[bpIndex] = ubBindingPoint;

                    ubBindingPoint++;
                }

                foreach (BufferDescriptor descriptor in shader.Info.SBuffers)
                {
                    int location = GL.GetProgramResourceIndex(Handle, ProgramInterface.ShaderStorageBlock, descriptor.Name);

                    if (location < 0)
                    {
                        continue;
                    }

                    GL.ShaderStorageBlockBinding(Handle, location, sbBindingPoint);

                    int bpIndex = (int)shader.Stage << SbStageShift | descriptor.Slot;

                    _sbBindingPoints[bpIndex] = sbBindingPoint;

                    sbBindingPoint++;
                }

                int samplerIndex = 0;

                foreach (TextureDescriptor descriptor in shader.Info.Textures)
                {
                    int location = GL.GetUniformLocation(Handle, descriptor.Name);

                    if (location < 0)
                    {
                        continue;
                    }

                    GL.Uniform1(location, textureUnit);

                    int uIndex = (int)shader.Stage << StageShift | samplerIndex++;

                    _textureUnits[uIndex] = textureUnit;

                    textureUnit++;
                }
            }
        }

        public void Bind()
        {
            GL.UseProgram(Handle);
        }

        public int GetUniformBufferBindingPoint(ShaderStage stage, int index)
        {
            return _ubBindingPoints[(int)stage << StageShift | index];
        }

        public int GetStorageBufferBindingPoint(ShaderStage stage, int index)
        {
            return _sbBindingPoints[(int)stage << SbStageShift | index];
        }

        public int GetTextureUnit(ShaderStage stage, int index)
        {
            return _textureUnits[(int)stage << StageShift | index];
        }

        private void CheckProgramLink()
        {
            int status = 0;

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out status);

            if (status == 0)
            {
                // throw new System.Exception(GL.GetProgramInfoLog(Handle));
            }
            else
            {
                IsLinked = true;
            }
        }

        public void Dispose()
        {
            if (Handle != 0)
            {
                GL.DeleteProgram(Handle);

                Handle = 0;
            }
        }
    }
}
