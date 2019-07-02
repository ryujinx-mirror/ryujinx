using Ryujinx.Common;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Texture;
using System;
using System.Collections.Generic;
using Ryujinx.Profiler;

namespace Ryujinx.Graphics.Graphics3d
{
    class NvGpuEngine3d : INvGpuEngine
    {
        public int[] Registers { get; private set; }

        private NvGpu _gpu;

        private Dictionary<int, NvGpuMethod> _methods;

        private struct ConstBuffer
        {
            public bool Enabled;
            public long Position;
            public int  Size;
        }

        private ConstBuffer[][] _constBuffers;

        // Viewport dimensions kept for scissor test limits
        private int _viewportX0 = 0;
        private int _viewportY0 = 0;
        private int _viewportX1 = 0;
        private int _viewportY1 = 0;
        private int _viewportWidth = 0;
        private int _viewportHeight = 0;

        private int _currentInstance = 0;

        public NvGpuEngine3d(NvGpu gpu)
        {
            _gpu = gpu;

            Registers = new int[0xe00];

            _methods = new Dictionary<int, NvGpuMethod>();

            void AddMethod(int meth, int count, int stride, NvGpuMethod method)
            {
                while (count-- > 0)
                {
                    _methods.Add(meth, method);

                    meth += stride;
                }
            }

            AddMethod(0x585,  1, 1, VertexEndGl);
            AddMethod(0x674,  1, 1, ClearBuffers);
            AddMethod(0x6c3,  1, 1, QueryControl);
            AddMethod(0x8e4, 16, 1, CbData);
            AddMethod(0x904,  5, 8, CbBind);

            _constBuffers = new ConstBuffer[6][];

            for (int index = 0; index < _constBuffers.Length; index++)
            {
                _constBuffers[index] = new ConstBuffer[18];
            }

            // Ensure that all components are enabled by default.
            // FIXME: Is this correct?
            WriteRegister(NvGpuEngine3dReg.ColorMaskN, 0x1111);

            WriteRegister(NvGpuEngine3dReg.FrameBufferSrgb, 1);

            WriteRegister(NvGpuEngine3dReg.FrontFace, (int)GalFrontFace.Cw);

            for (int index = 0; index < GalPipelineState.RenderTargetsCount; index++)
            {
                WriteRegister(NvGpuEngine3dReg.IBlendNEquationRgb   + index * 8, (int)GalBlendEquation.FuncAdd);
                WriteRegister(NvGpuEngine3dReg.IBlendNFuncSrcRgb    + index * 8, (int)GalBlendFactor.One);
                WriteRegister(NvGpuEngine3dReg.IBlendNFuncDstRgb    + index * 8, (int)GalBlendFactor.Zero);
                WriteRegister(NvGpuEngine3dReg.IBlendNEquationAlpha + index * 8, (int)GalBlendEquation.FuncAdd);
                WriteRegister(NvGpuEngine3dReg.IBlendNFuncSrcAlpha  + index * 8, (int)GalBlendFactor.One);
                WriteRegister(NvGpuEngine3dReg.IBlendNFuncDstAlpha  + index * 8, (int)GalBlendFactor.Zero);
            }
        }

        public void CallMethod(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            if (_methods.TryGetValue(methCall.Method, out NvGpuMethod method))
            {
                ProfileConfig profile = Profiles.GPU.Engine3d.CallMethod;

                profile.SessionItem = method.Method.Name;

                Profile.Begin(profile);

                method(vmm, methCall);

                Profile.End(profile);
            }
            else
            {
                WriteRegister(methCall);
            }
        }

        private void VertexEndGl(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            Profile.Begin(Profiles.GPU.Engine3d.VertexEnd);

            LockCaches();

            Profile.Begin(Profiles.GPU.Engine3d.ConfigureState);

            GalPipelineState state = new GalPipelineState();

            // Framebuffer must be run configured because viewport dimensions may be used in other methods
            SetFrameBuffer(state);

            Profile.End(Profiles.GPU.Engine3d.ConfigureState);

            for (int fbIndex = 0; fbIndex < 8; fbIndex++)
            {
                SetFrameBuffer(vmm, fbIndex);
            }

            SetFrontFace(state);
            SetCullFace(state);
            SetDepth(state);
            SetStencil(state);
            SetScissor(state);
            SetBlending(state);
            SetColorMask(state);
            SetPrimitiveRestart(state);

            SetZeta(vmm);

            SetRenderTargets();

            long[] keys = UploadShaders(vmm);

            _gpu.Renderer.Shader.BindProgram();

            UploadTextures(vmm, state, keys);
            UploadConstBuffers(vmm, state, keys);
            UploadVertexArrays(vmm, state);

            DispatchRender(vmm, state);

            UnlockCaches();

            Profile.End(Profiles.GPU.Engine3d.VertexEnd);
        }

        private void LockCaches()
        {
            _gpu.Renderer.Buffer.LockCache();
            _gpu.Renderer.Rasterizer.LockCaches();
            _gpu.Renderer.Texture.LockCache();
        }

        private void UnlockCaches()
        {
            _gpu.Renderer.Buffer.UnlockCache();
            _gpu.Renderer.Rasterizer.UnlockCaches();
            _gpu.Renderer.Texture.UnlockCache();
        }

