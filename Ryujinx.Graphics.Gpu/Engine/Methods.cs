using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Blend;
using Ryujinx.Graphics.GAL.DepthStencil;
using Ryujinx.Graphics.GAL.InputAssembler;
using Ryujinx.Graphics.GAL.Texture;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Memory;
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

            _bufferManager  = new BufferManager(context);
            _textureManager = new TextureManager(context);

            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            _context.State.RegisterCopyBufferCallback(CopyBuffer);
            _context.State.RegisterCopyTextureCallback(CopyTexture);

            _context.State.RegisterDrawEndCallback(DrawEnd);

            _context.State.RegisterDrawBeginCallback(DrawBegin);

            _context.State.RegisterSetIndexCountCallback(SetIndexCount);

            _context.State.RegisterClearCallback(Clear);

            _context.State.RegisterReportCallback(Report);

            _context.State.RegisterUniformBufferUpdateCallback(UniformBufferUpdate);

            _context.State.RegisterUniformBufferBind0Callback(UniformBufferBind0);
            _context.State.RegisterUniformBufferBind1Callback(UniformBufferBind1);
            _context.State.RegisterUniformBufferBind2Callback(UniformBufferBind2);
            _context.State.RegisterUniformBufferBind3Callback(UniformBufferBind3);
            _context.State.RegisterUniformBufferBind4Callback(UniformBufferBind4);

            _context.State.RegisterCallback(MethodOffset.TextureBarrier,      TextureBarrier);
            _context.State.RegisterCallback(MethodOffset.InvalidateTextures,  InvalidateTextures);
            _context.State.RegisterCallback(MethodOffset.TextureBarrierTiled, TextureBarrierTiled);


            _context.State.RegisterCallback(MethodOffset.ResetCounter, ResetCounter);

            _context.State.RegisterCallback(MethodOffset.Inline2MemoryExecute,  Execute);
            _context.State.RegisterCallback(MethodOffset.Inline2MemoryPushData, PushData);

            _context.State.RegisterCallback(MethodOffset.Dispatch, Dispatch);
        }

        public Image.Texture GetTexture(ulong address) => _textureManager.Find2(address);

        private void UpdateState()
        {
            if ((_context.State.StateWriteFlags & StateWriteFlags.Any) == 0)
            {
                CommitBindings();

                return;
            }

            // Shaders must be the first one to be updated if modified, because
            // some of the other state depends on information from the currently
            // bound shaders.
            if ((_context.State.StateWriteFlags & StateWriteFlags.ShaderState) != 0)
            {
                UpdateShaderState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.RenderTargetGroup) != 0)
            {
                UpdateRenderTargetGroupState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.DepthTestState) != 0)
            {
                UpdateDepthTestState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.ViewportTransform) != 0)
            {
                UpdateViewportTransform();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.DepthBiasState) != 0)
            {
                UpdateDepthBiasState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.StencilTestState) != 0)
            {
                UpdateStencilTestState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.SamplerPoolState) != 0)
            {
                UpdateSamplerPoolState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.TexturePoolState) != 0)
            {
                UpdateTexturePoolState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.InputAssemblerGroup) != 0)
            {
                UpdateInputAssemblerGroupState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.FaceState) != 0)
            {
                UpdateFaceState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.RtColorMask) != 0)
            {
                UpdateRtColorMask();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.BlendState) != 0)
            {
                UpdateBlendState();
            }

            _context.State.StateWriteFlags &= ~StateWriteFlags.Any;

            CommitBindings();
        }

        private void CommitBindings()
        {
            _bufferManager.CommitBindings();
            _textureManager.CommitGraphicsBindings();
        }

        private void UpdateRenderTargetGroupState()
        {
            TextureMsaaMode msaaMode = _context.State.GetRtMsaaMode();

            int samplesInX = msaaMode.SamplesInX();
            int samplesInY = msaaMode.SamplesInY();

            Image.Texture color3D = Get3DRenderTarget(samplesInX, samplesInY);

            if (color3D == null)
            {
                for (int index = 0; index < Constants.TotalRenderTargets; index++)
                {
                    RtColorState colorState = _context.State.GetRtColorState(index);

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

                    color.Modified = true;
                }
            }
            else
            {
                _textureManager.SetRenderTargetColor3D(color3D);

                color3D.Modified = true;
            }

            bool dsEnable = _context.State.Get<bool>(MethodOffset.RtDepthStencilEnable);

            Image.Texture depthStencil = null;

            if (dsEnable)
            {
                var dsState = _context.State.GetRtDepthStencilState();
                var dsSize  = _context.State.GetRtDepthStencilSize();

                depthStencil = _textureManager.FindOrCreateTexture(
                    dsState,
                    dsSize,
                    samplesInX,
                    samplesInY);
            }

            _textureManager.SetRenderTargetDepthStencil(depthStencil);
        }

        private Image.Texture Get3DRenderTarget(int samplesInX, int samplesInY)
        {
            RtColorState colorState0 = _context.State.GetRtColorState(0);

            if (!IsRtEnabled(colorState0) || !colorState0.MemoryLayout.UnpackIsTarget3D() || colorState0.Depth != 1)
            {
                return null;
            }

            int slices = 1;
            int unused = 0;

            for (int index = 1; index < Constants.TotalRenderTargets; index++)
            {
                RtColorState colorState = _context.State.GetRtColorState(index);

                if (!IsRtEnabled(colorState))
                {
                    unused++;

                    continue;
                }

                if (colorState.MemoryLayout.UnpackIsTarget3D() && colorState.Depth == 1)
                {
                    slices++;
                }
            }

            if (slices + unused == Constants.TotalRenderTargets)
            {
                colorState0.Depth = slices;

                return _textureManager.FindOrCreateTexture(colorState0, samplesInX, samplesInY);
            }

            return null;
        }

        private static bool IsRtEnabled(RtColorState colorState)
        {
            // Colors are disabled by writing 0 to the format.
            return colorState.Format != 0 && colorState.WidthOrStride != 0;
        }

        private void UpdateDepthTestState()
        {
            _context.Renderer.Pipeline.SetDepthTest(new DepthTestDescriptor(
                _context.State.GetDepthTestEnable().IsTrue(),
                _context.State.GetDepthWriteEnable().IsTrue(),
                _context.State.GetDepthTestFunc()));
        }

        private void UpdateViewportTransform()
        {
            Viewport[] viewports = new Viewport[Constants.TotalViewports];

            for (int index = 0; index < Constants.TotalViewports; index++)
            {
                var transform = _context.State.Get<ViewportTransform>(MethodOffset.ViewportTransform + index * 8);
                var extents   = _context.State.Get<ViewportExtents>  (MethodOffset.ViewportExtents   + index * 4);

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
            var polygonOffset = _context.State.Get<DepthBiasState>(MethodOffset.DepthBiasState);

            float factor = _context.State.Get<float>(MethodOffset.DepthBiasFactor);
            float units  = _context.State.Get<float>(MethodOffset.DepthBiasUnits);
            float clamp  = _context.State.Get<float>(MethodOffset.DepthBiasClamp);

            PolygonModeMask enables = 0;

            enables  = (polygonOffset.PointEnable.IsTrue() ? PolygonModeMask.Point : 0);
            enables |= (polygonOffset.LineEnable.IsTrue()  ? PolygonModeMask.Line  : 0);
            enables |= (polygonOffset.FillEnable.IsTrue()  ? PolygonModeMask.Fill  : 0);

            _context.Renderer.Pipeline.SetDepthBias(enables, factor, units, clamp);
        }

        private void UpdateStencilTestState()
        {
            StencilBackMasks     backMasks = _context.State.GetStencilBackMasks();
            StencilTestState     test      = _context.State.GetStencilTestState();
            StencilBackTestState backTest  = _context.State.GetStencilBackTestState();

            CompareOp backFunc;
            StencilOp backSFail;
            StencilOp backDpPass;
            StencilOp backDpFail;
            int       backFuncRef;
            int       backFuncMask;
            int       backMask;

            if (backTest.TwoSided.IsTrue())
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
                test.Enable.IsTrue(),
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
            PoolState samplerPool = _context.State.GetSamplerPoolState();

            _textureManager.SetGraphicsSamplerPool(samplerPool.Address.Pack(), samplerPool.MaximumId);
        }

        private void UpdateTexturePoolState()
        {
            PoolState texturePool = _context.State.GetTexturePoolState();

            _textureManager.SetGraphicsTexturePool(texturePool.Address.Pack(), texturePool.MaximumId);

            _textureManager.SetGraphicsTextureBufferIndex(_context.State.GetTextureBufferIndex());
        }

        private void UpdateInputAssemblerGroupState()
        {
            // Must be updated before the vertex buffer.
            if ((_context.State.StateWriteFlags & StateWriteFlags.VertexAttribState) != 0)
            {
                UpdateVertexAttribState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.PrimitiveRestartState) != 0)
            {
                UpdatePrimitiveRestartState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.IndexBufferState) != 0)
            {
                UpdateIndexBufferState();
            }

            if ((_context.State.StateWriteFlags & StateWriteFlags.VertexBufferState) != 0)
            {
                UpdateVertexBufferState();
            }
        }

        private void UpdateVertexAttribState()
        {
            VertexAttribDescriptor[] vertexAttribs = new VertexAttribDescriptor[16];

            for (int index = 0; index < 16; index++)
            {
                VertexAttribState vertexAttrib = _context.State.GetVertexAttribState(index);

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
            IndexBufferState indexBuffer = _context.State.GetIndexBufferState();

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

        private uint GetIndexBufferMaxIndex(ulong gpuVa, ulong size, IndexType type)
        {
            ulong address = _context.MemoryManager.Translate(gpuVa);

            Span<byte> data = _context.PhysicalMemory.Read(address, size);

            uint maxIndex = 0;

            switch (type)
            {
                case IndexType.UByte:
                {
                    for (int index = 0; index < data.Length; index++)
                    {
                        if (maxIndex < data[index])
                        {
                            maxIndex = data[index];
                        }
                    }

                    break;
                }

                case IndexType.UShort:
                {
                    Span<ushort> indices = MemoryMarshal.Cast<byte, ushort>(data);

                    for (int index = 0; index < indices.Length; index++)
                    {
                        if (maxIndex < indices[index])
                        {
                            maxIndex = indices[index];
                        }
                    }

                    break;
                }

                case IndexType.UInt:
                {
                    Span<uint> indices = MemoryMarshal.Cast<byte, uint>(data);

                    for (int index = 0; index < indices.Length; index++)
                    {
                        if (maxIndex < indices[index])
                        {
                            maxIndex = indices[index];
                        }
                    }

                    break;
                }
            }

            return maxIndex;
        }

        private void UpdateVertexBufferState()
        {
            _isAnyVbInstanced = false;

            for (int index = 0; index < 16; index++)
            {
                VertexBufferState vertexBuffer = _context.State.GetVertexBufferState(index);

                if (!vertexBuffer.UnpackEnable())
                {
                    _bufferManager.SetVertexBuffer(index, 0, 0, 0, 0);

                    continue;
                }

                GpuVa endAddress = _context.State.GetVertexBufferEndAddress(index);

                ulong address = vertexBuffer.Address.Pack();

                int stride = vertexBuffer.UnpackStride();

                bool instanced = _context.State.Get<bool>(MethodOffset.VertexBufferInstanced + index);

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
                    int firstInstance = _context.State.GetBaseInstance();

                    VertexBufferDrawState drawState = _context.State.GetVertexBufferDrawState();

                    size = (ulong)((firstInstance + drawState.First + drawState.Count) * stride);
                }

                _bufferManager.SetVertexBuffer(index, address, size, stride, divisor);
            }
        }

        private void UpdateFaceState()
        {
            FaceState face = _context.State.GetFaceState();

            _context.Renderer.Pipeline.SetFaceCulling(face.CullEnable.IsTrue(), face.CullFace);

            _context.Renderer.Pipeline.SetFrontFace(face.FrontFace);
        }

        private void UpdateRtColorMask()
        {
            uint[] componentMasks = new uint[Constants.TotalRenderTargets];

            for (int index = 0; index < Constants.TotalRenderTargets; index++)
            {
                RtColorMask colorMask = _context.State.Get<RtColorMask>(MethodOffset.RtColorMask + index);

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
                bool blendEnable = _context.State.GetBlendEnable(index).IsTrue();

                BlendState blend = _context.State.GetBlendState(index);

                BlendDescriptor descriptor = new BlendDescriptor(
                    blendEnable,
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

            ulong baseAddress = _context.State.GetShaderBaseAddress().Pack();

            for (int index = 0; index < 6; index++)
            {
                ShaderState shader = _context.State.GetShaderState(index);

                if (!shader.UnpackEnable() && index != 1)
                {
                    continue;
                }

                addressesArray[index] = baseAddress + shader.Offset;
            }

            GraphicsShader gs = _shaderCache.GetGraphicsShader(addresses);

            _vsUsesInstanceId = gs.Shader[0].Info.UsesInstanceId;

            for (int stage = 0; stage < Constants.TotalShaderStages; stage++)
            {
                ShaderProgramInfo info = gs.Shader[stage]?.Info;

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
                    BufferDescriptor sb = info.SBuffers[index];

                    sbEnableMask |= 1u << sb.Slot;

                    ulong sbDescAddress = _bufferManager.GetGraphicsUniformBufferAddress(stage, 0);

                    int sbDescOffset = 0x110 + stage * 0x100 + sb.Slot * 0x10;

                    sbDescAddress += (ulong)sbDescOffset;

                    Span<byte> sbDescriptorData = _context.PhysicalMemory.Read(sbDescAddress, 0x10);

                    SbDescriptor sbDescriptor = MemoryMarshal.Cast<byte, SbDescriptor>(sbDescriptorData)[0];

                    _bufferManager.SetGraphicsStorageBuffer(stage, sb.Slot, sbDescriptor.PackAddress(), (uint)sbDescriptor.Size);
                }

                for (int index = 0; index < info.CBuffers.Count; index++)
                {
                    ubEnableMask |= 1u << info.CBuffers[index].Slot;
                }

                _bufferManager.SetGraphicsStorageBufferEnableMask(stage, sbEnableMask);
                _bufferManager.SetGraphicsUniformBufferEnableMask(stage, ubEnableMask);
            }

            _context.Renderer.Pipeline.BindProgram(gs.Interface);
        }

        private static Target GetTarget(SamplerType type)
        {
            type &= ~SamplerType.Shadow;

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