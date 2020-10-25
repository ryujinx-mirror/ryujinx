using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.CodeGen.Glsl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Graphics.OpenGL
{
    class Program : IProgram
    {
        private const int ShaderStages = 6;

        private const int UbStageShift  = 5;
        private const int SbStageShift  = 4;
        private const int TexStageShift = 5;
        private const int ImgStageShift = 3;

        private const int UbsPerStage  = 1 << UbStageShift;
        private const int SbsPerStage  = 1 << SbStageShift;
        private const int TexsPerStage = 1 << TexStageShift;
        private const int ImgsPerStage = 1 << ImgStageShift;

        public int Handle { get; private set; }

        public int FragmentIsBgraUniform { get; }
        public int FragmentRenderScaleUniform { get; }
        public int ComputeRenderScaleUniform { get; }

        public bool IsLinked { get; private set; }

        private int[] _ubBindingPoints;
        private int[] _sbBindingPoints;
        private int[] _textureUnits;
        private int[] _imageUnits;

        public Program(IShader[] shaders, TransformFeedbackDescriptor[] transformFeedbackDescriptors)
        {
            _ubBindingPoints = new int[UbsPerStage  * ShaderStages];
            _sbBindingPoints = new int[SbsPerStage  * ShaderStages];
            _textureUnits    = new int[TexsPerStage * ShaderStages];
            _imageUnits      = new int[ImgsPerStage * ShaderStages];

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

            for (int index = 0; index < _imageUnits.Length; index++)
            {
                _imageUnits[index] = -1;
            }

            Handle = GL.CreateProgram();

            for (int index = 0; index < shaders.Length; index++)
            {
                int shaderHandle = ((Shader)shaders[index]).Handle;

                GL.AttachShader(Handle, shaderHandle);
            }

            if (transformFeedbackDescriptors != null)
            {
                List<string> varyings = new List<string>();

                int cbi = 0;

                foreach (var tfd in transformFeedbackDescriptors.OrderBy(x => x.BufferIndex))
                {
                    if (tfd.VaryingLocations.Length == 0)
                    {
                        continue;
                    }

                    while (cbi < tfd.BufferIndex)
                    {
                        varyings.Add("gl_NextBuffer");

                        cbi++;
                    }

                    int stride = Math.Min(128 * 4, (tfd.Stride + 3) & ~3);

                    int j = 0;

                    for (; j < tfd.VaryingLocations.Length && j * 4 < stride; j++)
                    {
                        byte location = tfd.VaryingLocations[j];

                        varyings.Add(Varying.GetName(location) ?? "gl_SkipComponents1");

                        j += Varying.GetSize(location) - 1;
                    }

                    int feedbackBytes = j * 4;

                    while (feedbackBytes < stride)
                    {
                        int bytes = Math.Min(16, stride - feedbackBytes);

                        varyings.Add($"gl_SkipComponents{(bytes / 4)}");

                        feedbackBytes += bytes;
                    }
                }

                GL.TransformFeedbackVaryings(Handle, varyings.Count, varyings.ToArray(), TransformFeedbackMode.InterleavedAttribs);
            }

            GL.LinkProgram(Handle);

            for (int index = 0; index < shaders.Length; index++)
            {
                int shaderHandle = ((Shader)shaders[index]).Handle;

                GL.DetachShader(Handle, shaderHandle);
            }

            CheckProgramLink();

            int ubBindingPoint = 0;
            int sbBindingPoint = 0;
            int textureUnit    = 0;
            int imageUnit      = 0;

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

                    int bpIndex = (int)shader.Stage << UbStageShift | descriptor.Slot;

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

                    GL.ProgramUniform1(Handle, location, textureUnit);

                    int uIndex = (int)shader.Stage << TexStageShift | samplerIndex++;

                    _textureUnits[uIndex] = textureUnit;

                    textureUnit++;
                }

                int imageIndex = 0;

                foreach (TextureDescriptor descriptor in shader.Info.Images)
                {
                    int location = GL.GetUniformLocation(Handle, descriptor.Name);

                    if (location < 0)
                    {
                        continue;
                    }

                    GL.ProgramUniform1(Handle, location, imageUnit);

                    int uIndex = (int)shader.Stage << ImgStageShift | imageIndex++;

                    _imageUnits[uIndex] = imageUnit;

                    imageUnit++;
                }
            }

            FragmentIsBgraUniform = GL.GetUniformLocation(Handle, "is_bgra");
            FragmentRenderScaleUniform = GL.GetUniformLocation(Handle, "fp_renderScale");
            ComputeRenderScaleUniform = GL.GetUniformLocation(Handle, "cp_renderScale");
        }

        public void Bind()
        {
            GL.UseProgram(Handle);
        }

        public int GetUniformBufferBindingPoint(ShaderStage stage, int index)
        {
            return _ubBindingPoints[(int)stage << UbStageShift | index];
        }

        public int GetStorageBufferBindingPoint(ShaderStage stage, int index)
        {
            return _sbBindingPoints[(int)stage << SbStageShift | index];
        }

        public int GetTextureUnit(ShaderStage stage, int index)
        {
            return _textureUnits[(int)stage << TexStageShift | index];
        }

        public int GetImageUnit(ShaderStage stage, int index)
        {
            return _imageUnits[(int)stage << ImgStageShift | index];
        }

        private void CheckProgramLink()
        {
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int status);

            if (status == 0)
            {
                // Use GL.GetProgramInfoLog(Handle), it may be too long to print on the log.
                Logger.Debug?.Print(LogClass.Gpu, "Shader linking failed.");
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