        private void ClearBuffers(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            Profile.Begin(Profiles.GPU.Engine3d.ClearBuffers);

            int attachment = (methCall.Argument >> 6) & 0xf;

            GalClearBufferFlags flags = (GalClearBufferFlags)(methCall.Argument & 0x3f);

            float red   = ReadRegisterFloat(NvGpuEngine3dReg.ClearNColor + 0);
            float green = ReadRegisterFloat(NvGpuEngine3dReg.ClearNColor + 1);
            float blue  = ReadRegisterFloat(NvGpuEngine3dReg.ClearNColor + 2);
            float alpha = ReadRegisterFloat(NvGpuEngine3dReg.ClearNColor + 3);

            float depth = ReadRegisterFloat(NvGpuEngine3dReg.ClearDepth);

            int stencil = ReadRegister(NvGpuEngine3dReg.ClearStencil);

            SetFrameBuffer(vmm, attachment);

            SetZeta(vmm);

            SetRenderTargets();

            _gpu.Renderer.RenderTarget.Bind();

            _gpu.Renderer.Rasterizer.ClearBuffers(flags, attachment, red, green, blue, alpha, depth, stencil);

            _gpu.Renderer.Pipeline.ResetDepthMask();
            _gpu.Renderer.Pipeline.ResetColorMask(attachment);

            Profile.End(Profiles.GPU.Engine3d.ClearBuffers);
        }

        private void SetFrameBuffer(NvGpuVmm vmm, int fbIndex)
        {
            ProfileConfig profile = Profiles.GPU.Engine3d.SetFrameBuffer;

            profile.SessionItem = fbIndex.ToString();

            Profile.Begin(profile);

            long va = MakeInt64From2xInt32(NvGpuEngine3dReg.FrameBufferNAddress + fbIndex * 0x10);

            int surfFormat = ReadRegister(NvGpuEngine3dReg.FrameBufferNFormat + fbIndex * 0x10);

            if (va == 0 || surfFormat == 0)
            {
                _gpu.Renderer.RenderTarget.UnbindColor(fbIndex);

                Profile.End(profile);

                return;
            }

            long key = vmm.GetPhysicalAddress(va);

            int width  = ReadRegister(NvGpuEngine3dReg.FrameBufferNWidth  + fbIndex * 0x10);
            int height = ReadRegister(NvGpuEngine3dReg.FrameBufferNHeight + fbIndex * 0x10);

            int arrayMode   = ReadRegister(NvGpuEngine3dReg.FrameBufferNArrayMode + fbIndex * 0x10);
            int layerCount  = arrayMode & 0xFFFF;
            int layerStride = ReadRegister(NvGpuEngine3dReg.FrameBufferNLayerStride + fbIndex * 0x10);
            int baseLayer   = ReadRegister(NvGpuEngine3dReg.FrameBufferNBaseLayer + fbIndex * 0x10);
            int blockDim    = ReadRegister(NvGpuEngine3dReg.FrameBufferNBlockDim + fbIndex * 0x10);

            int gobBlockHeight = 1 << ((blockDim >> 4) & 7);

            GalMemoryLayout layout = (GalMemoryLayout)((blockDim >> 12) & 1);

            float tx = ReadRegisterFloat(NvGpuEngine3dReg.ViewportNTranslateX + fbIndex * 8);
            float ty = ReadRegisterFloat(NvGpuEngine3dReg.ViewportNTranslateY + fbIndex * 8);

            float sx = ReadRegisterFloat(NvGpuEngine3dReg.ViewportNScaleX + fbIndex * 8);
            float sy = ReadRegisterFloat(NvGpuEngine3dReg.ViewportNScaleY + fbIndex * 8);

            _viewportX0 = (int)MathF.Max(0, tx - MathF.Abs(sx));
            _viewportY0 = (int)MathF.Max(0, ty - MathF.Abs(sy));

            _viewportX1 = (int)(tx + MathF.Abs(sx));
            _viewportY1 = (int)(ty + MathF.Abs(sy));

            GalImageFormat format = ImageUtils.ConvertSurface((GalSurfaceFormat)surfFormat);

            GalImage image = new GalImage(width, height, 1, 1, 1, gobBlockHeight, 1, layout, format, GalTextureTarget.TwoD);

            _gpu.ResourceManager.SendColorBuffer(vmm, key, fbIndex, image);

            _gpu.Renderer.RenderTarget.SetViewport(fbIndex, _viewportX0, _viewportY0, _viewportX1 - _viewportX0, _viewportY1 - _viewportY0);

            Profile.End(profile);
        }

        private void SetFrameBuffer(GalPipelineState state)
        {
            state.FramebufferSrgb = ReadRegisterBool(NvGpuEngine3dReg.FrameBufferSrgb);

            state.FlipX = GetFlipSign(NvGpuEngine3dReg.ViewportNScaleX);
            state.FlipY = GetFlipSign(NvGpuEngine3dReg.ViewportNScaleY);

            int screenYControl = ReadRegister(NvGpuEngine3dReg.ScreenYControl);

            bool negateY = (screenYControl & 1) != 0;

            if (negateY)
            {
                state.FlipY = -state.FlipY;
            }
        }

        private void SetZeta(NvGpuVmm vmm)
        {
            Profile.Begin(Profiles.GPU.Engine3d.SetZeta);

            long va = MakeInt64From2xInt32(NvGpuEngine3dReg.ZetaAddress);

            int zetaFormat = ReadRegister(NvGpuEngine3dReg.ZetaFormat);

            int blockDim = ReadRegister(NvGpuEngine3dReg.ZetaBlockDimensions);

            int gobBlockHeight = 1 << ((blockDim >> 4) & 7);

            GalMemoryLayout layout = (GalMemoryLayout)((blockDim >> 12) & 1); //?

            bool zetaEnable = ReadRegisterBool(NvGpuEngine3dReg.ZetaEnable);

            if (va == 0 || zetaFormat == 0 || !zetaEnable)
            {
                _gpu.Renderer.RenderTarget.UnbindZeta();

                Profile.End(Profiles.GPU.Engine3d.SetZeta);

                return;
            }

            long key = vmm.GetPhysicalAddress(va);

            int width  = ReadRegister(NvGpuEngine3dReg.ZetaHoriz);
            int height = ReadRegister(NvGpuEngine3dReg.ZetaVert);

            GalImageFormat format = ImageUtils.ConvertZeta((GalZetaFormat)zetaFormat);

            // TODO: Support non 2D?
            GalImage image = new GalImage(width, height, 1, 1, 1, gobBlockHeight, 1, layout, format, GalTextureTarget.TwoD);

            _gpu.ResourceManager.SendZetaBuffer(vmm, key, image);

            Profile.End(Profiles.GPU.Engine3d.SetZeta);
        }

