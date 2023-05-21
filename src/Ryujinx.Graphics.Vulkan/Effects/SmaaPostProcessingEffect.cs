using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using Silk.NET.Vulkan;
using System;
using Format = Ryujinx.Graphics.GAL.Format;

namespace Ryujinx.Graphics.Vulkan.Effects
{
    internal partial class SmaaPostProcessingEffect : IPostProcessingEffect
    {
        public const int AreaWidth = 160;
        public const int AreaHeight = 560;
        public const int SearchWidth = 64;
        public const int SearchHeight = 16;

        private readonly VulkanRenderer _renderer;
        private ISampler _samplerLinear;
        private SmaaConstants _specConstants;
        private ShaderCollection _edgeProgram;
        private ShaderCollection _blendProgram;
        private ShaderCollection _neighbourProgram;

        private PipelineHelperShader _pipeline;

        private TextureView _outputTexture;
        private TextureView _edgeOutputTexture;
        private TextureView _blendOutputTexture;
        private TextureView _areaTexture;
        private TextureView _searchTexture;
        private Device _device;
        private bool _recreatePipelines;
        private int _quality;

        public SmaaPostProcessingEffect(VulkanRenderer renderer, Device device, int quality)
        {
            _device = device;
            _renderer = renderer;
            _quality = quality;

            Initialize();
        }

        public int Quality
        {
            get => _quality;
            set
            {
                _quality = value;

                _recreatePipelines = true;
            }
        }

        public void Dispose()
        {
            DeletePipelines();
            _samplerLinear?.Dispose();
            _outputTexture?.Dispose();
            _edgeOutputTexture?.Dispose();
            _blendOutputTexture?.Dispose();
            _areaTexture?.Dispose();
            _searchTexture?.Dispose();
        }

        private unsafe void RecreateShaders(int width, int height)
        {
            _recreatePipelines = false;

            DeletePipelines();
            _pipeline = new PipelineHelperShader(_renderer, _device);

            _pipeline.Initialize();

            var edgeShader = EmbeddedResources.Read("Ryujinx.Graphics.Vulkan/Effects/Shaders/SmaaEdge.spv");
            var blendShader = EmbeddedResources.Read("Ryujinx.Graphics.Vulkan/Effects/Shaders/SmaaBlend.spv");
            var neighbourShader = EmbeddedResources.Read("Ryujinx.Graphics.Vulkan/Effects/Shaders/SmaaNeighbour.spv");

            var edgeResourceLayout = new ResourceLayoutBuilder()
                .Add(ResourceStages.Compute, ResourceType.UniformBuffer, 2)
                .Add(ResourceStages.Compute, ResourceType.TextureAndSampler, 1)
                .Add(ResourceStages.Compute, ResourceType.Image, 0).Build();

            var blendResourceLayout = new ResourceLayoutBuilder()
                .Add(ResourceStages.Compute, ResourceType.UniformBuffer, 2)
                .Add(ResourceStages.Compute, ResourceType.TextureAndSampler, 1)
                .Add(ResourceStages.Compute, ResourceType.TextureAndSampler, 3)
                .Add(ResourceStages.Compute, ResourceType.TextureAndSampler, 4)
                .Add(ResourceStages.Compute, ResourceType.Image, 0).Build();

            var neighbourResourceLayout = new ResourceLayoutBuilder()
                .Add(ResourceStages.Compute, ResourceType.UniformBuffer, 2)
                .Add(ResourceStages.Compute, ResourceType.TextureAndSampler, 1)
                .Add(ResourceStages.Compute, ResourceType.TextureAndSampler, 3)
                .Add(ResourceStages.Compute, ResourceType.Image, 0).Build();

            _samplerLinear = _renderer.CreateSampler(GAL.SamplerCreateInfo.Create(MinFilter.Linear, MagFilter.Linear));

            _specConstants = new SmaaConstants()
            {
                Width = width,
                Height = height,
                QualityLow = Quality == 0 ? 1 : 0,
                QualityMedium = Quality == 1 ? 1 : 0,
                QualityHigh = Quality == 2 ? 1 : 0,
                QualityUltra = Quality == 3 ? 1 : 0,
            };

            var specInfo = new SpecDescription(
                (0, SpecConstType.Int32),
                (1, SpecConstType.Int32),
                (2, SpecConstType.Int32),
                (3, SpecConstType.Int32),
                (4, SpecConstType.Float32),
                (5, SpecConstType.Float32));

            _edgeProgram = _renderer.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(edgeShader, ShaderStage.Compute, TargetLanguage.Spirv)
            }, edgeResourceLayout, new[] { specInfo });

