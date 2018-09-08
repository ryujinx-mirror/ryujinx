using System;

namespace Ryujinx.Graphics.Gal
{
    public interface IGalRenderer
    {
        void QueueAction(Action ActionMthd);

        void RunActions();

        IGalConstBuffer Buffer { get; }

        IGalRenderTarget RenderTarget { get; }

        IGalRasterizer Rasterizer { get; }

        IGalShader Shader { get; }

        IGalPipeline Pipeline { get; }

        IGalTexture Texture { get; }
    }
}