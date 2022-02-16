using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.OpenGL.Image;
using Ryujinx.Graphics.OpenGL.Queries;
using Ryujinx.Graphics.Shader;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.OpenGL
{
    class Pipeline : IPipeline, IDisposable
    {
        private readonly DrawTextureEmulation _drawTexture;

        internal ulong DrawCount { get; private set; }

        private Program _program;

        private bool _rasterizerDiscard;

        private VertexArray _vertexArray;
        private Framebuffer _framebuffer;

        private IntPtr _indexBaseOffset;

        private DrawElementsType _elementsType;

        private PrimitiveType _primitiveType;

        private int _stencilFrontMask;
        private bool _depthMask;
        private bool _depthTestEnable;
        private bool _stencilTestEnable;
        private bool _cullEnable;

        private float[] _viewportArray = Array.Empty<float>();
        private double[] _depthRangeArray = Array.Empty<double>();

        private int _boundDrawFramebuffer;
        private int _boundReadFramebuffer;

        private CounterQueueEvent _activeConditionalRender;

        private Vector4<int>[] _fpIsBgra = new Vector4<int>[SupportBuffer.FragmentIsBgraCount];
        private Vector4<float>[] _renderScale = new Vector4<float>[65];
        private int _fragmentScaleCount;

        private TextureBase _unit0Texture;
        private Sampler _unit0Sampler;

        private FrontFaceDirection _frontFace;
        private ClipOrigin _clipOrigin;
        private ClipDepthMode _clipDepthMode;

        private uint _fragmentOutputMap;
        private uint _componentMasks;
        private uint _currentComponentMasks;

        private uint _scissorEnables;

        private bool _tfEnabled;
        private TransformFeedbackPrimitiveType _tfTopology;

        private SupportBufferUpdater _supportBuffer;
        private readonly BufferHandle[] _tfbs;
        private readonly BufferRange[] _tfbTargets;

        private ColorF _blendConstant;

        internal Pipeline()
        {
            _drawTexture = new DrawTextureEmulation();
            _rasterizerDiscard = false;
            _clipOrigin = ClipOrigin.LowerLeft;
            _clipDepthMode = ClipDepthMode.NegativeOneToOne;

            _fragmentOutputMap = uint.MaxValue;
            _componentMasks = uint.MaxValue;

            var defaultScale = new Vector4<float> { X = 1f, Y = 0f, Z = 0f, W = 0f };
            new Span<Vector4<float>>(_renderScale).Fill(defaultScale);

            _tfbs = new BufferHandle[Constants.MaxTransformFeedbackBuffers];
            _tfbTargets = new BufferRange[Constants.MaxTransformFeedbackBuffers];
        }

        public void Initialize(Renderer renderer)
        {
            _supportBuffer = new SupportBufferUpdater(renderer);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, Unsafe.As<BufferHandle, int>(ref _supportBuffer.Handle));

            _supportBuffer.UpdateFragmentIsBgra(_fpIsBgra, 0, SupportBuffer.FragmentIsBgraCount);
            _supportBuffer.UpdateRenderScale(_renderScale, 0, SupportBuffer.RenderScaleMaxCount);
        }

        public void Barrier()
        {
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
        }

        public void BeginTransformFeedback(PrimitiveTopology topology)
        {
            GL.BeginTransformFeedback(_tfTopology = topology.ConvertToTfType());
            _tfEnabled = true;
        }

        public void ClearBuffer(BufferHandle destination, int offset, int size, uint value)
        {
            Buffer.Clear(destination, offset, size, value);
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

            GL.ClearBuffer(OpenTK.Graphics.OpenGL.ClearBuffer.Color, index, colors);

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
                GL.ClearBuffer(OpenTK.Graphics.OpenGL.ClearBuffer.Depth, 0, ref depthValue);
            }
            else if (stencilMask != 0)
            {
                GL.ClearBuffer(OpenTK.Graphics.OpenGL.ClearBuffer.Stencil, 0, ref stencilValue);
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

        public void CommandBufferBarrier()
        {
            GL.MemoryBarrier(MemoryBarrierFlags.CommandBarrierBit);
        }

        public void CopyBuffer(BufferHandle source, BufferHandle destination, int srcOffset, int dstOffset, int size)
        {
            Buffer.Copy(source, destination, srcOffset, dstOffset, size);
        }

        public void DispatchCompute(int groupsX, int groupsY, int groupsZ)
        {
            if (!_program.IsLinked)
            {
                Logger.Debug?.Print(LogClass.Gpu, "Dispatch error, shader not linked.");
                return;
            }

            PrepareForDispatch();

            GL.DispatchCompute(groupsX, groupsY, groupsZ);
        }

        public void Draw(int vertexCount, int instanceCount, int firstVertex, int firstInstance)
        {
            if (!_program.IsLinked)
            {
                Logger.Debug?.Print(LogClass.Gpu, "Draw error, shader not linked.");
                return;
            }

            PreDraw();

            if (_primitiveType == PrimitiveType.Quads && !HwCapabilities.SupportsQuads)
            {
                DrawQuadsImpl(vertexCount, instanceCount, firstVertex, firstInstance);
            }
            else if (_primitiveType == PrimitiveType.QuadStrip && !HwCapabilities.SupportsQuads)
            {
                DrawQuadStripImpl(vertexCount, instanceCount, firstVertex, firstInstance);
            }
            else
            {
                DrawImpl(vertexCount, instanceCount, firstVertex, firstInstance);
            }

            PostDraw();
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
                Logger.Debug?.Print(LogClass.Gpu, "Draw error, shader not linked.");
                return;
            }

            PreDraw();

            int indexElemSize = 1;

            switch (_elementsType)
            {
                case DrawElementsType.UnsignedShort: indexElemSize = 2; break;
                case DrawElementsType.UnsignedInt: indexElemSize = 4; break;
            }

            IntPtr indexBaseOffset = _indexBaseOffset + firstIndex * indexElemSize;

            if (_primitiveType == PrimitiveType.Quads && !HwCapabilities.SupportsQuads)
            {
                DrawQuadsIndexedImpl(
                    indexCount,
                    instanceCount,
                    indexBaseOffset,
                    indexElemSize,
                    firstVertex,
                    firstInstance);
            }
            else if (_primitiveType == PrimitiveType.QuadStrip && !HwCapabilities.SupportsQuads)
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

            PostDraw();
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

        public void DrawTexture(ITexture texture, ISampler sampler, Extents2DF srcRegion, Extents2DF dstRegion)
        {
            if (texture is TextureView view && sampler is Sampler samp)
            {
                _supportBuffer.Commit();

                if (HwCapabilities.SupportsDrawTexture)
                {
                    GL.NV.DrawTexture(
                        view.Handle,
                        samp.Handle,
                        dstRegion.X1,
                        dstRegion.Y1,
                        dstRegion.X2,
                        dstRegion.Y2,
                        0,
                        srcRegion.X1 / view.Width,
                        srcRegion.Y1 / view.Height,
                        srcRegion.X2 / view.Width,
                        srcRegion.Y2 / view.Height);
                }
                else
                {
                    static void Disable(EnableCap cap, bool enabled)
                    {
                        if (enabled)
                        {
                            GL.Disable(cap);
                        }
                    }

                    static void Enable(EnableCap cap, bool enabled)
                    {
                        if (enabled)
                        {
                            GL.Enable(cap);
                        }
                    }

                    Disable(EnableCap.CullFace, _cullEnable);
                    Disable(EnableCap.StencilTest, _stencilTestEnable);
                    Disable(EnableCap.DepthTest, _depthTestEnable);

                    if (_depthMask)
                    {
                        GL.DepthMask(false);
                    }

                    if (_tfEnabled)
                    {
                        GL.EndTransformFeedback();
                    }

                    _drawTexture.Draw(
                        view,
                        samp,
                        dstRegion.X1,
                        dstRegion.Y1,
                        dstRegion.X2,
                        dstRegion.Y2,
                        srcRegion.X1 / view.Width,
                        srcRegion.Y1 / view.Height,
                        srcRegion.X2 / view.Width,
                        srcRegion.Y2 / view.Height);

                    _program?.Bind();
                    _unit0Sampler?.Bind(0);

                    GL.ViewportArray(0, 1, _viewportArray);

                    Enable(EnableCap.CullFace, _cullEnable);
                    Enable(EnableCap.StencilTest, _stencilTestEnable);
                    Enable(EnableCap.DepthTest, _depthTestEnable);

                    if (_depthMask)
                    {
                        GL.DepthMask(true);
                    }

                    if (_tfEnabled)
                    {
                        GL.BeginTransformFeedback(_tfTopology);
                    }
                }
            }
        }

        public void EndTransformFeedback()
        {
            GL.EndTransformFeedback();
            _tfEnabled = false;
        }

        public void MultiDrawIndirectCount(BufferRange indirectBuffer, BufferRange parameterBuffer, int maxDrawCount, int stride)
        {
            if (!_program.IsLinked)
            {
                Logger.Debug?.Print(LogClass.Gpu, "Draw error, shader not linked.");
                return;
            }

            PreDraw();

            GL.BindBuffer((BufferTarget)All.DrawIndirectBuffer, indirectBuffer.Handle.ToInt32());
            GL.BindBuffer((BufferTarget)All.ParameterBuffer, parameterBuffer.Handle.ToInt32());

            GL.MultiDrawArraysIndirectCount(
                _primitiveType,
                (IntPtr)indirectBuffer.Offset,
                (IntPtr)parameterBuffer.Offset,
                maxDrawCount,
                stride);

            PostDraw();
        }

        public void MultiDrawIndexedIndirectCount(BufferRange indirectBuffer, BufferRange parameterBuffer, int maxDrawCount, int stride)
        {
            if (!_program.IsLinked)
            {
                Logger.Debug?.Print(LogClass.Gpu, "Draw error, shader not linked.");
                return;
            }

            PreDraw();

            _vertexArray.SetRangeOfIndexBuffer();

            GL.BindBuffer((BufferTarget)All.DrawIndirectBuffer, indirectBuffer.Handle.ToInt32());
            GL.BindBuffer((BufferTarget)All.ParameterBuffer, parameterBuffer.Handle.ToInt32());

            GL.MultiDrawElementsIndirectCount(
                _primitiveType,
                (Version46)_elementsType,
                (IntPtr)indirectBuffer.Offset,
                (IntPtr)parameterBuffer.Offset,
                maxDrawCount,
                stride);

            _vertexArray.RestoreIndexBuffer();

            PostDraw();
        }

        public void SetAlphaTest(bool enable, float reference, CompareOp op)
        {
            if (!enable)
            {
                GL.Disable(EnableCap.AlphaTest);
                return;
            }

            GL.AlphaFunc((AlphaFunction)op.Convert(), reference);
            GL.Enable(EnableCap.AlphaTest);
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

            static bool IsDualSource(BlendFactor factor)
            {
                switch (factor)
                {
                    case BlendFactor.Src1Color:
                    case BlendFactor.Src1ColorGl:
                    case BlendFactor.Src1Alpha:
                    case BlendFactor.Src1AlphaGl:
                    case BlendFactor.OneMinusSrc1Color:
                    case BlendFactor.OneMinusSrc1ColorGl:
                    case BlendFactor.OneMinusSrc1Alpha:
                    case BlendFactor.OneMinusSrc1AlphaGl:
                        return true;
                }

                return false;
            }

            EnsureFramebuffer();

            _framebuffer.SetDualSourceBlend(
                IsDualSource(blend.ColorSrcFactor) ||
                IsDualSource(blend.ColorDstFactor) ||
                IsDualSource(blend.AlphaSrcFactor) ||
                IsDualSource(blend.AlphaDstFactor));

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

            if (HwCapabilities.SupportsPolygonOffsetClamp)
            {
                GL.PolygonOffsetClamp(factor, units, clamp);
            }
            else
            {
                GL.PolygonOffset(factor, units);
            }
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
            if (depthTest.TestEnable)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc((DepthFunction)depthTest.Func.Convert());
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }

            GL.DepthMask(depthTest.WriteEnable);
            _depthMask = depthTest.WriteEnable;
            _depthTestEnable = depthTest.TestEnable;
        }

        public void SetFaceCulling(bool enable, Face face)
        {
            _cullEnable = enable;

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
            SetFrontFace(_frontFace = frontFace.Convert());
        }

        public void SetImage(int binding, ITexture texture, Format imageFormat)
        {
            if (texture == null)
            {
                return;
            }

            TextureBase texBase = (TextureBase)texture;

            SizedInternalFormat format = FormatTable.GetImageFormat(imageFormat);

            if (format != 0)
            {
                GL.BindImageTexture(binding, texBase.Handle, 0, true, 0, TextureAccess.ReadWrite, format);
            }
        }

        public void SetIndexBuffer(BufferRange buffer, IndexType type)
        {
            _elementsType = type.Convert();

            _indexBaseOffset = (IntPtr)buffer.Offset;

            EnsureVertexArray();

            _vertexArray.SetIndexBuffer(buffer);
        }

        public void SetLogicOpState(bool enable, LogicalOp op)
        {
            if (enable)
            {
                GL.Enable(EnableCap.ColorLogicOp);

                GL.LogicOp((LogicOp)op.Convert());
            }
            else
            {
                GL.Disable(EnableCap.ColorLogicOp);
            }
        }

        public void SetLineParameters(float width, bool smooth)
        {
            if (smooth)
            {
                GL.Enable(EnableCap.LineSmooth);
            }
            else
            {
                GL.Disable(EnableCap.LineSmooth);
            }

            GL.LineWidth(width);
        }

        public unsafe void SetPatchParameters(int vertices, ReadOnlySpan<float> defaultOuterLevel, ReadOnlySpan<float> defaultInnerLevel)
        {
            GL.PatchParameter(PatchParameterInt.PatchVertices, vertices);

            fixed (float* pOuterLevel = defaultOuterLevel)
            {
                GL.PatchParameter(PatchParameterFloat.PatchDefaultOuterLevel, pOuterLevel);
            }

            fixed (float* pInnerLevel = defaultInnerLevel)
            {
                GL.PatchParameter(PatchParameterFloat.PatchDefaultInnerLevel, pInnerLevel);
            }
        }

        public void SetPointParameters(float size, bool isProgramPointSize, bool enablePointSprite, Origin origin)
        {
            // GL_POINT_SPRITE was deprecated in core profile 3.2+ and causes GL_INVALID_ENUM when set.
            // As we don't know if the current context is core or compat, it's safer to keep this code.
            if (enablePointSprite)
            {
                GL.Enable(EnableCap.PointSprite);
            }
            else
            {
                GL.Disable(EnableCap.PointSprite);
            }

            if (isProgramPointSize)
            {
                GL.Enable(EnableCap.ProgramPointSize);
            }
            else
            {
                GL.Disable(EnableCap.ProgramPointSize);
            }

            GL.PointParameter(origin == Origin.LowerLeft
                ? PointSpriteCoordOriginParameter.LowerLeft
                : PointSpriteCoordOriginParameter.UpperLeft);

            // Games seem to set point size to 0 which generates a GL_INVALID_VALUE
            // From the spec, GL_INVALID_VALUE is generated if size is less than or equal to 0.
            GL.PointSize(Math.Max(float.Epsilon, size));
        }

        public void SetPolygonMode(GAL.PolygonMode frontMode, GAL.PolygonMode backMode)
        {
            if (frontMode == backMode)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, frontMode.Convert());
            }
            else
            {
                GL.PolygonMode(MaterialFace.Front, frontMode.Convert());
                GL.PolygonMode(MaterialFace.Back, backMode.Convert());
            }
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
            Program prg = (Program)program;

            if (_tfEnabled)
            {
                GL.EndTransformFeedback();
                prg.Bind();
                GL.BeginTransformFeedback(_tfTopology);
            }
            else
            {
                prg.Bind();
            }

            if (prg.HasFragmentShader && _fragmentOutputMap != (uint)prg.FragmentOutputMap)
            {
                _fragmentOutputMap = (uint)prg.FragmentOutputMap;

                for (int index = 0; index < Constants.MaxRenderTargets; index++)
                {
                    RestoreComponentMask(index, force: false);
                }
            }

            _program = prg;
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
            _renderScale[0].X = scale;
            _supportBuffer.UpdateRenderScale(_renderScale, 0, 1); // Just the first element.
        }

        public void SetRenderTargetColorMasks(ReadOnlySpan<uint> componentMasks)
        {
            _componentMasks = 0;

            for (int index = 0; index < componentMasks.Length; index++)
            {
                _componentMasks |= componentMasks[index] << (index * 4);

                RestoreComponentMask(index, force: false);
            }
        }

        public void SetRenderTargets(ITexture[] colors, ITexture depthStencil)
        {
            EnsureFramebuffer();

            bool isBgraChanged = false;

            for (int index = 0; index < colors.Length; index++)
            {
                TextureView color = (TextureView)colors[index];

                _framebuffer.AttachColor(index, color);

                if (color != null)
                {
                    int isBgra = color.Format.IsBgr() ? 1 : 0;

                    if (_fpIsBgra[index].X != isBgra)
                    {
                        _fpIsBgra[index].X = isBgra;
                        isBgraChanged = true;

                        RestoreComponentMask(index);
                    }
                }
            }

            if (isBgraChanged)
            {
                _supportBuffer.UpdateFragmentIsBgra(_fpIsBgra, 0, SupportBuffer.FragmentIsBgraCount);
            }

            TextureView depthStencilView = (TextureView)depthStencil;

            _framebuffer.AttachDepthStencil(depthStencilView);
            _framebuffer.SetDrawBuffers(colors.Length);
        }

        public void SetSampler(int binding, ISampler sampler)
        {
            if (sampler == null)
            {
                return;
            }

            Sampler samp = (Sampler)sampler;

            if (binding == 0)
            {
                _unit0Sampler = samp;
            }

            samp.Bind(binding);
        }

        public void SetScissor(int index, bool enable, int x, int y, int width, int height)
        {
            uint mask = 1u << index;

            if (!enable)
            {
                if ((_scissorEnables & mask) != 0)
                {
                    _scissorEnables &= ~mask;
                    GL.Disable(IndexedEnableCap.ScissorTest, index);
                }

                return;
            }

            if ((_scissorEnables & mask) == 0)
            {
                _scissorEnables |= mask;
                GL.Enable(IndexedEnableCap.ScissorTest, index);
            }

            GL.ScissorIndexed(index, x, y, width, height);
        }

        public void SetStencilTest(StencilTestDescriptor stencilTest)
        {
            _stencilTestEnable = stencilTest.TestEnable;

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

        public void SetStorageBuffers(int first, ReadOnlySpan<BufferRange> buffers)
        {
            SetBuffers(first, buffers, isStorage: true);
        }

        public void SetTexture(int binding, ITexture texture)
        {
            if (texture == null)
            {
                return;
            }

            if (binding == 0)
            {
                _unit0Texture = (TextureBase)texture;
            }
            else
            {
                ((TextureBase)texture).Bind(binding);
            }
        }

        public void SetTransformFeedbackBuffers(ReadOnlySpan<BufferRange> buffers)
        {
            if (_tfEnabled)
            {
                GL.EndTransformFeedback();
            }

            int count = Math.Min(buffers.Length, Constants.MaxTransformFeedbackBuffers);

            for (int i = 0; i < count; i++)
            {
                BufferRange buffer = buffers[i];
                _tfbTargets[i] = buffer;

                if (buffer.Handle == BufferHandle.Null)
                {
                    GL.BindBufferBase(BufferRangeTarget.TransformFeedbackBuffer, i, 0);
                    continue;
                }

                if (_tfbs[i] == BufferHandle.Null)
                {
                    _tfbs[i] = Buffer.Create();
                }

                Buffer.Resize(_tfbs[i], buffer.Size);
                Buffer.Copy(buffer.Handle, _tfbs[i], buffer.Offset, 0, buffer.Size);
                GL.BindBufferBase(BufferRangeTarget.TransformFeedbackBuffer, i, _tfbs[i].ToInt32());
            }

            if (_tfEnabled)
            {
                GL.BeginTransformFeedback(_tfTopology);
            }
        }

        public void SetUniformBuffers(int first, ReadOnlySpan<BufferRange> buffers)
        {
            SetBuffers(first, buffers, isStorage: false);
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
            Array.Resize(ref _viewportArray, viewports.Length * 4);
            Array.Resize(ref _depthRangeArray, viewports.Length * 2);

            float[] viewportArray = _viewportArray;
            double[] depthRangeArray = _depthRangeArray;

            for (int index = 0; index < viewports.Length; index++)
            {
                int viewportElemIndex = index * 4;

                Viewport viewport = viewports[index];

                viewportArray[viewportElemIndex + 0] = viewport.Region.X;
                viewportArray[viewportElemIndex + 1] = viewport.Region.Y + (viewport.Region.Height < 0 ? viewport.Region.Height : 0);
                viewportArray[viewportElemIndex + 2] = viewport.Region.Width;
                viewportArray[viewportElemIndex + 3] = MathF.Abs(viewport.Region.Height);

                if (HwCapabilities.SupportsViewportSwizzle)
                {
                    GL.NV.ViewportSwizzle(
                        index,
                        viewport.SwizzleX.Convert(),
                        viewport.SwizzleY.Convert(),
                        viewport.SwizzleZ.Convert(),
                        viewport.SwizzleW.Convert());
                }

                depthRangeArray[index * 2 + 0] = viewport.DepthNear;
                depthRangeArray[index * 2 + 1] = viewport.DepthFar;
            }

            bool flipY = viewports.Length != 0 && viewports[0].Region.Height < 0;

            SetOrigin(flipY ? ClipOrigin.UpperLeft : ClipOrigin.LowerLeft);

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

        private void SetBuffers(int first, ReadOnlySpan<BufferRange> buffers, bool isStorage)
        {
            BufferRangeTarget target = isStorage ? BufferRangeTarget.ShaderStorageBuffer : BufferRangeTarget.UniformBuffer;

            for (int index = 0; index < buffers.Length; index++)
            {
                BufferRange buffer = buffers[index];

                if (buffer.Handle == BufferHandle.Null)
                {
                    GL.BindBufferRange(target, first + index, 0, IntPtr.Zero, 0);
                    continue;
                }

                GL.BindBufferRange(target, first + index, buffer.Handle.ToInt32(), (IntPtr)buffer.Offset, buffer.Size);
            }
        }

        private void SetOrigin(ClipOrigin origin)
        {
            if (_clipOrigin != origin)
            {
                _clipOrigin = origin;

                GL.ClipControl(origin, _clipDepthMode);

                SetFrontFace(_frontFace);
            }
        }

        private void SetFrontFace(FrontFaceDirection frontFace)
        {
            // Changing clip origin will also change the front face to compensate
            // for the flipped viewport, we flip it again here to compensate as
            // this effect is undesirable for us.
            if (_clipOrigin == ClipOrigin.UpperLeft)
            {
                frontFace = frontFace == FrontFaceDirection.Ccw ? FrontFaceDirection.Cw : FrontFaceDirection.Ccw;
            }

            GL.FrontFace(frontFace);
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
            if (BackgroundContextWorker.InBackground)
            {
                return (0, 0);
            }

            return (_boundDrawFramebuffer, _boundReadFramebuffer);
        }

        public void UpdateRenderScale(ReadOnlySpan<float> scales, int totalCount, int fragmentCount)
        {
            bool changed = false;

            for (int index = 0; index < totalCount; index++)
            {
                if (_renderScale[1 + index].X != scales[index])
                {
                    _renderScale[1 + index].X = scales[index];
                    changed = true;
                }
            }

            // Only update fragment count if there are scales after it for the vertex stage.
            if (fragmentCount != totalCount && fragmentCount != _fragmentScaleCount)
            {
                _fragmentScaleCount = fragmentCount;
                _supportBuffer.UpdateFragmentRenderScaleCount(_fragmentScaleCount);
            }

            if (changed)
            {
                _supportBuffer.UpdateRenderScale(_renderScale, 0, 1 + totalCount);
            }
        }

        private void PrepareForDispatch()
        {
            _unit0Texture?.Bind(0);
            _supportBuffer.Commit();
        }

        private void PreDraw()
        {
            DrawCount++;

            _vertexArray.Validate();
            _unit0Texture?.Bind(0);
            _supportBuffer.Commit();
        }

        private void PostDraw()
        {
            if (_tfEnabled)
            {
                for (int i = 0; i < Constants.MaxTransformFeedbackBuffers; i++)
                {
                    if (_tfbTargets[i].Handle != BufferHandle.Null)
                    {
                        Buffer.Copy(_tfbs[i], _tfbTargets[i].Handle, 0, _tfbTargets[i].Offset, _tfbTargets[i].Size);
                    }
                }
            }
        }

        public void RestoreComponentMask(int index, bool force = true)
        {
            // If the bound render target is bgra, swap the red and blue masks.
            uint redMask = _fpIsBgra[index].X == 0 ? 1u : 4u;
            uint blueMask = _fpIsBgra[index].X == 0 ? 4u : 1u;

            int shift = index * 4;
            uint componentMask = _componentMasks & _fragmentOutputMap;
            uint checkMask = 0xfu << shift;
            uint componentMaskAtIndex = componentMask & checkMask;

            if (!force && componentMaskAtIndex == (_currentComponentMasks & checkMask))
            {
                return;
            }

            componentMask >>= shift;
            componentMask &= 0xfu;

            GL.ColorMask(
                index,
                (componentMask & redMask) != 0,
                (componentMask & 2u) != 0,
                (componentMask & blueMask) != 0,
                (componentMask & 8u) != 0);

            _currentComponentMasks &= ~checkMask;
            _currentComponentMasks |= componentMaskAtIndex;
        }

        public void RestoreScissor0Enable()
        {
            if ((_scissorEnables & 1u) != 0)
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

                if (compare == 0 && evt.Type == QueryTarget.SamplesPassed && evt.ClearCounter)
                {
                    if (!value.ReserveForHostAccess())
                    {
                        // If the event has been flushed, then just use the values on the CPU.
                        // The query object may already be repurposed for another draw (eg. begin + end).
                        return false;
                    }

                    GL.BeginConditionalRender(evt.Query, isEqual ? ConditionalRenderType.QueryNoWaitInverted : ConditionalRenderType.QueryNoWait);
                    _activeConditionalRender = evt;

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

            _activeConditionalRender?.ReleaseHostAccess();
            _activeConditionalRender = null;
        }

        public void Dispose()
        {
            _supportBuffer?.Dispose();

            for (int i = 0; i < Constants.MaxTransformFeedbackBuffers; i++)
            {
                if (_tfbs[i] != BufferHandle.Null)
                {
                    Buffer.Delete(_tfbs[i]);
                    _tfbs[i] = BufferHandle.Null;
                }
            }

            _activeConditionalRender?.ReleaseHostAccess();
            _framebuffer?.Dispose();
            _vertexArray?.Dispose();
            _drawTexture.Dispose();
        }
    }
}
