using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using Silk.NET.Vulkan;
using System;
using Extent2D = Ryujinx.Graphics.GAL.Extents2D;

namespace Ryujinx.Graphics.Vulkan.Effects
{
    internal partial class FsrScalingFilter : IScalingFilter
    {
        private readonly VulkanRenderer _renderer;
        private PipelineHelperShader _pipeline;
        private ISampler _sampler;
        private ShaderCollection _scalingProgram;
        private ShaderCollection _sharpeningProgram;
        private float _sharpeningLevel = 1;
        private Device _device;
        private TextureView _intermediaryTexture;

        public float Level
        {
            get => _sharpeningLevel;
            set
            {
                _sharpeningLevel = MathF.Max(0.01f, value);
            }
        }

        public FsrScalingFilter(VulkanRenderer renderer, Device device)
        {
            _device = device;
            _renderer = renderer;

            Initialize();
        }

        public void Dispose()
        {
            _pipeline.Dispose();
            _scalingProgram.Dispose();
            _sharpeningProgram.Dispose();
            _sampler.Dispose();
            _intermediaryTexture?.Dispose();
        }

        public void Initialize()
        {
            _pipeline = new PipelineHelperShader(_renderer, _device);

            _pipeline.Initialize();

            var scalingShader = EmbeddedResources.Read("Ryujinx.Graphics.Vulkan/Effects/Shaders/FsrScaling.spv");
            var sharpeningShader = EmbeddedResources.Read("Ryujinx.Graphics.Vulkan/Effects/Shaders/FsrSharpening.spv");

            var computeBindings = new ShaderBindings(
                new[] { 2 },
                Array.Empty<int>(),
                new[] { 1 },
                new[] { 0 });

            var sharpeningBindings = new ShaderBindings(
                new[] { 2, 3, 4 },
                Array.Empty<int>(),
                new[] { 1 },
                new[] { 0 });

            _sampler = _renderer.CreateSampler(GAL.SamplerCreateInfo.Create(MinFilter.Linear, MagFilter.Linear));

            _scalingProgram = _renderer.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(scalingShader, computeBindings, ShaderStage.Compute, TargetLanguage.Spirv)
            });

            _sharpeningProgram = _renderer.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(sharpeningShader, sharpeningBindings, ShaderStage.Compute, TargetLanguage.Spirv)
            });
        }

        public void Run(
            TextureView view,
            CommandBufferScoped cbs,
            Auto<DisposableImageView> destinationTexture,
            Silk.NET.Vulkan.Format format,
            int width,
            int height,
            Extent2D source,
            Extent2D destination)
        {
            if (_intermediaryTexture == null
                || _intermediaryTexture.Info.Width != width
                || _intermediaryTexture.Info.Height != height
                || !_intermediaryTexture.Info.Equals(view.Info))
            {
                var originalInfo = view.Info;

                var swapRB = originalInfo.Format.IsBgr() && originalInfo.SwizzleR == SwizzleComponent.Red;

                var info = new TextureCreateInfo(
                    width,
                    height,
                    originalInfo.Depth,
                    originalInfo.Levels,
                    originalInfo.Samples,
                    originalInfo.BlockWidth,
                    originalInfo.BlockHeight,
                    originalInfo.BytesPerPixel,
                    originalInfo.Format,
                    originalInfo.DepthStencilMode,
                    originalInfo.Target,
                    swapRB ? originalInfo.SwizzleB : originalInfo.SwizzleR,
                    originalInfo.SwizzleG,
                    swapRB ? originalInfo.SwizzleR : originalInfo.SwizzleB,
                    originalInfo.SwizzleA);
                _intermediaryTexture?.Dispose();
                _intermediaryTexture = _renderer.CreateTexture(info, view.ScaleFactor) as TextureView;
            }

            Span<GAL.Viewport> viewports = stackalloc GAL.Viewport[1];
            Span<Rectangle<int>> scissors = stackalloc Rectangle<int>[1];

            viewports[0] = new GAL.Viewport(
                new Rectangle<float>(0, 0, view.Width, view.Height),
                ViewportSwizzle.PositiveX,
                ViewportSwizzle.PositiveY,
                ViewportSwizzle.PositiveZ,
                ViewportSwizzle.PositiveW,
                0f,
                1f);

            scissors[0] = new Rectangle<int>(0, 0, view.Width, view.Height);

            _pipeline.SetCommandBuffer(cbs);
            _pipeline.SetProgram(_scalingProgram);
            _pipeline.SetTextureAndSampler(ShaderStage.Compute, 1, view, _sampler);

            float srcWidth = Math.Abs(source.X2 - source.X1);
            float srcHeight = Math.Abs(source.Y2 - source.Y1);
            float scaleX = srcWidth / view.Width;
            float scaleY = srcHeight / view.Height;

            ReadOnlySpan<float> dimensionsBuffer = stackalloc float[]
            {
                source.X1,
                source.X2,
                source.Y1,
                source.Y2,
                destination.X1,
                destination.X2,
                destination.Y1,
                destination.Y2,
                scaleX,
                scaleY
            };

            int rangeSize = dimensionsBuffer.Length * sizeof(float);
            var bufferHandle = _renderer.BufferManager.CreateWithHandle(_renderer, rangeSize);
            _renderer.BufferManager.SetData(bufferHandle, 0, dimensionsBuffer);

            ReadOnlySpan<float> sharpeningBuffer = stackalloc float[] { 1.5f - (Level * 0.01f * 1.5f)};
            var sharpeningBufferHandle = _renderer.BufferManager.CreateWithHandle(_renderer, sizeof(float));
            _renderer.BufferManager.SetData(sharpeningBufferHandle, 0, sharpeningBuffer);

            int threadGroupWorkRegionDim = 16;
            int dispatchX = (width + (threadGroupWorkRegionDim - 1)) / threadGroupWorkRegionDim;
            int dispatchY = (height + (threadGroupWorkRegionDim - 1)) / threadGroupWorkRegionDim;

            var bufferRanges = new BufferRange(bufferHandle, 0, rangeSize);
            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(2, bufferRanges) });
            _pipeline.SetScissors(scissors);
            _pipeline.SetViewports(viewports, false);
            _pipeline.SetImage(0, _intermediaryTexture, GAL.Format.R8G8B8A8Unorm);
            _pipeline.DispatchCompute(dispatchX, dispatchY, 1);
            _pipeline.ComputeBarrier();

            viewports[0] = new GAL.Viewport(
                new Rectangle<float>(0, 0, width, height),
                ViewportSwizzle.PositiveX,
                ViewportSwizzle.PositiveY,
                ViewportSwizzle.PositiveZ,
                ViewportSwizzle.PositiveW,
                0f,
                1f);

            scissors[0] = new Rectangle<int>(0, 0, width, height);

            // Sharpening pass
            _pipeline.SetCommandBuffer(cbs);
            _pipeline.SetProgram(_sharpeningProgram);
            _pipeline.SetTextureAndSampler(ShaderStage.Compute, 1, _intermediaryTexture, _sampler);
            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(2, bufferRanges) });
            var sharpeningRange = new BufferRange(sharpeningBufferHandle, 0, sizeof(float));
            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(4, sharpeningRange) });
            _pipeline.SetScissors(scissors);
            _pipeline.SetViewports(viewports, false);
            _pipeline.SetImage(0, destinationTexture);
            _pipeline.DispatchCompute(dispatchX, dispatchY, 1);
            _pipeline.ComputeBarrier();

            _pipeline.Finish();

            _renderer.BufferManager.Delete(bufferHandle);
            _renderer.BufferManager.Delete(sharpeningBufferHandle);
        }
    }
}