        private long[] UploadShaders(NvGpuVmm vmm)
        {
            Profile.Begin(Profiles.GPU.Engine3d.UploadShaders);

            long[] keys = new long[5];

            long basePosition = MakeInt64From2xInt32(NvGpuEngine3dReg.ShaderAddress);

            int index = 1;

            int vpAControl = ReadRegister(NvGpuEngine3dReg.ShaderNControl);

            bool vpAEnable = (vpAControl & 1) != 0;

            if (vpAEnable)
            {
                // Note: The maxwell supports 2 vertex programs, usually
                // only VP B is used, but in some cases VP A is also used.
                // In this case, it seems to function as an extra vertex
                // shader stage.
                // The graphics abstraction layer has a special overload for this
                // case, which should merge the two shaders into one vertex shader.
                int vpAOffset = ReadRegister(NvGpuEngine3dReg.ShaderNOffset);
                int vpBOffset = ReadRegister(NvGpuEngine3dReg.ShaderNOffset + 0x10);

                long vpAPos = basePosition + (uint)vpAOffset;
                long vpBPos = basePosition + (uint)vpBOffset;

                keys[(int)GalShaderType.Vertex] = vpBPos;

                _gpu.Renderer.Shader.Create(vmm, vpAPos, vpBPos, GalShaderType.Vertex);
                _gpu.Renderer.Shader.Bind(vpBPos);

                index = 2;
            }

            for (; index < 6; index++)
            {
                GalShaderType type = GetTypeFromProgram(index);

                int control = ReadRegister(NvGpuEngine3dReg.ShaderNControl + index * 0x10);
                int offset  = ReadRegister(NvGpuEngine3dReg.ShaderNOffset  + index * 0x10);

                // Note: Vertex Program (B) is always enabled.
                bool enable = (control & 1) != 0 || index == 1;

                if (!enable)
                {
                    _gpu.Renderer.Shader.Unbind(type);

                    continue;
                }

                long key = basePosition + (uint)offset;

                keys[(int)type] = key;

                _gpu.Renderer.Shader.Create(vmm, key, type);
                _gpu.Renderer.Shader.Bind(key);
            }

            Profile.End(Profiles.GPU.Engine3d.UploadShaders);

            return keys;
        }

        private static GalShaderType GetTypeFromProgram(int program)
        {
            switch (program)
            {
                case 0:
                case 1: return GalShaderType.Vertex;
                case 2: return GalShaderType.TessControl;
                case 3: return GalShaderType.TessEvaluation;
                case 4: return GalShaderType.Geometry;
                case 5: return GalShaderType.Fragment;
            }

            throw new ArgumentOutOfRangeException(nameof(program));
        }

        private void SetFrontFace(GalPipelineState state)
        {
            float signX = GetFlipSign(NvGpuEngine3dReg.ViewportNScaleX);
            float signY = GetFlipSign(NvGpuEngine3dReg.ViewportNScaleY);

            GalFrontFace frontFace = (GalFrontFace)ReadRegister(NvGpuEngine3dReg.FrontFace);

            // Flipping breaks facing. Flipping front facing too fixes it
            if (signX != signY)
            {
                switch (frontFace)
                {
                    case GalFrontFace.Cw:  frontFace = GalFrontFace.Ccw; break;
                    case GalFrontFace.Ccw: frontFace = GalFrontFace.Cw;  break;
                }
            }

            state.FrontFace = frontFace;
        }

        private void SetCullFace(GalPipelineState state)
        {
            state.CullFaceEnabled = ReadRegisterBool(NvGpuEngine3dReg.CullFaceEnable);

            if (state.CullFaceEnabled)
            {
                state.CullFace = (GalCullFace)ReadRegister(NvGpuEngine3dReg.CullFace);
            }
        }

        private void SetDepth(GalPipelineState state)
        {
            state.DepthTestEnabled = ReadRegisterBool(NvGpuEngine3dReg.DepthTestEnable);

            state.DepthWriteEnabled = ReadRegisterBool(NvGpuEngine3dReg.DepthWriteEnable);

            if (state.DepthTestEnabled)
            {
                state.DepthFunc = (GalComparisonOp)ReadRegister(NvGpuEngine3dReg.DepthTestFunction);
            }

            state.DepthRangeNear = ReadRegisterFloat(NvGpuEngine3dReg.DepthRangeNNear);
            state.DepthRangeFar  = ReadRegisterFloat(NvGpuEngine3dReg.DepthRangeNFar);
        }

        private void SetStencil(GalPipelineState state)
        {
            state.StencilTestEnabled = ReadRegisterBool(NvGpuEngine3dReg.StencilEnable);

            if (state.StencilTestEnabled)
            {
                state.StencilBackFuncFunc = (GalComparisonOp)ReadRegister(NvGpuEngine3dReg.StencilBackFuncFunc);
                state.StencilBackFuncRef  =                  ReadRegister(NvGpuEngine3dReg.StencilBackFuncRef);
                state.StencilBackFuncMask =            (uint)ReadRegister(NvGpuEngine3dReg.StencilBackFuncMask);
                state.StencilBackOpFail   =    (GalStencilOp)ReadRegister(NvGpuEngine3dReg.StencilBackOpFail);
                state.StencilBackOpZFail  =    (GalStencilOp)ReadRegister(NvGpuEngine3dReg.StencilBackOpZFail);
                state.StencilBackOpZPass  =    (GalStencilOp)ReadRegister(NvGpuEngine3dReg.StencilBackOpZPass);
                state.StencilBackMask     =            (uint)ReadRegister(NvGpuEngine3dReg.StencilBackMask);

                state.StencilFrontFuncFunc = (GalComparisonOp)ReadRegister(NvGpuEngine3dReg.StencilFrontFuncFunc);
                state.StencilFrontFuncRef  =                  ReadRegister(NvGpuEngine3dReg.StencilFrontFuncRef);
                state.StencilFrontFuncMask =            (uint)ReadRegister(NvGpuEngine3dReg.StencilFrontFuncMask);
                state.StencilFrontOpFail   =    (GalStencilOp)ReadRegister(NvGpuEngine3dReg.StencilFrontOpFail);
                state.StencilFrontOpZFail  =    (GalStencilOp)ReadRegister(NvGpuEngine3dReg.StencilFrontOpZFail);
                state.StencilFrontOpZPass  =    (GalStencilOp)ReadRegister(NvGpuEngine3dReg.StencilFrontOpZPass);
                state.StencilFrontMask     =            (uint)ReadRegister(NvGpuEngine3dReg.StencilFrontMask);
            }
        }

