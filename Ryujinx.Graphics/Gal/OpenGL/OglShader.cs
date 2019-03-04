using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.Gal.Shader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OglShader : IGalShader
    {
        public const int ReservedCbufCount = 1;

        private const int ExtraDataSize = 4;

        public OglShaderProgram Current;

        private ConcurrentDictionary<long, OglShaderStage> _stages;

        private Dictionary<OglShaderProgram, int> _programs;

        public int CurrentProgramHandle { get; private set; }

        private OglConstBuffer _buffer;

        private int _extraUboHandle;

        public OglShader(OglConstBuffer buffer)
        {
            _buffer = buffer;

            _stages = new ConcurrentDictionary<long, OglShaderStage>();

            _programs = new Dictionary<OglShaderProgram, int>();
        }

        public void Create(IGalMemory memory, long key, GalShaderType type)
        {
            _stages.GetOrAdd(key, (stage) => ShaderStageFactory(memory, key, 0, false, type));
        }

        public void Create(IGalMemory memory, long vpAPos, long key, GalShaderType type)
        {
            _stages.GetOrAdd(key, (stage) => ShaderStageFactory(memory, vpAPos, key, true, type));
        }

        private OglShaderStage ShaderStageFactory(
            IGalMemory    memory,
            long          position,
            long          positionB,
            bool          isDualVp,
            GalShaderType type)
        {
            GlslProgram program;

            GlslDecompiler decompiler = new GlslDecompiler(OglLimit.MaxUboSize, OglExtension.NvidiaDriver);

            int shaderDumpIndex = ShaderDumper.DumpIndex;

            if (isDualVp)
            {
                ShaderDumper.Dump(memory, position,  type, "a");
                ShaderDumper.Dump(memory, positionB, type, "b");

                program = decompiler.Decompile(memory, position, positionB, type);
            }
            else
            {
                ShaderDumper.Dump(memory, position, type);

                program = decompiler.Decompile(memory, position, type);
            }

            string code = program.Code;

            if (ShaderDumper.IsDumpEnabled())
            {
                code = "//Shader " + shaderDumpIndex + Environment.NewLine + code;
            }

            return new OglShaderStage(type, code, program.Uniforms, program.Textures);
        }

        public IEnumerable<ShaderDeclInfo> GetConstBufferUsage(long key)
        {
            if (_stages.TryGetValue(key, out OglShaderStage stage))
            {
                return stage.ConstBufferUsage;
            }

            return Enumerable.Empty<ShaderDeclInfo>();
        }

        public IEnumerable<ShaderDeclInfo> GetTextureUsage(long key)
        {
            if (_stages.TryGetValue(key, out OglShaderStage stage))
            {
                return stage.TextureUsage;
            }

            return Enumerable.Empty<ShaderDeclInfo>();
        }

        public unsafe void SetExtraData(float flipX, float flipY, int instance)
        {
            BindProgram();

            EnsureExtraBlock();

            GL.BindBuffer(BufferTarget.UniformBuffer, _extraUboHandle);

            float* data = stackalloc float[ExtraDataSize];
            data[0] = flipX;
            data[1] = flipY;
            data[2] = BitConverter.Int32BitsToSingle(instance);

            //Invalidate buffer
            GL.BufferData(BufferTarget.UniformBuffer, ExtraDataSize * sizeof(float), IntPtr.Zero, BufferUsageHint.StreamDraw);

            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, ExtraDataSize * sizeof(float), (IntPtr)data);
        }

        public void Bind(long key)
        {
            if (_stages.TryGetValue(key, out OglShaderStage stage))
            {
                Bind(stage);
            }
        }

        private void Bind(OglShaderStage stage)
        {
            if (stage.Type == GalShaderType.Geometry)
            {
                //Enhanced layouts are required for Geometry shaders
                //skip this stage if current driver has no ARB_enhanced_layouts
                if (!OglExtension.EnhancedLayouts)
                {
                    return;
                }
            }

            switch (stage.Type)
            {
                case GalShaderType.Vertex:         Current.Vertex         = stage; break;
                case GalShaderType.TessControl:    Current.TessControl    = stage; break;
                case GalShaderType.TessEvaluation: Current.TessEvaluation = stage; break;
                case GalShaderType.Geometry:       Current.Geometry       = stage; break;
                case GalShaderType.Fragment:       Current.Fragment       = stage; break;
            }
        }

        public void Unbind(GalShaderType type)
        {
            switch (type)
            {
                case GalShaderType.Vertex:         Current.Vertex         = null; break;
                case GalShaderType.TessControl:    Current.TessControl    = null; break;
                case GalShaderType.TessEvaluation: Current.TessEvaluation = null; break;
                case GalShaderType.Geometry:       Current.Geometry       = null; break;
                case GalShaderType.Fragment:       Current.Fragment       = null; break;
            }
        }

        public void BindProgram()
        {
            if (Current.Vertex   == null ||
                Current.Fragment == null)
            {
                return;
            }

            if (!_programs.TryGetValue(Current, out int handle))
            {
                handle = GL.CreateProgram();

                AttachIfNotNull(handle, Current.Vertex);
                AttachIfNotNull(handle, Current.TessControl);
                AttachIfNotNull(handle, Current.TessEvaluation);
                AttachIfNotNull(handle, Current.Geometry);
                AttachIfNotNull(handle, Current.Fragment);

                GL.LinkProgram(handle);

                CheckProgramLink(handle);

                BindUniformBlocks(handle);
                BindTextureLocations(handle);

                _programs.Add(Current, handle);
            }

            GL.UseProgram(handle);

            CurrentProgramHandle = handle;
        }

        private void EnsureExtraBlock()
        {
            if (_extraUboHandle == 0)
            {
                _extraUboHandle = GL.GenBuffer();

                GL.BindBuffer(BufferTarget.UniformBuffer, _extraUboHandle);

                GL.BufferData(BufferTarget.UniformBuffer, ExtraDataSize * sizeof(float), IntPtr.Zero, BufferUsageHint.StreamDraw);

                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, _extraUboHandle);
            }
        }

        private void AttachIfNotNull(int programHandle, OglShaderStage stage)
        {
            if (stage != null)
            {
                stage.Compile();

                GL.AttachShader(programHandle, stage.Handle);
            }
        }

        private void BindUniformBlocks(int programHandle)
        {
            int extraBlockindex = GL.GetUniformBlockIndex(programHandle, GlslDecl.ExtraUniformBlockName);

            GL.UniformBlockBinding(programHandle, extraBlockindex, 0);

            int freeBinding = ReservedCbufCount;

            void BindUniformBlocksIfNotNull(OglShaderStage stage)
            {
                if (stage != null)
                {
                    foreach (ShaderDeclInfo declInfo in stage.ConstBufferUsage)
                    {
                        int blockIndex = GL.GetUniformBlockIndex(programHandle, declInfo.Name);

                        if (blockIndex < 0)
                        {
                            //It is expected that its found, if it's not then driver might be in a malfunction
                            throw new InvalidOperationException();
                        }

                        GL.UniformBlockBinding(programHandle, blockIndex, freeBinding);

                        freeBinding++;
                    }
                }
            }

            BindUniformBlocksIfNotNull(Current.Vertex);
            BindUniformBlocksIfNotNull(Current.TessControl);
            BindUniformBlocksIfNotNull(Current.TessEvaluation);
            BindUniformBlocksIfNotNull(Current.Geometry);
            BindUniformBlocksIfNotNull(Current.Fragment);
        }

        private void BindTextureLocations(int programHandle)
        {
            int index = 0;

            void BindTexturesIfNotNull(OglShaderStage stage)
            {
                if (stage != null)
                {
                    foreach (ShaderDeclInfo decl in stage.TextureUsage)
                    {
                        int location = GL.GetUniformLocation(programHandle, decl.Name);

                        GL.Uniform1(location, index);

                        index++;
                    }
                }
            }

            GL.UseProgram(programHandle);

            BindTexturesIfNotNull(Current.Vertex);
            BindTexturesIfNotNull(Current.TessControl);
            BindTexturesIfNotNull(Current.TessEvaluation);
            BindTexturesIfNotNull(Current.Geometry);
            BindTexturesIfNotNull(Current.Fragment);
        }

        private static void CheckProgramLink(int handle)
        {
            int status = 0;

            GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out status);

            if (status == 0)
            {
                throw new ShaderException(GL.GetProgramInfoLog(handle));
            }
        }
    }
}