            _blendProgram = _renderer.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(blendShader, ShaderStage.Compute, TargetLanguage.Spirv)
            }, blendResourceLayout, new[] { specInfo });

            _neighbourProgram = _renderer.CreateProgramWithMinimalLayout(new[]
            {
                new ShaderSource(neighbourShader, ShaderStage.Compute, TargetLanguage.Spirv)
            }, neighbourResourceLayout, new[] { specInfo });
        }

        public void DeletePipelines()
        {
            _pipeline?.Dispose();
            _edgeProgram?.Dispose();
            _blendProgram?.Dispose();
            _neighbourProgram?.Dispose();
        }

        private void Initialize()
        {
            var areaInfo = new TextureCreateInfo(AreaWidth,
                AreaHeight,
                1,
                1,
                1,
                1,
                1,
                1,
                Format.R8G8Unorm,
                DepthStencilMode.Depth,
                Target.Texture2D,
                SwizzleComponent.Red,
                SwizzleComponent.Green,
                SwizzleComponent.Blue,
                SwizzleComponent.Alpha);

            var searchInfo = new TextureCreateInfo(SearchWidth,
                SearchHeight,
                1,
                1,
                1,
                1,
                1,
                1,
                Format.R8Unorm,
                DepthStencilMode.Depth,
                Target.Texture2D,
                SwizzleComponent.Red,
                SwizzleComponent.Green,
                SwizzleComponent.Blue,
                SwizzleComponent.Alpha);

            var areaTexture = EmbeddedResources.Read("Ryujinx.Graphics.Vulkan/Effects/Textures/SmaaAreaTexture.bin");
            var searchTexture = EmbeddedResources.Read("Ryujinx.Graphics.Vulkan/Effects/Textures/SmaaSearchTexture.bin");

            _areaTexture = _renderer.CreateTexture(areaInfo, 1) as TextureView;
            _searchTexture = _renderer.CreateTexture(searchInfo, 1) as TextureView;

            _areaTexture.SetData(areaTexture);
            _searchTexture.SetData(searchTexture);
        }

        public TextureView Run(TextureView view, CommandBufferScoped cbs, int width, int height)
        {
            if (_recreatePipelines || _outputTexture == null || _outputTexture.Info.Width != view.Width || _outputTexture.Info.Height != view.Height)
            {
                RecreateShaders(view.Width, view.Height);
                _outputTexture?.Dispose();
                _edgeOutputTexture?.Dispose();
                _blendOutputTexture?.Dispose();

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

                _outputTexture = _renderer.CreateTexture(info, view.ScaleFactor) as TextureView;
                _edgeOutputTexture = _renderer.CreateTexture(info, view.ScaleFactor) as TextureView;
                _blendOutputTexture = _renderer.CreateTexture(info, view.ScaleFactor) as TextureView;
            }

            _pipeline.SetCommandBuffer(cbs);

            Clear(_edgeOutputTexture);
            Clear(_blendOutputTexture);

            _renderer.Pipeline.TextureBarrier();

            var dispatchX = BitUtils.DivRoundUp(view.Width, IPostProcessingEffect.LocalGroupSize);
            var dispatchY = BitUtils.DivRoundUp(view.Height, IPostProcessingEffect.LocalGroupSize);

            // Edge pass
            _pipeline.SetProgram(_edgeProgram);
            _pipeline.SetTextureAndSampler(ShaderStage.Compute, 1, view, _samplerLinear);
            _pipeline.Specialize(_specConstants);

            ReadOnlySpan<float> resolutionBuffer = stackalloc float[] { view.Width, view.Height };
            int rangeSize = resolutionBuffer.Length * sizeof(float);
            var bufferHandle = _renderer.BufferManager.CreateWithHandle(_renderer, rangeSize);

            _renderer.BufferManager.SetData(bufferHandle, 0, resolutionBuffer);
            var bufferRanges = new BufferRange(bufferHandle, 0, rangeSize);
            _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(2, bufferRanges) });
            _pipeline.SetImage(0, _edgeOutputTexture, GAL.Format.R8G8B8A8Unorm);
            _pipeline.DispatchCompute(dispatchX, dispatchY, 1);
            _pipeline.ComputeBarrier();

            // Blend pass
            _pipeline.SetProgram(_blendProgram);
            _pipeline.Specialize(_specConstants);
            _pipeline.SetTextureAndSampler(ShaderStage.Compute, 1, _edgeOutputTexture, _samplerLinear);
            _pipeline.SetTextureAndSampler(ShaderStage.Compute, 3, _areaTexture, _samplerLinear);
            _pipeline.SetTextureAndSampler(ShaderStage.Compute, 4, _searchTexture, _samplerLinear);
            _pipeline.SetImage(0, _blendOutputTexture, GAL.Format.R8G8B8A8Unorm);
            _pipeline.DispatchCompute(dispatchX, dispatchY, 1);
            _pipeline.ComputeBarrier();

            // Neighbour pass
            _pipeline.SetProgram(_neighbourProgram);
            _pipeline.Specialize(_specConstants);
            _pipeline.SetTextureAndSampler(ShaderStage.Compute, 3, _blendOutputTexture, _samplerLinear);
            _pipeline.SetTextureAndSampler(ShaderStage.Compute, 1, view, _samplerLinear);
            _pipeline.SetImage(0, _outputTexture, GAL.Format.R8G8B8A8Unorm);
            _pipeline.DispatchCompute(dispatchX, dispatchY, 1);
            _pipeline.ComputeBarrier();

            _pipeline.Finish();

            _renderer.BufferManager.Delete(bufferHandle);

            return _outputTexture;
        }

        private void Clear(TextureView texture)
        {
            Span<uint> colorMasks = stackalloc uint[1];

            colorMasks[0] = 0xf;

            Span<Rectangle<int>> scissors = stackalloc Rectangle<int>[1];

            scissors[0] = new Rectangle<int>(0, 0, texture.Width, texture.Height);

            _pipeline.SetRenderTarget(texture.GetImageViewForAttachment(), (uint)texture.Width, (uint)texture.Height, false, texture.VkFormat);
            _pipeline.SetRenderTargetColorMasks(colorMasks);
            _pipeline.SetScissors(scissors);
            _pipeline.ClearRenderTargetColor(0, 0, 1, new ColorF(0f, 0f, 0f, 1f));
        }
    }
}