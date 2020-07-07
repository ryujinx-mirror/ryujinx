using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.OpenGL.Image;
using Ryujinx.Graphics.OpenGL.Queries;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    class Pipeline : IPipeline, IDisposable
    {
        private Program _program;

        private bool _rasterizerDiscard;

        private VertexArray _vertexArray;
        private Framebuffer _framebuffer;

        private IntPtr _indexBaseOffset;

        private DrawElementsType _elementsType;

        private PrimitiveType _primitiveType;

        private int _stencilFrontMask;
        private bool _depthMask;
        private bool _depthTest;
        private bool _hasDepthBuffer;

        private int _boundDrawFramebuffer;
        private int _boundReadFramebuffer;

        private float[] _fpRenderScale = new float[33];
        private float[] _cpRenderScale = new float[32];

        private TextureBase _unit0Texture;
        private TextureBase _rtColor0Texture;
        private TextureBase _rtDepthTexture;

        private ClipOrigin _clipOrigin;
        private ClipDepthMode _clipDepthMode;

        private readonly uint[] _componentMasks;

        private bool _scissor0Enable = false;

        ColorF _blendConstant = new ColorF(0, 0, 0, 0);

        internal Pipeline()
        {
            _rasterizerDiscard = false;
            _clipOrigin = ClipOrigin.LowerLeft;
            _clipDepthMode = ClipDepthMode.NegativeOneToOne;

            _componentMasks = new uint[Constants.MaxRenderTargets];

            for (int index = 0; index < Constants.MaxRenderTargets; index++)
            {
                _componentMasks[index] = 0xf;
            }

            for (int index = 0; index < _fpRenderScale.Length; index++)
            {
                _fpRenderScale[index] = 1f;
            }

            for (int index = 0; index < _cpRenderScale.Length; index++)
            {
                _cpRenderScale[index] = 1f;
            }
        }

        public void Barrier()
        {
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
        }

        public void ClearRenderTargetColor(int index, uint componentMask, ColorF color)
        {
            GL.ColorMask(
                index,
                (componentMask & 1) != 0,
                (componentMask & 2) != 0,
                (componentMask & 4) != 0,
                (componentMask & 8) != 0);

            float[] colors = new float[] { color.Red, color.Green, color.Blue, color.Alpha };

            GL.ClearBuffer(ClearBuffer.Color, index, colors);

            RestoreComponentMask(index);

            _framebuffer.SignalModified();
        }

        public void ClearRenderTargetDepthStencil(float depthValue, bool depthMask, int stencilValue, int stencilMask)
        {
            bool stencilMaskChanged =
                stencilMask != 0 &&
                stencilMask != _stencilFrontMask;

            bool depthMaskChanged = depthMask && depthMask != _depthMask;

            if (stencilMaskChanged)
            {
                GL.StencilMaskSeparate(StencilFace.Front, stencilMask);
            }

            if (depthMaskChanged)
            {
                GL.DepthMask(depthMask);
            }

            if (depthMask && stencilMask != 0)
            {
                GL.ClearBuffer(ClearBufferCombined.DepthStencil, 0, depthValue, stencilValue);
            }
            else if (depthMask)
            {
                GL.ClearBuffer(ClearBuffer.Depth, 0, ref depthValue);
            }
            else if (stencilMask != 0)
            {
                GL.ClearBuffer(ClearBuffer.Stencil, 0, ref stencilValue);
            }

            if (stencilMaskChanged)
            {
                GL.StencilMaskSeparate(StencilFace.Front, _stencilFrontMask);
            }

            if (depthMaskChanged)
            {
                GL.DepthMask(_depthMask);
            }

            _framebuffer.SignalModified();
        }

        public void CopyBuffer(BufferHandle source, BufferHandle destination, int srcOffset, int dstOffset, int size)
        {
            Buffer.Copy(source, destination, srcOffset, dstOffset, size);
        }

        public void DispatchCompute(int groupsX, int groupsY, int groupsZ)
        {
            if (!_program.IsLinked)
            {
                Logger.PrintDebug(LogClass.Gpu, "Dispatch error, shader not linked.");

                return;
            }

            PrepareForDispatch();

            GL.DispatchCompute(groupsX, groupsY, groupsZ);
        }

        public void Draw(int vertexCount, int instanceCount, int firstVertex, int firstInstance)
        {
            if (!_program.IsLinked)
            {
                Logger.PrintDebug(LogClass.Gpu, "Draw error, shader not linked.");

                return;
            }

            PrepareForDraw();

            if (_primitiveType == PrimitiveType.Quads)
            {
                DrawQuadsImpl(vertexCount, instanceCount, firstVertex, firstInstance);
            }
            else if (_primitiveType == PrimitiveType.QuadStrip)
            {
                DrawQuadStripImpl(vertexCount, instanceCount, firstVertex, firstInstance);
            }
            else
            {
                DrawImpl(vertexCount, instanceCount, firstVertex, firstInstance);
            }

            _framebuffer.SignalModified();
        }

        private void DrawQuadsImpl(
            int vertexCount,
            int instanceCount,
            int firstVertex,
            int firstInstance)
        {
            // TODO: Instanced rendering.
            int quadsCount = vertexCount / 4;

            int[] firsts = new int[quadsCount];
            int[] counts = new int[quadsCount];

            for (int quadIndex = 0; quadIndex < quadsCount; quadIndex++)
            {
                firsts[quadIndex] = firstVertex + quadIndex * 4;
                counts[quadIndex] = 4;
            }

            GL.MultiDrawArrays(
                PrimitiveType.TriangleFan,
                firsts,
                counts,
                quadsCount);
        }

        private void DrawQuadStripImpl(
            int vertexCount,
            int instanceCount,
            int firstVertex,
            int firstInstance)
        {
            int quadsCount = (vertexCount - 2) / 2;

            if (firstInstance != 0 || instanceCount != 1)
            {
                for (int quadIndex = 0; quadIndex < quadsCount; quadIndex++)
                {
                    GL.DrawArraysInstancedBaseInstance(PrimitiveType.TriangleFan, firstVertex + quadIndex * 2, 4, instanceCount, firstInstance);
                }
            }
            else
            {
                int[] firsts = new int[quadsCount];
                int[] counts = new int[quadsCount];

                firsts[0] = firstVertex;
                counts[0] = 4;

                for (int quadIndex = 1; quadIndex < quadsCount; quadIndex++)
                {
                    firsts[quadIndex] = firstVertex + quadIndex * 2;
                    counts[quadIndex] = 4;
                }

                GL.MultiDrawArrays(
                    PrimitiveType.TriangleFan,
                    firsts,
                    counts,
                    quadsCount);
            }
        }

        private void DrawImpl(
            int vertexCount,
            int instanceCount,
            int firstVertex,
            int firstInstance)
        {
            if (firstInstance == 0 && instanceCount == 1)
            {
                GL.DrawArrays(_primitiveType, firstVertex, vertexCount);
            }
            else if (firstInstance == 0)
            {
                GL.DrawArraysInstanced(_primitiveType, firstVertex, vertexCount, instanceCount);
            }
            else
            {
                GL.DrawArraysInstancedBaseInstance(
                    _primitiveType,
                    firstVertex,
                    vertexCount,
                    instanceCount,
                    firstInstance);
            }
        }

        public void DrawIndexed(
            int indexCount,
            int instanceCount,
            int firstIndex,
            int firstVertex,
            int firstInstance)
        {
            if (!_program.IsLinked)
            {
                Logger.PrintDebug(LogClass.Gpu, "Draw error, shader not linked.");

                return;
            }

            PrepareForDraw();

            int indexElemSize = 1;

            switch (_elementsType)
            {
                case DrawElementsType.UnsignedShort: indexElemSize = 2; break;
                case DrawElementsType.UnsignedInt: indexElemSize = 4; break;
            }

            IntPtr indexBaseOffset = _indexBaseOffset + firstIndex * indexElemSize;

            if (_primitiveType == PrimitiveType.Quads)
            {
                DrawQuadsIndexedImpl(
                    indexCount,
                    instanceCount,
                    indexBaseOffset,
                    indexElemSize,
                    firstVertex,
                    firstInstance);
            }
            else if (_primitiveType == PrimitiveType.QuadStrip)
            {
                DrawQuadStripIndexedImpl(
                    indexCount,
                    instanceCount,
                    indexBaseOffset,
                    indexElemSize,
                    firstVertex,
                    firstInstance);
            }
            else
            {
                DrawIndexedImpl(
                    indexCount,
                    instanceCount,
                    indexBaseOffset,
                    firstVertex,
                    firstInstance);
            }

            _framebuffer.SignalModified();
        }

        private void DrawQuadsIndexedImpl(
            int indexCount,
            int instanceCount,
            IntPtr indexBaseOffset,
            int indexElemSize,
            int firstVertex,
            int firstInstance)
        {
            int quadsCount = indexCount / 4;

            if (firstInstance != 0 || instanceCount != 1)
            {
                if (firstVertex != 0 && firstInstance != 0)
                {
                    for (int quadIndex = 0; quadIndex < quadsCount; quadIndex++)
                    {
                        GL.DrawElementsInstancedBaseVertexBaseInstance(
                            PrimitiveType.TriangleFan,
                            4,
                            _elementsType,
                            indexBaseOffset + quadIndex * 4 * indexElemSize,
                            instanceCount,
                            firstVertex,
                            firstInstance);
                    }
                }
                else if (firstInstance != 0)
                {
                    for (int quadIndex = 0; quadIndex < quadsCount; quadIndex++)
                    {
                        GL.DrawElementsInstancedBaseInstance(
                            PrimitiveType.TriangleFan,
                            4,
                            _elementsType,
                            indexBaseOffset + quadIndex * 4 * indexElemSize,
                            instanceCount,
                            firstInstance);
                    }
                }
                else
                {
                    for (int quadIndex = 0; quadIndex < quadsCount; quadIndex++)
                    {
                        GL.DrawElementsInstanced(
                            PrimitiveType.TriangleFan,
                            4,
                            _elementsType,
                            indexBaseOffset + quadIndex * 4 * indexElemSize,
                            instanceCount);
                    }
                }
            }
            else
            {
                IntPtr[] indices = new IntPtr[quadsCount];

                int[] counts = new int[quadsCount];

                int[] baseVertices = new int[quadsCount];

                for (int quadIndex = 0; quadIndex < quadsCount; quadIndex++)
                {
                    indices[quadIndex] = indexBaseOffset + quadIndex * 4 * indexElemSize;

                    counts[quadIndex] = 4;

                    baseVertices[quadIndex] = firstVertex;
                }

                GL.MultiDrawElementsBaseVertex(
                    PrimitiveType.TriangleFan,
                    counts,
                    _elementsType,
                    indices,
                    quadsCount,
                    baseVertices);
            }
        }

        private void DrawQuadStripIndexedImpl(
            int indexCount,
            int instanceCount,
            IntPtr indexBaseOffset,
            int indexElemSize,
            int firstVertex,
            int firstInstance)
        {
            // TODO: Instanced rendering.
            int quadsCount = (indexCount - 2) / 2;

            IntPtr[] indices = new IntPtr[quadsCount];

            int[] counts = new int[quadsCount];

            int[] baseVertices = new int[quadsCount];

            indices[0] = indexBaseOffset;

            counts[0] = 4;

            baseVertices[0] = firstVertex;

            for (int quadIndex = 1; quadIndex < quadsCount; quadIndex++)
            {
                indices[quadIndex] = indexBaseOffset + quadIndex * 2 * indexElemSize;

                counts[quadIndex] = 4;

                baseVertices[quadIndex] = firstVertex;
            }

            GL.MultiDrawElementsBaseVertex(
                PrimitiveType.TriangleFan,
                counts,
                _elementsType,
                indices,
                quadsCount,
                baseVertices);
        }

        private void DrawIndexedImpl(
            int indexCount,
            int instanceCount,
            IntPtr indexBaseOffset,
            int firstVertex,
            int firstInstance)
        {
            if (firstInstance == 0 && firstVertex == 0 && instanceCount == 1)
            {
                GL.DrawElements(_primitiveType, indexCount, _elementsType, indexBaseOffset);
            }
            else if (firstInstance == 0 && instanceCount == 1)
            {
                GL.DrawElementsBaseVertex(
                    _primitiveType,
                    indexCount,
                    _elementsType,
                    indexBaseOffset,
                    firstVertex);
            }
            else if (firstInstance == 0 && firstVertex == 0)
            {
                GL.DrawElementsInstanced(
                    _primitiveType,
                    indexCount,
                    _elementsType,
                    indexBaseOffset,
                    instanceCount);
            }
            else if (firstInstance == 0)
            {
                GL.DrawElementsInstancedBaseVertex(
                    _primitiveType,
                    indexCount,
                    _elementsType,
                    indexBaseOffset,
                    instanceCount,
                    firstVertex);
            }
            else if (firstVertex == 0)
            {
                GL.DrawElementsInstancedBaseInstance(
                    _primitiveType,
                    indexCount,
                    _elementsType,
                    indexBaseOffset,
                    instanceCount,
                    firstInstance);
            }
            else
            {
                GL.DrawElementsInstancedBaseVertexBaseInstance(
                    _primitiveType,
                    indexCount,
                    _elementsType,
                    indexBaseOffset,
                    instanceCount,
                    firstVertex,
                    firstInstance);
            }
        }

        public void SetBlendState(int index, BlendDescriptor blend)
        {
            if (!blend.Enable)
            {
                GL.Disable(IndexedEnableCap.Blend, index);

                return;
            }

            GL.BlendEquationSeparate(
                index,
                blend.ColorOp.Convert(),
                blend.AlphaOp.Convert());

            GL.BlendFuncSeparate(
                index,
                (BlendingFactorSrc)blend.ColorSrcFactor.Convert(),
                (BlendingFactorDest)blend.ColorDstFactor.Convert(),
                (BlendingFactorSrc)blend.AlphaSrcFactor.Convert(),
                (BlendingFactorDest)blend.AlphaDstFactor.Convert());

            if (_blendConstant != blend.BlendConstant)
            {
                _blendConstant = blend.BlendConstant;

                GL.BlendColor(
                    blend.BlendConstant.Red,
                    blend.BlendConstant.Green,
                    blend.BlendConstant.Blue,
                    blend.BlendConstant.Alpha);
            }

            GL.Enable(IndexedEnableCap.Blend, index);
        }

        public void SetDepthBias(PolygonModeMask enables, float factor, float units, float clamp)
        {
            if ((enables & PolygonModeMask.Point) != 0)
            {
                GL.Enable(EnableCap.PolygonOffsetPoint);
            }
            else
            {
                GL.Disable(EnableCap.PolygonOffsetPoint);
            }

            if ((enables & PolygonModeMask.Line) != 0)
            {
                GL.Enable(EnableCap.PolygonOffsetLine);
            }
            else
            {
                GL.Disable(EnableCap.PolygonOffsetLine);
            }

            if ((enables & PolygonModeMask.Fill) != 0)
            {
                GL.Enable(EnableCap.PolygonOffsetFill);
            }
            else
            {
                GL.Disable(EnableCap.PolygonOffsetFill);
            }

            if (enables == 0)
            {
                return;
            }

            GL.PolygonOffset(factor, units / 2f);
            // TODO: Enable when GL_EXT_polygon_offset_clamp is supported.
            // GL.PolygonOffsetClamp(factor, units, clamp);
        }

        public void SetDepthClamp(bool clamp)
        {
            if (!clamp)
            {
                GL.Disable(EnableCap.DepthClamp);
                return;
            }

            GL.Enable(EnableCap.DepthClamp);
        }

        public void SetDepthMode(DepthMode mode)
        {
            ClipDepthMode depthMode = mode.Convert();

            if (_clipDepthMode != depthMode)
            {
                _clipDepthMode = depthMode;

                GL.ClipControl(_clipOrigin, depthMode);
            }
        }

        public void SetDepthTest(DepthTestDescriptor depthTest)
        {
            GL.DepthFunc((DepthFunction)depthTest.Func.Convert());

            _depthMask = depthTest.WriteEnable;
            _depthTest = depthTest.TestEnable;

            UpdateDepthTest();
        }

        public void SetFaceCulling(bool enable, Face face)
        {
            if (!enable)
            {
                GL.Disable(EnableCap.CullFace);

                return;
            }

            GL.CullFace(face.Convert());

            GL.Enable(EnableCap.CullFace);
        }

        public void SetFrontFace(FrontFace frontFace)
        {
            GL.FrontFace(frontFace.Convert());
        }

        public void SetImage(int index, ShaderStage stage, ITexture texture)
        {
            int unit = _program.GetImageUnit(stage, index);

            if (unit != -1 && texture != null)
            {
                TextureBase texBase = (TextureBase)texture;

                FormatInfo formatInfo = FormatTable.GetFormatInfo(texBase.Format);

                SizedInternalFormat format = (SizedInternalFormat)formatInfo.PixelInternalFormat;

                GL.BindImageTexture(unit, texBase.Handle, 0, true, 0, TextureAccess.ReadWrite, format);
            }
        }

        public void SetIndexBuffer(BufferRange buffer, IndexType type)
        {
            _elementsType = type.Convert();

            _indexBaseOffset = (IntPtr)buffer.Offset;

            EnsureVertexArray();

            _vertexArray.SetIndexBuffer(buffer.Handle);
        }

        public void SetOrigin(Origin origin)
        {
            ClipOrigin clipOrigin = origin == Origin.UpperLeft ? ClipOrigin.UpperLeft : ClipOrigin.LowerLeft;

            SetOrigin(clipOrigin);
        }

        public void SetPointSize(float size)
        {
            GL.PointSize(size);
        }

        public void SetPrimitiveRestart(bool enable, int index)
        {
            if (!enable)
            {
                GL.Disable(EnableCap.PrimitiveRestart);

                return;
            }

            GL.PrimitiveRestartIndex(index);

            GL.Enable(EnableCap.PrimitiveRestart);
        }

        public void SetPrimitiveTopology(PrimitiveTopology topology)
        {
            _primitiveType = topology.Convert();
        }

        public void SetProgram(IProgram program)
        {
            _program = (Program)program;
            _program.Bind();

            SetRenderTargetScale(_fpRenderScale[0]);
        }

        public void SetRasterizerDiscard(bool discard)
        {
            if (discard)
            {
                GL.Enable(EnableCap.RasterizerDiscard);
            }
            else
            {
                GL.Disable(EnableCap.RasterizerDiscard);
            }

            _rasterizerDiscard = discard;
        }

        public void SetRenderTargetScale(float scale)
        {
            _fpRenderScale[0] = scale;

            if (_program != null && _program.FragmentRenderScaleUniform != -1)
            {
                GL.Uniform1(_program.FragmentRenderScaleUniform, 1, _fpRenderScale); // Just the first element.
            }
        }

        public void SetRenderTargetColorMasks(ReadOnlySpan<uint> componentMasks)
        {
            for (int index = 0; index < componentMasks.Length; index++)
            {
                _componentMasks[index] = componentMasks[index];

                RestoreComponentMask(index);
            }
        }

        public void SetRenderTargets(ITexture[] colors, ITexture depthStencil)
        {
            EnsureFramebuffer();

            _rtColor0Texture = (TextureBase)colors[0];
            _rtDepthTexture = (TextureBase)depthStencil;

            for (int index = 0; index < colors.Length; index++)
            {
                TextureView color = (TextureView)colors[index];

                _framebuffer.AttachColor(index, color);
            }

            TextureView depthStencilView = (TextureView)depthStencil;

            _framebuffer.AttachDepthStencil(depthStencilView);

            _framebuffer.SetDrawBuffers(colors.Length);

            _hasDepthBuffer = depthStencil != null && depthStencilView.Format != Format.S8Uint;

            UpdateDepthTest();
        }

        public void SetSampler(int index, ShaderStage stage, ISampler sampler)
        {
            int unit = _program.GetTextureUnit(stage, index);

            if (unit != -1 && sampler != null)
            {
                ((Sampler)sampler).Bind(unit);
            }
        }

        public void SetScissorEnable(int index, bool enable)
        {
            if (enable)
            {
                GL.Enable(IndexedEnableCap.ScissorTest, index);
            }
            else
            {
                GL.Disable(IndexedEnableCap.ScissorTest, index);
            }

            if (index == 0)
            {
                _scissor0Enable = enable;
            }
        }

        public void SetScissor(int index, int x, int y, int width, int height)
        {
            GL.ScissorIndexed(index, x, y, width, height);
        }

        public void SetStencilTest(StencilTestDescriptor stencilTest)
        {
            if (!stencilTest.TestEnable)
            {
                GL.Disable(EnableCap.StencilTest);

                return;
            }

            GL.StencilOpSeparate(
                StencilFace.Front,
                stencilTest.FrontSFail.Convert(),
                stencilTest.FrontDpFail.Convert(),
                stencilTest.FrontDpPass.Convert());

            GL.StencilFuncSeparate(
                StencilFace.Front,
                (StencilFunction)stencilTest.FrontFunc.Convert(),
                stencilTest.FrontFuncRef,
                stencilTest.FrontFuncMask);

            GL.StencilMaskSeparate(StencilFace.Front, stencilTest.FrontMask);

            GL.StencilOpSeparate(
                StencilFace.Back,
                stencilTest.BackSFail.Convert(),
                stencilTest.BackDpFail.Convert(),
                stencilTest.BackDpPass.Convert());

            GL.StencilFuncSeparate(
                StencilFace.Back,
                (StencilFunction)stencilTest.BackFunc.Convert(),
                stencilTest.BackFuncRef,
                stencilTest.BackFuncMask);

            GL.StencilMaskSeparate(StencilFace.Back, stencilTest.BackMask);

            GL.Enable(EnableCap.StencilTest);

            _stencilFrontMask = stencilTest.FrontMask;
        }

        public void SetStorageBuffer(int index, ShaderStage stage, BufferRange buffer)
        {
            SetBuffer(index, stage, buffer, isStorage: true);
        }

        public void SetTexture(int index, ShaderStage stage, ITexture texture)
        {
            int unit = _program.GetTextureUnit(stage, index);

            if (unit != -1 && texture != null)
            {
                if (unit == 0)
                {
                    _unit0Texture = (TextureBase)texture;
                }
                else
                {
                    ((TextureBase)texture).Bind(unit);
                }

                // Update scale factor for bound textures.

                switch (stage)
                {
                    case ShaderStage.Fragment:
                        if (_program.FragmentRenderScaleUniform != -1)
                        {
                            // Only update and send sampled texture scales if the shader uses them.
                            bool interpolate = false;
                            float scale = texture.ScaleFactor;

                            if (scale != 1)
                            {
                                TextureBase activeTarget = _rtColor0Texture ?? _rtDepthTexture;

                                if (activeTarget != null && activeTarget.Width / (float)texture.Width == activeTarget.Height / (float)texture.Height)
                                {
                                    // If the texture's size is a multiple of the sampler size, enable interpolation using gl_FragCoord. (helps "invent" new integer values between scaled pixels)
                                    interpolate = true;
                                }
                            }

                            _fpRenderScale[index + 1] = interpolate ? -scale : scale;
                        }
                        break;

                    case ShaderStage.Compute:
                        _cpRenderScale[index] = texture.ScaleFactor;
                        break;
                }
            }
        }

        public void SetUniformBuffer(int index, ShaderStage stage, BufferRange buffer)
        {
            SetBuffer(index, stage, buffer, isStorage: false);
        }

        public void SetUserClipDistance(int index, bool enableClip)
        {
            if (!enableClip)
            {
                GL.Disable(EnableCap.ClipDistance0 + index);
                return;
            }

            GL.Enable(EnableCap.ClipDistance0 + index);
        }

        public void SetVertexAttribs(ReadOnlySpan<VertexAttribDescriptor> vertexAttribs)
        {
            EnsureVertexArray();

            _vertexArray.SetVertexAttributes(vertexAttribs);
        }

        public void SetVertexBuffers(ReadOnlySpan<VertexBufferDescriptor> vertexBuffers)
        {
            EnsureVertexArray();

            _vertexArray.SetVertexBuffers(vertexBuffers);
        }

        public void SetViewports(int first, ReadOnlySpan<Viewport> viewports)
        {
            float[] viewportArray = new float[viewports.Length * 4];

            double[] depthRangeArray = new double[viewports.Length * 2];

            for (int index = 0; index < viewports.Length; index++)
            {
                int viewportElemIndex = index * 4;

                Viewport viewport = viewports[index];

                viewportArray[viewportElemIndex + 0] = viewport.Region.X;
                viewportArray[viewportElemIndex + 1] = viewport.Region.Y;

                if (HwCapabilities.SupportsViewportSwizzle)
                {
                    GL.NV.ViewportSwizzle(
                        index,
                        viewport.SwizzleX.Convert(),
                        viewport.SwizzleY.Convert(),
                        viewport.SwizzleZ.Convert(),
                        viewport.SwizzleW.Convert());
                }

                viewportArray[viewportElemIndex + 2] = MathF.Abs(viewport.Region.Width);
                viewportArray[viewportElemIndex + 3] = MathF.Abs(viewport.Region.Height);

                depthRangeArray[index * 2 + 0] = viewport.DepthNear;
                depthRangeArray[index * 2 + 1] = viewport.DepthFar;
            }

            GL.ViewportArray(first, viewports.Length, viewportArray);

            GL.DepthRangeArray(first, viewports.Length, depthRangeArray);
        }

        public void TextureBarrier()
        {
            GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);
        }

        public void TextureBarrierTiled()
        {
            GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);
        }

        private void SetBuffer(int index, ShaderStage stage, BufferRange buffer, bool isStorage)
        {
            int bindingPoint = isStorage
                ? _program.GetStorageBufferBindingPoint(stage, index)
                : _program.GetUniformBufferBindingPoint(stage, index);

            if (bindingPoint == -1)
            {
                return;
            }

            BufferRangeTarget target = isStorage
                ? BufferRangeTarget.ShaderStorageBuffer
                : BufferRangeTarget.UniformBuffer;

            if (buffer.Handle == null)
            {
                GL.BindBufferRange(target, bindingPoint, 0, IntPtr.Zero, 0);

                return;
            }

            IntPtr bufferOffset = (IntPtr)buffer.Offset;

            GL.BindBufferRange(target, bindingPoint, buffer.Handle.ToInt32(), bufferOffset, buffer.Size);
        }

        private void SetOrigin(ClipOrigin origin)
        {
            if (_clipOrigin != origin)
            {
                _clipOrigin = origin;

                GL.ClipControl(origin, _clipDepthMode);
            }
        }

        private void EnsureVertexArray()
        {
            if (_vertexArray == null)
            {
                _vertexArray = new VertexArray();

                _vertexArray.Bind();
            }
        }

        private void EnsureFramebuffer()
        {
            if (_framebuffer == null)
            {
                _framebuffer = new Framebuffer();

                int boundHandle = _framebuffer.Bind();
                _boundDrawFramebuffer = _boundReadFramebuffer = boundHandle;

                GL.Enable(EnableCap.FramebufferSrgb);
            }
        }

        internal (int drawHandle, int readHandle) GetBoundFramebuffers()
        {
            return (_boundDrawFramebuffer, _boundReadFramebuffer);
        }

        private void UpdateDepthTest()
        {
            // Enabling depth operations is only valid when we have
            // a depth buffer, otherwise it's not allowed.
            if (_hasDepthBuffer)
            {
                if (_depthTest)
                {
                    GL.Enable(EnableCap.DepthTest);
                }
                else
                {
                    GL.Disable(EnableCap.DepthTest);
                }

                GL.DepthMask(_depthMask);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);

                GL.DepthMask(false);
            }
        }

        private void PrepareForDispatch()
        {
            if (_unit0Texture != null)
            {
                _unit0Texture.Bind(0);
            }
        }

        private void PrepareForDraw()
        {
            _vertexArray.Validate();

            if (_unit0Texture != null)
            {
                _unit0Texture.Bind(0);
            }
        }

        private void RestoreComponentMask(int index)
        {
            GL.ColorMask(
                index,
                (_componentMasks[index] & 1u) != 0,
                (_componentMasks[index] & 2u) != 0,
                (_componentMasks[index] & 4u) != 0,
                (_componentMasks[index] & 8u) != 0);
        }

        public void RestoreScissor0Enable()
        {
            if (_scissor0Enable)
            {
                GL.Enable(IndexedEnableCap.ScissorTest, 0);
            }
        }

        public void RestoreRasterizerDiscard()
        {
            if (_rasterizerDiscard)
            {
                GL.Enable(EnableCap.RasterizerDiscard);
            }
        }

        public bool TryHostConditionalRendering(ICounterEvent value, ulong compare, bool isEqual)
        {
            if (value is CounterQueueEvent)
            {
                // Compare an event and a constant value.
                CounterQueueEvent evt = (CounterQueueEvent)value;

                // Easy host conditional rendering when the check matches what GL can do:
                //  - Event is of type samples passed.
                //  - Result is not a combination of multiple queries.
                //  - Comparing against 0.
                //  - Event has not already been flushed.

                if (evt.Disposed)
                {
                    // If the event has been flushed, then just use the values on the CPU.
                    // The query object may already be repurposed for another draw (eg. begin + end).
                    return false; 
                }

                if (compare == 0 && evt.Type == QueryTarget.SamplesPassed && evt.ClearCounter)
                {
                    GL.BeginConditionalRender(evt.Query, isEqual ? ConditionalRenderType.QueryNoWaitInverted : ConditionalRenderType.QueryNoWait);
                    return true;
                }
            }

            // The GPU will flush the queries to CPU and evaluate the condition there instead.

            GL.Flush(); // The thread will be stalled manually flushing the counter, so flush GL commands now.
            return false; 
        }

        public bool TryHostConditionalRendering(ICounterEvent value, ICounterEvent compare, bool isEqual)
        {
            GL.Flush(); // The GPU thread will be stalled manually flushing the counter, so flush GL commands now.
            return false; // We don't currently have a way to compare two counters for conditional rendering.
        }

        public void EndHostConditionalRendering()
        {
            GL.EndConditionalRender();
        }

        public void Dispose()
        {
            _framebuffer?.Dispose();
            _vertexArray?.Dispose();
        }

        public void UpdateRenderScale(ShaderStage stage, int textureCount)
        {
            if (_program != null)
            {
                switch (stage)
                {
                    case ShaderStage.Fragment:
                        if (_program.FragmentRenderScaleUniform != -1)
                        {
                            GL.Uniform1(_program.FragmentRenderScaleUniform, textureCount + 1, _fpRenderScale);
                        }
                        break;

                    case ShaderStage.Compute:
                        if (_program.ComputeRenderScaleUniform != -1)
                        {
                            GL.Uniform1(_program.ComputeRenderScaleUniform, textureCount, _cpRenderScale);
                        }
                        break;
                }
            }
        }
    }
}
