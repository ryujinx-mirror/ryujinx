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

        public bool IsLinked
        {
            get
            {
                if (_status == ProgramLinkStatus.Incomplete)
                {
                    CheckProgramLink(true);
                }

                return _status == ProgramLinkStatus.Success;
            }
        }

        private ProgramLinkStatus _status = ProgramLinkStatus.Incomplete;
        private IShader[] _shaders;

        public Program(IShader[] shaders)
        {
            Handle = GL.CreateProgram();

            GL.ProgramParameter(Handle, ProgramParameterName.ProgramBinaryRetrievableHint, 1);

            for (int index = 0; index < shaders.Length; index++)
            {
                int shaderHandle = ((Shader)shaders[index]).Handle;

                GL.AttachShader(Handle, shaderHandle);
            }

            GL.LinkProgram(Handle);

            _shaders = shaders;
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
        }

        public void Bind()
        {
            GL.UseProgram(Handle);
        }

        public ProgramLinkStatus CheckProgramLink(bool blocking)
        {
            if (!blocking && HwCapabilities.SupportsParallelShaderCompile)
            {
                GL.GetProgram(Handle, (GetProgramParameterName)ArbParallelShaderCompile.CompletionStatusArb, out int completed);

                if (completed == 0)
                {
                    return ProgramLinkStatus.Incomplete;
                }
            }

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int status);

            if (_shaders != null)
            {
                for (int index = 0; index < _shaders.Length; index++)
                {
                    int shaderHandle = ((Shader)_shaders[index]).Handle;

                    GL.DetachShader(Handle, shaderHandle);
                }

                _shaders = null;
            }

            if (status == 0)
            {
                // Use GL.GetProgramInfoLog(Handle), it may be too long to print on the log.
                _status = ProgramLinkStatus.Failure;
                Logger.Debug?.Print(LogClass.Gpu, "Shader linking failed.");
            }
            else
            {
                _status = ProgramLinkStatus.Success;
            }

            return _status;
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
