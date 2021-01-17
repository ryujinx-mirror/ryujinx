using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Texture;
using Ryujinx.Memory.Range;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.Graphics.Gpu
{
    using Texture = Image.Texture;

    /// <summary>
    /// GPU image presentation window.
    /// </summary>
    public class Window
    {
        private readonly GpuContext _context;

        /// <summary>
        /// Texture presented on the window.
        /// </summary>
        private struct PresentationTexture
        {
            /// <summary>
            /// Texture information.
            /// </summary>
            public TextureInfo Info { get; }

            /// <summary>
            /// Physical memory locations where the texture data is located.
            /// </summary>
            public MultiRange Range { get; }

            /// <summary>
            /// Texture crop region.
            /// </summary>
            public ImageCrop Crop { get; }

            /// <summary>
            /// Texture acquire callback.
            /// </summary>
            public Action<GpuContext, object> AcquireCallback { get; }

            /// <summary>
            /// Texture release callback.
            /// </summary>
            public Action<object> ReleaseCallback { get; }

            /// <summary>
            /// User defined object, passed to the various callbacks.
            /// </summary>
            public object UserObj { get; }

            /// <summary>
            /// Creates a new instance of the presentation texture.
            /// </summary>
            /// <param name="info">Information of the texture to be presented</param>
            /// <param name="range">Physical memory locations where the texture data is located</param>
            /// <param name="crop">Texture crop region</param>
            /// <param name="acquireCallback">Texture acquire callback</param>
            /// <param name="releaseCallback">Texture release callback</param>
            /// <param name="userObj">User defined object passed to the release callback, can be used to identify the texture</param>
            public PresentationTexture(
                TextureInfo                info,
                MultiRange                 range,
                ImageCrop                  crop,
                Action<GpuContext, object> acquireCallback,
                Action<object>             releaseCallback,
                object                     userObj)
            {
                Info            = info;
                Range           = range;
                Crop            = crop;
                AcquireCallback = acquireCallback;
                ReleaseCallback = releaseCallback;
                UserObj         = userObj;
            }
        }

        private readonly ConcurrentQueue<PresentationTexture> _frameQueue;

        private int _framesAvailable;

        /// <summary>
        /// Creates a new instance of the GPU presentation window.
        /// </summary>
        /// <param name="context">GPU emulation context</param>
        public Window(GpuContext context)
        {
            _context = context;

            _frameQueue = new ConcurrentQueue<PresentationTexture>();
        }

        /// <summary>
        /// Enqueues a frame for presentation.
        /// This method is thread safe and can be called from any thread.
        /// When the texture is presented and not needed anymore, the release callback is called.
        /// It's an error to modify the texture after calling this method, before the release callback is called.
        /// </summary>
        /// <param name="address">CPU virtual address of the texture data</param>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <param name="stride">Texture stride for linear texture, should be zero otherwise</param>
        /// <param name="isLinear">Indicates if the texture is linear, normally false</param>
        /// <param name="gobBlocksInY">GOB blocks in the Y direction, for block linear textures</param>
        /// <param name="format">Texture format</param>
        /// <param name="bytesPerPixel">Texture format bytes per pixel (must match the format)</param>
        /// <param name="crop">Texture crop region</param>
        /// <param name="acquireCallback">Texture acquire callback</param>
        /// <param name="releaseCallback">Texture release callback</param>
        /// <param name="userObj">User defined object passed to the release callback</param>
        public void EnqueueFrameThreadSafe(
            ulong                      address,
            int                        width,
            int                        height,
            int                        stride,
            bool                       isLinear,
            int                        gobBlocksInY,
            Format                     format,
            int                        bytesPerPixel,
            ImageCrop                  crop,
            Action<GpuContext, object> acquireCallback,
            Action<object>             releaseCallback,
            object                     userObj)
        {
            FormatInfo formatInfo = new FormatInfo(format, 1, 1, bytesPerPixel, 4);

            TextureInfo info = new TextureInfo(
                0UL,
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

            int size = SizeCalculator.GetBlockLinearTextureSize(
                width,
                height,
                1,
                1,
                1,
                1,
                1,
                bytesPerPixel,
                gobBlocksInY,
                1,
                1).TotalSize;

            MultiRange range = new MultiRange(address, (ulong)size);

            _frameQueue.Enqueue(new PresentationTexture(info, range, crop, acquireCallback, releaseCallback, userObj));
        }

        /// <summary>
        /// Presents a texture on the queue.
        /// If the queue is empty, then no texture is presented.
        /// </summary>
        /// <param name="swapBuffersCallback">Callback method to call when a new texture should be presented on the screen</param>
        public void Present(Action swapBuffersCallback)
        {
            _context.AdvanceSequence();

            if (_frameQueue.TryDequeue(out PresentationTexture pt))
            {
                pt.AcquireCallback(_context, pt.UserObj);

                Texture texture = _context.Methods.TextureManager.FindOrCreateTexture(TextureSearchFlags.WithUpscale, pt.Info, 0, null, pt.Range);

                texture.SynchronizeMemory();

                _context.Renderer.Window.Present(texture.HostTexture, pt.Crop);

                swapBuffersCallback();

                pt.ReleaseCallback(pt.UserObj);
            }
        }

        /// <summary>
        /// Indicate that a frame on the queue is ready to be acquired.
        /// </summary>
        public void SignalFrameReady()
        {
            Interlocked.Increment(ref _framesAvailable);
        }

        /// <summary>
        /// Determine if any frames are available, and decrement the available count if there are.
        /// </summary>
        /// <returns>True if a frame is available, false otherwise</returns>
        public bool ConsumeFrameAvailable()
        {
            if (Interlocked.CompareExchange(ref _framesAvailable, 0, 0) != 0)
            {
                Interlocked.Decrement(ref _framesAvailable);

                return true;
            }

            return false;
        }
    }
}