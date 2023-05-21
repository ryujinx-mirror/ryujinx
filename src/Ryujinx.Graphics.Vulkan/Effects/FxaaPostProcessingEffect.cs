using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using Silk.NET.Vulkan;
using System;

namespace Ryujinx.Graphics.Vulkan.Effects
{
    internal partial class FxaaPostProcessingEffect : IPostProcessingEffect
    {
        private readonly VulkanRenderer _renderer;
        private ISampler _samplerLinear;
        private ShaderCollection _shaderProgram;

        private PipelineHelperShader _pipeline;
        private TextureView _texture;

        public FxaaPostProcessingEffect(VulkanRenderer renderer, Device device)
        {
            _renderer = renderer;
            _pipeline = new PipelineHelperShader(renderer, device);

            Initialize();
        }

        public void Dispose()
        {
            _shaderProgram.Dispose();
            _pipeline.Dispose();
            _samplerLinear.Dispose();
            _texture?.Dispose();
        }

        private void Initialize()
        {
            _pipeline.Initialize();

            var shader = EmbeddedResources.Read("Ryujinx.Graphics.Vulkan/Effects/Shaders/Fxaa.spv");

            var resourceLayout = new ResourceLayoutBuilder()
                .Add(ResourceStages.Compute, ResourceType.UniformBuffer, 2)
                .Add(ResourceStages.Compute, ResourceType.TextureAndSampler, 1)
                .Add(ResourceStages.Compute, ResourceType.Image, 0).Build();

            _samplerLinear = _renderer.CreateSampler(GAL.SamplerCreateInfo.Create(MinFilter.Linear, MagFilter.Linear));

            _shaderProgram = _renderer.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(shader, ShaderStage.Compute, TargetLanguage.Spirv)
            }, resourceLayout);
        }

        public TextureView Run(TextureView view, CommandBufferScoped cbs, int width, int height)
        {
            if (_texture == null || _texture.Width != view.Width || _texture.Height != view.Height)
            {
                _texture?.Dispose();

                var info = view.Info;

                if (view.Info.Format.IsBgr())
                {
                    info = new TextureCreateInfo(info.Width,
                        info.Height,
                        info.Depth,
                        info.Levels,
                        info.Samples,
                        info.BlockWidth,
                        info.BlockHeight,
                        info.BytesPerPixel,
                        info.Format,
                        info.DepthStencilMode,
                        info.Target,
                        info.SwizzleB,
                        info.SwizzleG,
                        info.SwizzleR,
                        info.SwizzleA);
                }
                _texture = _renderer.CreateTexture(info, view.ScaleFactor) as TextureView;
            }

            _pipeline.SetCommandBuffer(cbs);
            _pipeline.SetProgram(_shaderProgram);
            _pipeline.SetTextureAndSampler(ShaderStage.Compute, 1, view, _samplerLinear);

            ReadOnlySpan<float> resolutionBuffer = stackalloc float[] { view.Width, view.Height };
            int rangeSize = resolutionBuffer.Length * sizeof(float);
            var bufferHandle = _renderer.BufferManager.CreateWithHandle(_renderer, rangeSize);

            _renderer.BufferManager.SetData(bufferHandle, 0, resolutionBuffer);

            var bufferRanges = new BufferRange(bufferHandle, 0, rangeSize);
            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(2, bufferRanges) });

            var dispatchX = BitUtils.DivRoundUp(view.Width, IPostProcessingEffect.LocalGroupSize);
            var dispatchY = BitUtils.DivRoundUp(view.Height, IPostProcessingEffect.LocalGroupSize);

            _pipeline.SetImage(0, _texture, GAL.Format.R8G8B8A8Unorm);
            _pipeline.DispatchCompute(dispatchX, dispatchY, 1);

            _renderer.BufferManager.Delete(bufferHandle);
            _pipeline.ComputeBarrier();

            _pipeline.Finish();

            return _texture;
        }
    }
}