using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLPipeline : IGalPipeline
    {
        private static Dictionary<GalVertexAttribSize, int> AttribElements =
                   new Dictionary<GalVertexAttribSize, int>()
        {
            { GalVertexAttribSize._32_32_32_32, 4 },
            { GalVertexAttribSize._32_32_32,    3 },
            { GalVertexAttribSize._16_16_16_16, 4 },
            { GalVertexAttribSize._32_32,       2 },
            { GalVertexAttribSize._16_16_16,    3 },
            { GalVertexAttribSize._8_8_8_8,     4 },
            { GalVertexAttribSize._16_16,       2 },
            { GalVertexAttribSize._32,          1 },
            { GalVertexAttribSize._8_8_8,       3 },
            { GalVertexAttribSize._8_8,         2 },
            { GalVertexAttribSize._16,          1 },
            { GalVertexAttribSize._8,           1 },
            { GalVertexAttribSize._10_10_10_2,  4 },
            { GalVertexAttribSize._11_11_10,    3 }
        };

        private static Dictionary<GalVertexAttribSize, VertexAttribPointerType> AttribTypes =
                   new Dictionary<GalVertexAttribSize, VertexAttribPointerType>()
        {
            { GalVertexAttribSize._32_32_32_32, VertexAttribPointerType.Int   },
            { GalVertexAttribSize._32_32_32,    VertexAttribPointerType.Int   },
            { GalVertexAttribSize._16_16_16_16, VertexAttribPointerType.Short },
            { GalVertexAttribSize._32_32,       VertexAttribPointerType.Int   },
            { GalVertexAttribSize._16_16_16,    VertexAttribPointerType.Short },
            { GalVertexAttribSize._8_8_8_8,     VertexAttribPointerType.Byte  },
            { GalVertexAttribSize._16_16,       VertexAttribPointerType.Short },
            { GalVertexAttribSize._32,          VertexAttribPointerType.Int   },
            { GalVertexAttribSize._8_8_8,       VertexAttribPointerType.Byte  },
            { GalVertexAttribSize._8_8,         VertexAttribPointerType.Byte  },
            { GalVertexAttribSize._16,          VertexAttribPointerType.Short },
            { GalVertexAttribSize._8,           VertexAttribPointerType.Byte  },
            { GalVertexAttribSize._10_10_10_2,  VertexAttribPointerType.Int   }, //?
            { GalVertexAttribSize._11_11_10,    VertexAttribPointerType.Int   }  //?
        };

        private GalPipelineState Old;

        private OGLConstBuffer Buffer;
        private OGLRasterizer Rasterizer;
        private OGLShader Shader;

        private int VaoHandle;

        public OGLPipeline(OGLConstBuffer Buffer, OGLRasterizer Rasterizer, OGLShader Shader)
        {
            this.Buffer     = Buffer;
            this.Rasterizer = Rasterizer;
            this.Shader     = Shader;

            //These values match OpenGL's defaults
            Old = new GalPipelineState
            {
                FrontFace = GalFrontFace.CCW,

                CullFaceEnabled = false,
                CullFace = GalCullFace.Back,

                DepthTestEnabled = false,
                DepthFunc = GalComparisonOp.Less,

                StencilTestEnabled = false,

                StencilBackFuncFunc = GalComparisonOp.Always,
                StencilBackFuncRef = 0,
                StencilBackFuncMask = UInt32.MaxValue,
                StencilBackOpFail = GalStencilOp.Keep,
                StencilBackOpZFail = GalStencilOp.Keep,
                StencilBackOpZPass = GalStencilOp.Keep,
                StencilBackMask = UInt32.MaxValue,

                StencilFrontFuncFunc = GalComparisonOp.Always,
                StencilFrontFuncRef = 0,
                StencilFrontFuncMask = UInt32.MaxValue,
                StencilFrontOpFail = GalStencilOp.Keep,
                StencilFrontOpZFail = GalStencilOp.Keep,
                StencilFrontOpZPass = GalStencilOp.Keep,
                StencilFrontMask = UInt32.MaxValue,

                BlendEnabled = false,
                BlendSeparateAlpha = false,

                BlendEquationRgb = 0,
                BlendFuncSrcRgb = GalBlendFactor.One,
                BlendFuncDstRgb = GalBlendFactor.Zero,
                BlendEquationAlpha = 0,
                BlendFuncSrcAlpha = GalBlendFactor.One,
                BlendFuncDstAlpha = GalBlendFactor.Zero,

                PrimitiveRestartEnabled = false,
                PrimitiveRestartIndex = 0
            };
        }

        public void Bind(GalPipelineState New)
        {
            BindConstBuffers(New);

            BindVertexLayout(New);

            if (New.FlipX != Old.FlipX || New.FlipY != Old.FlipY)
            {
                Shader.SetFlip(New.FlipX, New.FlipY);
            }

            //Note: Uncomment SetFrontFace and SetCullFace when flipping issues are solved

            //if (New.FrontFace != O.FrontFace)
            //{
            //    GL.FrontFace(OGLEnumConverter.GetFrontFace(New.FrontFace));
            //}

            //if (New.CullFaceEnabled != O.CullFaceEnabled)
            //{
            //    Enable(EnableCap.CullFace, New.CullFaceEnabled);
            //}

            //if (New.CullFaceEnabled)
            //{
            //    if (New.CullFace != O.CullFace)
            //    {
            //        GL.CullFace(OGLEnumConverter.GetCullFace(New.CullFace));
            //    }
            //}

            if (New.DepthTestEnabled != Old.DepthTestEnabled)
            {
                Enable(EnableCap.DepthTest, New.DepthTestEnabled);
            }

            if (New.DepthTestEnabled)
            {
                if (New.DepthFunc != Old.DepthFunc)
                {
                    GL.DepthFunc(OGLEnumConverter.GetDepthFunc(New.DepthFunc));
                }
            }

            if (New.StencilTestEnabled != Old.StencilTestEnabled)
            {
                Enable(EnableCap.StencilTest, New.StencilTestEnabled);
            }

            if (New.StencilTestEnabled)
            {
                if (New.StencilBackFuncFunc != Old.StencilBackFuncFunc ||
                    New.StencilBackFuncRef  != Old.StencilBackFuncRef  ||
                    New.StencilBackFuncMask != Old.StencilBackFuncMask)
                {
                    GL.StencilFuncSeparate(
                        StencilFace.Back,
                        OGLEnumConverter.GetStencilFunc(New.StencilBackFuncFunc),
                        New.StencilBackFuncRef,
                        New.StencilBackFuncMask);
                }

                if (New.StencilBackOpFail  != Old.StencilBackOpFail  ||
                    New.StencilBackOpZFail != Old.StencilBackOpZFail ||
                    New.StencilBackOpZPass != Old.StencilBackOpZPass)
                {
                    GL.StencilOpSeparate(
                        StencilFace.Back,
                        OGLEnumConverter.GetStencilOp(New.StencilBackOpFail),
                        OGLEnumConverter.GetStencilOp(New.StencilBackOpZFail),
                        OGLEnumConverter.GetStencilOp(New.StencilBackOpZPass));
                }

                if (New.StencilBackMask != Old.StencilBackMask)
                {
                    GL.StencilMaskSeparate(StencilFace.Back, New.StencilBackMask);
                }

                if (New.StencilFrontFuncFunc != Old.StencilFrontFuncFunc ||
                    New.StencilFrontFuncRef  != Old.StencilFrontFuncRef  ||
                    New.StencilFrontFuncMask != Old.StencilFrontFuncMask)
                {
                    GL.StencilFuncSeparate(
                        StencilFace.Front,
                        OGLEnumConverter.GetStencilFunc(New.StencilFrontFuncFunc),
                        New.StencilFrontFuncRef,
                        New.StencilFrontFuncMask);
                }

                if (New.StencilFrontOpFail  != Old.StencilFrontOpFail  ||
                    New.StencilFrontOpZFail != Old.StencilFrontOpZFail ||
                    New.StencilFrontOpZPass != Old.StencilFrontOpZPass)
                {
                    GL.StencilOpSeparate(
                        StencilFace.Front,
                        OGLEnumConverter.GetStencilOp(New.StencilFrontOpFail),
                        OGLEnumConverter.GetStencilOp(New.StencilFrontOpZFail),
                        OGLEnumConverter.GetStencilOp(New.StencilFrontOpZPass));
                }

                if (New.StencilFrontMask != Old.StencilFrontMask)
                {
                    GL.StencilMaskSeparate(StencilFace.Front, New.StencilFrontMask);
                }
            }

            if (New.BlendEnabled != Old.BlendEnabled)
            {
                Enable(EnableCap.Blend, New.BlendEnabled);
            }

            if (New.BlendEnabled)
            {
                if (New.BlendSeparateAlpha)
                {
                    if (New.BlendEquationRgb   != Old.BlendEquationRgb ||
                        New.BlendEquationAlpha != Old.BlendEquationAlpha)
                    {
                        GL.BlendEquationSeparate(
                            OGLEnumConverter.GetBlendEquation(New.BlendEquationRgb),
                            OGLEnumConverter.GetBlendEquation(New.BlendEquationAlpha));
                    }

                    if (New.BlendFuncSrcRgb   != Old.BlendFuncSrcRgb   ||
                        New.BlendFuncDstRgb   != Old.BlendFuncDstRgb   ||
                        New.BlendFuncSrcAlpha != Old.BlendFuncSrcAlpha ||
                        New.BlendFuncDstAlpha != Old.BlendFuncDstAlpha)
                    {
                        GL.BlendFuncSeparate(
                            (BlendingFactorSrc) OGLEnumConverter.GetBlendFactor(New.BlendFuncSrcRgb),
                            (BlendingFactorDest)OGLEnumConverter.GetBlendFactor(New.BlendFuncDstRgb),
                            (BlendingFactorSrc) OGLEnumConverter.GetBlendFactor(New.BlendFuncSrcAlpha),
                            (BlendingFactorDest)OGLEnumConverter.GetBlendFactor(New.BlendFuncDstAlpha));
                    }
                }
                else
                {
                    if (New.BlendEquationRgb != Old.BlendEquationRgb)
                    {
                        GL.BlendEquation(OGLEnumConverter.GetBlendEquation(New.BlendEquationRgb));
                    }

                    if (New.BlendFuncSrcRgb != Old.BlendFuncSrcRgb ||
                        New.BlendFuncDstRgb != Old.BlendFuncDstRgb)
                    {
                        GL.BlendFunc(
                            OGLEnumConverter.GetBlendFactor(New.BlendFuncSrcRgb),
                            OGLEnumConverter.GetBlendFactor(New.BlendFuncDstRgb));
                    }
                }
            }

            if (New.PrimitiveRestartEnabled != Old.PrimitiveRestartEnabled)
            {
                Enable(EnableCap.PrimitiveRestart, New.PrimitiveRestartEnabled);
            }

            if (New.PrimitiveRestartEnabled)
            {
                if (New.PrimitiveRestartIndex != Old.PrimitiveRestartIndex)
                {
                    GL.PrimitiveRestartIndex(New.PrimitiveRestartIndex);
                }
            }

            Old = New;
        }

        private void BindConstBuffers(GalPipelineState New)
        {
            //Index 0 is reserved
            int FreeBinding = 1;

            void BindIfNotNull(OGLShaderStage Stage)
            {
                if (Stage != null)
                {
                    foreach (ShaderDeclInfo DeclInfo in Stage.ConstBufferUsage)
                    {
                        long Key = New.ConstBufferKeys[(int)Stage.Type][DeclInfo.Cbuf];

                        if (Key != 0 && Buffer.TryGetUbo(Key, out int UboHandle))
                        {
                            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, FreeBinding, UboHandle);
                        }

                        FreeBinding++;
                    }
                }
            }

            BindIfNotNull(Shader.Current.Vertex);
            BindIfNotNull(Shader.Current.TessControl);
            BindIfNotNull(Shader.Current.TessEvaluation);
            BindIfNotNull(Shader.Current.Geometry);
            BindIfNotNull(Shader.Current.Fragment);
        }

        private void BindVertexLayout(GalPipelineState New)
        {
            foreach (GalVertexBinding Binding in New.VertexBindings)
            {
                if (!Binding.Enabled || !Rasterizer.TryGetVbo(Binding.VboKey, out int VboHandle))
                {
                    continue;
                }

                if (VaoHandle == 0)
                {
                    VaoHandle = GL.GenVertexArray();

                    //Vertex arrays shouldn't be used anywhere else in OpenGL's backend
                    //if you want to use it, move this line out of the if
                    GL.BindVertexArray(VaoHandle);
                }

                foreach (GalVertexAttrib Attrib in Binding.Attribs)
                {
                    GL.EnableVertexAttribArray(Attrib.Index);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);

                    bool Unsigned =
                        Attrib.Type == GalVertexAttribType.Unorm ||
                        Attrib.Type == GalVertexAttribType.Uint ||
                        Attrib.Type == GalVertexAttribType.Uscaled;

                    bool Normalize =
                        Attrib.Type == GalVertexAttribType.Snorm ||
                        Attrib.Type == GalVertexAttribType.Unorm;

                    VertexAttribPointerType Type = 0;

                    if (Attrib.Type == GalVertexAttribType.Float)
                    {
                        Type = VertexAttribPointerType.Float;
                    }
                    else
                    {
                        Type = AttribTypes[Attrib.Size] + (Unsigned ? 1 : 0);
                    }

                    int Size = AttribElements[Attrib.Size];
                    int Offset = Attrib.Offset;

                    if (Attrib.Type == GalVertexAttribType.Sint ||
                        Attrib.Type == GalVertexAttribType.Uint)
                    {
                        IntPtr Pointer = new IntPtr(Offset);

                        VertexAttribIntegerType IType = (VertexAttribIntegerType)Type;

                        GL.VertexAttribIPointer(Attrib.Index, Size, IType, Binding.Stride, Pointer);
                    }
                    else
                    {
                        GL.VertexAttribPointer(Attrib.Index, Size, Type, Normalize, Binding.Stride, Offset);
                    }
                }
            }
        }

        private void Enable(EnableCap Cap, bool Enabled)
        {
            if (Enabled)
            {
                GL.Enable(Cap);
            }
            else
            {
                GL.Disable(Cap);
            }
        }
    }
}