using Silk.NET.Vulkan;
using VkFormat = Silk.NET.Vulkan.Format;

namespace Ryujinx.Graphics.Vulkan
{
    class PipelineHelperShader : PipelineBase
    {
        public PipelineHelperShader(VulkanRenderer gd, Device device) : base(gd, device)
        {
        }

        public void SetRenderTarget(Auto<DisposableImageView> view, uint width, uint height, bool isDepthStencil, VkFormat format)
        {
            SetRenderTarget(view, width, height, 1u, isDepthStencil, format);
        }

        public void SetRenderTarget(Auto<DisposableImageView> view, uint width, uint height, uint samples, bool isDepthStencil, VkFormat format)
        {
            CreateFramebuffer(view, width, height, samples, isDepthStencil, format);
            CreateRenderPass();
            SignalStateChange();
        }

        private void CreateFramebuffer(Auto<DisposableImageView> view, uint width, uint height, uint samples, bool isDepthStencil, VkFormat format)
        {
            FramebufferParams = new FramebufferParams(Device, view, width, height, samples, isDepthStencil, format);
            UpdatePipelineAttachmentFormats();
        }

        public void SetCommandBuffer(CommandBufferScoped cbs)
        {
            CommandBuffer = (Cbs = cbs).CommandBuffer;

            // Restore per-command buffer state.

            if (Pipeline != null)
            {
                Gd.Api.CmdBindPipeline(CommandBuffer, Pbp, Pipeline.Get(CurrentCommandBuffer).Value);
            }

            SignalCommandBufferChange();
        }

        public void Finish()
        {
            EndRenderPass();
        }

        public void Finish(VulkanRenderer gd, CommandBufferScoped cbs)
        {
            Finish();

            if (gd.PipelineInternal.IsCommandBufferActive(cbs.CommandBuffer))
            {
                gd.PipelineInternal.Restore();
            }
        }
    }
}
