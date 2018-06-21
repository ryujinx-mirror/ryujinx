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
                OGLEnumConverter.GetBlendFactor(FuncSrc),
                OGLEnumConverter.GetBlendFactor(FuncDst));
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
                (BlendingFactorSrc)OGLEnumConverter.GetBlendFactor(FuncSrcRgb),
                (BlendingFactorDest)OGLEnumConverter.GetBlendFactor(FuncDstRgb),
                (BlendingFactorSrc)OGLEnumConverter.GetBlendFactor(FuncSrcAlpha),
                (BlendingFactorDest)OGLEnumConverter.GetBlendFactor(FuncDstAlpha));
        }
    }
}