using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Buffers.Binary;

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
        private int[] _shaderHandles;

        public bool HasFragmentShader;
        public int FragmentOutputMap { get; }

        public Program(ShaderSource[] shaders, int fragmentOutputMap)
        {
            Handle = GL.CreateProgram();

            GL.ProgramParameter(Handle, ProgramParameterName.ProgramBinaryRetrievableHint, 1);

            _shaderHandles = new int[shaders.Length];

            for (int index = 0; index < shaders.Length; index++)
            {
                ShaderSource shader = shaders[index];

                if (shader.Stage == ShaderStage.Fragment)
                {
                    HasFragmentShader = true;
                }

                int shaderHandle = GL.CreateShader(shader.Stage.Convert());

                switch (shader.Language)
                {
                    case TargetLanguage.Glsl:
                        GL.ShaderSource(shaderHandle, shader.Code);
                        GL.CompileShader(shaderHandle);
                        break;
                    case TargetLanguage.Spirv:
                        GL.ShaderBinary(1, ref shaderHandle, (BinaryFormat)All.ShaderBinaryFormatSpirVArb, shader.BinaryCode, shader.BinaryCode.Length);
                        GL.SpecializeShader(shaderHandle, "main", 0, (int[])null, (int[])null);
                        break;
                }

                GL.AttachShader(Handle, shaderHandle);

                _shaderHandles[index] = shaderHandle;
            }

            GL.LinkProgram(Handle);

            FragmentOutputMap = fragmentOutputMap;
        }

        public Program(ReadOnlySpan<byte> code, bool hasFragmentShader, int fragmentOutputMap)
        {
            Handle = GL.CreateProgram();

            if (code.Length >= 4)
            {
                BinaryFormat binaryFormat = (BinaryFormat)BinaryPrimitives.ReadInt32LittleEndian(code.Slice(code.Length - 4, 4));

                unsafe
                {
                    fixed (byte* ptr = code)
                    {
                        GL.ProgramBinary(Handle, binaryFormat, (IntPtr)ptr, code.Length - 4);
                    }
                }
            }

            HasFragmentShader = hasFragmentShader;
            FragmentOutputMap = fragmentOutputMap;
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
            DeleteShaders();

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

            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(size, 4), (int)binFormat);

            return data;
        }

        private void DeleteShaders()
        {
            if (_shaderHandles != null)
            {
                foreach (int shaderHandle in _shaderHandles)
                {
                    GL.DetachShader(Handle, shaderHandle);
                    GL.DeleteShader(shaderHandle);
                }

                _shaderHandles = null;
            }
        }

        public void Dispose()
        {
            if (Handle != 0)
            {
                DeleteShaders();
                GL.DeleteProgram(Handle);

                Handle = 0;
            }
        }
    }
}
