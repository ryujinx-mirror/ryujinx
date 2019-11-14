using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Blend;
using Ryujinx.Graphics.GAL.DepthStencil;
using Ryujinx.Graphics.GAL.InputAssembler;
using Ryujinx.Graphics.GAL.Texture;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.Shader;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private GpuContext _context;

        private ShaderCache _shaderCache;

        private ShaderProgramInfo[] _currentProgramInfo;

        private BufferManager  _bufferManager;
        private TextureManager _textureManager;

        public BufferManager  BufferManager  => _bufferManager;
        public TextureManager TextureManager => _textureManager;

        private bool _isAnyVbInstanced;
        private bool _vsUsesInstanceId;

        public Methods(GpuContext context)
        {
            _context = context;

            _shaderCache = new ShaderCache(_context);

            _currentProgramInfo = new ShaderProgramInfo[Constants.TotalShaderStages];

            _bufferManager  = new BufferManager(context);
            _textureManager = new TextureManager(context);

            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            _context.State.RegisterCallback(MethodOffset.LaunchDma,      LaunchDma);
            _context.State.RegisterCallback(MethodOffset.LoadInlineData, LoadInlineData);

            _context.State.RegisterCallback(MethodOffset.Dispatch, Dispatch);

            _context.State.RegisterCallback(MethodOffset.CopyBuffer,  CopyBuffer);
            _context.State.RegisterCallback(MethodOffset.CopyTexture, CopyTexture);

            _context.State.RegisterCallback(MethodOffset.TextureBarrier,      TextureBarrier);
            _context.State.RegisterCallback(MethodOffset.InvalidateTextures,  InvalidateTextures);
            _context.State.RegisterCallback(MethodOffset.TextureBarrierTiled, TextureBarrierTiled);

            _context.State.RegisterCallback(MethodOffset.ResetCounter, ResetCounter);

            _context.State.RegisterCallback(MethodOffset.DrawEnd,   DrawEnd);
            _context.State.RegisterCallback(MethodOffset.DrawBegin, DrawBegin);

            _context.State.RegisterCallback(MethodOffset.IndexBufferCount, SetIndexBufferCount);

            _context.State.RegisterCallback(MethodOffset.Clear, Clear);

            _context.State.RegisterCallback(MethodOffset.Report, Report);

            _context.State.RegisterCallback(MethodOffset.UniformBufferUpdateData, 16, UniformBufferUpdate);

            _context.State.RegisterCallback(MethodOffset.UniformBufferBindVertex,         UniformBufferBindVertex);
            _context.State.RegisterCallback(MethodOffset.UniformBufferBindTessControl,    UniformBufferBindTessControl);
            _context.State.RegisterCallback(MethodOffset.UniformBufferBindTessEvaluation, UniformBufferBindTessEvaluation);
            _context.State.RegisterCallback(MethodOffset.UniformBufferBindGeometry,       UniformBufferBindGeometry);
            _context.State.RegisterCallback(MethodOffset.UniformBufferBindFragment,       UniformBufferBindFragment);
        }

        public Image.Texture GetTexture(ulong address) => _textureManager.Find2(address);

        private void UpdateState()
        {
            // Shaders must be the first one to be updated if modified, because
            // some of the other state depends on information from the currently
            // bound shaders.
            if (_context.State.QueryModified(MethodOffset.ShaderBaseAddress, MethodOffset.ShaderState))
            {
                UpdateShaderState();
            }

            UpdateRenderTargetStateIfNeeded();

            if (_context.State.QueryModified(MethodOffset.DepthTestEnable,
                                             MethodOffset.DepthWriteEnable,
                                             MethodOffset.DepthTestFunc))
            {
                UpdateDepthTestState();
            }

            if (_context.State.QueryModified(MethodOffset.ViewportTransform, MethodOffset.ViewportExtents))
            {
                UpdateViewportTransform();
            }

            if (_context.State.QueryModified(MethodOffset.DepthBiasState,
                                             MethodOffset.DepthBiasFactor,
                                             MethodOffset.DepthBiasUnits,
                                             MethodOffset.DepthBiasClamp))
            {
                UpdateDepthBiasState();
            }

            if (_context.State.QueryModified(MethodOffset.StencilBackMasks,
                                             MethodOffset.StencilTestState,
                                             MethodOffset.StencilBackTestState))
            {
                UpdateStencilTestState();
            }

            // Pools.
            if (_context.State.QueryModified(MethodOffset.SamplerPoolState))
            {
                UpdateSamplerPoolState();
            }

            if (_context.State.QueryModified(MethodOffset.TexturePoolState))
            {
                UpdateTexturePoolState();
            }

            // Input assembler state.
            if (_context.State.QueryModified(MethodOffset.VertexAttribState))
            {
                UpdateVertexAttribState();
            }

            if (_context.State.QueryModified(MethodOffset.PrimitiveRestartState))
            {
                UpdatePrimitiveRestartState();
            }

            if (_context.State.QueryModified(MethodOffset.IndexBufferState))
            {
                UpdateIndexBufferState();
            }

            if (_context.State.QueryModified(MethodOffset.VertexBufferDrawState,
                                             MethodOffset.VertexBufferInstanced,
                                             MethodOffset.VertexBufferState,
                                             MethodOffset.VertexBufferEndAddress))
            {
                UpdateVertexBufferState();
            }

            if (_context.State.QueryModified(MethodOffset.FaceState))
            {
                UpdateFaceState();
            }

            if (_context.State.QueryModified(MethodOffset.RtColorMask))
            {
                UpdateRtColorMask();
            }

            if (_context.State.QueryModified(MethodOffset.BlendEnable, MethodOffset.BlendState))
            {
                UpdateBlendState();
            }

            CommitBindings();
        }

        private void CommitBindings()
        {
            UpdateStorageBuffers();

            _bufferManager.CommitBindings();
            _textureManager.CommitGraphicsBindings();
        }

        private void UpdateStorageBuffers()
        {
            for (int stage = 0; stage < _currentProgramInfo.Length; stage++)
            {
                ShaderProgramInfo info = _currentProgramInfo[stage];

                if (info == null)
                {
                    continue;
                }

                for (int index = 0; index < info.SBuffers.Count; index++)
                {
                    BufferDescriptor sb = info.SBuffers[index];

                    ulong sbDescAddress = _bufferManager.GetGraphicsUniformBufferAddress(stage, 0);

                    int sbDescOffset = 0x110 + stage * 0x100 + sb.Slot * 0x10;

                    sbDescAddress += (ulong)sbDescOffset;

                    Span<byte> sbDescriptorData = _context.PhysicalMemory.Read(sbDescAddress, 0x10);

                    SbDescriptor sbDescriptor = MemoryMarshal.Cast<byte, SbDescriptor>(sbDescriptorData)[0];

                    _bufferManager.SetGraphicsStorageBuffer(stage, sb.Slot, sbDescriptor.PackAddress(), (uint)sbDescriptor.Size);
                }
            }
        }

        private void UpdateRenderTargetStateIfNeeded()
        {
            if (_context.State.QueryModified(MethodOffset.RtColorState,
                                             MethodOffset.RtDepthStencilState,
                                             MethodOffset.RtDepthStencilSize,
                                             MethodOffset.RtDepthStencilEnable))
            {
                UpdateRenderTargetState();
            }
        }

        private void UpdateRenderTargetState()
        {
            var msaaMode = _context.State.Get<TextureMsaaMode>(MethodOffset.RtMsaaMode);

            int samplesInX = msaaMode.SamplesInX();
            int samplesInY = msaaMode.SamplesInY();

            for (int index = 0; index < Constants.TotalRenderTargets; index++)
            {
                var colorState = _context.State.Get<RtColorState>(MethodOffset.RtColorState, index);

                if (!IsRtEnabled(colorState))
                {
                    _textureManager.SetRenderTargetColor(index, null);

                    continue;
                }

                Image.Texture color = _textureManager.FindOrCreateTexture(
                    colorState,
                    samplesInX,
                    samplesInY);

                _textureManager.SetRenderTargetColor(index, color);

                if (color != null)
                {
                    color.Modified = true;
                }
            }

            bool dsEnable = _context.State.Get<Boolean32>(MethodOffset.RtDepthStencilEnable);

            Image.Texture depthStencil = null;

            if (dsEnable)
            {
                var dsState = _context.State.Get<RtDepthStencilState>(MethodOffset.RtDepthStencilState);
                var dsSize  = _context.State.Get<Size3D>             (MethodOffset.RtDepthStencilSize);

                depthStencil = _textureManager.FindOrCreateTexture(
                    dsState,
                    dsSize,
                    samplesInX,
                    samplesInY);
            }

            _textureManager.SetRenderTargetDepthStencil(depthStencil);

            if (depthStencil != null)
            {
                depthStencil.Modified = true;
            }
        }

        private static bool IsRtEnabled(RtColorState colorState)
        {
            // Colors are disabled by writing 0 to the format.
            return colorState.Format != 0 && colorState.WidthOrStride != 0;
        }

        private void UpdateDepthTestState()
        {
            _context.Renderer.Pipeline.SetDepthTest(new DepthTestDescriptor(
                _context.State.Get<Boolean32>(MethodOffset.DepthTestEnable),
                _context.State.Get<Boolean32>(MethodOffset.DepthWriteEnable),
                _context.State.Get<CompareOp>(MethodOffset.DepthTestFunc)));
        }

        private void UpdateViewportTransform()
        {
            Viewport[] viewports = new Viewport[Constants.TotalViewports];

            for (int index = 0; index < Constants.TotalViewports; index++)
            {
                var transform = _context.State.Get<ViewportTransform>(MethodOffset.ViewportTransform, index);
                var extents   = _context.State.Get<ViewportExtents>  (MethodOffset.ViewportExtents,   index);

                float x = transform.TranslateX - MathF.Abs(transform.ScaleX);
                float y = transform.TranslateY - MathF.Abs(transform.ScaleY);

                float width  = transform.ScaleX * 2;
                float height = transform.ScaleY * 2;

                RectangleF region = new RectangleF(x, y, width, height);

                viewports[index] = new Viewport(
                    region,
                    transform.UnpackSwizzleX(),
                    transform.UnpackSwizzleY(),
                    transform.UnpackSwizzleZ(),
                    transform.UnpackSwizzleW(),
                    extents.DepthNear,
                    extents.DepthFar);
            }

            _context.Renderer.Pipeline.SetViewports(0, viewports);
        }

        private void UpdateDepthBiasState()
        {
            var depthBias = _context.State.Get<DepthBiasState>(MethodOffset.DepthBiasState);

            float factor = _context.State.Get<float>(MethodOffset.DepthBiasFactor);
            float units  = _context.State.Get<float>(MethodOffset.DepthBiasUnits);
            float clamp  = _context.State.Get<float>(MethodOffset.DepthBiasClamp);

            PolygonModeMask enables = 0;

            enables  = (depthBias.PointEnable ? PolygonModeMask.Point : 0);
            enables |= (depthBias.LineEnable  ? PolygonModeMask.Line  : 0);
            enables |= (depthBias.FillEnable  ? PolygonModeMask.Fill  : 0);

            _context.Renderer.Pipeline.SetDepthBias(enables, factor, units, clamp);
        }

        private void UpdateStencilTestState()
        {
            var backMasks = _context.State.Get<StencilBackMasks>    (MethodOffset.StencilBackMasks);
            var test      = _context.State.Get<StencilTestState>    (MethodOffset.StencilTestState);
            var backTest  = _context.State.Get<StencilBackTestState>(MethodOffset.StencilBackTestState);

            CompareOp backFunc;
            StencilOp backSFail;
            StencilOp backDpPass;
            StencilOp backDpFail;
            int       backFuncRef;
            int       backFuncMask;
            int       backMask;

            if (backTest.TwoSided)
            {
                backFunc     = backTest.BackFunc;
                backSFail    = backTest.BackSFail;
                backDpPass   = backTest.BackDpPass;
                backDpFail   = backTest.BackDpFail;
                backFuncRef  = backMasks.FuncRef;
                backFuncMask = backMasks.FuncMask;
                backMask     = backMasks.Mask;
            }
            else
            {
                backFunc     = test.FrontFunc;
                backSFail    = test.FrontSFail;
                backDpPass   = test.FrontDpPass;
                backDpFail   = test.FrontDpFail;
                backFuncRef  = test.FrontFuncRef;
                backFuncMask = test.FrontFuncMask;
                backMask     = test.FrontMask;
            }

            _context.Renderer.Pipeline.SetStencilTest(new StencilTestDescriptor(
                test.Enable,
                test.FrontFunc,
                test.FrontSFail,
                test.FrontDpPass,
                test.FrontDpFail,
                test.FrontFuncRef,
                test.FrontFuncMask,
                test.FrontMask,
                backFunc,
                backSFail,
                backDpPass,
                backDpFail,
                backFuncRef,
                backFuncMask,
                backMask));
        }

        private void UpdateSamplerPoolState()
        {
            var samplerPool = _context.State.Get<PoolState>(MethodOffset.SamplerPoolState);

            _textureManager.SetGraphicsSamplerPool(samplerPool.Address.Pack(), samplerPool.MaximumId);
        }

        private void UpdateTexturePoolState()
        {
            var texturePool = _context.State.Get<PoolState>(MethodOffset.TexturePoolState);

            _textureManager.SetGraphicsTexturePool(texturePool.Address.Pack(), texturePool.MaximumId);

            _textureManager.SetGraphicsTextureBufferIndex(_context.State.Get<int>(MethodOffset.TextureBufferIndex));
        }

        private void UpdateVertexAttribState()
        {
            VertexAttribDescriptor[] vertexAttribs = new VertexAttribDescriptor[16];

            for (int index = 0; index < 16; index++)
            {
                var vertexAttrib = _context.State.Get<VertexAttribState>(MethodOffset.VertexAttribState, index);

                if (!FormatTable.TryGetAttribFormat(vertexAttrib.UnpackFormat(), out Format format))
                {
                    // TODO: warning.

                    format = Format.R32G32B32A32Float;
                }

                vertexAttribs[index] = new VertexAttribDescriptor(
                    vertexAttrib.UnpackBufferIndex(),
                    vertexAttrib.UnpackOffset(),
                    format);
            }

            _context.Renderer.Pipeline.BindVertexAttribs(vertexAttribs);
        }

        private void UpdatePrimitiveRestartState()
        {
            PrimitiveRestartState primitiveRestart = _context.State.Get<PrimitiveRestartState>(MethodOffset.PrimitiveRestartState);

            _context.Renderer.Pipeline.SetPrimitiveRestart(
                primitiveRestart.Enable,
                primitiveRestart.Index);
        }

        private void UpdateIndexBufferState()
        {
            var indexBuffer = _context.State.Get<IndexBufferState>(MethodOffset.IndexBufferState);

            _firstIndex = indexBuffer.First;
            _indexCount = indexBuffer.Count;

            if (_indexCount == 0)
            {
                return;
            }

            ulong gpuVa = indexBuffer.Address.Pack();

            // Do not use the end address to calculate the size, because
            // the result may be much larger than the real size of the index buffer.
            ulong size = (ulong)(_firstIndex + _indexCount);

            switch (indexBuffer.Type)
            {
                case IndexType.UShort: size *= 2; break;
                case IndexType.UInt:   size *= 4; break;
            }

            _bufferManager.SetIndexBuffer(gpuVa, size, indexBuffer.Type);

            // The index buffer affects the vertex buffer size calculation, we
            // need to ensure that they are updated.
            UpdateVertexBufferState();
        }

        private void UpdateVertexBufferState()
        {
            _isAnyVbInstanced = false;

            for (int index = 0; index < 16; index++)
            {
                var vertexBuffer = _context.State.Get<VertexBufferState>(MethodOffset.VertexBufferState, index);

                if (!vertexBuffer.UnpackEnable())
                {
                    _bufferManager.SetVertexBuffer(index, 0, 0, 0, 0);

                    continue;
                }

                GpuVa endAddress = _context.State.Get<GpuVa>(MethodOffset.VertexBufferEndAddress, index);

                ulong address = vertexBuffer.Address.Pack();

                int stride = vertexBuffer.UnpackStride();

                bool instanced = _context.State.Get<Boolean32>(MethodOffset.VertexBufferInstanced + index);

                int divisor = instanced ? vertexBuffer.Divisor : 0;

                _isAnyVbInstanced |= divisor != 0;

                ulong size;

                if (_drawIndexed || stride == 0 || instanced)
                {
                    // This size may be (much) larger than the real vertex buffer size.
                    // Avoid calculating it this way, unless we don't have any other option.
                    size = endAddress.Pack() - address + 1;
                }
                else
                {
                    // For non-indexed draws, we can guess the size from the vertex count
                    // and stride.
                    int firstInstance = _context.State.Get<int>(MethodOffset.FirstInstance);

                    var drawState = _context.State.Get<VertexBufferDrawState>(MethodOffset.VertexBufferDrawState);

                    size = (ulong)((firstInstance + drawState.First + drawState.Count) * stride);
                }

                _bufferManager.SetVertexBuffer(index, address, size, stride, divisor);
            }
        }

        private void UpdateFaceState()
        {
            var face = _context.State.Get<FaceState>(MethodOffset.FaceState);

            _context.Renderer.Pipeline.SetFaceCulling(face.CullEnable, face.CullFace);

            _context.Renderer.Pipeline.SetFrontFace(face.FrontFace);
        }

        private void UpdateRtColorMask()
        {
            uint[] componentMasks = new uint[Constants.TotalRenderTargets];

            for (int index = 0; index < Constants.TotalRenderTargets; index++)
            {
                var colorMask = _context.State.Get<RtColorMask>(MethodOffset.RtColorMask, index);

                uint componentMask = 0;

                componentMask  = (colorMask.UnpackRed()   ? 1u : 0u);
                componentMask |= (colorMask.UnpackGreen() ? 2u : 0u);
                componentMask |= (colorMask.UnpackBlue()  ? 4u : 0u);
                componentMask |= (colorMask.UnpackAlpha() ? 8u : 0u);

                componentMasks[index] = componentMask;
            }

            _context.Renderer.Pipeline.SetRenderTargetColorMasks(componentMasks);
        }

        private void UpdateBlendState()
        {
            BlendState[] blends = new BlendState[8];

            for (int index = 0; index < 8; index++)
            {
                bool enable = _context.State.Get<Boolean32>(MethodOffset.BlendEnable, index);

                var blend = _context.State.Get<BlendState>(MethodOffset.BlendState, index);

                BlendDescriptor descriptor = new BlendDescriptor(
                    enable,
                    blend.ColorOp,
                    blend.ColorSrcFactor,
                    blend.ColorDstFactor,
                    blend.AlphaOp,
                    blend.AlphaSrcFactor,
                    blend.AlphaDstFactor);

                _context.Renderer.Pipeline.BindBlendState(index, descriptor);
            }
        }

        private struct SbDescriptor
        {
            public uint AddressLow;
            public uint AddressHigh;
            public int  Size;
            public int  Padding;

            public ulong PackAddress()
            {
                return AddressLow | ((ulong)AddressHigh << 32);
            }
        }

        private void UpdateShaderState()
        {
            ShaderAddresses addresses = new ShaderAddresses();

            Span<ShaderAddresses> addressesSpan = MemoryMarshal.CreateSpan(ref addresses, 1);

            Span<ulong> addressesArray = MemoryMarshal.Cast<ShaderAddresses, ulong>(addressesSpan);

            ulong baseAddress = _context.State.Get<GpuVa>(MethodOffset.ShaderBaseAddress).Pack();

            for (int index = 0; index < 6; index++)
            {
                var shader = _context.State.Get<ShaderState>(MethodOffset.ShaderState, index);

                if (!shader.UnpackEnable() && index != 1)
                {
                    continue;
                }

                addressesArray[index] = baseAddress + shader.Offset;
            }

            GraphicsShader gs = _shaderCache.GetGraphicsShader(addresses);

            _vsUsesInstanceId = gs.Shader[0].Program.Info.UsesInstanceId;

            for (int stage = 0; stage < Constants.TotalShaderStages; stage++)
            {
                ShaderProgramInfo info = gs.Shader[stage].Program?.Info;

                _currentProgramInfo[stage] = info;

                if (info == null)
                {
                    continue;
                }

                var textureBindings = new TextureBindingInfo[info.Textures.Count];

                for (int index = 0; index < info.Textures.Count; index++)
                {
                    var descriptor = info.Textures[index];

                    Target target = GetTarget(descriptor.Type);

                    textureBindings[index] = new TextureBindingInfo(target, descriptor.HandleIndex);
                }

                _textureManager.SetGraphicsTextures(stage, textureBindings);

                var imageBindings = new TextureBindingInfo[info.Images.Count];

                for (int index = 0; index < info.Images.Count; index++)
                {
                    var descriptor = info.Images[index];

                    Target target = GetTarget(descriptor.Type);

                    imageBindings[index] = new TextureBindingInfo(target, descriptor.HandleIndex);
                }

                _textureManager.SetGraphicsImages(stage, imageBindings);

                uint sbEnableMask = 0;
                uint ubEnableMask = 0;

                for (int index = 0; index < info.SBuffers.Count; index++)
                {
                    sbEnableMask |= 1u << info.SBuffers[index].Slot;
                }

                for (int index = 0; index < info.CBuffers.Count; index++)
                {
                    ubEnableMask |= 1u << info.CBuffers[index].Slot;
                }

                _bufferManager.SetGraphicsStorageBufferEnableMask(stage, sbEnableMask);
                _bufferManager.SetGraphicsUniformBufferEnableMask(stage, ubEnableMask);
            }

            _context.Renderer.Pipeline.BindProgram(gs.HostProgram);
        }

        private static Target GetTarget(SamplerType type)
        {
            type &= ~(SamplerType.Indexed | SamplerType.Shadow);

            switch (type)
            {
                case SamplerType.Texture1D:
                    return Target.Texture1D;

                case SamplerType.TextureBuffer:
                    return Target.TextureBuffer;

                case SamplerType.Texture1D | SamplerType.Array:
                    return Target.Texture1DArray;

                case SamplerType.Texture2D:
                    return Target.Texture2D;

                case SamplerType.Texture2D | SamplerType.Array:
                    return Target.Texture2DArray;

                case SamplerType.Texture2D | SamplerType.Multisample:
                    return Target.Texture2DMultisample;

                case SamplerType.Texture2D | SamplerType.Multisample | SamplerType.Array:
                    return Target.Texture2DMultisampleArray;

                case SamplerType.Texture3D:
                    return Target.Texture3D;

                case SamplerType.TextureCube:
                    return Target.Cubemap;

                case SamplerType.TextureCube | SamplerType.Array:
                    return Target.CubemapArray;
            }

            // TODO: Warning.

            return Target.Texture2D;
        }

        private void TextureBarrier(int argument)
        {
            _context.Renderer.Pipeline.TextureBarrier();
        }

        private void InvalidateTextures(int argument)
        {
            _textureManager.Flush();
        }

        private void TextureBarrierTiled(int argument)
        {
            _context.Renderer.Pipeline.TextureBarrierTiled();
        }
    }
}