        private void SetScissor(GalPipelineState state)
        {
            int count = 0;

            for (int index = 0; index < GalPipelineState.RenderTargetsCount; index++)
            {
                state.ScissorTestEnabled[index] = ReadRegisterBool(NvGpuEngine3dReg.ScissorEnable + index * 4);

                if (state.ScissorTestEnabled[index])
                {
                    uint scissorHorizontal = (uint)ReadRegister(NvGpuEngine3dReg.ScissorHorizontal + index * 4);
                    uint scissorVertical   = (uint)ReadRegister(NvGpuEngine3dReg.ScissorVertical   + index * 4);

                    int left  = (int)(scissorHorizontal & 0xFFFF); // Left, lower 16 bits
                    int right = (int)(scissorHorizontal >> 16);    // Right, upper 16 bits

                    int bottom = (int)(scissorVertical & 0xFFFF); // Bottom, lower 16 bits
                    int top    = (int)(scissorVertical >> 16);    // Top, upper 16 bits

                    int width  = Math.Abs(right - left);
                    int height = Math.Abs(top   - bottom);

                    // If the scissor test covers the whole possible viewport, i.e. uninitialized, disable scissor test
                    if ((width > NvGpu.MaxViewportSize && height > NvGpu.MaxViewportSize) || width <= 0 || height <= 0)
                    {
                        state.ScissorTestEnabled[index] = false;
                        continue;
                    }

                    // Keep track of how many scissor tests are active.
                    // If only 1, and it's the first user should apply to all viewports
                    count++;

                    // Flip X
                    if (state.FlipX == -1)
                    {
                        left  = _viewportX1 - (left  - _viewportX0);
                        right = _viewportX1 - (right - _viewportX0);
                    }

                    // Ensure X is in the right order
                    if (left > right)
                    {
                        int temp = left;
                        left     = right;
                        right    = temp;
                    }

                    // Flip Y
                    if (state.FlipY == -1)
                    {
                        bottom = _viewportY1 - (bottom - _viewportY0);
                        top    = _viewportY1 - (top - _viewportY0);
                    }

                    // Ensure Y is in the right order
                    if (bottom > top)
                    {
                        int temp = top;
                        top      = bottom;
                        bottom   = temp;
                    }

                    // Handle out of active viewport dimensions
                    left   = Math.Clamp(left,   _viewportX0, _viewportX1);
                    right  = Math.Clamp(right,  _viewportX0, _viewportX1);
                    top    = Math.Clamp(top,    _viewportY0, _viewportY1);
                    bottom = Math.Clamp(bottom, _viewportY0, _viewportY1);

                    // Save values to state
                    state.ScissorTestX[index] = left;
                    state.ScissorTestY[index] = bottom;

                    state.ScissorTestWidth[index]  = right - left;
                    state.ScissorTestHeight[index] = top - bottom;
                }
            }

            state.ScissorTestCount = count;
        }

        private void SetBlending(GalPipelineState state)
        {
            bool blendIndependent = ReadRegisterBool(NvGpuEngine3dReg.BlendIndependent);

            state.BlendIndependent = blendIndependent;

            for (int index = 0; index < GalPipelineState.RenderTargetsCount; index++)
            {
                if (blendIndependent)
                {
                    state.Blends[index].Enabled = ReadRegisterBool(NvGpuEngine3dReg.IBlendNEnable + index);

                    if (state.Blends[index].Enabled)
                    {
                        state.Blends[index].SeparateAlpha = ReadRegisterBool(NvGpuEngine3dReg.IBlendNSeparateAlpha + index * 8);

                        state.Blends[index].EquationRgb   = ReadBlendEquation(NvGpuEngine3dReg.IBlendNEquationRgb   + index * 8);
                        state.Blends[index].FuncSrcRgb    = ReadBlendFactor  (NvGpuEngine3dReg.IBlendNFuncSrcRgb    + index * 8);
                        state.Blends[index].FuncDstRgb    = ReadBlendFactor  (NvGpuEngine3dReg.IBlendNFuncDstRgb    + index * 8);
                        state.Blends[index].EquationAlpha = ReadBlendEquation(NvGpuEngine3dReg.IBlendNEquationAlpha + index * 8);
                        state.Blends[index].FuncSrcAlpha  = ReadBlendFactor  (NvGpuEngine3dReg.IBlendNFuncSrcAlpha  + index * 8);
                        state.Blends[index].FuncDstAlpha  = ReadBlendFactor  (NvGpuEngine3dReg.IBlendNFuncDstAlpha  + index * 8);
                    }
                }
                else
                {
                    // It seems that even when independent blend is disabled, the first IBlend enable
                    // register is still set to indicate whenever blend is enabled or not (?).
                    state.Blends[index].Enabled = ReadRegisterBool(NvGpuEngine3dReg.IBlendNEnable);

                    if (state.Blends[index].Enabled)
                    {
                        state.Blends[index].SeparateAlpha = ReadRegisterBool(NvGpuEngine3dReg.BlendSeparateAlpha);

                        state.Blends[index].EquationRgb   = ReadBlendEquation(NvGpuEngine3dReg.BlendEquationRgb);
                        state.Blends[index].FuncSrcRgb    = ReadBlendFactor  (NvGpuEngine3dReg.BlendFuncSrcRgb);
                        state.Blends[index].FuncDstRgb    = ReadBlendFactor  (NvGpuEngine3dReg.BlendFuncDstRgb);
                        state.Blends[index].EquationAlpha = ReadBlendEquation(NvGpuEngine3dReg.BlendEquationAlpha);
                        state.Blends[index].FuncSrcAlpha  = ReadBlendFactor  (NvGpuEngine3dReg.BlendFuncSrcAlpha);
                        state.Blends[index].FuncDstAlpha  = ReadBlendFactor  (NvGpuEngine3dReg.BlendFuncDstAlpha);
                    }
                }
            }
        }

