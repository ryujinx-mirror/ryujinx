using System;
using System.Collections.Concurrent;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    public class OglRenderer : IGalRenderer
    {
        public IGalConstBuffer Buffer { get; private set; }

        public IGalRenderTarget RenderTarget { get; private set; }

        public IGalRasterizer Rasterizer { get; private set; }

        public IGalShader Shader { get; private set; }

        public IGalPipeline Pipeline { get; private set; }

        public IGalTexture Texture { get; private set; }

        private ConcurrentQueue<Action> _actionsQueue;

        public OglRenderer()
        {
            Buffer = new OglConstBuffer();

            Texture = new OglTexture();

            RenderTarget = new OglRenderTarget(Texture as OglTexture);

            Rasterizer = new OglRasterizer();

            Shader = new OglShader(Buffer as OglConstBuffer);

            Pipeline = new OglPipeline(
                Buffer       as OglConstBuffer,
                RenderTarget as OglRenderTarget,
                Rasterizer   as OglRasterizer,
                Shader       as OglShader);

            _actionsQueue = new ConcurrentQueue<Action>();
        }

        public void QueueAction(Action actionMthd)
        {
            _actionsQueue.Enqueue(actionMthd);
        }

        public void RunActions()
        {
            int count = _actionsQueue.Count;

            while (count-- > 0 && _actionsQueue.TryDequeue(out Action renderAction))
            {
                renderAction();
            }
        }
    }
}