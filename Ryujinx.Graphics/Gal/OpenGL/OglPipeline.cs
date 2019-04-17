using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.Shader;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OglPipeline : IGalPipeline
    {
        private static Dictionary<GalVertexAttribSize, int> _attribElements =
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

        private static Dictionary<GalVertexAttribSize, VertexAttribPointerType> _floatAttribTypes =
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

        private static Dictionary<GalVertexAttribSize, VertexAttribPointerType> _signedAttribTypes =
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

        private static Dictionary<GalVertexAttribSize, VertexAttribPointerType> _unsignedAttribTypes =
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

        private GalPipelineState _old;

        private OglConstBuffer  _buffer;
        private OglRenderTarget _renderTarget;
        private OglRasterizer   _rasterizer;
        private OglShader       _shader;

        private int _vaoHandle;

        public OglPipeline(
            OglConstBuffer  buffer,
            OglRenderTarget renderTarget,
            OglRasterizer   rasterizer,
            OglShader       shader)
        {
            _buffer       = buffer;
            _renderTarget = renderTarget;
            _rasterizer   = rasterizer;
            _shader       = shader;

            //These values match OpenGL's defaults
            _old = new GalPipelineState
            {
                FrontFace = GalFrontFace.Ccw,

                CullFaceEnabled = false,
                CullFace        = GalCullFace.Back,

                DepthTestEnabled  = false,
                DepthWriteEnabled = true,
                DepthFunc         = GalComparisonOp.Less,
                DepthRangeNear    = 0,
                DepthRangeFar     = 1,

                StencilTestEnabled = false,

                StencilBackFuncFunc = GalComparisonOp.Always,
                StencilBackFuncRef  = 0,
                StencilBackFuncMask = UInt32.MaxValue,
                StencilBackOpFail   = GalStencilOp.Keep,
                StencilBackOpZFail  = GalStencilOp.Keep,
                StencilBackOpZPass  = GalStencilOp.Keep,
                StencilBackMask     = UInt32.MaxValue,

                StencilFrontFuncFunc = GalComparisonOp.Always,
                StencilFrontFuncRef  = 0,
                StencilFrontFuncMask = UInt32.MaxValue,
                StencilFrontOpFail   = GalStencilOp.Keep,
                StencilFrontOpZFail  = GalStencilOp.Keep,
                StencilFrontOpZPass  = GalStencilOp.Keep,
                StencilFrontMask     = UInt32.MaxValue,

                BlendIndependent = false,

                PrimitiveRestartEnabled = false,
                PrimitiveRestartIndex   = 0
            };

            for (int index = 0; index < GalPipelineState.RenderTargetsCount; index++)
            {
                _old.Blends[index] = BlendState.Default;

                _old.ColorMasks[index] = ColorMaskState.Default;
            }
        }

        public void Bind(GalPipelineState New)
        {
            BindConstBuffers(New);

            BindVertexLayout(New);

            if (New.FramebufferSrgb != _old.FramebufferSrgb)
            {
                Enable(EnableCap.FramebufferSrgb, New.FramebufferSrgb);

                _renderTarget.FramebufferSrgb = New.FramebufferSrgb;
            }

            if (New.FlipX != _old.FlipX || New.FlipY != _old.FlipY || New.Instance != _old.Instance)
            {
                _shader.SetExtraData(New.FlipX, New.FlipY, New.Instance);
            }

            if (New.FrontFace != _old.FrontFace)
            {
                GL.FrontFace(OglEnumConverter.GetFrontFace(New.FrontFace));
            }

            if (New.CullFaceEnabled != _old.CullFaceEnabled)
            {
                Enable(EnableCap.CullFace, New.CullFaceEnabled);
            }

            if (New.CullFaceEnabled)
            {
                if (New.CullFace != _old.CullFace)
                {
                    GL.CullFace(OglEnumConverter.GetCullFace(New.CullFace));
                }
            }

            if (New.DepthTestEnabled != _old.DepthTestEnabled)
            {
                Enable(EnableCap.DepthTest, New.DepthTestEnabled);
            }

            if (New.DepthWriteEnabled != _old.DepthWriteEnabled)
            {
                GL.DepthMask(New.DepthWriteEnabled);
            }

            if (New.DepthTestEnabled)
            {
                if (New.DepthFunc != _old.DepthFunc)
                {
                    GL.DepthFunc(OglEnumConverter.GetDepthFunc(New.DepthFunc));
                }
            }

            if (New.DepthRangeNear != _old.DepthRangeNear ||
                New.DepthRangeFar  != _old.DepthRangeFar)
            {
                GL.DepthRange(New.DepthRangeNear, New.DepthRangeFar);
            }

            if (New.StencilTestEnabled != _old.StencilTestEnabled)
            {
                Enable(EnableCap.StencilTest, New.StencilTestEnabled);
            }

            if (New.StencilTwoSideEnabled != _old.StencilTwoSideEnabled)
            {
                Enable((EnableCap)All.StencilTestTwoSideExt, New.StencilTwoSideEnabled);
            }

            if (New.StencilTestEnabled)
            {
                if (New.StencilBackFuncFunc != _old.StencilBackFuncFunc ||
                    New.StencilBackFuncRef  != _old.StencilBackFuncRef  ||
                    New.StencilBackFuncMask != _old.StencilBackFuncMask)
                {
                    GL.StencilFuncSeparate(
                        StencilFace.Back,
                        OglEnumConverter.GetStencilFunc(New.StencilBackFuncFunc),
                        New.StencilBackFuncRef,
                        New.StencilBackFuncMask);
                }

                if (New.StencilBackOpFail  != _old.StencilBackOpFail  ||
                    New.StencilBackOpZFail != _old.StencilBackOpZFail ||
                    New.StencilBackOpZPass != _old.StencilBackOpZPass)
                {
                    GL.StencilOpSeparate(
                        StencilFace.Back,
                        OglEnumConverter.GetStencilOp(New.StencilBackOpFail),
                        OglEnumConverter.GetStencilOp(New.StencilBackOpZFail),
                        OglEnumConverter.GetStencilOp(New.StencilBackOpZPass));
                }

                if (New.StencilBackMask != _old.StencilBackMask)
                {
                    GL.StencilMaskSeparate(StencilFace.Back, New.StencilBackMask);
                }

                if (New.StencilFrontFuncFunc != _old.StencilFrontFuncFunc ||
                    New.StencilFrontFuncRef  != _old.StencilFrontFuncRef  ||
                    New.StencilFrontFuncMask != _old.StencilFrontFuncMask)
                {
                    GL.StencilFuncSeparate(
                        StencilFace.Front,
                        OglEnumConverter.GetStencilFunc(New.StencilFrontFuncFunc),
                        New.StencilFrontFuncRef,
                        New.StencilFrontFuncMask);
                }

                if (New.StencilFrontOpFail  != _old.StencilFrontOpFail  ||
                    New.StencilFrontOpZFail != _old.StencilFrontOpZFail ||
                    New.StencilFrontOpZPass != _old.StencilFrontOpZPass)
                {
                    GL.StencilOpSeparate(
                        StencilFace.Front,
                        OglEnumConverter.GetStencilOp(New.StencilFrontOpFail),
                        OglEnumConverter.GetStencilOp(New.StencilFrontOpZFail),
                        OglEnumConverter.GetStencilOp(New.StencilFrontOpZPass));
                }

                if (New.StencilFrontMask != _old.StencilFrontMask)
                {
                    GL.StencilMaskSeparate(StencilFace.Front, New.StencilFrontMask);
                }
            }


            // Scissor Test
            // All scissor test are disabled before drawing final framebuffer to screen so we don't need to handle disabling
            // Skip if there are no scissor tests to enable
            if (New.ScissorTestCount != 0)
            {
                int  scissorsApplied = 0;
                bool applyToAll      = false;

                for (int index = 0; index < GalPipelineState.RenderTargetsCount; index++)
                {
                    if (New.ScissorTestEnabled[index])
                    {
                        // If viewport arrays are unavailable apply first scissor test to all or
                        // there is only 1 scissor test and it's the first, the scissor test applies to all viewports
                        if (!OglExtension.Required.ViewportArray || (index == 0 && New.ScissorTestCount == 1))
                        {
                            GL.Enable(EnableCap.ScissorTest);
                            applyToAll = true;
                        }
                        else
                        {
                            GL.Enable(IndexedEnableCap.ScissorTest, index);
                        }

                        if (New.ScissorTestEnabled[index] != _old.ScissorTestEnabled[index] ||
                            New.ScissorTestX[index]       != _old.ScissorTestX[index]       ||
                            New.ScissorTestY[index]       != _old.ScissorTestY[index]       ||
                            New.ScissorTestWidth[index]   != _old.ScissorTestWidth[index]   ||
                            New.ScissorTestHeight[index]  != _old.ScissorTestHeight[index])
                        {
                            if (applyToAll)
                            {
                                GL.Scissor(New.ScissorTestX[index],     New.ScissorTestY[index],
                                           New.ScissorTestWidth[index], New.ScissorTestHeight[index]);
                            }
                            else
                            {
                                GL.ScissorIndexed(index, New.ScissorTestX[index],     New.ScissorTestY[index],
                                                         New.ScissorTestWidth[index], New.ScissorTestHeight[index]);
                            }
                        }

                        // If all scissor tests have been applied, or viewport arrays are unavailable we can skip remaining iterations
                        if (!OglExtension.Required.ViewportArray || ++scissorsApplied == New.ScissorTestCount)
                        {
                            break;
                        }
                    }
                }
            }


            if (New.BlendIndependent)
            {
                for (int index = 0; index < GalPipelineState.RenderTargetsCount; index++)
                {
                    SetBlendState(index, New.Blends[index], _old.Blends[index]);
                }
            }
            else
            {
                if (New.BlendIndependent != _old.BlendIndependent)
                {
                    SetAllBlendState(New.Blends[0]);
                }
                else
                {
                    SetBlendState(New.Blends[0], _old.Blends[0]);
                }
            }

            if (New.ColorMaskCommon)
            {
                if (New.ColorMaskCommon != _old.ColorMaskCommon || !New.ColorMasks[0].Equals(_old.ColorMasks[0]))
                {
                    GL.ColorMask(
                        New.ColorMasks[0].Red,
                        New.ColorMasks[0].Green,
                        New.ColorMasks[0].Blue,
                        New.ColorMasks[0].Alpha);
                }
            }
            else
            {
                for (int index = 0; index < GalPipelineState.RenderTargetsCount; index++)
                {
                    if (!New.ColorMasks[index].Equals(_old.ColorMasks[index]))
                    {
                        GL.ColorMask(
                            index,
                            New.ColorMasks[index].Red,
                            New.ColorMasks[index].Green,
                            New.ColorMasks[index].Blue,
                            New.ColorMasks[index].Alpha);
                    }
                }
            }

            if (New.PrimitiveRestartEnabled != _old.PrimitiveRestartEnabled)
            {
                Enable(EnableCap.PrimitiveRestart, New.PrimitiveRestartEnabled);
            }

            if (New.PrimitiveRestartEnabled)
            {
                if (New.PrimitiveRestartIndex != _old.PrimitiveRestartIndex)
                {
                    GL.PrimitiveRestartIndex(New.PrimitiveRestartIndex);
                }
            }

            _old = New;
        }

        public void Unbind(GalPipelineState state)
        {
            if (state.ScissorTestCount > 0)
            {
                GL.Disable(EnableCap.ScissorTest);
            }
        }

        private void SetAllBlendState(BlendState New)
        {
            Enable(EnableCap.Blend, New.Enabled);

            if (New.Enabled)
            {
                if (New.SeparateAlpha)
                {
                    GL.BlendEquationSeparate(
                        OglEnumConverter.GetBlendEquation(New.EquationRgb),
                        OglEnumConverter.GetBlendEquation(New.EquationAlpha));

                    GL.BlendFuncSeparate(
                        (BlendingFactorSrc) OglEnumConverter.GetBlendFactor(New.FuncSrcRgb),
                        (BlendingFactorDest)OglEnumConverter.GetBlendFactor(New.FuncDstRgb),
                        (BlendingFactorSrc) OglEnumConverter.GetBlendFactor(New.FuncSrcAlpha),
                        (BlendingFactorDest)OglEnumConverter.GetBlendFactor(New.FuncDstAlpha));
                }
                else
                {
                    GL.BlendEquation(OglEnumConverter.GetBlendEquation(New.EquationRgb));

                    GL.BlendFunc(
                        OglEnumConverter.GetBlendFactor(New.FuncSrcRgb),
                        OglEnumConverter.GetBlendFactor(New.FuncDstRgb));
                }
            }
        }

        private void SetBlendState(BlendState New, BlendState old)
        {
            if (New.Enabled != old.Enabled)
            {
                Enable(EnableCap.Blend, New.Enabled);
            }

            if (New.Enabled)
            {
                if (New.SeparateAlpha)
                {
                    if (New.EquationRgb   != old.EquationRgb ||
                        New.EquationAlpha != old.EquationAlpha)
                    {
                        GL.BlendEquationSeparate(
                            OglEnumConverter.GetBlendEquation(New.EquationRgb),
                            OglEnumConverter.GetBlendEquation(New.EquationAlpha));
                    }

                    if (New.FuncSrcRgb   != old.FuncSrcRgb   ||
                        New.FuncDstRgb   != old.FuncDstRgb   ||
                        New.FuncSrcAlpha != old.FuncSrcAlpha ||
                        New.FuncDstAlpha != old.FuncDstAlpha)
                    {
                        GL.BlendFuncSeparate(
                            (BlendingFactorSrc) OglEnumConverter.GetBlendFactor(New.FuncSrcRgb),
                            (BlendingFactorDest)OglEnumConverter.GetBlendFactor(New.FuncDstRgb),
                            (BlendingFactorSrc) OglEnumConverter.GetBlendFactor(New.FuncSrcAlpha),
                            (BlendingFactorDest)OglEnumConverter.GetBlendFactor(New.FuncDstAlpha));
                    }
                }
                else
                {
                    if (New.EquationRgb != old.EquationRgb)
                    {
                        GL.BlendEquation(OglEnumConverter.GetBlendEquation(New.EquationRgb));
                    }

                    if (New.FuncSrcRgb != old.FuncSrcRgb ||
                        New.FuncDstRgb != old.FuncDstRgb)
                    {
                        GL.BlendFunc(
                            OglEnumConverter.GetBlendFactor(New.FuncSrcRgb),
                            OglEnumConverter.GetBlendFactor(New.FuncDstRgb));
                    }
                }
            }
        }

        private void SetBlendState(int index, BlendState New, BlendState old)
        {
            if (New.Enabled != old.Enabled)
            {
                Enable(IndexedEnableCap.Blend, index, New.Enabled);
            }

            if (New.Enabled)
            {
                if (New.SeparateAlpha)
                {
                    if (New.EquationRgb   != old.EquationRgb ||
                        New.EquationAlpha != old.EquationAlpha)
                    {
                        GL.BlendEquationSeparate(
                            index,
                            OglEnumConverter.GetBlendEquation(New.EquationRgb),
                            OglEnumConverter.GetBlendEquation(New.EquationAlpha));
                    }

                    if (New.FuncSrcRgb   != old.FuncSrcRgb   ||
                        New.FuncDstRgb   != old.FuncDstRgb   ||
                        New.FuncSrcAlpha != old.FuncSrcAlpha ||
                        New.FuncDstAlpha != old.FuncDstAlpha)
                    {
                        GL.BlendFuncSeparate(
                            index,
                            (BlendingFactorSrc) OglEnumConverter.GetBlendFactor(New.FuncSrcRgb),
                            (BlendingFactorDest)OglEnumConverter.GetBlendFactor(New.FuncDstRgb),
                            (BlendingFactorSrc) OglEnumConverter.GetBlendFactor(New.FuncSrcAlpha),
                            (BlendingFactorDest)OglEnumConverter.GetBlendFactor(New.FuncDstAlpha));
                    }
                }
                else
                {
                    if (New.EquationRgb != old.EquationRgb)
                    {
                        GL.BlendEquation(index, OglEnumConverter.GetBlendEquation(New.EquationRgb));
                    }

                    if (New.FuncSrcRgb != old.FuncSrcRgb ||
                        New.FuncDstRgb != old.FuncDstRgb)
                    {
                        GL.BlendFunc(
                            index,
                            (BlendingFactorSrc) OglEnumConverter.GetBlendFactor(New.FuncSrcRgb),
                            (BlendingFactorDest)OglEnumConverter.GetBlendFactor(New.FuncDstRgb));
                    }
                }
            }
        }

        private void BindConstBuffers(GalPipelineState New)
        {
            int freeBinding = OglShader.ReservedCbufCount;

            void BindIfNotNull(OglShaderStage stage)
            {
                if (stage != null)
                {
                    foreach (CBufferDescriptor desc in stage.ConstBufferUsage)
                    {
                        long key = New.ConstBufferKeys[(int)stage.Type][desc.Slot];

                        if (key != 0 && _buffer.TryGetUbo(key, out int uboHandle))
                        {
                            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, freeBinding, uboHandle);
                        }

                        freeBinding++;
                    }
                }
            }

            BindIfNotNull(_shader.Current.Vertex);
            BindIfNotNull(_shader.Current.TessControl);
            BindIfNotNull(_shader.Current.TessEvaluation);
            BindIfNotNull(_shader.Current.Geometry);
            BindIfNotNull(_shader.Current.Fragment);
        }

        private void BindVertexLayout(GalPipelineState New)
        {
            foreach (GalVertexBinding binding in New.VertexBindings)
            {
                if (!binding.Enabled || !_rasterizer.TryGetVbo(binding.VboKey, out int vboHandle))
                {
                    continue;
                }

                if (_vaoHandle == 0)
                {
                    _vaoHandle = GL.GenVertexArray();

                    //Vertex arrays shouldn't be used anywhere else in OpenGL's backend
                    //if you want to use it, move this line out of the if
                    GL.BindVertexArray(_vaoHandle);
                }

                foreach (GalVertexAttrib attrib in binding.Attribs)
                {
                    //Skip uninitialized attributes.
                    if (attrib.Size == 0)
                    {
                        continue;
                    }

                    GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);

                    bool unsigned =
                        attrib.Type == GalVertexAttribType.Unorm ||
                        attrib.Type == GalVertexAttribType.Uint  ||
                        attrib.Type == GalVertexAttribType.Uscaled;

                    bool normalize =
                        attrib.Type == GalVertexAttribType.Snorm ||
                        attrib.Type == GalVertexAttribType.Unorm;

                    VertexAttribPointerType type = 0;

                    if (attrib.Type == GalVertexAttribType.Float)
                    {
                        type = GetType(_floatAttribTypes, attrib);
                    }
                    else
                    {
                        if (unsigned)
                        {
                            type = GetType(_unsignedAttribTypes, attrib);
                        }
                        else
                        {
                            type = GetType(_signedAttribTypes, attrib);
                        }
                    }

                    if (!_attribElements.TryGetValue(attrib.Size, out int size))
                    {
                        throw new InvalidOperationException("Invalid attribute size \"" + attrib.Size + "\"!");
                    }

                    int offset = attrib.Offset;

                    if (binding.Stride != 0)
                    {
                        GL.EnableVertexAttribArray(attrib.Index);

                        if (attrib.Type == GalVertexAttribType.Sint ||
                            attrib.Type == GalVertexAttribType.Uint)
                        {
                            IntPtr pointer = new IntPtr(offset);

                            VertexAttribIntegerType iType = (VertexAttribIntegerType)type;

                            GL.VertexAttribIPointer(attrib.Index, size, iType, binding.Stride, pointer);
                        }
                        else
                        {
                            GL.VertexAttribPointer(attrib.Index, size, type, normalize, binding.Stride, offset);
                        }
                    }
                    else
                    {
                        GL.DisableVertexAttribArray(attrib.Index);

                        SetConstAttrib(attrib);
                    }

                    if (binding.Instanced && binding.Divisor != 0)
                    {
                        GL.VertexAttribDivisor(attrib.Index, 1);
                    }
                    else
                    {
                        GL.VertexAttribDivisor(attrib.Index, 0);
                    }
                }
            }
        }

        private static VertexAttribPointerType GetType(Dictionary<GalVertexAttribSize, VertexAttribPointerType> dict, GalVertexAttrib attrib)
        {
            if (!dict.TryGetValue(attrib.Size, out VertexAttribPointerType type))
            {
                ThrowUnsupportedAttrib(attrib);
            }

            return type;
        }

        private static unsafe void SetConstAttrib(GalVertexAttrib attrib)
        {
            if (attrib.Size == GalVertexAttribSize._10_10_10_2 ||
                attrib.Size == GalVertexAttribSize._11_11_10)
            {
                ThrowUnsupportedAttrib(attrib);
            }

            fixed (byte* ptr = attrib.Data)
            {
                if (attrib.Type == GalVertexAttribType.Unorm)
                {
                    switch (attrib.Size)
                    {
                        case GalVertexAttribSize._8:
                        case GalVertexAttribSize._8_8:
                        case GalVertexAttribSize._8_8_8:
                        case GalVertexAttribSize._8_8_8_8:
                            GL.VertexAttrib4N((uint)attrib.Index, ptr);
                            break;

                        case GalVertexAttribSize._16:
                        case GalVertexAttribSize._16_16:
                        case GalVertexAttribSize._16_16_16:
                        case GalVertexAttribSize._16_16_16_16:
                            GL.VertexAttrib4N((uint)attrib.Index, (ushort*)ptr);
                            break;

                        case GalVertexAttribSize._32:
                        case GalVertexAttribSize._32_32:
                        case GalVertexAttribSize._32_32_32:
                        case GalVertexAttribSize._32_32_32_32:
                            GL.VertexAttrib4N((uint)attrib.Index, (uint*)ptr);
                            break;
                    }
                }
                else if (attrib.Type == GalVertexAttribType.Snorm)
                {
                    switch (attrib.Size)
                    {
                        case GalVertexAttribSize._8:
                        case GalVertexAttribSize._8_8:
                        case GalVertexAttribSize._8_8_8:
                        case GalVertexAttribSize._8_8_8_8:
                            GL.VertexAttrib4N((uint)attrib.Index, (sbyte*)ptr);
                            break;

                        case GalVertexAttribSize._16:
                        case GalVertexAttribSize._16_16:
                        case GalVertexAttribSize._16_16_16:
                        case GalVertexAttribSize._16_16_16_16:
                            GL.VertexAttrib4N((uint)attrib.Index, (short*)ptr);
                            break;

                        case GalVertexAttribSize._32:
                        case GalVertexAttribSize._32_32:
                        case GalVertexAttribSize._32_32_32:
                        case GalVertexAttribSize._32_32_32_32:
                            GL.VertexAttrib4N((uint)attrib.Index, (int*)ptr);
                            break;
                    }
                }
                else if (attrib.Type == GalVertexAttribType.Uint)
                {
                    switch (attrib.Size)
                    {
                        case GalVertexAttribSize._8:
                        case GalVertexAttribSize._8_8:
                        case GalVertexAttribSize._8_8_8:
                        case GalVertexAttribSize._8_8_8_8:
                            GL.VertexAttribI4((uint)attrib.Index, ptr);
                            break;

                        case GalVertexAttribSize._16:
                        case GalVertexAttribSize._16_16:
                        case GalVertexAttribSize._16_16_16:
                        case GalVertexAttribSize._16_16_16_16:
                            GL.VertexAttribI4((uint)attrib.Index, (ushort*)ptr);
                            break;

                        case GalVertexAttribSize._32:
                        case GalVertexAttribSize._32_32:
                        case GalVertexAttribSize._32_32_32:
                        case GalVertexAttribSize._32_32_32_32:
                            GL.VertexAttribI4((uint)attrib.Index, (uint*)ptr);
                            break;
                    }
                }
                else if (attrib.Type == GalVertexAttribType.Sint)
                {
                    switch (attrib.Size)
                    {
                        case GalVertexAttribSize._8:
                        case GalVertexAttribSize._8_8:
                        case GalVertexAttribSize._8_8_8:
                        case GalVertexAttribSize._8_8_8_8:
                            GL.VertexAttribI4((uint)attrib.Index, (sbyte*)ptr);
                            break;

                        case GalVertexAttribSize._16:
                        case GalVertexAttribSize._16_16:
                        case GalVertexAttribSize._16_16_16:
                        case GalVertexAttribSize._16_16_16_16:
                            GL.VertexAttribI4((uint)attrib.Index, (short*)ptr);
                            break;

                        case GalVertexAttribSize._32:
                        case GalVertexAttribSize._32_32:
                        case GalVertexAttribSize._32_32_32:
                        case GalVertexAttribSize._32_32_32_32:
                            GL.VertexAttribI4((uint)attrib.Index, (int*)ptr);
                            break;
                    }
                }
                else if (attrib.Type == GalVertexAttribType.Float)
                {
                    switch (attrib.Size)
                    {
                        case GalVertexAttribSize._32:
                        case GalVertexAttribSize._32_32:
                        case GalVertexAttribSize._32_32_32:
                        case GalVertexAttribSize._32_32_32_32:
                            GL.VertexAttrib4(attrib.Index, (float*)ptr);
                            break;

                        default: ThrowUnsupportedAttrib(attrib); break;
                    }
                }
            }
        }

        private static void ThrowUnsupportedAttrib(GalVertexAttrib attrib)
        {
            throw new NotImplementedException("Unsupported size \"" + attrib.Size + "\" on type \"" + attrib.Type + "\"!");
        }

        private void Enable(EnableCap cap, bool enabled)
        {
            if (enabled)
            {
                GL.Enable(cap);
            }
            else
            {
                GL.Disable(cap);
            }
        }

        private void Enable(IndexedEnableCap cap, int index, bool enabled)
        {
            if (enabled)
            {
                GL.Enable(cap, index);
            }
            else
            {
                GL.Disable(cap, index);
            }
        }

        public void ResetDepthMask()
        {
            _old.DepthWriteEnabled = true;
        }

        public void ResetColorMask(int index)
        {
            _old.ColorMasks[index] = ColorMaskState.Default;
        }
    }
}