        private GalBlendEquation ReadBlendEquation(NvGpuEngine3dReg register)
        {
            return (GalBlendEquation)ReadRegister(register);
        }

        private GalBlendFactor ReadBlendFactor(NvGpuEngine3dReg register)
        {
            return (GalBlendFactor)ReadRegister(register);
        }

        private void SetColorMask(GalPipelineState state)
        {
            bool colorMaskCommon = ReadRegisterBool(NvGpuEngine3dReg.ColorMaskCommon);

            state.ColorMaskCommon = colorMaskCommon;

            for (int index = 0; index < GalPipelineState.RenderTargetsCount; index++)
            {
                int colorMask = ReadRegister(NvGpuEngine3dReg.ColorMaskN + (colorMaskCommon ? 0 : index));

                state.ColorMasks[index].Red   = ((colorMask >> 0)  & 0xf) != 0;
                state.ColorMasks[index].Green = ((colorMask >> 4)  & 0xf) != 0;
                state.ColorMasks[index].Blue  = ((colorMask >> 8)  & 0xf) != 0;
                state.ColorMasks[index].Alpha = ((colorMask >> 12) & 0xf) != 0;
            }
        }

        private void SetPrimitiveRestart(GalPipelineState state)
        {
            state.PrimitiveRestartEnabled = ReadRegisterBool(NvGpuEngine3dReg.PrimRestartEnable);

            if (state.PrimitiveRestartEnabled)
            {
                state.PrimitiveRestartIndex = (uint)ReadRegister(NvGpuEngine3dReg.PrimRestartIndex);
            }
        }

        private void SetRenderTargets()
        {
            // Commercial games do not seem to
            // bool SeparateFragData = ReadRegisterBool(NvGpuEngine3dReg.RTSeparateFragData);

            uint control = (uint)(ReadRegister(NvGpuEngine3dReg.RtControl));

            uint count = control & 0xf;

            if (count > 0)
            {
                int[] map = new int[count];

                for (int index = 0; index < count; index++)
                {
                    int shift = 4 + index * 3;

                    map[index] = (int)((control >> shift) & 7);
                }

                _gpu.Renderer.RenderTarget.SetMap(map);
            }
            else
            {
                _gpu.Renderer.RenderTarget.SetMap(null);
            }
        }

        private void UploadTextures(NvGpuVmm vmm, GalPipelineState state, long[] keys)
        {
            Profile.Begin(Profiles.GPU.Engine3d.UploadTextures);

            long baseShPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.ShaderAddress);

            int textureCbIndex = ReadRegister(NvGpuEngine3dReg.TextureCbIndex);

            List<(long, GalImage, GalTextureSampler)> unboundTextures = new List<(long, GalImage, GalTextureSampler)>();

            for (int index = 0; index < keys.Length; index++)
            {
                foreach (TextureDescriptor desc in _gpu.Renderer.Shader.GetTextureUsage(keys[index]))
                {
                    int textureHandle;

                    if (desc.IsBindless)
                    {
                        long position = _constBuffers[index][desc.CbufSlot].Position;

                        textureHandle = vmm.ReadInt32(position + desc.CbufOffset * 4);
                    }
                    else
                    {
                        long position = _constBuffers[index][textureCbIndex].Position;

                        textureHandle = vmm.ReadInt32(position + desc.HandleIndex * 4);
                    }

                    unboundTextures.Add(UploadTexture(vmm, textureHandle));
                }
            }

            for (int index = 0; index < unboundTextures.Count; index++)
            {
                (long key, GalImage image, GalTextureSampler sampler) = unboundTextures[index];

                if (key == 0)
                {
                    continue;
                }

                _gpu.Renderer.Texture.Bind(key, index, image);
                _gpu.Renderer.Texture.SetSampler(image, sampler);
            }

            Profile.End(Profiles.GPU.Engine3d.UploadTextures);
        }

