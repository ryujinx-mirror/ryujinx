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
            //TODO
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

        public void SetTextureAndSampler(int Index, GalTexture Texture, GalTextureSampler Sampler)
        {
            ActionsQueue.Enqueue(() =>
            {
                this.Texture.Set(Index, Texture);

                OGLTexture.Set(Sampler);
            });
        }

        public void BindTexture(int Index)
        {
            ActionsQueue.Enqueue(() => Texture.Bind(Index));
        }
    }
}