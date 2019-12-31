using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    class Pipeline : IPipeline, IDisposable
    {
        private Program _program;

        private VertexArray _vertexArray;
        private Framebuffer _framebuffer;

        private IntPtr _indexBaseOffset;

        private DrawElementsType _elementsType;

        private PrimitiveType _primitiveType;

        private int  _stencilFrontMask;
        private bool _depthMask;
        private bool _depthTest;
        private bool _hasDepthBuffer;

        private TextureView _unit0Texture;

        private ClipOrigin    _clipOrigin;
        private ClipDepthMode _clipDepthMode;

        private uint[] _componentMasks;

        internal Pipeline()
        {
            _clipOrigin    = ClipOrigin.LowerLeft;
            _clipDepthMode = ClipDepthMode.NegativeOneToOne;
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
            // TODO: Instanced rendering.
            int quadsCount = (vertexCount - 2) / 2;

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
                case DrawElementsType.UnsignedInt:   indexElemSize = 4; break;
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
        }

        private void DrawQuadsIndexedImpl(
            int    indexCount,
            int    instanceCount,
            IntPtr indexBaseOffset,
            int    indexElemSize,
            int    firstVertex,
            int    firstInstance)
        {
            // TODO: Instanced rendering.
            int quadsCount = indexCount / 4;

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

        private void DrawQuadStripIndexedImpl(
            int    indexCount,
            int    instanceCount,
            IntPtr indexBaseOffset,
            int    indexElemSize,
            int    firstVertex,
            int    firstInstance)
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
            int    indexCount,
            int    instanceCount,
            IntPtr indexBaseOffset,
            int    firstVertex,
            int    firstInstance)
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

        public void SetBlendColor(ColorF color)
        {
            GL.BlendColor(color.Red, color.Green, color.Blue, color.Alpha);
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

            GL.PolygonOffset(factor, units);
            // GL.PolygonOffsetClamp(factor, units, clamp);
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
                TextureView view = (TextureView)texture;

                FormatInfo formatInfo = FormatTable.GetFormatInfo(view.Format);

                SizedInternalFormat format = (SizedInternalFormat)formatInfo.PixelInternalFormat;

                GL.BindImageTexture(unit, view.Handle, 0, true, 0, TextureAccess.ReadWrite, format);
            }
        }

        public void SetIndexBuffer(BufferRange buffer, IndexType type)
        {
            _elementsType = type.Convert();

            _indexBaseOffset = (IntPtr)buffer.Offset;

            EnsureVertexArray();

            _vertexArray.SetIndexBuffer((Buffer)buffer.Buffer);
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
        }

        public void SetRenderTargetColorMasks(uint[] componentMasks)
        {
            _componentMasks = (uint[])componentMasks.Clone();

            for (int index = 0; index < componentMasks.Length; index++)
            {
                RestoreComponentMask(index);
            }
        }

        public void SetRenderTargets(ITexture[] colors, ITexture depthStencil)
        {
            EnsureFramebuffer();

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
                    _unit0Texture = ((TextureView)texture);
                }
                else
                {
                    ((TextureView)texture).Bind(unit);
                }
            }
        }

        public void SetUniformBuffer(int index, ShaderStage stage, BufferRange buffer)
        {
            SetBuffer(index, stage, buffer, isStorage: false);
        }

        public void SetVertexAttribs(VertexAttribDescriptor[] vertexAttribs)
        {
            EnsureVertexArray();

            _vertexArray.SetVertexAttributes(vertexAttribs);
        }

        public void SetVertexBuffers(VertexBufferDescriptor[] vertexBuffers)
        {
            EnsureVertexArray();

            _vertexArray.SetVertexBuffers(vertexBuffers);
        }

        public void SetViewports(int first, Viewport[] viewports)
        {
            bool flipY = false;

            float[] viewportArray = new float[viewports.Length * 4];

            double[] depthRangeArray = new double[viewports.Length * 2];

            for (int index = 0; index < viewports.Length; index++)
            {
                int viewportElemIndex = index * 4;

                Viewport viewport = viewports[index];

                viewportArray[viewportElemIndex + 0] = viewport.Region.X;
                viewportArray[viewportElemIndex + 1] = viewport.Region.Y;

                // OpenGL does not support per-viewport flipping, so
                // instead we decide that based on the viewport 0 value.
                // It will apply to all viewports.
                if (index == 0)
                {
                    flipY = viewport.Region.Height < 0;
                }

                if (viewport.SwizzleY == ViewportSwizzle.NegativeY)
                {
                    flipY = !flipY;
                }

                viewportArray[viewportElemIndex + 2] = MathF.Abs(viewport.Region.Width);
                viewportArray[viewportElemIndex + 3] = MathF.Abs(viewport.Region.Height);

                depthRangeArray[index * 2 + 0] = viewport.DepthNear;
                depthRangeArray[index * 2 + 1] = viewport.DepthFar;
            }

            GL.ViewportArray(first, viewports.Length, viewportArray);

            GL.DepthRangeArray(first, viewports.Length, depthRangeArray);

            SetOrigin(flipY ? ClipOrigin.UpperLeft : ClipOrigin.LowerLeft);
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

            if (buffer.Buffer == null)
            {
                GL.BindBufferRange(target, bindingPoint, 0, IntPtr.Zero, 0);

                return;
            }

            int bufferHandle = ((Buffer)buffer.Buffer).Handle;

            IntPtr bufferOffset = (IntPtr)buffer.Offset;

            GL.BindBufferRange(target, bindingPoint, bufferHandle, bufferOffset, buffer.Size);
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

                _framebuffer.Bind();

                GL.Enable(EnableCap.FramebufferSrgb);
            }
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
            if (_componentMasks != null)
            {
                GL.ColorMask(
                    index,
                    (_componentMasks[index] & 1u) != 0,
                    (_componentMasks[index] & 2u) != 0,
                    (_componentMasks[index] & 4u) != 0,
                    (_componentMasks[index] & 8u) != 0);
            }
        }

        public void Dispose()
        {
            _framebuffer?.Dispose();
            _vertexArray?.Dispose();
        }
    }
}