        private (long, GalImage, GalTextureSampler) UploadTexture(NvGpuVmm vmm, int textureHandle)
        {
            if (textureHandle == 0)
            {
                // FIXME: Some games like puyo puyo will use handles with the value 0.
                // This is a bug, most likely caused by sync issues.
                return (0, default(GalImage), default(GalTextureSampler));
            }

            Profile.Begin(Profiles.GPU.Engine3d.UploadTexture);

            bool linkedTsc = ReadRegisterBool(NvGpuEngine3dReg.LinkedTsc);

            int ticIndex = (textureHandle >>  0) & 0xfffff;

            int tscIndex = linkedTsc ? ticIndex : (textureHandle >> 20) & 0xfff;

            long ticPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.TexHeaderPoolOffset);
            long tscPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.TexSamplerPoolOffset);

            ticPosition += ticIndex * 0x20;
            tscPosition += tscIndex * 0x20;

            GalImage image = TextureFactory.MakeTexture(vmm, ticPosition);

            GalTextureSampler sampler = TextureFactory.MakeSampler(_gpu, vmm, tscPosition);

            long key = vmm.ReadInt64(ticPosition + 4) & 0xffffffffffff;

            if (image.Layout == GalMemoryLayout.BlockLinear)
            {
                key &= ~0x1ffL;
            }
            else if (image.Layout == GalMemoryLayout.Pitch)
            {
                key &= ~0x1fL;
            }

            key = vmm.GetPhysicalAddress(key);

            if (key == -1)
            {
                Profile.End(Profiles.GPU.Engine3d.UploadTexture);

                // FIXME: Shouldn't ignore invalid addresses.
                return (0, default(GalImage), default(GalTextureSampler));
            }

            _gpu.ResourceManager.SendTexture(vmm, key, image);

            Profile.End(Profiles.GPU.Engine3d.UploadTexture);

            return (key, image, sampler);
        }

        private void UploadConstBuffers(NvGpuVmm vmm, GalPipelineState state, long[] keys)
        {
            Profile.Begin(Profiles.GPU.Engine3d.UploadConstBuffers);

            for (int stage = 0; stage < keys.Length; stage++)
            {
                foreach (CBufferDescriptor desc in _gpu.Renderer.Shader.GetConstBufferUsage(keys[stage]))
                {
                    ConstBuffer cb = _constBuffers[stage][desc.Slot];

                    if (!cb.Enabled)
                    {
                        continue;
                    }

                    long key = vmm.GetPhysicalAddress(cb.Position);

                    if (_gpu.ResourceManager.MemoryRegionModified(vmm, key, cb.Size, NvGpuBufferType.ConstBuffer))
                    {
                        if (vmm.TryGetHostAddress(cb.Position, cb.Size, out IntPtr cbPtr))
                        {
                            _gpu.Renderer.Buffer.SetData(key, cb.Size, cbPtr);
                        }
                        else
                        {
                            _gpu.Renderer.Buffer.SetData(key, vmm.ReadBytes(cb.Position, cb.Size));
                        }
                    }

                    state.ConstBufferKeys[stage][desc.Slot] = key;
                }
            }

            Profile.End(Profiles.GPU.Engine3d.UploadConstBuffers);
        }

        private void UploadVertexArrays(NvGpuVmm vmm, GalPipelineState state)
        {
            Profile.Begin(Profiles.GPU.Engine3d.UploadVertexArrays);

            long ibPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.IndexArrayAddress);

            long iboKey = vmm.GetPhysicalAddress(ibPosition);

            int indexEntryFmt = ReadRegister(NvGpuEngine3dReg.IndexArrayFormat);
            int indexCount    = ReadRegister(NvGpuEngine3dReg.IndexBatchCount);
            int primCtrl      = ReadRegister(NvGpuEngine3dReg.VertexBeginGl);

            GalPrimitiveType primType = (GalPrimitiveType)(primCtrl & 0xffff);

            GalIndexFormat indexFormat = (GalIndexFormat)indexEntryFmt;

            int indexEntrySize = 1 << indexEntryFmt;

            if (indexEntrySize > 4)
            {
                throw new InvalidOperationException("Invalid index entry size \"" + indexEntrySize + "\"!");
            }

            if (indexCount != 0)
            {
                int ibSize = indexCount * indexEntrySize;

                bool iboCached = _gpu.Renderer.Rasterizer.IsIboCached(iboKey, (uint)ibSize);

                bool usesLegacyQuads =
                    primType == GalPrimitiveType.Quads ||
                    primType == GalPrimitiveType.QuadStrip;

                if (!iboCached || _gpu.ResourceManager.MemoryRegionModified(vmm, iboKey, (uint)ibSize, NvGpuBufferType.Index))
                {
                    if (!usesLegacyQuads)
                    {
                        if (vmm.TryGetHostAddress(ibPosition, ibSize, out IntPtr ibPtr))
                        {
                            _gpu.Renderer.Rasterizer.CreateIbo(iboKey, ibSize, ibPtr);
                        }
                        else
                        {
                            _gpu.Renderer.Rasterizer.CreateIbo(iboKey, ibSize, vmm.ReadBytes(ibPosition, ibSize));
                        }
                    }
                    else
                    {
                        byte[] buffer = vmm.ReadBytes(ibPosition, ibSize);

                        if (primType == GalPrimitiveType.Quads)
                        {
                            buffer = QuadHelper.ConvertQuadsToTris(buffer, indexEntrySize, indexCount);
                        }
                        else /* if (PrimType == GalPrimitiveType.QuadStrip) */
                        {
                            buffer = QuadHelper.ConvertQuadStripToTris(buffer, indexEntrySize, indexCount);
                        }

                        _gpu.Renderer.Rasterizer.CreateIbo(iboKey, ibSize, buffer);
                    }
                }

                if (!usesLegacyQuads)
                {
                    _gpu.Renderer.Rasterizer.SetIndexArray(ibSize, indexFormat);
                }
                else
                {
                    if (primType == GalPrimitiveType.Quads)
                    {
                        _gpu.Renderer.Rasterizer.SetIndexArray(QuadHelper.ConvertSizeQuadsToTris(ibSize), indexFormat);
                    }
                    else /* if (PrimType == GalPrimitiveType.QuadStrip) */
                    {
                        _gpu.Renderer.Rasterizer.SetIndexArray(QuadHelper.ConvertSizeQuadStripToTris(ibSize), indexFormat);
                    }
                }
            }

            List<GalVertexAttrib>[] attribs = new List<GalVertexAttrib>[32];

            for (int attr = 0; attr < 16; attr++)
            {
                int packed = ReadRegister(NvGpuEngine3dReg.VertexAttribNFormat + attr);

                int arrayIndex = packed & 0x1f;

                if (attribs[arrayIndex] == null)
                {
                    attribs[arrayIndex] = new List<GalVertexAttrib>();
                }

                long vbPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.VertexArrayNAddress + arrayIndex * 4);

                if (vbPosition == 0)
                {
                    continue;
                }

                bool isConst = ((packed >> 6) & 1) != 0;

                int offset = (packed >> 7) & 0x3fff;

                GalVertexAttribSize size = (GalVertexAttribSize)((packed >> 21) & 0x3f);
                GalVertexAttribType type = (GalVertexAttribType)((packed >> 27) & 0x7);

                bool isRgba = ((packed >> 31) & 1) != 0;

                // Check vertex array is enabled to avoid out of bounds exception when reading bytes
                bool enable = (ReadRegister(NvGpuEngine3dReg.VertexArrayNControl + arrayIndex * 4) & 0x1000) != 0;

                // Note: 16 is the maximum size of an attribute,
                // having a component size of 32-bits with 4 elements (a vec4).
                if (enable)
                {
                    byte[] data = vmm.ReadBytes(vbPosition + offset, 16);

                    attribs[arrayIndex].Add(new GalVertexAttrib(attr, isConst, offset, data, size, type, isRgba));
                }
            }

            state.VertexBindings = new GalVertexBinding[32];

            for (int index = 0; index < 32; index++)
            {
                if (attribs[index] == null)
                {
                    continue;
                }

                int control = ReadRegister(NvGpuEngine3dReg.VertexArrayNControl + index * 4);

                bool enable = (control & 0x1000) != 0;

                if (!enable)
                {
                    continue;
                }

                long vbPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.VertexArrayNAddress + index * 4);
                long vbEndPos   = MakeInt64From2xInt32(NvGpuEngine3dReg.VertexArrayNEndAddr + index * 2);

                int vertexDivisor = ReadRegister(NvGpuEngine3dReg.VertexArrayNDivisor + index * 4);

                bool instanced = ReadRegisterBool(NvGpuEngine3dReg.VertexArrayNInstance + index);

                int stride = control & 0xfff;

                if (instanced && vertexDivisor != 0)
                {
                    vbPosition += stride * (_currentInstance / vertexDivisor);
                }

                if (vbPosition > vbEndPos)
                {
                    // Instance is invalid, ignore the draw call
                    continue;
                }

                long vboKey = vmm.GetPhysicalAddress(vbPosition);

                long vbSize = (vbEndPos - vbPosition) + 1;
                int modifiedVbSize = (int)vbSize;


                // If quads convert size to triangle length
                if (stride == 0)
                {
                    if (primType == GalPrimitiveType.Quads)
                    {
                        modifiedVbSize = QuadHelper.ConvertSizeQuadsToTris(modifiedVbSize);
                    }
                    else if (primType == GalPrimitiveType.QuadStrip)
                    {
                        modifiedVbSize = QuadHelper.ConvertSizeQuadStripToTris(modifiedVbSize);
                    }
                }

                bool vboCached = _gpu.Renderer.Rasterizer.IsVboCached(vboKey, modifiedVbSize);

                if (!vboCached || _gpu.ResourceManager.MemoryRegionModified(vmm, vboKey, vbSize, NvGpuBufferType.Vertex))
                {
                    if ((primType == GalPrimitiveType.Quads | primType == GalPrimitiveType.QuadStrip) && stride != 0)
                    {
                        // Convert quad buffer to triangles
                        byte[] data = vmm.ReadBytes(vbPosition, vbSize);

                        if (primType == GalPrimitiveType.Quads)
                        {
                            data = QuadHelper.ConvertQuadsToTris(data, stride, (int)(vbSize / stride));
                        }
                        else
                        {
                            data = QuadHelper.ConvertQuadStripToTris(data, stride, (int)(vbSize / stride));
                        }
                        _gpu.Renderer.Rasterizer.CreateVbo(vboKey, data);
                    }
                    else if (vmm.TryGetHostAddress(vbPosition, vbSize, out IntPtr vbPtr))
                    {
                        _gpu.Renderer.Rasterizer.CreateVbo(vboKey, (int)vbSize, vbPtr);
                    }
                    else
                    {
                        _gpu.Renderer.Rasterizer.CreateVbo(vboKey, vmm.ReadBytes(vbPosition, vbSize));
                    }
                }

                state.VertexBindings[index].Enabled   = true;
                state.VertexBindings[index].Stride    = stride;
                state.VertexBindings[index].VboKey    = vboKey;
                state.VertexBindings[index].Instanced = instanced;
                state.VertexBindings[index].Divisor   = vertexDivisor;
                state.VertexBindings[index].Attribs   = attribs[index].ToArray();
            }

            Profile.End(Profiles.GPU.Engine3d.UploadVertexArrays);
        }

        private void DispatchRender(NvGpuVmm vmm, GalPipelineState state)
        {
            int indexCount = ReadRegister(NvGpuEngine3dReg.IndexBatchCount);
            int primCtrl   = ReadRegister(NvGpuEngine3dReg.VertexBeginGl);

            GalPrimitiveType primType = (GalPrimitiveType)(primCtrl & 0xffff);

            bool instanceNext = ((primCtrl >> 26) & 1) != 0;
            bool instanceCont = ((primCtrl >> 27) & 1) != 0;

            if (instanceNext && instanceCont)
            {
                throw new InvalidOperationException("GPU tried to increase and reset instance count at the same time");
            }

            if (instanceNext)
            {
                _currentInstance++;
            }
            else if (!instanceCont)
            {
                _currentInstance = 0;
            }

            state.Instance = _currentInstance;

            _gpu.Renderer.Pipeline.Bind(state);

            _gpu.Renderer.RenderTarget.Bind();

            if (indexCount != 0)
            {
                int indexEntryFmt = ReadRegister(NvGpuEngine3dReg.IndexArrayFormat);
                int indexFirst    = ReadRegister(NvGpuEngine3dReg.IndexBatchFirst);
                int vertexBase    = ReadRegister(NvGpuEngine3dReg.VertexArrayElemBase);

                long indexPosition = MakeInt64From2xInt32(NvGpuEngine3dReg.IndexArrayAddress);

                long iboKey = vmm.GetPhysicalAddress(indexPosition);

                // Quad primitive types were deprecated on OpenGL 3.x,
                // they are converted to a triangles index buffer on IB creation,
                // so we should use the triangles type here too.
                if (primType == GalPrimitiveType.Quads || primType == GalPrimitiveType.QuadStrip)
                {
                    // Note: We assume that index first points to the first
                    // vertex of a quad, if it points to the middle of a
                    // quad (First % 4 != 0 for Quads) then it will not work properly.
                    if (primType == GalPrimitiveType.Quads)
                    {
                        indexFirst = QuadHelper.ConvertSizeQuadsToTris(indexFirst);
                    }
                    else // QuadStrip
                    {
                        indexFirst = QuadHelper.ConvertSizeQuadStripToTris(indexFirst);
                    }

                    primType = GalPrimitiveType.Triangles;
                }

                _gpu.Renderer.Rasterizer.DrawElements(iboKey, indexFirst, vertexBase, primType);
            }
            else
            {
                int vertexFirst = ReadRegister(NvGpuEngine3dReg.VertexArrayFirst);
                int vertexCount = ReadRegister(NvGpuEngine3dReg.VertexArrayCount);

                // Quad primitive types were deprecated on OpenGL 3.x,
                // they are converted to a triangles index buffer on IB creation,
                // so we should use the triangles type here too.
                if (primType == GalPrimitiveType.Quads || primType == GalPrimitiveType.QuadStrip)
                {
                    // Note: We assume that index first points to the first
                    // vertex of a quad, if it points to the middle of a
                    // quad (First % 4 != 0 for Quads) then it will not work properly.
                    if (primType == GalPrimitiveType.Quads)
                    {
                        vertexFirst = QuadHelper.ConvertSizeQuadsToTris(vertexFirst);
                    }
                    else // QuadStrip
                    {
                        vertexFirst = QuadHelper.ConvertSizeQuadStripToTris(vertexFirst);
                    }

                    primType = GalPrimitiveType.Triangles;
                    vertexCount = QuadHelper.ConvertSizeQuadsToTris(vertexCount);
                }

                _gpu.Renderer.Rasterizer.DrawArrays(vertexFirst, vertexCount, primType);
            }

            // Reset pipeline for host OpenGL calls
            _gpu.Renderer.Pipeline.Unbind(state);

            // Is the GPU really clearing those registers after draw?
            WriteRegister(NvGpuEngine3dReg.IndexBatchFirst, 0);
            WriteRegister(NvGpuEngine3dReg.IndexBatchCount, 0);
        }

        private enum QueryMode
        {
            WriteSeq,
            Sync,
            WriteCounterAndTimestamp
        }

        private void QueryControl(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            WriteRegister(methCall);

            long position = MakeInt64From2xInt32(NvGpuEngine3dReg.QueryAddress);

            int seq  = Registers[(int)NvGpuEngine3dReg.QuerySequence];
            int ctrl = Registers[(int)NvGpuEngine3dReg.QueryControl];

            QueryMode mode = (QueryMode)(ctrl & 3);

            switch (mode)
            {
                case QueryMode.WriteSeq: vmm.WriteInt32(position, seq); break;

                case QueryMode.WriteCounterAndTimestamp:
                {
                    // TODO: Implement counters.
                    long counter = 1;

                    long timestamp = PerformanceCounter.ElapsedMilliseconds;

                    vmm.WriteInt64(position + 0, counter);
                    vmm.WriteInt64(position + 8, timestamp);

                    break;
                }
            }
        }

        private void CbData(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            long position = MakeInt64From2xInt32(NvGpuEngine3dReg.ConstBufferAddress);

            int offset = ReadRegister(NvGpuEngine3dReg.ConstBufferOffset);

            vmm.WriteInt32(position + offset, methCall.Argument);

            WriteRegister(NvGpuEngine3dReg.ConstBufferOffset, offset + 4);

            _gpu.ResourceManager.ClearPbCache(NvGpuBufferType.ConstBuffer);
        }

        private void CbBind(NvGpuVmm vmm, GpuMethodCall methCall)
        {
            int stage = (methCall.Method - 0x904) >> 3;

            int index = methCall.Argument;

            bool enabled = (index & 1) != 0;

            index = (index >> 4) & 0x1f;

            long position = MakeInt64From2xInt32(NvGpuEngine3dReg.ConstBufferAddress);

            long cbKey = vmm.GetPhysicalAddress(position);

            int size = ReadRegister(NvGpuEngine3dReg.ConstBufferSize);

            if (!_gpu.Renderer.Buffer.IsCached(cbKey, size))
            {
                _gpu.Renderer.Buffer.Create(cbKey, size);
            }

            ConstBuffer cb = _constBuffers[stage][index];

            if (cb.Position != position || cb.Enabled != enabled || cb.Size != size)
            {
                _constBuffers[stage][index].Position = position;
                _constBuffers[stage][index].Enabled = enabled;
                _constBuffers[stage][index].Size = size;
            }
        }

        private float GetFlipSign(NvGpuEngine3dReg reg)
        {
            return MathF.Sign(ReadRegisterFloat(reg));
        }

        private long MakeInt64From2xInt32(NvGpuEngine3dReg reg)
        {
            return
                (long)Registers[(int)reg + 0] << 32 |
                (uint)Registers[(int)reg + 1];
        }

        private void WriteRegister(GpuMethodCall methCall)
        {
            Registers[methCall.Method] = methCall.Argument;
        }

        private int ReadRegister(NvGpuEngine3dReg reg)
        {
            return Registers[(int)reg];
        }

        private float ReadRegisterFloat(NvGpuEngine3dReg reg)
        {
            return BitConverter.Int32BitsToSingle(ReadRegister(reg));
        }

        private bool ReadRegisterBool(NvGpuEngine3dReg reg)
        {
            return (ReadRegister(reg) & 1) != 0;
        }

        private void WriteRegister(NvGpuEngine3dReg reg, int value)
        {
            Registers[(int)reg] = value;
        }
    }
}
