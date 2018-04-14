using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal
{
    public unsafe interface IGalRenderer
    {
        void QueueAction(Action ActionMthd);

        void RunActions();

        void Render();

        void SetWindowSize(int Width, int Height);

        //Blend
        void SetBlendEnable(bool Enable);

        void SetBlend(
            GalBlendEquation Equation,
            GalBlendFactor   FuncSrc,
            GalBlendFactor   FuncDst);

        void SetBlendSeparate(
            GalBlendEquation EquationRgb,
            GalBlendEquation EquationAlpha,
            GalBlendFactor   FuncSrcRgb,
            GalBlendFactor   FuncDstRgb,
            GalBlendFactor   FuncSrcAlpha,
            GalBlendFactor   FuncDstAlpha);

        //Frame Buffer
        void CreateFrameBuffer(long Tag, int Width, int Height);

        void BindFrameBuffer(long Tag);

        void BindFrameBufferTexture(long Tag, int Index, GalTextureSampler Sampler);

        void SetFrameBuffer(long Tag);

        void SetFrameBuffer(byte[] Data, int Width, int Height);

        void SetFrameBufferTransform(float SX, float SY, float Rotate, float TX, float TY);

        void SetViewport(int X, int Y, int Width, int Height);

        //Rasterizer
        void ClearBuffers(int RtIndex, GalClearBufferFlags Flags);

        void SetVertexArray(int VbIndex, int Stride, byte[] Buffer, GalVertexAttrib[] Attribs);

        void SetIndexArray(byte[] Buffer, GalIndexFormat Format);

        void DrawArrays(int VbIndex, GalPrimitiveType PrimType);

        void DrawElements(int VbIndex, int First, GalPrimitiveType PrimType);

        //Shader
        void CreateShader(long Tag, GalShaderType Type, byte[] Data);

        IEnumerable<ShaderDeclInfo> GetTextureUsage(long Tag);

        void SetConstBuffer(long Tag, int Cbuf, byte[] Data);

        void SetUniform1(string UniformName, int Value);

        void SetUniform2F(string UniformName, float X, float Y);

        void BindShader(long Tag);

        void BindProgram();

        //Texture
        void SetTextureAndSampler(int Index, GalTexture Texture, GalTextureSampler Sampler);

        void BindTexture(int Index);
    }
}