using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal
{
    public unsafe interface IGalRenderer
    {
        void QueueAction(Action ActionMthd);
        void RunActions();

        void InitializeFrameBuffer();
        void ResetFrameBuffer();
        void Render();
        void SetWindowSize(int Width, int Height);
        void SetFrameBuffer(
            byte* Fb,
            int   Width,
            int   Height,
            float ScaleX,
            float ScaleY,
            float OffsX,
            float OffsY,
            float Rotate);

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
        void SetFb(int FbIndex, int Width, int Height);

        void BindFrameBuffer(int FbIndex);

        void DrawFrameBuffer(int FbIndex);

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

        void BindShader(long Tag);

        void BindProgram();

        //Texture
        void SetTexture(int Index, GalTexture Tex);

        void SetSampler(int Index, GalTextureSampler Sampler);
    }
}