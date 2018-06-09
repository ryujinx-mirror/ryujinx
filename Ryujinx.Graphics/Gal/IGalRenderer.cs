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

        void GetFrameBufferData(long Tag, Action<byte[]> Callback);

        //Rasterizer
        void ClearBuffers(int RtIndex, GalClearBufferFlags Flags);

        bool IsVboCached(long Tag, long DataSize);

        bool IsIboCached(long Tag, long DataSize);

        void CreateVbo(long Tag, byte[] Buffer);

        void CreateIbo(long Tag, byte[] Buffer);

        void SetVertexArray(int VbIndex, int Stride, long VboTag, GalVertexAttrib[] Attribs);

        void SetIndexArray(long Tag, int Size, GalIndexFormat Format);

        void DrawArrays(int First, int PrimCount, GalPrimitiveType PrimType);

        void DrawElements(long IboTag, int First, GalPrimitiveType PrimType);

        //Shader
        void CreateShader(IGalMemory Memory, long Tag, GalShaderType Type);

        IEnumerable<ShaderDeclInfo> GetTextureUsage(long Tag);

        void SetConstBuffer(long Tag, int Cbuf, byte[] Data);

        void SetUniform1(string UniformName, int Value);

        void SetUniform2F(string UniformName, float X, float Y);

        void BindShader(long Tag);

        void BindProgram();

        //Texture
        void SetTextureAndSampler(long Tag, byte[] Data, GalTexture Texture, GalTextureSampler Sampler);

        bool TryGetCachedTexture(long Tag, long DataSize, out GalTexture Texture);

        void BindTexture(long Tag, int Index);
    }
}