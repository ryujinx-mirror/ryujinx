using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using Ryujinx.Graphics.Vulkan.Shaders;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Numerics;
using VkFormat = Silk.NET.Vulkan.Format;

namespace Ryujinx.Graphics.Vulkan
{
    enum ComponentType
    {
        Float,
        SignedInteger,
        UnsignedInteger
    }

    class HelperShader : IDisposable
    {
        private const int UniformBufferAlignment = 256;

        private readonly PipelineHelperShader _pipeline;
        private readonly ISampler _samplerLinear;
        private readonly ISampler _samplerNearest;
        private readonly IProgram _programColorBlit;
        private readonly IProgram _programColorBlitMs;
        private readonly IProgram _programColorBlitClearAlpha;
        private readonly IProgram _programColorClearF;
        private readonly IProgram _programColorClearSI;
        private readonly IProgram _programColorClearUI;
        private readonly IProgram _programStrideChange;
        private readonly IProgram _programConvertIndexBuffer;
        private readonly IProgram _programConvertIndirectData;
        private readonly IProgram _programColorCopyShortening;
        private readonly IProgram _programColorCopyToNonMs;
        private readonly IProgram _programColorCopyWidening;
        private readonly IProgram _programColorDrawToMs;
        private readonly IProgram _programDepthBlit;
        private readonly IProgram _programDepthBlitMs;
        private readonly IProgram _programDepthDrawToMs;
        private readonly IProgram _programDepthDrawToNonMs;
        private readonly IProgram _programStencilBlit;
        private readonly IProgram _programStencilBlitMs;
        private readonly IProgram _programStencilDrawToMs;
        private readonly IProgram _programStencilDrawToNonMs;

        public HelperShader(VulkanRenderer gd, Device device)
        {
            _pipeline = new PipelineHelperShader(gd, device);
            _pipeline.Initialize();

            _samplerLinear = gd.CreateSampler(GAL.SamplerCreateInfo.Create(MinFilter.Linear, MagFilter.Linear));
            _samplerNearest = gd.CreateSampler(GAL.SamplerCreateInfo.Create(MinFilter.Nearest, MagFilter.Nearest));

            var blitVertexBindings = new ShaderBindings(
                new[] { 1 },
                Array.Empty<int>(),
                Array.Empty<int>(),
                Array.Empty<int>());

            var blitFragmentBindings = new ShaderBindings(
                Array.Empty<int>(),
                Array.Empty<int>(),
                new[] { 0 },
                Array.Empty<int>());

            _programColorBlit = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorBlitVertexShaderSource, blitVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                new ShaderSource(ShaderBinaries.ColorBlitFragmentShaderSource, blitFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
            });

            _programColorBlitMs = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorBlitVertexShaderSource, blitVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                new ShaderSource(ShaderBinaries.ColorBlitMsFragmentShaderSource, blitFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
            });

