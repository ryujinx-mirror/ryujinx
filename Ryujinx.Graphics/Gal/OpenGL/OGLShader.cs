using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.Gal.Shader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLShader : IGalShader
    {
        public OGLShaderProgram Current;

        private ConcurrentDictionary<long, OGLShaderStage> Stages;

        private Dictionary<OGLShaderProgram, int> Programs;

        public int CurrentProgramHandle { get; private set; }

        private OGLConstBuffer Buffer;

        private int ExtraUboHandle;

        public OGLShader(OGLConstBuffer Buffer)
        {
            this.Buffer = Buffer;

            Stages = new ConcurrentDictionary<long, OGLShaderStage>();

            Programs = new Dictionary<OGLShaderProgram, int>();
        }

        public void Create(IGalMemory Memory, long Key, GalShaderType Type)
        {
            Stages.GetOrAdd(Key, (Stage) => ShaderStageFactory(Memory, Key, 0, false, Type));
        }

        public void Create(IGalMemory Memory, long VpAPos, long Key, GalShaderType Type)
        {
            Stages.GetOrAdd(Key, (Stage) => ShaderStageFactory(Memory, VpAPos, Key, true, Type));
        }

        private OGLShaderStage ShaderStageFactory(
            IGalMemory    Memory,
            long          Position,
            long          PositionB,
            bool          IsDualVp,
            GalShaderType Type)
        {
            GlslProgram Program;

            GlslDecompiler Decompiler = new GlslDecompiler();

            if (IsDualVp)
            {
                ShaderDumper.Dump(Memory, Position,  Type, "a");
                ShaderDumper.Dump(Memory, PositionB, Type, "b");

                Program = Decompiler.Decompile(
                    Memory,
                    Position,
                    PositionB,
                    Type);
            }
            else
            {
                ShaderDumper.Dump(Memory, Position, Type);

                Program = Decompiler.Decompile(Memory, Position, Type);
            }

            return new OGLShaderStage(
                Type,
                Program.Code,
                Program.Uniforms,
                Program.Textures);
        }

        public IEnumerable<ShaderDeclInfo> GetConstBufferUsage(long Key)
        {
            if (Stages.TryGetValue(Key, out OGLShaderStage Stage))
            {
                return Stage.ConstBufferUsage;
            }

            return Enumerable.Empty<ShaderDeclInfo>();
        }

        public IEnumerable<ShaderDeclInfo> GetTextureUsage(long Key)
        {
            if (Stages.TryGetValue(Key, out OGLShaderStage Stage))
            {
                return Stage.TextureUsage;
            }

            return Enumerable.Empty<ShaderDeclInfo>();
        }

        public void EnsureTextureBinding(string UniformName, int Value)
        {
            BindProgram();

            int Location = GL.GetUniformLocation(CurrentProgramHandle, UniformName);

            GL.Uniform1(Location, Value);
        }

        public unsafe void SetFlip(float X, float Y)
        {
            BindProgram();

            EnsureExtraBlock();

            GL.BindBuffer(BufferTarget.UniformBuffer, ExtraUboHandle);

            float* Data = stackalloc float[4];
            Data[0] = X;
            Data[1] = Y;

            //Invalidate buffer
            GL.BufferData(BufferTarget.UniformBuffer, 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.StreamDraw);

            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, 4 * sizeof(float), (IntPtr)Data);
        }

        public void Bind(long Key)
        {
            if (Stages.TryGetValue(Key, out OGLShaderStage Stage))
            {
                Bind(Stage);
            }
        }

        private void Bind(OGLShaderStage Stage)
        {
            if (Stage.Type == GalShaderType.Geometry)
            {
                //Enhanced layouts are required for Geometry shaders
                //skip this stage if current driver has no ARB_enhanced_layouts
                if (!OGLExtension.HasEnhancedLayouts())
                {
                    return;
                }
            }

            switch (Stage.Type)
            {
                case GalShaderType.Vertex:         Current.Vertex         = Stage; break;
                case GalShaderType.TessControl:    Current.TessControl    = Stage; break;
                case GalShaderType.TessEvaluation: Current.TessEvaluation = Stage; break;
                case GalShaderType.Geometry:       Current.Geometry       = Stage; break;
                case GalShaderType.Fragment:       Current.Fragment       = Stage; break;
            }
        }

        public void Unbind(GalShaderType Type)
        {
            switch (Type)
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

            if (!Programs.TryGetValue(Current, out int Handle))
            {
                Handle = GL.CreateProgram();

                AttachIfNotNull(Handle, Current.Vertex);
                AttachIfNotNull(Handle, Current.TessControl);
                AttachIfNotNull(Handle, Current.TessEvaluation);
                AttachIfNotNull(Handle, Current.Geometry);
                AttachIfNotNull(Handle, Current.Fragment);

                GL.LinkProgram(Handle);

                CheckProgramLink(Handle);

                BindUniformBlocks(Handle);

                Programs.Add(Current, Handle);
            }

            GL.UseProgram(Handle);

            CurrentProgramHandle = Handle;
        }

        private void EnsureExtraBlock()
        {
            if (ExtraUboHandle == 0)
            {
                ExtraUboHandle = GL.GenBuffer();

                GL.BindBuffer(BufferTarget.UniformBuffer, ExtraUboHandle);

                GL.BufferData(BufferTarget.UniformBuffer, 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.StreamDraw);

                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, ExtraUboHandle);
            }
        }

        private void AttachIfNotNull(int ProgramHandle, OGLShaderStage Stage)
        {
            if (Stage != null)
            {
                Stage.Compile();

                GL.AttachShader(ProgramHandle, Stage.Handle);
            }
        }

        private void BindUniformBlocks(int ProgramHandle)
        {
            int ExtraBlockindex = GL.GetUniformBlockIndex(ProgramHandle, GlslDecl.ExtraUniformBlockName);

            GL.UniformBlockBinding(ProgramHandle, ExtraBlockindex, 0);

            //First index is reserved
            int FreeBinding = 1;

            void BindUniformBlocksIfNotNull(OGLShaderStage Stage)
            {
                if (Stage != null)
                {
                    foreach (ShaderDeclInfo DeclInfo in Stage.ConstBufferUsage)
                    {
                        int BlockIndex = GL.GetUniformBlockIndex(ProgramHandle, DeclInfo.Name);

                        if (BlockIndex < 0)
                        {
                            //It is expected that its found, if it's not then driver might be in a malfunction
                            throw new InvalidOperationException();
                        }

                        GL.UniformBlockBinding(ProgramHandle, BlockIndex, FreeBinding);

                        FreeBinding++;
                    }
                }
            }

            BindUniformBlocksIfNotNull(Current.Vertex);
            BindUniformBlocksIfNotNull(Current.TessControl);
            BindUniformBlocksIfNotNull(Current.TessEvaluation);
            BindUniformBlocksIfNotNull(Current.Geometry);
            BindUniformBlocksIfNotNull(Current.Fragment);
        }

        private static void CheckProgramLink(int Handle)
        {
            int Status = 0;

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out Status);

            if (Status == 0)
            {
                throw new ShaderException(GL.GetProgramInfoLog(Handle));
            }
        }
    }
}