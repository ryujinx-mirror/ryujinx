using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader.CodeGen.Glsl;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Graphics.OpenGL
{
    class Program : IProgram
    {
        public int Handle { get; private set; }

        public int FragmentIsBgraUniform { get; }
        public int FragmentRenderScaleUniform { get; }
        public int ComputeRenderScaleUniform { get; }

        public bool IsLinked { get; private set; }

        public Program(IShader[] shaders, TransformFeedbackDescriptor[] transformFeedbackDescriptors)
        {
            Handle = GL.CreateProgram();

            GL.ProgramParameter(Handle, ProgramParameterName.ProgramBinaryRetrievableHint, 1);

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

            FragmentIsBgraUniform = GL.GetUniformLocation(Handle, "is_bgra");
            FragmentRenderScaleUniform = GL.GetUniformLocation(Handle, "fp_renderScale");
            ComputeRenderScaleUniform = GL.GetUniformLocation(Handle, "cp_renderScale");
        }

        public Program(ReadOnlySpan<byte> code)
        {
            BinaryFormat binaryFormat = (BinaryFormat)BinaryPrimitives.ReadInt32LittleEndian(code.Slice(code.Length - 4, 4));

            Handle = GL.CreateProgram();

            unsafe
            {
                fixed (byte* ptr = code)
                {
                    GL.ProgramBinary(Handle, binaryFormat, (IntPtr)ptr, code.Length - 4);
                }
            }

            CheckProgramLink();

            FragmentIsBgraUniform = GL.GetUniformLocation(Handle, "is_bgra");
            FragmentRenderScaleUniform = GL.GetUniformLocation(Handle, "fp_renderScale");
            ComputeRenderScaleUniform = GL.GetUniformLocation(Handle, "cp_renderScale");
        }

        public void Bind()
        {
            GL.UseProgram(Handle);
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

        public byte[] GetBinary()
        {
            GL.GetProgram(Handle, (GetProgramParameterName)All.ProgramBinaryLength, out int size);

            byte[] data = new byte[size + 4];

            GL.GetProgramBinary(Handle, size, out _, out BinaryFormat binFormat, data);

            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan().Slice(size, 4), (int)binFormat);

            return data;
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
