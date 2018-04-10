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

        private FrameBuffer FbRenderer;

        public OpenGLRenderer()
        {
            Blend = new OGLBlend();

            FrameBuffer = new OGLFrameBuffer();

            Rasterizer = new OGLRasterizer();

            Shader = new OGLShader();

            Texture = new OGLTexture();

            ActionsQueue = new ConcurrentQueue<Action>();
        }

        public void InitializeFrameBuffer()
        {
            FbRenderer = new FrameBuffer(1280, 720);
        }

        public void ResetFrameBuffer()
        {
            FbRenderer.Reset();
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
            //FbRenderer.Render();
        }

        public void SetWindowSize(int Width, int Height)
        {
            FbRenderer.WindowWidth  = Width;
            FbRenderer.WindowHeight = Height;
        }

        public unsafe void SetFrameBuffer(
            byte* Fb,
            int   Width,
            int   Height,
            float ScaleX,
            float ScaleY,
            float OffsX,
            float OffsY,
            float Rotate)
        {
            Matrix2 Transform;

            Transform  = Matrix2.CreateScale(ScaleX, ScaleY);
            Transform *= Matrix2.CreateRotation(Rotate);

            Vector2 Offs = new Vector2(OffsX, OffsY);

            FbRenderer.Set(Fb, Width, Height, Transform, Offs);
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

        public void SetFb(int FbIndex, int Width, int Height)
        {
            ActionsQueue.Enqueue(() => FrameBuffer.Set(FbIndex, Width, Height));
        }

        public void BindFrameBuffer(int FbIndex)
        {
            ActionsQueue.Enqueue(() => FrameBuffer.Bind(FbIndex));
        }

        public void DrawFrameBuffer(int FbIndex)
        {
            ActionsQueue.Enqueue(() => FrameBuffer.Draw(FbIndex));
        }

        public void ClearBuffers(int RtIndex, GalClearBufferFlags Flags)
        {
            ActionsQueue.Enqueue(() => Rasterizer.ClearBuffers(RtIndex, Flags));
        }

        public void SetVertexArray(int VbIndex, int Stride, byte[] Buffer, GalVertexAttrib[] Attribs)
        {
            if ((uint)VbIndex > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(VbIndex));
            }

            ActionsQueue.Enqueue(() => Rasterizer.SetVertexArray(VbIndex, Stride,
                Buffer  ?? throw new ArgumentNullException(nameof(Buffer)),
                Attribs ?? throw new ArgumentNullException(nameof(Attribs))));
        }

        public void SetIndexArray(byte[] Buffer, GalIndexFormat Format)
        {
            if (Buffer == null)
            {
                throw new ArgumentNullException(nameof(Buffer));
            }

            ActionsQueue.Enqueue(() => Rasterizer.SetIndexArray(Buffer, Format));
        }

        public void DrawArrays(int VbIndex, GalPrimitiveType PrimType)
        {
            if ((uint)VbIndex > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(VbIndex));
            }

            ActionsQueue.Enqueue(() => Rasterizer.DrawArrays(VbIndex, PrimType));
        }

        public void DrawElements(int VbIndex, int First, GalPrimitiveType PrimType)
        {
            if ((uint)VbIndex > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(VbIndex));
            }

            ActionsQueue.Enqueue(() => Rasterizer.DrawElements(VbIndex, First, PrimType));
        }

        public void CreateShader(long Tag, GalShaderType Type, byte[] Data)
        {
            if (Data == null)
            {
                throw new ArgumentNullException(nameof(Data));
            }

            Shader.Create(Tag, Type, Data);
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

        public void SetTexture(int Index, GalTexture Tex)
        {
            ActionsQueue.Enqueue(() => Texture.Set(Index, Tex));
        }

        public void SetSampler(int Index, GalTextureSampler Sampler)
        {
            ActionsQueue.Enqueue(() => Texture.Set(Index, Sampler));
        }
    }
}