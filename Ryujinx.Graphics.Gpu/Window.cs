using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Texture;
using Ryujinx.Graphics.Gpu.Image;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.Graphics.Gpu
{
    public class Window
    {
        private GpuContext _context;

        private struct PresentationTexture
        {
            public TextureInfo    Info     { get; }
            public ImageCrop      Crop     { get; }
            public Action<object> Callback { get; }
            public object         UserObj  { get; }

            public PresentationTexture(
                TextureInfo    info,
                ImageCrop      crop,
                Action<object> callback,
                object         userObj)
            {
                Info     = info;
                Crop     = crop;
                Callback = callback;
                UserObj  = userObj;
            }
        }

        private ConcurrentQueue<PresentationTexture> _frameQueue;

        public Window(GpuContext context)
        {
            _context = context;

            _frameQueue = new ConcurrentQueue<PresentationTexture>();
        }

        public void EnqueueFrameThreadSafe(
            ulong          address,
            int            width,
            int            height,
            int            stride,
            bool           isLinear,
            int            gobBlocksInY,
            Format         format,
            int            bytesPerPixel,
            ImageCrop      crop,
            Action<object> callback,
            object         userObj)
        {
            FormatInfo formatInfo = new FormatInfo(format, 1, 1, bytesPerPixel);

            TextureInfo info = new TextureInfo(
                address,
                width,
                height,
                1,
                1,
                1,
                1,
                stride,
                isLinear,
                gobBlocksInY,
                1,
                1,
                Target.Texture2D,
                formatInfo);

            _frameQueue.Enqueue(new PresentationTexture(info, crop, callback, userObj));
        }

        public void Present(Action swapBuffersCallback)
        {
            _context.AdvanceSequence();

            if (_frameQueue.TryDequeue(out PresentationTexture pt))
            {
                Image.Texture texture = _context.Methods.TextureManager.FindOrCreateTexture(pt.Info);

                texture.SynchronizeMemory();

                _context.Renderer.Window.Present(texture.HostTexture, pt.Crop);

                swapBuffersCallback();

                pt.Callback(pt.UserObj);
            }
        }
    }
}