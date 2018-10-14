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

        private static Dictionary<GalVertexAttribSize, VertexAttribPointerType> FloatAttribTypes =
                   new Dictionary<GalVertexAttribSize, VertexAttribPointerType>()
        {
            { GalVertexAttribSize._32_32_32_32, VertexAttribPointerType.Float     },
            { GalVertexAttribSize._32_32_32,    VertexAttribPointerType.Float     },
            { GalVertexAttribSize._16_16_16_16, VertexAttribPointerType.HalfFloat },
            { GalVertexAttribSize._32_32,       VertexAttribPointerType.Float     },
            { GalVertexAttribSize._16_16_16,    VertexAttribPointerType.HalfFloat },
            { GalVertexAttribSize._16_16,       VertexAttribPointerType.HalfFloat },
            { GalVertexAttribSize._32,          VertexAttribPointerType.Float     },
            { GalVertexAttribSize._16,          VertexAttribPointerType.HalfFloat }
        };

        private static Dictionary<GalVertexAttribSize, VertexAttribPointerType> SignedAttribTypes =
                   new Dictionary<GalVertexAttribSize, VertexAttribPointerType>()
        {
            { GalVertexAttribSize._32_32_32_32, VertexAttribPointerType.Int           },
            { GalVertexAttribSize._32_32_32,    VertexAttribPointerType.Int           },
            { GalVertexAttribSize._16_16_16_16, VertexAttribPointerType.Short         },
            { GalVertexAttribSize._32_32,       VertexAttribPointerType.Int           },
            { GalVertexAttribSize._16_16_16,    VertexAttribPointerType.Short         },
            { GalVertexAttribSize._8_8_8_8,     VertexAttribPointerType.Byte          },
            { GalVertexAttribSize._16_16,       VertexAttribPointerType.Short         },
            { GalVertexAttribSize._32,          VertexAttribPointerType.Int           },
            { GalVertexAttribSize._8_8_8,       VertexAttribPointerType.Byte          },
            { GalVertexAttribSize._8_8,         VertexAttribPointerType.Byte          },
            { GalVertexAttribSize._16,          VertexAttribPointerType.Short         },
            { GalVertexAttribSize._8,           VertexAttribPointerType.Byte          },
            { GalVertexAttribSize._10_10_10_2,  VertexAttribPointerType.Int2101010Rev }
        };

        private static Dictionary<GalVertexAttribSize, VertexAttribPointerType> UnsignedAttribTypes =
                   new Dictionary<GalVertexAttribSize, VertexAttribPointerType>()
        {
            { GalVertexAttribSize._32_32_32_32, VertexAttribPointerType.UnsignedInt             },
            { GalVertexAttribSize._32_32_32,    VertexAttribPointerType.UnsignedInt             },
            { GalVertexAttribSize._16_16_16_16, VertexAttribPointerType.UnsignedShort           },
            { GalVertexAttribSize._32_32,       VertexAttribPointerType.UnsignedInt             },
            { GalVertexAttribSize._16_16_16,    VertexAttribPointerType.UnsignedShort           },
            { GalVertexAttribSize._8_8_8_8,     VertexAttribPointerType.UnsignedByte            },
            { GalVertexAttribSize._16_16,       VertexAttribPointerType.UnsignedShort           },
            { GalVertexAttribSize._32,          VertexAttribPointerType.UnsignedInt             },
            { GalVertexAttribSize._8_8_8,       VertexAttribPointerType.UnsignedByte            },
            { GalVertexAttribSize._8_8,         VertexAttribPointerType.UnsignedByte            },
            { GalVertexAttribSize._16,          VertexAttribPointerType.UnsignedShort           },
            { GalVertexAttribSize._8,           VertexAttribPointerType.UnsignedByte            },
            { GalVertexAttribSize._10_10_10_2,  VertexAttribPointerType.UnsignedInt2101010Rev   },
            { GalVertexAttribSize._11_11_10,    VertexAttribPointerType.UnsignedInt10F11F11FRev }
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
                DepthWriteEnabled = true,
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

            if (New.FramebufferSrgb != Old.FramebufferSrgb)
            {
                Enable(EnableCap.FramebufferSrgb, New.FramebufferSrgb);
            }

            if (New.FlipX != Old.FlipX || New.FlipY != Old.FlipY || New.Instance != Old.Instance)
            {
                Shader.SetExtraData(New.FlipX, New.FlipY, New.Instance);
            }

            //Note: Uncomment SetFrontFace and SetCullFace when flipping issues are solved

            //if (New.FrontFace != Old.FrontFace)
            //{
            //    GL.FrontFace(OGLEnumConverter.GetFrontFace(New.FrontFace));
            //}

            //if (New.CullFaceEnabled != Old.CullFaceEnabled)
            //{
            //    Enable(EnableCap.CullFace, New.CullFaceEnabled);
            //}

            //if (New.CullFaceEnabled)
            //{
            //    if (New.CullFace != Old.CullFace)
            //    {
            //        GL.CullFace(OGLEnumConverter.GetCullFace(New.CullFace));
            //    }
            //}

            if (New.DepthTestEnabled != Old.DepthTestEnabled)
            {
                Enable(EnableCap.DepthTest, New.DepthTestEnabled);
            }

            if (New.DepthWriteEnabled != Old.DepthWriteEnabled)
            {
                Rasterizer.DepthWriteEnabled = New.DepthWriteEnabled;

                GL.DepthMask(New.DepthWriteEnabled);
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

            if (New.StencilTwoSideEnabled != Old.StencilTwoSideEnabled)
            {
                Enable((EnableCap)All.StencilTestTwoSideExt, New.StencilTwoSideEnabled);
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

            if (New.ColorMaskR != Old.ColorMaskR ||
                New.ColorMaskG != Old.ColorMaskG ||
                New.ColorMaskB != Old.ColorMaskB ||
                New.ColorMaskA != Old.ColorMaskA)
            {
                GL.ColorMask(
                    New.ColorMaskR,
                    New.ColorMaskG,
                    New.ColorMaskB,
                    New.ColorMaskA);
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
            int FreeBinding = OGLShader.ReservedCbufCount;

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
                    //Skip uninitialized attributes.
                    if (Attrib.Size == 0)
                    {
                        continue;
                    }

                    GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);

                    bool Unsigned =
                        Attrib.Type == GalVertexAttribType.Unorm ||
                        Attrib.Type == GalVertexAttribType.Uint  ||
                        Attrib.Type == GalVertexAttribType.Uscaled;

                    bool Normalize =
                        Attrib.Type == GalVertexAttribType.Snorm ||
                        Attrib.Type == GalVertexAttribType.Unorm;

                    VertexAttribPointerType Type = 0;

                    if (Attrib.Type == GalVertexAttribType.Float)
                    {
                        Type = GetType(FloatAttribTypes, Attrib);
                    }
                    else
                    {
                        if (Unsigned)
                        {
                            Type = GetType(UnsignedAttribTypes, Attrib);
                        }
                        else
                        {
                            Type = GetType(SignedAttribTypes, Attrib);
                        }
                    }

                    if (!AttribElements.TryGetValue(Attrib.Size, out int Size))
                    {
                        throw new InvalidOperationException("Invalid attribute size \"" + Attrib.Size + "\"!");
                    }

                    int Offset = Attrib.Offset;

                    if (Binding.Stride != 0)
                    {
                        GL.EnableVertexAttribArray(Attrib.Index);

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
                    else
                    {
                        GL.DisableVertexAttribArray(Attrib.Index);

                        SetConstAttrib(Attrib);
                    }

                    if (Binding.Instanced && Binding.Divisor != 0)
                    {
                        GL.VertexAttribDivisor(Attrib.Index, 1);
                    }
                    else
                    {
                        GL.VertexAttribDivisor(Attrib.Index, 0);
                    }
                }
            }
        }

        private static VertexAttribPointerType GetType(Dictionary<GalVertexAttribSize, VertexAttribPointerType> Dict, GalVertexAttrib Attrib)
        {
            if (!Dict.TryGetValue(Attrib.Size, out VertexAttribPointerType Type))
            {
                throw new NotImplementedException("Unsupported size \"" + Attrib.Size + "\" on type \"" + Attrib.Type + "\"!");
            }

            return Type;
        }

        private unsafe static void SetConstAttrib(GalVertexAttrib Attrib)
        {
            void Unsupported()
            {
                throw new NotImplementedException("Constant attribute " + Attrib.Size + " not implemented!");
            }

            if (Attrib.Size == GalVertexAttribSize._10_10_10_2 ||
                Attrib.Size == GalVertexAttribSize._11_11_10)
            {
                Unsupported();
            }

            if (Attrib.Type == GalVertexAttribType.Unorm)
            {
                switch (Attrib.Size)
                {
                    case GalVertexAttribSize._8:
                    case GalVertexAttribSize._8_8:
                    case GalVertexAttribSize._8_8_8:
                    case GalVertexAttribSize._8_8_8_8:
                        GL.VertexAttrib4N((uint)Attrib.Index, (byte*)Attrib.Pointer);
                        break;

                    case GalVertexAttribSize._16:
                    case GalVertexAttribSize._16_16:
                    case GalVertexAttribSize._16_16_16:
                    case GalVertexAttribSize._16_16_16_16:
                        GL.VertexAttrib4N((uint)Attrib.Index, (ushort*)Attrib.Pointer);
                        break;

                    case GalVertexAttribSize._32:
                    case GalVertexAttribSize._32_32:
                    case GalVertexAttribSize._32_32_32:
                    case GalVertexAttribSize._32_32_32_32:
                        GL.VertexAttrib4N((uint)Attrib.Index, (uint*)Attrib.Pointer);
                        break;
                }
            }
            else if (Attrib.Type == GalVertexAttribType.Snorm)
            {
                switch (Attrib.Size)
                {
                    case GalVertexAttribSize._8:
                    case GalVertexAttribSize._8_8:
                    case GalVertexAttribSize._8_8_8:
                    case GalVertexAttribSize._8_8_8_8:
                        GL.VertexAttrib4N((uint)Attrib.Index, (sbyte*)Attrib.Pointer);
                        break;

                    case GalVertexAttribSize._16:
                    case GalVertexAttribSize._16_16:
                    case GalVertexAttribSize._16_16_16:
                    case GalVertexAttribSize._16_16_16_16:
                        GL.VertexAttrib4N((uint)Attrib.Index, (short*)Attrib.Pointer);
                        break;

                    case GalVertexAttribSize._32:
                    case GalVertexAttribSize._32_32:
                    case GalVertexAttribSize._32_32_32:
                    case GalVertexAttribSize._32_32_32_32:
                        GL.VertexAttrib4N((uint)Attrib.Index, (int*)Attrib.Pointer);
                        break;
                }
            }
            else if (Attrib.Type == GalVertexAttribType.Uint)
            {
                switch (Attrib.Size)
                {
                    case GalVertexAttribSize._8:
                    case GalVertexAttribSize._8_8:
                    case GalVertexAttribSize._8_8_8:
                    case GalVertexAttribSize._8_8_8_8:
                        GL.VertexAttribI4((uint)Attrib.Index, (byte*)Attrib.Pointer);
                        break;

                    case GalVertexAttribSize._16:
                    case GalVertexAttribSize._16_16:
                    case GalVertexAttribSize._16_16_16:
                    case GalVertexAttribSize._16_16_16_16:
                        GL.VertexAttribI4((uint)Attrib.Index, (ushort*)Attrib.Pointer);
                        break;

                    case GalVertexAttribSize._32:
                    case GalVertexAttribSize._32_32:
                    case GalVertexAttribSize._32_32_32:
                    case GalVertexAttribSize._32_32_32_32:
                        GL.VertexAttribI4((uint)Attrib.Index, (uint*)Attrib.Pointer);
                        break;
                }
            }
            else if (Attrib.Type == GalVertexAttribType.Sint)
            {
                switch (Attrib.Size)
                {
                    case GalVertexAttribSize._8:
                    case GalVertexAttribSize._8_8:
                    case GalVertexAttribSize._8_8_8:
                    case GalVertexAttribSize._8_8_8_8:
                        GL.VertexAttribI4((uint)Attrib.Index, (sbyte*)Attrib.Pointer);
                        break;

                    case GalVertexAttribSize._16:
                    case GalVertexAttribSize._16_16:
                    case GalVertexAttribSize._16_16_16:
                    case GalVertexAttribSize._16_16_16_16:
                        GL.VertexAttribI4((uint)Attrib.Index, (short*)Attrib.Pointer);
                        break;

                    case GalVertexAttribSize._32:
                    case GalVertexAttribSize._32_32:
                    case GalVertexAttribSize._32_32_32:
                    case GalVertexAttribSize._32_32_32_32:
                        GL.VertexAttribI4((uint)Attrib.Index, (int*)Attrib.Pointer);
                        break;
                }
            }
            else if (Attrib.Type == GalVertexAttribType.Float)
            {
                switch (Attrib.Size)
                {
                    case GalVertexAttribSize._32:
                    case GalVertexAttribSize._32_32:
                    case GalVertexAttribSize._32_32_32:
                    case GalVertexAttribSize._32_32_32_32:
                        GL.VertexAttrib4(Attrib.Index, (float*)Attrib.Pointer);
                        break;

                    default: Unsupported(); break;
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