            _programColorBlitClearAlpha = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorBlitVertexShaderSource, blitVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                new ShaderSource(ShaderBinaries.ColorBlitClearAlphaFragmentShaderSource, blitFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
            });

            var colorClearFragmentBindings = new ShaderBindings(
                Array.Empty<int>(),
                Array.Empty<int>(),
                Array.Empty<int>(),
                Array.Empty<int>());

            _programColorClearF = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorClearVertexShaderSource, blitVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                new ShaderSource(ShaderBinaries.ColorClearFFragmentShaderSource, colorClearFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
            });

            _programColorClearSI = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorClearVertexShaderSource, blitVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                new ShaderSource(ShaderBinaries.ColorClearSIFragmentShaderSource, colorClearFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
            });

            _programColorClearUI = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorClearVertexShaderSource, blitVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                new ShaderSource(ShaderBinaries.ColorClearUIFragmentShaderSource, colorClearFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
            });

            var strideChangeBindings = new ShaderBindings(
                new[] { 0 },
                new[] { 1, 2 },
                Array.Empty<int>(),
                Array.Empty<int>());

            _programStrideChange = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ChangeBufferStrideShaderSource, strideChangeBindings, ShaderStage.Compute, TargetLanguage.Spirv),
            });

            var colorCopyBindings = new ShaderBindings(
                new[] { 0 },
                Array.Empty<int>(),
                new[] { 0 },
                new[] { 0 });

            _programColorCopyShortening = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorCopyShorteningComputeShaderSource, colorCopyBindings, ShaderStage.Compute, TargetLanguage.Spirv),
            });

            _programColorCopyToNonMs = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorCopyToNonMsComputeShaderSource, colorCopyBindings, ShaderStage.Compute, TargetLanguage.Spirv),
            });

            _programColorCopyWidening = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorCopyWideningComputeShaderSource, colorCopyBindings, ShaderStage.Compute, TargetLanguage.Spirv),
            });

            var colorDrawToMsVertexBindings = new ShaderBindings(
                Array.Empty<int>(),
                Array.Empty<int>(),
                Array.Empty<int>(),
                Array.Empty<int>());

            var colorDrawToMsFragmentBindings = new ShaderBindings(
                new[] { 0 },
                Array.Empty<int>(),
                new[] { 0 },
                Array.Empty<int>());

            _programColorDrawToMs = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorDrawToMsVertexShaderSource, colorDrawToMsVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                new ShaderSource(ShaderBinaries.ColorDrawToMsFragmentShaderSource, colorDrawToMsFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
            });

            var convertIndexBufferBindings = new ShaderBindings(
                new[] { 0 },
                new[] { 1, 2 },
                Array.Empty<int>(),
                Array.Empty<int>());

            _programConvertIndexBuffer = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ConvertIndexBufferShaderSource, convertIndexBufferBindings, ShaderStage.Compute, TargetLanguage.Spirv),
            });

            var convertIndirectDataBindings = new ShaderBindings(
                new[] { 0 },
                new[] { 1, 2, 3 },
                Array.Empty<int>(),
                Array.Empty<int>());

            _programConvertIndirectData = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ConvertIndirectDataShaderSource, convertIndirectDataBindings, ShaderStage.Compute, TargetLanguage.Spirv),
            });

            _programDepthBlit = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorBlitVertexShaderSource, blitVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                new ShaderSource(ShaderBinaries.DepthBlitFragmentShaderSource, blitFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
            });

            _programDepthBlitMs = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorBlitVertexShaderSource, blitVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                new ShaderSource(ShaderBinaries.DepthBlitMsFragmentShaderSource, blitFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
            });

            _programDepthDrawToMs = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorDrawToMsVertexShaderSource, colorDrawToMsVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                new ShaderSource(ShaderBinaries.DepthDrawToMsFragmentShaderSource, colorDrawToMsFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
            });

            _programDepthDrawToNonMs = gd.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(ShaderBinaries.ColorDrawToMsVertexShaderSource, colorDrawToMsVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                new ShaderSource(ShaderBinaries.DepthDrawToNonMsFragmentShaderSource, colorDrawToMsFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
            });

            if (gd.Capabilities.SupportsShaderStencilExport)
            {
                _programStencilBlit = gd.CreateProgramWithMinimalLayout(new[]
                {
                    new ShaderSource(ShaderBinaries.ColorBlitVertexShaderSource, blitVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                    new ShaderSource(ShaderBinaries.StencilBlitFragmentShaderSource, blitFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
                });

                _programStencilBlitMs = gd.CreateProgramWithMinimalLayout(new[]
                {
                    new ShaderSource(ShaderBinaries.ColorBlitVertexShaderSource, blitVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                    new ShaderSource(ShaderBinaries.StencilBlitMsFragmentShaderSource, blitFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
                });

                _programStencilDrawToMs = gd.CreateProgramWithMinimalLayout(new[]
                {
                    new ShaderSource(ShaderBinaries.ColorDrawToMsVertexShaderSource, colorDrawToMsVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                    new ShaderSource(ShaderBinaries.StencilDrawToMsFragmentShaderSource, colorDrawToMsFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
                });

                _programStencilDrawToNonMs = gd.CreateProgramWithMinimalLayout(new[]
                {
                    new ShaderSource(ShaderBinaries.ColorDrawToMsVertexShaderSource, colorDrawToMsVertexBindings, ShaderStage.Vertex, TargetLanguage.Spirv),
                    new ShaderSource(ShaderBinaries.StencilDrawToNonMsFragmentShaderSource, colorDrawToMsFragmentBindings, ShaderStage.Fragment, TargetLanguage.Spirv),
                });
            }
        }

        public void Blit(
            VulkanRenderer gd,
            TextureView src,
            TextureView dst,
            Extents2D srcRegion,
            Extents2D dstRegion,
            int layers,
            int levels,
            bool isDepthOrStencil,
            bool linearFilter,
            bool clearAlpha = false)
        {
            gd.FlushAllCommands();

            using var cbs = gd.CommandBufferPool.Rent();

            var dstFormat = dst.VkFormat;
            var dstSamples = dst.Info.Samples;

            for (int l = 0; l < levels; l++)
            {
                int srcWidth = Math.Max(1, src.Width >> l);
                int srcHeight = Math.Max(1, src.Height >> l);

                int dstWidth = Math.Max(1, dst.Width >> l);
                int dstHeight = Math.Max(1, dst.Height >> l);

                var mipSrcRegion = new Extents2D(
                    srcRegion.X1 >> l,
                    srcRegion.Y1 >> l,
                    srcRegion.X2 >> l,
                    srcRegion.Y2 >> l);

                var mipDstRegion = new Extents2D(
                    dstRegion.X1 >> l,
                    dstRegion.Y1 >> l,
                    dstRegion.X2 >> l,
                    dstRegion.Y2 >> l);

                for (int z = 0; z < layers; z++)
                {
                    var srcView = Create2DLayerView(src, z, l);
                    var dstView = Create2DLayerView(dst, z, l);

                    if (isDepthOrStencil)
                    {
                        BlitDepthStencil(
                            gd,
                            cbs,
                            srcView,
                            dst.GetImageViewForAttachment(),
                            dstWidth,
                            dstHeight,
                            dstSamples,
                            dstFormat,
                            mipSrcRegion,
                            mipDstRegion);
                    }
                    else
                    {
                        BlitColor(
                            gd,
                            cbs,
                            srcView,
                            dst.GetImageViewForAttachment(),
                            dstWidth,
                            dstHeight,
                            dstSamples,
                            dstFormat,
                            false,
                            mipSrcRegion,
                            mipDstRegion,
                            linearFilter,
                            clearAlpha);
                    }

                    if (srcView != src)
                    {
                        srcView.Release();
                    }

                    if (dstView != dst)
                    {
                        dstView.Release();
                    }
                }
            }
        }

        public void CopyColor(
            VulkanRenderer gd,
            CommandBufferScoped cbs,
            TextureView src,
            TextureView dst,
            int srcLayer,
            int dstLayer,
            int srcLevel,
            int dstLevel,
            int depth,
            int levels)
        {
            for (int l = 0; l < levels; l++)
            {
                int mipSrcLevel = srcLevel + l;
                int mipDstLevel = dstLevel + l;

                int srcWidth = Math.Max(1, src.Width >> mipSrcLevel);
                int srcHeight = Math.Max(1, src.Height >> mipSrcLevel);

                int dstWidth = Math.Max(1, dst.Width >> mipDstLevel);
                int dstHeight = Math.Max(1, dst.Height >> mipDstLevel);

                var extents = new Extents2D(
                    0,
                    0,
                    Math.Min(srcWidth, dstWidth),
                    Math.Min(srcHeight, dstHeight));

                for (int z = 0; z < depth; z++)
                {
                    var srcView = Create2DLayerView(src, srcLayer + z, mipSrcLevel);
                    var dstView = Create2DLayerView(dst, dstLayer + z, mipDstLevel);

                    BlitColor(
                        gd,
                        cbs,
                        srcView,
                        dstView.GetImageViewForAttachment(),
                        dstView.Width,
                        dstView.Height,
                        dstView.Info.Samples,
                        dstView.VkFormat,
                        dstView.Info.Format.IsDepthOrStencil(),
                        extents,
                        extents,
                        false);

                    if (srcView != src)
                    {
                        srcView.Release();
                    }

                    if (dstView != dst)
                    {
                        dstView.Release();
                    }
                }
            }
        }

        public void BlitColor(
            VulkanRenderer gd,
            CommandBufferScoped cbs,
            TextureView src,
            Auto<DisposableImageView> dst,
            int dstWidth,
            int dstHeight,
            int dstSamples,
            VkFormat dstFormat,
            bool dstIsDepthOrStencil,
            Extents2D srcRegion,
            Extents2D dstRegion,
            bool linearFilter,
            bool clearAlpha = false)
        {
            _pipeline.SetCommandBuffer(cbs);

            const int RegionBufferSize = 16;

            var sampler = linearFilter ? _samplerLinear : _samplerNearest;

            _pipeline.SetTextureAndSampler(ShaderStage.Fragment, 0, src, sampler);

            Span<float> region = stackalloc float[RegionBufferSize / sizeof(float)];

            region[0] = (float)srcRegion.X1 / src.Width;
            region[1] = (float)srcRegion.X2 / src.Width;
            region[2] = (float)srcRegion.Y1 / src.Height;
            region[3] = (float)srcRegion.Y2 / src.Height;

            if (dstRegion.X1 > dstRegion.X2)
            {
                (region[0], region[1]) = (region[1], region[0]);
            }

            if (dstRegion.Y1 > dstRegion.Y2)
            {
                (region[2], region[3]) = (region[3], region[2]);
            }

            var bufferHandle = gd.BufferManager.CreateWithHandle(gd, RegionBufferSize);

            gd.BufferManager.SetData<float>(bufferHandle, 0, region);

            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(1, new BufferRange(bufferHandle, 0, RegionBufferSize)) });

            Span<GAL.Viewport> viewports = stackalloc GAL.Viewport[1];

            var rect = new Rectangle<float>(
                MathF.Min(dstRegion.X1, dstRegion.X2),
                MathF.Min(dstRegion.Y1, dstRegion.Y2),
                MathF.Abs(dstRegion.X2 - dstRegion.X1),
                MathF.Abs(dstRegion.Y2 - dstRegion.Y1));

            viewports[0] = new GAL.Viewport(
                rect,
                ViewportSwizzle.PositiveX,
                ViewportSwizzle.PositiveY,
                ViewportSwizzle.PositiveZ,
                ViewportSwizzle.PositiveW,
                0f,
                1f);

            Span<Rectangle<int>> scissors = stackalloc Rectangle<int>[1];

            scissors[0] = new Rectangle<int>(0, 0, dstWidth, dstHeight);

            if (dstIsDepthOrStencil)
            {
                _pipeline.SetProgram(src.Info.Target.IsMultisample() ? _programDepthBlitMs : _programDepthBlit);
                _pipeline.SetDepthTest(new DepthTestDescriptor(true, true, GAL.CompareOp.Always));
            }
            else if (src.Info.Target.IsMultisample())
            {
                _pipeline.SetProgram(_programColorBlitMs);
            }
            else if (clearAlpha)
            {
                _pipeline.SetProgram(_programColorBlitClearAlpha);
            }
            else
            {
                _pipeline.SetProgram(_programColorBlit);
            }

            _pipeline.SetRenderTarget(dst, (uint)dstWidth, (uint)dstHeight, (uint)dstSamples, dstIsDepthOrStencil, dstFormat);
            _pipeline.SetRenderTargetColorMasks(new uint[] { 0xf });
            _pipeline.SetScissors(scissors);

            if (clearAlpha)
            {
                _pipeline.ClearRenderTargetColor(0, 0, 1, new ColorF(0f, 0f, 0f, 1f));
            }

            _pipeline.SetViewports(viewports, false);
            _pipeline.SetPrimitiveTopology(GAL.PrimitiveTopology.TriangleStrip);
            _pipeline.Draw(4, 1, 0, 0);

            if (dstIsDepthOrStencil)
            {
                _pipeline.SetDepthTest(new DepthTestDescriptor(false, false, GAL.CompareOp.Always));
            }

            _pipeline.Finish(gd, cbs);

            gd.BufferManager.Delete(bufferHandle);
        }

        private void BlitDepthStencil(
            VulkanRenderer gd,
            CommandBufferScoped cbs,
            TextureView src,
            Auto<DisposableImageView> dst,
            int dstWidth,
            int dstHeight,
            int dstSamples,
            VkFormat dstFormat,
            Extents2D srcRegion,
            Extents2D dstRegion)
        {
            _pipeline.SetCommandBuffer(cbs);

            const int RegionBufferSize = 16;

            Span<float> region = stackalloc float[RegionBufferSize / sizeof(float)];

            region[0] = (float)srcRegion.X1 / src.Width;
            region[1] = (float)srcRegion.X2 / src.Width;
            region[2] = (float)srcRegion.Y1 / src.Height;
            region[3] = (float)srcRegion.Y2 / src.Height;

            if (dstRegion.X1 > dstRegion.X2)
            {
                (region[0], region[1]) = (region[1], region[0]);
            }

            if (dstRegion.Y1 > dstRegion.Y2)
            {
                (region[2], region[3]) = (region[3], region[2]);
            }

            var bufferHandle = gd.BufferManager.CreateWithHandle(gd, RegionBufferSize);

            gd.BufferManager.SetData<float>(bufferHandle, 0, region);

            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(1, new BufferRange(bufferHandle, 0, RegionBufferSize)) });

            Span<GAL.Viewport> viewports = stackalloc GAL.Viewport[1];

            var rect = new Rectangle<float>(
                MathF.Min(dstRegion.X1, dstRegion.X2),
                MathF.Min(dstRegion.Y1, dstRegion.Y2),
                MathF.Abs(dstRegion.X2 - dstRegion.X1),
                MathF.Abs(dstRegion.Y2 - dstRegion.Y1));

            viewports[0] = new GAL.Viewport(
                rect,
                ViewportSwizzle.PositiveX,
                ViewportSwizzle.PositiveY,
                ViewportSwizzle.PositiveZ,
                ViewportSwizzle.PositiveW,
                0f,
                1f);

            Span<Rectangle<int>> scissors = stackalloc Rectangle<int>[1];

            scissors[0] = new Rectangle<int>(0, 0, dstWidth, dstHeight);

            _pipeline.SetRenderTarget(dst, (uint)dstWidth, (uint)dstHeight, (uint)dstSamples, true, dstFormat);
            _pipeline.SetScissors(scissors);
            _pipeline.SetViewports(viewports, false);
            _pipeline.SetPrimitiveTopology(GAL.PrimitiveTopology.TriangleStrip);

            var aspectFlags = src.Info.Format.ConvertAspectFlags();

            if (aspectFlags.HasFlag(ImageAspectFlags.DepthBit))
            {
                var depthTexture = CreateDepthOrStencilView(src, DepthStencilMode.Depth);

                BlitDepthStencilDraw(depthTexture, isDepth: true);

                if (depthTexture != src)
                {
                    depthTexture.Release();
                }
            }

            if (aspectFlags.HasFlag(ImageAspectFlags.StencilBit) && _programStencilBlit != null)
            {
                var stencilTexture = CreateDepthOrStencilView(src, DepthStencilMode.Stencil);

                BlitDepthStencilDraw(stencilTexture, isDepth: false);

                if (stencilTexture != src)
                {
                    stencilTexture.Release();
                }
            }

            _pipeline.Finish(gd, cbs);

            gd.BufferManager.Delete(bufferHandle);
        }

        private static TextureView CreateDepthOrStencilView(TextureView depthStencilTexture, DepthStencilMode depthStencilMode)
        {
            if (depthStencilTexture.Info.DepthStencilMode == depthStencilMode)
            {
                return depthStencilTexture;
            }

            return (TextureView)depthStencilTexture.CreateView(new TextureCreateInfo(
                depthStencilTexture.Info.Width,
                depthStencilTexture.Info.Height,
                depthStencilTexture.Info.Depth,
                depthStencilTexture.Info.Levels,
                depthStencilTexture.Info.Samples,
                depthStencilTexture.Info.BlockWidth,
                depthStencilTexture.Info.BlockHeight,
                depthStencilTexture.Info.BytesPerPixel,
                depthStencilTexture.Info.Format,
                depthStencilMode,
                depthStencilTexture.Info.Target,
                SwizzleComponent.Red,
                SwizzleComponent.Green,
                SwizzleComponent.Blue,
                SwizzleComponent.Alpha), 0, 0);
        }

        private void BlitDepthStencilDraw(TextureView src, bool isDepth)
        {
            _pipeline.SetTextureAndSampler(ShaderStage.Fragment, 0, src, _samplerNearest);

            if (isDepth)
            {
                _pipeline.SetProgram(src.Info.Target.IsMultisample() ? _programDepthBlitMs : _programDepthBlit);
                _pipeline.SetDepthTest(new DepthTestDescriptor(true, true, GAL.CompareOp.Always));
            }
            else
            {
                _pipeline.SetProgram(src.Info.Target.IsMultisample() ? _programStencilBlitMs : _programStencilBlit);
                _pipeline.SetStencilTest(CreateStencilTestDescriptor(true));
            }

            _pipeline.Draw(4, 1, 0, 0);

            if (isDepth)
            {
                _pipeline.SetDepthTest(new DepthTestDescriptor(false, false, GAL.CompareOp.Always));
            }
            else
            {
                _pipeline.SetStencilTest(CreateStencilTestDescriptor(false));
            }
        }

        private static StencilTestDescriptor CreateStencilTestDescriptor(bool enabled)
        {
            return new StencilTestDescriptor(
                enabled,
                GAL.CompareOp.Always,
                GAL.StencilOp.Replace,
                GAL.StencilOp.Replace,
                GAL.StencilOp.Replace,
                0,
                0xff,
                0xff,
                GAL.CompareOp.Always,
                GAL.StencilOp.Replace,
                GAL.StencilOp.Replace,
                GAL.StencilOp.Replace,
                0,
                0xff,
                0xff);
        }

        public void Clear(
            VulkanRenderer gd,
            Auto<DisposableImageView> dst,
            ReadOnlySpan<float> clearColor,
            uint componentMask,
            int dstWidth,
            int dstHeight,
            VkFormat dstFormat,
            ComponentType type,
            Rectangle<int> scissor)
        {
            const int ClearColorBufferSize = 16;

            gd.FlushAllCommands();

            using var cbs = gd.CommandBufferPool.Rent();

            _pipeline.SetCommandBuffer(cbs);

            var bufferHandle = gd.BufferManager.CreateWithHandle(gd, ClearColorBufferSize);

            gd.BufferManager.SetData<float>(bufferHandle, 0, clearColor);

            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(1, new BufferRange(bufferHandle, 0, ClearColorBufferSize)) });

            Span<GAL.Viewport> viewports = stackalloc GAL.Viewport[1];

            viewports[0] = new GAL.Viewport(
                new Rectangle<float>(0, 0, dstWidth, dstHeight),
                ViewportSwizzle.PositiveX,
                ViewportSwizzle.PositiveY,
                ViewportSwizzle.PositiveZ,
                ViewportSwizzle.PositiveW,
                0f,
                1f);

            Span<Rectangle<int>> scissors = stackalloc Rectangle<int>[1];

            scissors[0] = scissor;

            IProgram program;

            if (type == ComponentType.SignedInteger)
            {
                program = _programColorClearSI;
            }
            else if (type == ComponentType.UnsignedInteger)
            {
                program = _programColorClearUI;
            }
            else
            {
                program = _programColorClearF;
            }

            _pipeline.SetProgram(program);
            _pipeline.SetRenderTarget(dst, (uint)dstWidth, (uint)dstHeight, false, dstFormat);
            _pipeline.SetRenderTargetColorMasks(new uint[] { componentMask });
            _pipeline.SetViewports(viewports, false);
            _pipeline.SetScissors(scissors);
            _pipeline.SetPrimitiveTopology(GAL.PrimitiveTopology.TriangleStrip);
            _pipeline.Draw(4, 1, 0, 0);
            _pipeline.Finish();

            gd.BufferManager.Delete(bufferHandle);
        }

        public void DrawTexture(
            VulkanRenderer gd,
            PipelineBase pipeline,
            TextureView src,
            ISampler srcSampler,
            Extents2DF srcRegion,
            Extents2DF dstRegion)
        {
            const int RegionBufferSize = 16;

            pipeline.SetTextureAndSampler(ShaderStage.Fragment, 0, src, srcSampler);

            Span<float> region = stackalloc float[RegionBufferSize / sizeof(float)];

            region[0] = srcRegion.X1 / src.Width;
            region[1] = srcRegion.X2 / src.Width;
            region[2] = srcRegion.Y1 / src.Height;
            region[3] = srcRegion.Y2 / src.Height;

            if (dstRegion.X1 > dstRegion.X2)
            {
                (region[0], region[1]) = (region[1], region[0]);
            }

            if (dstRegion.Y1 > dstRegion.Y2)
            {
                (region[2], region[3]) = (region[3], region[2]);
            }

            var bufferHandle = gd.BufferManager.CreateWithHandle(gd, RegionBufferSize);

            gd.BufferManager.SetData<float>(bufferHandle, 0, region);

            pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(1, new BufferRange(bufferHandle, 0, RegionBufferSize)) });

            Span<GAL.Viewport> viewports = stackalloc GAL.Viewport[1];

            var rect = new Rectangle<float>(
                MathF.Min(dstRegion.X1, dstRegion.X2),
                MathF.Min(dstRegion.Y1, dstRegion.Y2),
                MathF.Abs(dstRegion.X2 - dstRegion.X1),
                MathF.Abs(dstRegion.Y2 - dstRegion.Y1));

            viewports[0] = new GAL.Viewport(
                rect,
                ViewportSwizzle.PositiveX,
                ViewportSwizzle.PositiveY,
                ViewportSwizzle.PositiveZ,
                ViewportSwizzle.PositiveW,
                0f,
                1f);

            Span<Rectangle<int>> scissors = stackalloc Rectangle<int>[1];

            pipeline.SetProgram(_programColorBlit);
            pipeline.SetViewports(viewports, false);
            pipeline.SetPrimitiveTopology(GAL.PrimitiveTopology.TriangleStrip);
            pipeline.Draw(4, 1, 0, 0);

            gd.BufferManager.Delete(bufferHandle);
        }

        public unsafe void ConvertI8ToI16(VulkanRenderer gd, CommandBufferScoped cbs, BufferHolder src, BufferHolder dst, int srcOffset, int size)
        {
            ChangeStride(gd, cbs, src, dst, srcOffset, size, 1, 2);
        }

        public unsafe void ChangeStride(VulkanRenderer gd, CommandBufferScoped cbs, BufferHolder src, BufferHolder dst, int srcOffset, int size, int stride, int newStride)
        {
            bool supportsUint8 = gd.Capabilities.SupportsShaderInt8;

            int elems = size / stride;
            int newSize = elems * newStride;

            var srcBufferAuto = src.GetBuffer();
            var dstBufferAuto = dst.GetBuffer();

            var srcBuffer = srcBufferAuto.Get(cbs, srcOffset, size).Value;
            var dstBuffer = dstBufferAuto.Get(cbs, 0, newSize).Value;

            var access = supportsUint8 ? AccessFlags.ShaderWriteBit : AccessFlags.TransferWriteBit;
            var stage = supportsUint8 ? PipelineStageFlags.ComputeShaderBit : PipelineStageFlags.TransferBit;

            BufferHolder.InsertBufferBarrier(
                gd,
                cbs.CommandBuffer,
                dstBuffer,
                BufferHolder.DefaultAccessFlags,
                access,
                PipelineStageFlags.AllCommandsBit,
                stage,
                0,
                newSize);

            if (supportsUint8)
            {
                const int ParamsBufferSize = 16;

                Span<int> shaderParams = stackalloc int[ParamsBufferSize / sizeof(int)];

                shaderParams[0] = stride;
                shaderParams[1] = newStride;
                shaderParams[2] = size;
                shaderParams[3] = srcOffset;

                var bufferHandle = gd.BufferManager.CreateWithHandle(gd, ParamsBufferSize);

                gd.BufferManager.SetData<int>(bufferHandle, 0, shaderParams);

                _pipeline.SetCommandBuffer(cbs);

                _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(0, new BufferRange(bufferHandle, 0, ParamsBufferSize)) });

                Span<Auto<DisposableBuffer>> sbRanges = new Auto<DisposableBuffer>[2];

                sbRanges[0] = srcBufferAuto;
                sbRanges[1] = dstBufferAuto;

                _pipeline.SetStorageBuffers(1, sbRanges);

                _pipeline.SetProgram(_programStrideChange);
                _pipeline.DispatchCompute(1, 1, 1);

                gd.BufferManager.Delete(bufferHandle);

                _pipeline.Finish(gd, cbs);
            }
            else
            {
                gd.Api.CmdFillBuffer(cbs.CommandBuffer, dstBuffer, 0, Vk.WholeSize, 0);

                var bufferCopy = new BufferCopy[elems];

                for (ulong i = 0; i < (ulong)elems; i++)
                {
                    bufferCopy[i] = new BufferCopy((ulong)srcOffset + i * (ulong)stride, i * (ulong)newStride, (ulong)stride);
                }

                fixed (BufferCopy* pBufferCopy = bufferCopy)
                {
                    gd.Api.CmdCopyBuffer(cbs.CommandBuffer, srcBuffer, dstBuffer, (uint)elems, pBufferCopy);
                }
            }

            BufferHolder.InsertBufferBarrier(
                gd,
                cbs.CommandBuffer,
                dstBuffer,
                access,
                BufferHolder.DefaultAccessFlags,
                stage,
                PipelineStageFlags.AllCommandsBit,
                0,
                newSize);
        }

        public unsafe void ConvertIndexBuffer(VulkanRenderer gd,
            CommandBufferScoped cbs,
            BufferHolder src,
            BufferHolder dst,
            IndexBufferPattern pattern,
            int indexSize,
            int srcOffset,
            int indexCount)
        {
            // TODO: Support conversion with primitive restart enabled.
            // TODO: Convert with a compute shader?

            int convertedCount = pattern.GetConvertedCount(indexCount);
            int outputIndexSize = 4;

            var srcBuffer = src.GetBuffer().Get(cbs, srcOffset, indexCount * indexSize).Value;
            var dstBuffer = dst.GetBuffer().Get(cbs, 0, convertedCount * outputIndexSize).Value;

            gd.Api.CmdFillBuffer(cbs.CommandBuffer, dstBuffer, 0, Vk.WholeSize, 0);

            var bufferCopy = new List<BufferCopy>();
            int outputOffset = 0;

            // Try to merge copies of adjacent indices to reduce copy count.
            int sequenceStart = 0;
            int sequenceLength = 0;

            foreach (var index in pattern.GetIndexMapping(indexCount))
            {
                if (sequenceLength > 0)
                {
                    if (index == sequenceStart + sequenceLength && indexSize == outputIndexSize)
                    {
                        sequenceLength++;
                        continue;
                    }

                    // Commit the copy so far.
                    bufferCopy.Add(new BufferCopy((ulong)(srcOffset + sequenceStart * indexSize), (ulong)outputOffset, (ulong)(indexSize * sequenceLength)));
                    outputOffset += outputIndexSize * sequenceLength;
                }

                sequenceStart = index;
                sequenceLength = 1;
            }

            if (sequenceLength > 0)
            {
                // Commit final pending copy.
                bufferCopy.Add(new BufferCopy((ulong)(srcOffset + sequenceStart * indexSize), (ulong)outputOffset, (ulong)(indexSize * sequenceLength)));
            }

            var bufferCopyArray = bufferCopy.ToArray();

            BufferHolder.InsertBufferBarrier(
                gd,
                cbs.CommandBuffer,
                dstBuffer,
                BufferHolder.DefaultAccessFlags,
                AccessFlags.TransferWriteBit,
                PipelineStageFlags.AllCommandsBit,
                PipelineStageFlags.TransferBit,
                0,
                convertedCount * outputIndexSize);

            fixed (BufferCopy* pBufferCopy = bufferCopyArray)
            {
                gd.Api.CmdCopyBuffer(cbs.CommandBuffer, srcBuffer, dstBuffer, (uint)bufferCopyArray.Length, pBufferCopy);
            }

            BufferHolder.InsertBufferBarrier(
                gd,
                cbs.CommandBuffer,
                dstBuffer,
                AccessFlags.TransferWriteBit,
                BufferHolder.DefaultAccessFlags,
                PipelineStageFlags.TransferBit,
                PipelineStageFlags.AllCommandsBit,
                0,
                convertedCount * outputIndexSize);
        }

        public void CopyIncompatibleFormats(
            VulkanRenderer gd,
            CommandBufferScoped cbs,
            TextureView src,
            TextureView dst,
            int srcLayer,
            int dstLayer,
            int srcLevel,
            int dstLevel,
            int depth,
            int levels)
        {
            const int ParamsBufferSize = 4;

            Span<int> shaderParams = stackalloc int[sizeof(int)];

            int srcBpp = src.Info.BytesPerPixel;
            int dstBpp = dst.Info.BytesPerPixel;

            int ratio = srcBpp < dstBpp ? dstBpp / srcBpp : srcBpp / dstBpp;

            shaderParams[0] = BitOperations.Log2((uint)ratio);

            var bufferHandle = gd.BufferManager.CreateWithHandle(gd, ParamsBufferSize);

            gd.BufferManager.SetData<int>(bufferHandle, 0, shaderParams);

            TextureView.InsertImageBarrier(
                gd.Api,
                cbs.CommandBuffer,
                src.GetImage().Get(cbs).Value,
                TextureStorage.DefaultAccessMask,
                AccessFlags.ShaderReadBit,
                PipelineStageFlags.AllCommandsBit,
                PipelineStageFlags.ComputeShaderBit,
                ImageAspectFlags.ColorBit,
                src.FirstLayer + srcLayer,
                src.FirstLevel + srcLevel,
                depth,
                levels);

            _pipeline.SetCommandBuffer(cbs);

            _pipeline.SetProgram(srcBpp < dstBpp ? _programColorCopyWidening : _programColorCopyShortening);

            // Calculate ideal component size, given our constraints:
            // - Component size must not exceed bytes per pixel of source and destination image formats.
            // - Maximum component size is 4 (R32).
            int componentSize = Math.Min(Math.Min(srcBpp, dstBpp), 4);

            var srcFormat = GetFormat(componentSize, srcBpp / componentSize);
            var dstFormat = GetFormat(componentSize, dstBpp / componentSize);

            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(0, new BufferRange(bufferHandle, 0, ParamsBufferSize)) });

            for (int l = 0; l < levels; l++)
            {
                for (int z = 0; z < depth; z++)
                {
                    var srcView = Create2DLayerView(src, srcLayer + z, srcLevel + l, srcFormat);
                    var dstView = Create2DLayerView(dst, dstLayer + z, dstLevel + l);

                    _pipeline.SetTextureAndSampler(ShaderStage.Compute, 0, srcView, null);
                    _pipeline.SetImage(0, dstView, dstFormat);

                    int dispatchX = (Math.Min(srcView.Info.Width, dstView.Info.Width) + 31) / 32;
                    int dispatchY = (Math.Min(srcView.Info.Height, dstView.Info.Height) + 31) / 32;

                    _pipeline.DispatchCompute(dispatchX, dispatchY, 1);

                    if (srcView != src)
                    {
                        srcView.Release();
                    }

                    if (dstView != dst)
                    {
                        dstView.Release();
                    }
                }
            }

            gd.BufferManager.Delete(bufferHandle);

            _pipeline.Finish(gd, cbs);

            TextureView.InsertImageBarrier(
                gd.Api,
                cbs.CommandBuffer,
                dst.GetImage().Get(cbs).Value,
                AccessFlags.ShaderWriteBit,
                TextureStorage.DefaultAccessMask,
                PipelineStageFlags.ComputeShaderBit,
                PipelineStageFlags.AllCommandsBit,
                ImageAspectFlags.ColorBit,
                dst.FirstLayer + dstLayer,
                dst.FirstLevel + dstLevel,
                depth,
                levels);
        }

        public void CopyMSToNonMS(VulkanRenderer gd, CommandBufferScoped cbs, TextureView src, TextureView dst, int srcLayer, int dstLayer, int depth)
        {
            const int ParamsBufferSize = 16;

            Span<int> shaderParams = stackalloc int[ParamsBufferSize / sizeof(int)];

            int samples = src.Info.Samples;
            bool isDepthOrStencil = src.Info.Format.IsDepthOrStencil();
            var aspectFlags = src.Info.Format.ConvertAspectFlags();

            // X and Y are the expected texture samples.
            // Z and W are the actual texture samples used.
            // They may differ if the GPU does not support the samples count requested and we had to use a lower amount.
            (shaderParams[0], shaderParams[1]) = GetSampleCountXYLog2(samples);
            (shaderParams[2], shaderParams[3]) = GetSampleCountXYLog2((int)TextureStorage.ConvertToSampleCountFlags(gd.Capabilities.SupportedSampleCounts, (uint)samples));

            var bufferHandle = gd.BufferManager.CreateWithHandle(gd, ParamsBufferSize);

            gd.BufferManager.SetData<int>(bufferHandle, 0, shaderParams);

            TextureView.InsertImageBarrier(
                gd.Api,
                cbs.CommandBuffer,
                src.GetImage().Get(cbs).Value,
                TextureStorage.DefaultAccessMask,
                AccessFlags.ShaderReadBit,
                PipelineStageFlags.AllCommandsBit,
                isDepthOrStencil ? PipelineStageFlags.FragmentShaderBit : PipelineStageFlags.ComputeShaderBit,
                aspectFlags,
                src.FirstLayer + srcLayer,
                src.FirstLevel,
                depth,
                1);

            _pipeline.SetCommandBuffer(cbs);
            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(0, new BufferRange(bufferHandle, 0, ParamsBufferSize)) });

            if (isDepthOrStencil)
            {
                // We can't use compute for this case because compute can't modify depth textures.

                Span<GAL.Viewport> viewports = stackalloc GAL.Viewport[1];

                var rect = new Rectangle<float>(0, 0, dst.Width, dst.Height);

                viewports[0] = new GAL.Viewport(
                    rect,
                    ViewportSwizzle.PositiveX,
                    ViewportSwizzle.PositiveY,
                    ViewportSwizzle.PositiveZ,
                    ViewportSwizzle.PositiveW,
                    0f,
                    1f);

                Span<Rectangle<int>> scissors = stackalloc Rectangle<int>[1];

                scissors[0] = new Rectangle<int>(0, 0, dst.Width, dst.Height);

                _pipeline.SetScissors(scissors);
                _pipeline.SetViewports(viewports, false);
                _pipeline.SetPrimitiveTopology(GAL.PrimitiveTopology.TriangleStrip);

                for (int z = 0; z < depth; z++)
                {
                    var srcView = Create2DLayerView(src, srcLayer + z, 0);
                    var dstView = Create2DLayerView(dst, dstLayer + z, 0);

                    _pipeline.SetRenderTarget(
                        ((TextureView)dstView).GetImageViewForAttachment(),
                        (uint)dst.Width,
                        (uint)dst.Height,
                        true,
                        dst.VkFormat);

                    CopyMSDraw(srcView, aspectFlags, fromMS: true);

                    if (srcView != src)
                    {
                        srcView.Release();
                    }

                    if (dstView != dst)
                    {
                        dstView.Release();
                    }
                }
            }
            else
            {
                var format = GetFormat(src.Info.BytesPerPixel);

                int dispatchX = (dst.Info.Width + 31) / 32;
                int dispatchY = (dst.Info.Height + 31) / 32;

                _pipeline.SetProgram(_programColorCopyToNonMs);

                for (int z = 0; z < depth; z++)
                {
                    var srcView = Create2DLayerView(src, srcLayer + z, 0, format);
                    var dstView = Create2DLayerView(dst, dstLayer + z, 0);

                    _pipeline.SetTextureAndSampler(ShaderStage.Compute, 0, srcView, null);
                    _pipeline.SetImage(0, dstView, format);

                    _pipeline.DispatchCompute(dispatchX, dispatchY, 1);

                    if (srcView != src)
                    {
                        srcView.Release();
                    }

                    if (dstView != dst)
                    {
                        dstView.Release();
                    }
                }
            }

            gd.BufferManager.Delete(bufferHandle);

            _pipeline.Finish(gd, cbs);

            TextureView.InsertImageBarrier(
                gd.Api,
                cbs.CommandBuffer,
                dst.GetImage().Get(cbs).Value,
                isDepthOrStencil ? AccessFlags.DepthStencilAttachmentWriteBit : AccessFlags.ShaderWriteBit,
                TextureStorage.DefaultAccessMask,
                isDepthOrStencil ? PipelineStageFlags.LateFragmentTestsBit : PipelineStageFlags.ComputeShaderBit,
                PipelineStageFlags.AllCommandsBit,
                aspectFlags,
                dst.FirstLayer + dstLayer,
                dst.FirstLevel,
                depth,
                1);
        }

        public void CopyNonMSToMS(VulkanRenderer gd, CommandBufferScoped cbs, TextureView src, TextureView dst, int srcLayer, int dstLayer, int depth)
        {
            const int ParamsBufferSize = 16;

            Span<int> shaderParams = stackalloc int[ParamsBufferSize / sizeof(int)];

            int samples = dst.Info.Samples;
            bool isDepthOrStencil = src.Info.Format.IsDepthOrStencil();
            var aspectFlags = src.Info.Format.ConvertAspectFlags();

            // X and Y are the expected texture samples.
            // Z and W are the actual texture samples used.
            // They may differ if the GPU does not support the samples count requested and we had to use a lower amount.
            (shaderParams[0], shaderParams[1]) = GetSampleCountXYLog2(samples);
            (shaderParams[2], shaderParams[3]) = GetSampleCountXYLog2((int)TextureStorage.ConvertToSampleCountFlags(gd.Capabilities.SupportedSampleCounts, (uint)samples));

            var bufferHandle = gd.BufferManager.CreateWithHandle(gd, ParamsBufferSize);

            gd.BufferManager.SetData<int>(bufferHandle, 0, shaderParams);

            TextureView.InsertImageBarrier(
                gd.Api,
                cbs.CommandBuffer,
                src.GetImage().Get(cbs).Value,
                TextureStorage.DefaultAccessMask,
                AccessFlags.ShaderReadBit,
                PipelineStageFlags.AllCommandsBit,
                PipelineStageFlags.FragmentShaderBit,
                aspectFlags,
                src.FirstLayer + srcLayer,
                src.FirstLevel,
                depth,
                1);

            _pipeline.SetCommandBuffer(cbs);

            Span<GAL.Viewport> viewports = stackalloc GAL.Viewport[1];

            var rect = new Rectangle<float>(0, 0, dst.Width, dst.Height);

            viewports[0] = new GAL.Viewport(
                rect,
                ViewportSwizzle.PositiveX,
                ViewportSwizzle.PositiveY,
                ViewportSwizzle.PositiveZ,
                ViewportSwizzle.PositiveW,
                0f,
                1f);

            Span<Rectangle<int>> scissors = stackalloc Rectangle<int>[1];

            scissors[0] = new Rectangle<int>(0, 0, dst.Width, dst.Height);

            _pipeline.SetRenderTargetColorMasks(new uint[] { 0xf });
            _pipeline.SetScissors(scissors);
            _pipeline.SetViewports(viewports, false);
            _pipeline.SetPrimitiveTopology(GAL.PrimitiveTopology.TriangleStrip);

            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(0, new BufferRange(bufferHandle, 0, ParamsBufferSize)) });

            if (isDepthOrStencil)
            {
                for (int z = 0; z < depth; z++)
                {
                    var srcView = Create2DLayerView(src, srcLayer + z, 0);
                    var dstView = Create2DLayerView(dst, dstLayer + z, 0);

                    _pipeline.SetRenderTarget(
                        ((TextureView)dstView).GetImageViewForAttachment(),
                        (uint)dst.Width,
                        (uint)dst.Height,
                        (uint)samples,
                        true,
                        dst.VkFormat);

                    CopyMSDraw(srcView, aspectFlags, fromMS: false);

                    if (srcView != src)
                    {
                        srcView.Release();
                    }

                    if (dstView != dst)
                    {
                        dstView.Release();
                    }
                }
            }
            else
            {
                _pipeline.SetProgram(_programColorDrawToMs);

                var format = GetFormat(src.Info.BytesPerPixel);
                var vkFormat = FormatTable.GetFormat(format);

                for (int z = 0; z < depth; z++)
                {
                    var srcView = Create2DLayerView(src, srcLayer + z, 0, format);
                    var dstView = Create2DLayerView(dst, dstLayer + z, 0);

                    _pipeline.SetTextureAndSampler(ShaderStage.Fragment, 0, srcView, null);
                    _pipeline.SetRenderTarget(
                        ((TextureView)dstView).GetView(format).GetImageViewForAttachment(),
                        (uint)dst.Width,
                        (uint)dst.Height,
                        (uint)samples,
                        false,
                        vkFormat);

                    _pipeline.Draw(4, 1, 0, 0);

                    if (srcView != src)
                    {
                        srcView.Release();
                    }

                    if (dstView != dst)
                    {
                        dstView.Release();
                    }
                }
            }

            gd.BufferManager.Delete(bufferHandle);

            _pipeline.Finish(gd, cbs);

            TextureView.InsertImageBarrier(
                gd.Api,
                cbs.CommandBuffer,
                dst.GetImage().Get(cbs).Value,
                isDepthOrStencil ? AccessFlags.DepthStencilAttachmentWriteBit : AccessFlags.ColorAttachmentWriteBit,
                TextureStorage.DefaultAccessMask,
                isDepthOrStencil ? PipelineStageFlags.LateFragmentTestsBit : PipelineStageFlags.ColorAttachmentOutputBit,
                PipelineStageFlags.AllCommandsBit,
                aspectFlags,
                dst.FirstLayer + dstLayer,
                dst.FirstLevel,
                depth,
                1);
        }

        private void CopyMSDraw(TextureView src, ImageAspectFlags aspectFlags, bool fromMS)
        {
            if (aspectFlags.HasFlag(ImageAspectFlags.DepthBit))
            {
                var depthTexture = CreateDepthOrStencilView(src, DepthStencilMode.Depth);

                CopyMSAspectDraw(depthTexture, fromMS, isDepth: true);

                if (depthTexture != src)
                {
                    depthTexture.Release();
                }
            }

            if (aspectFlags.HasFlag(ImageAspectFlags.StencilBit) && _programStencilDrawToMs != null)
            {
                var stencilTexture = CreateDepthOrStencilView(src, DepthStencilMode.Stencil);

                CopyMSAspectDraw(stencilTexture, fromMS, isDepth: false);

                if (stencilTexture != src)
                {
                    stencilTexture.Release();
                }
            }
        }

        private void CopyMSAspectDraw(TextureView src, bool fromMS, bool isDepth)
        {
            _pipeline.SetTextureAndSampler(ShaderStage.Fragment, 0, src, _samplerNearest);

            if (isDepth)
            {
                _pipeline.SetProgram(fromMS ? _programDepthDrawToNonMs : _programDepthDrawToMs);
                _pipeline.SetDepthTest(new DepthTestDescriptor(true, true, GAL.CompareOp.Always));
            }
            else
            {
                _pipeline.SetProgram(fromMS ? _programStencilDrawToNonMs : _programStencilDrawToMs);
                _pipeline.SetStencilTest(CreateStencilTestDescriptor(true));
            }

            _pipeline.Draw(4, 1, 0, 0);

            if (isDepth)
            {
                _pipeline.SetDepthTest(new DepthTestDescriptor(false, false, GAL.CompareOp.Always));
            }
            else
            {
                _pipeline.SetStencilTest(CreateStencilTestDescriptor(false));
            }
        }

        private static (int, int) GetSampleCountXYLog2(int samples)
        {
            int samplesInXLog2 = 0;
            int samplesInYLog2 = 0;

            switch (samples)
            {
                case 2: // 2x1
                    samplesInXLog2 = 1;
                    break;
                case 4: // 2x2
                    samplesInXLog2 = 1;
                    samplesInYLog2 = 1;
                    break;
                case 8: // 4x2
                    samplesInXLog2 = 2;
                    samplesInYLog2 = 1;
                    break;
                case 16: // 4x4
                    samplesInXLog2 = 2;
                    samplesInYLog2 = 2;
                    break;
                case 32: // 8x4
                    samplesInXLog2 = 3;
                    samplesInYLog2 = 2;
                    break;
                case 64: // 8x8
                    samplesInXLog2 = 3;
                    samplesInYLog2 = 3;
                    break;
            }

            return (samplesInXLog2, samplesInYLog2);
        }

        private static TextureView Create2DLayerView(TextureView from, int layer, int level, GAL.Format? format = null)
        {
            if (from.Info.Target == Target.Texture2D && level == 0 && (format == null || format.Value == from.Info.Format))
            {
                return from;
            }

            var target = from.Info.Target switch
            {
                Target.Texture1DArray => Target.Texture1D,
                Target.Texture2DMultisampleArray => Target.Texture2DMultisample,
                _ => Target.Texture2D
            };

            var info = new TextureCreateInfo(
                from.Info.Width,
                from.Info.Height,
                from.Info.Depth,
                1,
                from.Info.Samples,
                from.Info.BlockWidth,
                from.Info.BlockHeight,
                from.Info.BytesPerPixel,
                format ?? from.Info.Format,
                from.Info.DepthStencilMode,
                target,
                from.Info.SwizzleR,
                from.Info.SwizzleG,
                from.Info.SwizzleB,
                from.Info.SwizzleA);

            return from.CreateViewImpl(info, layer, level);
        }

        private static GAL.Format GetFormat(int bytesPerPixel)
        {
            return bytesPerPixel switch
            {
                1 => GAL.Format.R8Uint,
                2 => GAL.Format.R16Uint,
                4 => GAL.Format.R32Uint,
                8 => GAL.Format.R32G32Uint,
                16 => GAL.Format.R32G32B32A32Uint,
                _ => throw new ArgumentException($"Invalid bytes per pixel {bytesPerPixel}.")
            };
        }

        private static GAL.Format GetFormat(int componentSize, int componentsCount)
        {
            if (componentSize == 1)
            {
                return componentsCount switch
                {
                    1 => GAL.Format.R8Uint,
                    2 => GAL.Format.R8G8Uint,
                    4 => GAL.Format.R8G8B8A8Uint,
                    _ => throw new ArgumentException($"Invalid components count {componentsCount}.")
                };
            }
            else if (componentSize == 2)
            {
                return componentsCount switch
                {
                    1 => GAL.Format.R16Uint,
                    2 => GAL.Format.R16G16Uint,
                    4 => GAL.Format.R16G16B16A16Uint,
                    _ => throw new ArgumentException($"Invalid components count {componentsCount}.")
                };
            }
            else if (componentSize == 4)
            {
                return componentsCount switch
                {
                    1 => GAL.Format.R32Uint,
                    2 => GAL.Format.R32G32Uint,
                    4 => GAL.Format.R32G32B32A32Uint,
                    _ => throw new ArgumentException($"Invalid components count {componentsCount}.")
                };
            }
            else
            {
                throw new ArgumentException($"Invalid component size {componentSize}.");
            }
        }

        public void ConvertIndexBufferIndirect(
            VulkanRenderer gd,
            CommandBufferScoped cbs,
            BufferHolder srcIndirectBuffer,
            BufferHolder dstIndirectBuffer,
            BufferRange drawCountBuffer,
            BufferHolder srcIndexBuffer,
            BufferHolder dstIndexBuffer,
            IndexBufferPattern pattern,
            int indexSize,
            int srcIndexBufferOffset,
            int srcIndexBufferSize,
            int srcIndirectBufferOffset,
            bool hasDrawCount,
            int maxDrawCount,
            int indirectDataStride)
        {
            // TODO: Support conversion with primitive restart enabled.

            BufferRange drawCountBufferAligned = new BufferRange(
                drawCountBuffer.Handle,
                drawCountBuffer.Offset & ~(UniformBufferAlignment - 1),
                UniformBufferAlignment);

            int indirectDataSize = maxDrawCount * indirectDataStride;

            int indexCount = srcIndexBufferSize / indexSize;
            int primitivesCount = pattern.GetPrimitiveCount(indexCount);
            int convertedCount = pattern.GetConvertedCount(indexCount);
            int outputIndexSize = 4;

            var srcBuffer = srcIndexBuffer.GetBuffer().Get(cbs, srcIndexBufferOffset, indexCount * indexSize).Value;
            var dstBuffer = dstIndexBuffer.GetBuffer().Get(cbs, 0, convertedCount * outputIndexSize).Value;

            const int ParamsBufferSize = 24 * sizeof(int);
            const int ParamsIndirectDispatchOffset = 16 * sizeof(int);
            const int ParamsIndirectDispatchSize = 3 * sizeof(int);

            Span<int> shaderParams = stackalloc int[ParamsBufferSize / sizeof(int)];

            shaderParams[8] = pattern.PrimitiveVertices;
            shaderParams[9] = pattern.PrimitiveVerticesOut;
            shaderParams[10] = indexSize;
            shaderParams[11] = outputIndexSize;
            shaderParams[12] = pattern.BaseIndex;
            shaderParams[13] = pattern.IndexStride;
            shaderParams[14] = srcIndexBufferOffset;
            shaderParams[15] = primitivesCount;
            shaderParams[16] = 1;
            shaderParams[17] = 1;
            shaderParams[18] = 1;
            shaderParams[19] = hasDrawCount ? 1 : 0;
            shaderParams[20] = maxDrawCount;
            shaderParams[21] = (drawCountBuffer.Offset & (UniformBufferAlignment - 1)) / 4;
            shaderParams[22] = indirectDataStride / 4;
            shaderParams[23] = srcIndirectBufferOffset / 4;

            pattern.OffsetIndex.CopyTo(shaderParams.Slice(0, pattern.OffsetIndex.Length));

            var patternBufferHandle = gd.BufferManager.CreateWithHandle(gd, ParamsBufferSize, out var patternBuffer);
            var patternBufferAuto = patternBuffer.GetBuffer();

            gd.BufferManager.SetData<int>(patternBufferHandle, 0, shaderParams);

            _pipeline.SetCommandBuffer(cbs);

            BufferHolder.InsertBufferBarrier(
                gd,
                cbs.CommandBuffer,
                srcIndirectBuffer.GetBuffer().Get(cbs, srcIndirectBufferOffset, indirectDataSize).Value,
                BufferHolder.DefaultAccessFlags,
                AccessFlags.ShaderReadBit,
                PipelineStageFlags.AllCommandsBit,
                PipelineStageFlags.ComputeShaderBit,
                srcIndirectBufferOffset,
                indirectDataSize);

            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(0, drawCountBufferAligned) });
            _pipeline.SetStorageBuffers(1, new[] { srcIndirectBuffer.GetBuffer(), dstIndirectBuffer.GetBuffer(), patternBuffer.GetBuffer() });

            _pipeline.SetProgram(_programConvertIndirectData);
            _pipeline.DispatchCompute(1, 1, 1);

            BufferHolder.InsertBufferBarrier(
                gd,
                cbs.CommandBuffer,
                patternBufferAuto.Get(cbs, ParamsIndirectDispatchOffset, ParamsIndirectDispatchSize).Value,
                AccessFlags.ShaderWriteBit,
                AccessFlags.IndirectCommandReadBit,
                PipelineStageFlags.ComputeShaderBit,
                PipelineStageFlags.DrawIndirectBit,
                ParamsIndirectDispatchOffset,
                ParamsIndirectDispatchSize);

            BufferHolder.InsertBufferBarrier(
                gd,
                cbs.CommandBuffer,
                dstBuffer,
                BufferHolder.DefaultAccessFlags,
                AccessFlags.TransferWriteBit,
                PipelineStageFlags.AllCommandsBit,
                PipelineStageFlags.TransferBit,
                0,
                convertedCount * outputIndexSize);

            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(0, new BufferRange(patternBufferHandle, 0, ParamsBufferSize)) });
            _pipeline.SetStorageBuffers(1, new[] { srcIndexBuffer.GetBuffer(), dstIndexBuffer.GetBuffer() });

            _pipeline.SetProgram(_programConvertIndexBuffer);
            _pipeline.DispatchComputeIndirect(patternBufferAuto, ParamsIndirectDispatchOffset);

            BufferHolder.InsertBufferBarrier(
                gd,
                cbs.CommandBuffer,
                dstBuffer,
                AccessFlags.TransferWriteBit,
                BufferHolder.DefaultAccessFlags,
                PipelineStageFlags.TransferBit,
                PipelineStageFlags.AllCommandsBit,
                0,
                convertedCount * outputIndexSize);

            gd.BufferManager.Delete(patternBufferHandle);

            _pipeline.Finish(gd, cbs);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _programColorBlitClearAlpha.Dispose();
                _programColorBlit.Dispose();
                _programColorBlitMs.Dispose();
                _programColorClearF.Dispose();
                _programColorClearSI.Dispose();
                _programColorClearUI.Dispose();
                _programStrideChange.Dispose();
                _programConvertIndexBuffer.Dispose();
                _programConvertIndirectData.Dispose();
                _programColorCopyShortening.Dispose();
                _programColorCopyToNonMs.Dispose();
                _programColorCopyWidening.Dispose();
                _programColorDrawToMs.Dispose();
                _programDepthBlit.Dispose();
                _programDepthBlitMs.Dispose();
                _programDepthDrawToMs.Dispose();
                _programDepthDrawToNonMs.Dispose();
                _programStencilBlit?.Dispose();
                _programStencilBlitMs?.Dispose();
                _programStencilDrawToMs?.Dispose();
                _programStencilDrawToNonMs?.Dispose();
                _samplerNearest.Dispose();
                _samplerLinear.Dispose();
                _pipeline.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
