using System;

namespace Ryujinx.Graphics.Gal
{
    public interface IGalRenderer
    {
        void QueueAction(Action ActionMthd);

        void RunActions();

        IGalBlend Blend { get; }

        IGalFrameBuffer FrameBuffer { get; }

        IGalRasterizer Rasterizer { get; }

        IGalShader Shader { get; }

        IGalTexture Texture { get; }
    }
}