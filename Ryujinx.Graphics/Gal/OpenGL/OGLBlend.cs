using OpenTK.Graphics.OpenGL;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLBlend
    {
        public void Enable()
        {
            GL.Enable(EnableCap.Blend);
        }

        public void Disable()
        {
            GL.Disable(EnableCap.Blend);
        }

        public void Set(
            GalBlendEquation Equation,
            GalBlendFactor   FuncSrc,
            GalBlendFactor   FuncDst)
        {
            GL.BlendEquation(
                OGLEnumConverter.GetBlendEquation(Equation));

            GL.BlendFunc(
                OGLEnumConverter.GetBlendFactorSrc(FuncSrc),
                OGLEnumConverter.GetBlendFactorDst(FuncDst));
        }

        public void SetSeparate(
            GalBlendEquation EquationRgb,
            GalBlendEquation EquationAlpha,
            GalBlendFactor   FuncSrcRgb,
            GalBlendFactor   FuncDstRgb,
            GalBlendFactor   FuncSrcAlpha,
            GalBlendFactor   FuncDstAlpha)
        {
            GL.BlendEquationSeparate(
                OGLEnumConverter.GetBlendEquation(EquationRgb),
                OGLEnumConverter.GetBlendEquation(EquationAlpha));

            GL.BlendFuncSeparate(
                OGLEnumConverter.GetBlendFactorSrc(FuncSrcRgb),
                OGLEnumConverter.GetBlendFactorDst(FuncDstRgb),
                OGLEnumConverter.GetBlendFactorSrc(FuncSrcAlpha),
                OGLEnumConverter.GetBlendFactorDst(FuncDstAlpha));
        }
    }
}