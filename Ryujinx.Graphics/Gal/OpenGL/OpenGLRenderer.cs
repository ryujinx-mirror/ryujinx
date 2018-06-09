using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    public class OpenGLRenderer : IGalRenderer
    {
        private OGLBlend Blend;

        private OGLFrameBuffer FrameBuffer;

        private OGLRasterizer Rasterizer;

        private OGLShader Shader;

        private OGLTexture Texture;

        private ConcurrentQueue<Action> ActionsQueue;

        public OpenGLRenderer()
        {
            Blend = new OGLBlend();

            FrameBuffer = new OGLFrameBuffer();

            Rasterizer = new OGLRasterizer();

            Shader = new OGLShader();

            Texture = new OGLTexture();

            ActionsQueue = new ConcurrentQueue<Action>();
        }

        public void QueueAction(Action ActionMthd)
        {
            ActionsQueue.Enqueue(ActionMthd);
        }

        public void RunActions()
        {
            int Count = ActionsQueue.Count;

            while (Count-- > 0 && ActionsQueue.TryDequeue(out Action RenderAction))
            {
                RenderAction();
            }
        }

        public void Render()
        {
            FrameBuffer.Render();
        }

        public void SetWindowSize(int Width, int Height)
        {
            FrameBuffer.SetWindowSize(Width, Height);
        }

        public void SetBlendEnable(bool Enable)
        {
            if (Enable)
            {
                ActionsQueue.Enqueue(() => Blend.Enable());
            }
            else
            {
                ActionsQueue.Enqueue(() => Blend.Disable());
            }
        }

        public void SetBlend(
            GalBlendEquation Equation,
            GalBlendFactor   FuncSrc,
            GalBlendFactor   FuncDst)
        {
            ActionsQueue.Enqueue(() => Blend.Set(Equation, FuncSrc, FuncDst));
        }

        public void SetBlendSeparate(
            GalBlendEquation EquationRgb,
            GalBlendEquation EquationAlpha,
            GalBlendFactor   FuncSrcRgb,
            GalBlendFactor   FuncDstRgb,
            GalBlendFactor   FuncSrcAlpha,
            GalBlendFactor   FuncDstAlpha)
        {
            ActionsQueue.Enqueue(() =>
            {
                Blend.SetSeparate(
                    EquationRgb,
                    EquationAlpha,
                    FuncSrcRgb,
                    FuncDstRgb,
                    FuncSrcAlpha,
                    FuncDstAlpha);
            });
        }

        public void CreateFrameBuffer(long Tag, int Width, int Height)
        {
            ActionsQueue.Enqueue(() => FrameBuffer.Create(Tag, Width, Height));
        }

        public void BindFrameBuffer(long Tag)
        {
            ActionsQueue.Enqueue(() => FrameBuffer.Bind(Tag));
        }

        public void BindFrameBufferTexture(long Tag, int Index, GalTextureSampler Sampler)
        {
            ActionsQueue.Enqueue(() =>
            {
                FrameBuffer.BindTexture(Tag, Index);

                OGLTexture.Set(Sampler);
            });
        }

        public void SetFrameBuffer(long Tag)
        {
            ActionsQueue.Enqueue(() => FrameBuffer.Set(Tag));
        }

        public void SetFrameBuffer(byte[] Data, int Width, int Height)
        {
            ActionsQueue.Enqueue(() => FrameBuffer.Set(Data, Width, Height));
        }

        public void SetFrameBufferTransform(float SX, float SY, float Rotate, float TX, float TY)
        {
            Matrix2 Transform;

            Transform  = Matrix2.CreateScale(SX, SY);
            Transform *= Matrix2.CreateRotation(Rotate);

            Vector2 Offs = new Vector2(TX, TY);

            ActionsQueue.Enqueue(() => FrameBuffer.SetTransform(Transform, Offs));
        }

        public void SetViewport(int X, int Y, int Width, int Height)
        {
            ActionsQueue.Enqueue(() => FrameBuffer.SetViewport(X, Y, Width, Height));
        }

        public void GetFrameBufferData(long Tag, Action<byte[]> Callback)
        {
            ActionsQueue.Enqueue(() => FrameBuffer.GetBufferData(Tag, Callback));
        }

        public void ClearBuffers(int RtIndex, GalClearBufferFlags Flags)
        {
            ActionsQueue.Enqueue(() => Rasterizer.ClearBuffers(RtIndex, Flags));
        }

        public bool IsVboCached(long Tag, long DataSize)
        {
            return Rasterizer.IsVboCached(Tag, DataSize);
        }

        public bool IsIboCached(long Tag, long DataSize)
        {
            return Rasterizer.IsIboCached(Tag, DataSize);
        }

        public void CreateVbo(long Tag, byte[] Buffer)
        {
            ActionsQueue.Enqueue(() => Rasterizer.CreateVbo(Tag, Buffer));
        }

        public void CreateIbo(long Tag, byte[] Buffer)
        {
            ActionsQueue.Enqueue(() => Rasterizer.CreateIbo(Tag, Buffer));
        }

        public void SetVertexArray(int VbIndex, int Stride, long VboTag, GalVertexAttrib[] Attribs)
        {
            if ((uint)VbIndex > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(VbIndex));
            }

            if (Attribs == null)
            {
                throw new ArgumentNullException(nameof(Attribs));
            }

            ActionsQueue.Enqueue(() => Rasterizer.SetVertexArray(VbIndex, Stride, VboTag, Attribs));
        }

        public void SetIndexArray(long Tag, int Size, GalIndexFormat Format)
        {
            ActionsQueue.Enqueue(() => Rasterizer.SetIndexArray(Tag, Size, Format));
        }

        public void DrawArrays(int First, int PrimCount, GalPrimitiveType PrimType)
        {
            ActionsQueue.Enqueue(() => Rasterizer.DrawArrays(First, PrimCount, PrimType));
        }

        public void DrawElements(long IboTag, int First, GalPrimitiveType PrimType)
        {
            ActionsQueue.Enqueue(() => Rasterizer.DrawElements(IboTag, First, PrimType));
        }

        public void CreateShader(IGalMemory Memory, long Tag, GalShaderType Type)
        {
            if (Memory == null)
            {
                throw new ArgumentNullException(nameof(Memory));
            }

            Shader.Create(Memory, Tag, Type);
        }

        public void SetConstBuffer(long Tag, int Cbuf, byte[] Data)
        {
            if (Data == null)
            {
                throw new ArgumentNullException(nameof(Data));
            }

            ActionsQueue.Enqueue(() => Shader.SetConstBuffer(Tag, Cbuf, Data));
        }

        public void SetUniform1(string UniformName, int Value)
        {
            if (UniformName == null)
            {
                throw new ArgumentNullException(nameof(UniformName));
            }

            ActionsQueue.Enqueue(() => Shader.SetUniform1(UniformName, Value));
        }

        public void SetUniform2F(string UniformName, float X, float Y)
        {
            if (UniformName == null)
            {
                throw new ArgumentNullException(nameof(UniformName));
            }

            ActionsQueue.Enqueue(() => Shader.SetUniform2F(UniformName, X, Y));
        }

        public IEnumerable<ShaderDeclInfo> GetTextureUsage(long Tag)
        {
            return Shader.GetTextureUsage(Tag);
        }

        public void BindShader(long Tag)
        {
            ActionsQueue.Enqueue(() => Shader.Bind(Tag));
        }

        public void BindProgram()
        {
            ActionsQueue.Enqueue(() => Shader.BindProgram());
        }

        public void SetTextureAndSampler(long Tag, byte[] Data, GalTexture Texture, GalTextureSampler Sampler)
        {
            ActionsQueue.Enqueue(() =>
            {
                this.Texture.Create(Tag, Data, Texture);

                OGLTexture.Set(Sampler);
            });
        }

        public bool TryGetCachedTexture(long Tag, long DataSize, out GalTexture Texture)
        {
            return this.Texture.TryGetCachedTexture(Tag, DataSize, out Texture);
        }

        public void BindTexture(long Tag, int Index)
        {
            ActionsQueue.Enqueue(() => Texture.Bind(Tag, Index));
        }
    }
}