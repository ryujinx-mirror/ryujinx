using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Configuration;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    class SurfaceFlinger : IConsumerListener, IDisposable
    {
        private const int TargetFps = 60;

        private Switch _device;

        private Dictionary<long, Layer> _layers;

        private bool _isRunning;

        private Thread _composerThread;

        private Stopwatch _chrono;

        private ManualResetEvent _event = new ManualResetEvent(false);
        private AutoResetEvent _nextFrameEvent = new AutoResetEvent(true);
        private long _ticks;
        private long _ticksPerFrame;
        private long _spinTicks;
        private long _1msTicks;

        private int _swapInterval;

        private readonly object Lock = new object();

        public long LastId { get; private set; }

        private class Layer
        {
            public int                    ProducerBinderId;
            public IGraphicBufferProducer Producer;
            public BufferItemConsumer     Consumer;
            public BufferQueueCore        Core;
            public long                   Owner;
        }

        private class TextureCallbackInformation
        {
            public Layer        Layer;
            public BufferItem   Item;
        }

        public SurfaceFlinger(Switch device)
        {
            _device = device;
            _layers = new Dictionary<long, Layer>();
            LastId  = 0;

            _composerThread = new Thread(HandleComposition)
            {
                Name = "SurfaceFlinger.Composer"
            };

            _chrono = new Stopwatch();
            _chrono.Start();

            _ticks = 0;
            _spinTicks = Stopwatch.Frequency / 500;
            _1msTicks = Stopwatch.Frequency / 1000;

            UpdateSwapInterval(1);

            _composerThread.Start();
        }

        private void UpdateSwapInterval(int swapInterval)
        {
            _swapInterval = swapInterval;

            // If the swap interval is 0, Game VSync is disabled.
            if (_swapInterval == 0)
            {
                _nextFrameEvent.Set();
                _ticksPerFrame = 1;
            }
            else
            {
                _ticksPerFrame = Stopwatch.Frequency / (TargetFps / _swapInterval);
            }
        }

        public IGraphicBufferProducer OpenLayer(long pid, long layerId)
        {
            bool needCreate;

            lock (Lock)
            {
                needCreate = GetLayerByIdLocked(layerId) == null;
            }

            if (needCreate)
            {
                CreateLayerFromId(pid, layerId);
            }

            return GetProducerByLayerId(layerId);
        }

        public IGraphicBufferProducer CreateLayer(long pid, out long layerId)
        {
            layerId = 1;

            lock (Lock)
            {
                foreach (KeyValuePair<long, Layer> pair in _layers)
                {
                    if (pair.Key >= layerId)
                    {
                        layerId = pair.Key + 1;
                    }
                }
            }

            CreateLayerFromId(pid, layerId);

            return GetProducerByLayerId(layerId);
        }

        private void CreateLayerFromId(long pid, long layerId)
        {
            lock (Lock)
            {
                Logger.Info?.Print(LogClass.SurfaceFlinger, $"Creating layer {layerId}");

                BufferQueueCore core = BufferQueue.CreateBufferQueue(_device, pid, out BufferQueueProducer producer, out BufferQueueConsumer consumer);

                core.BufferQueued += () =>
                {
                    _nextFrameEvent.Set();
                };

                _layers.Add(layerId, new Layer
                {
                    ProducerBinderId = HOSBinderDriverServer.RegisterBinderObject(producer),
                    Producer         = producer,
                    Consumer         = new BufferItemConsumer(_device, consumer, 0, -1, false, this),
                    Core             = core,
                    Owner            = pid
                });

                LastId = layerId;
            }
        }

        public bool CloseLayer(long layerId)
        {
            lock (Lock)
            {
                Layer layer = GetLayerByIdLocked(layerId);

                if (layer != null)
                {
                    HOSBinderDriverServer.UnregisterBinderObject(layer.ProducerBinderId);
                }

                return _layers.Remove(layerId);
            }
        }

        private Layer GetLayerByIdLocked(long layerId)
        {
            foreach (KeyValuePair<long, Layer> pair in _layers)
            {
                if (pair.Key == layerId)
                {
                    return pair.Value;
                }
            }

            return null;
        }

        public IGraphicBufferProducer GetProducerByLayerId(long layerId)
        {
            lock (Lock)
            {
                Layer layer = GetLayerByIdLocked(layerId);

                if (layer != null)
                {
                    return layer.Producer;
                }
            }

            return null;
        }

        private void HandleComposition()
        {
            _isRunning = true;

            long lastTicks = _chrono.ElapsedTicks;

            while (_isRunning)
            {
                long ticks = _chrono.ElapsedTicks;

                if (_swapInterval == 0)
                {
                    Compose();

                    _device.System?.SignalVsync();

                    _nextFrameEvent.WaitOne(17);
                    lastTicks = ticks;
                }
                else
                {
                    _ticks += ticks - lastTicks;
                    lastTicks = ticks;

                    if (_ticks >= _ticksPerFrame)
                    {
                        Compose();

                        _device.System?.SignalVsync();

                        // Apply a maximum bound of 3 frames to the tick remainder, in case some event causes Ryujinx to pause for a long time or messes with the timer.
                        _ticks = Math.Min(_ticks - _ticksPerFrame, _ticksPerFrame * 3);
                    }

                    // Sleep if possible. If the time til the next frame is too low, spin wait instead.
                    long diff = _ticksPerFrame - (_ticks + _chrono.ElapsedTicks - ticks);
                    if (diff > 0)
                    {
                        if (diff < _spinTicks)
                        {
                            do
                            {
                                // SpinWait is a little more HT/SMT friendly than aggressively updating/checking ticks.
                                // The value of 5 still gives us quite a bit of precision (~0.0003ms variance at worst) while waiting a reasonable amount of time.
                                Thread.SpinWait(5);

                                ticks = _chrono.ElapsedTicks;
                                _ticks += ticks - lastTicks;
                                lastTicks = ticks;
                            } while (_ticks < _ticksPerFrame);
                        }
                        else
                        {
                            _event.WaitOne((int)(diff / _1msTicks));
                        }
                    }
                }
            }
        }

        public void Compose()
        {
            lock (Lock)
            {
                // TODO: support multilayers (& multidisplay ?)
                if (_layers.Count == 0)
                {
                    return;
                }

                Layer layer = GetLayerByIdLocked(LastId);

                Status acquireStatus = layer.Consumer.AcquireBuffer(out BufferItem item, 0);

                if (acquireStatus == Status.Success)
                {
                    // If device vsync is disabled, reflect the change.
                    if (!_device.EnableDeviceVsync)
                    {
                        if (_swapInterval != 0)
                        {
                            UpdateSwapInterval(0);
                        }
                    }
                    else if (item.SwapInterval != _swapInterval)
                    {
                        UpdateSwapInterval(item.SwapInterval);
                    }

                    PostFrameBuffer(layer, item);
                }
                else if (acquireStatus != Status.NoBufferAvailaible && acquireStatus != Status.InvalidOperation)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private void PostFrameBuffer(Layer layer, BufferItem item)
        {
            int frameBufferWidth  = item.GraphicBuffer.Object.Width;
            int frameBufferHeight = item.GraphicBuffer.Object.Height;

            int nvMapHandle = item.GraphicBuffer.Object.Buffer.Surfaces[0].NvMapHandle;

            if (nvMapHandle == 0)
            {
                nvMapHandle = item.GraphicBuffer.Object.Buffer.NvMapId;
            }

            ulong bufferOffset = (ulong)item.GraphicBuffer.Object.Buffer.Surfaces[0].Offset;

            NvMapHandle map = NvMapDeviceFile.GetMapFromHandle(layer.Owner, nvMapHandle);

            ulong frameBufferAddress = map.Address + bufferOffset;

            Format format = ConvertColorFormat(item.GraphicBuffer.Object.Buffer.Surfaces[0].ColorFormat);

            int bytesPerPixel =
                format == Format.B5G6R5Unorm ||
                format == Format.R4G4B4A4Unorm ? 2 : 4;

            int gobBlocksInY = 1 << item.GraphicBuffer.Object.Buffer.Surfaces[0].BlockHeightLog2;

            // Note: Rotation is being ignored.
            Rect cropRect = item.Crop;

            bool flipX = item.Transform.HasFlag(NativeWindowTransform.FlipX);
            bool flipY = item.Transform.HasFlag(NativeWindowTransform.FlipY);

            AspectRatio aspectRatio = ConfigurationState.Instance.Graphics.AspectRatio.Value;
            bool        isStretched = aspectRatio == AspectRatio.Stretched;

            ImageCrop crop = new ImageCrop(
                cropRect.Left,
                cropRect.Right,
                cropRect.Top,
                cropRect.Bottom,
                flipX,
                flipY,
                isStretched,
                aspectRatio.ToFloatX(),
                aspectRatio.ToFloatY());

            TextureCallbackInformation textureCallbackInformation = new TextureCallbackInformation
            {
                Layer = layer,
                Item  = item
            };

            if (item.Fence.FenceCount == 0)
            {
                _device.Gpu.Window.SignalFrameReady();
                _device.Gpu.GPFifo.Interrupt();
            }
            else
            {
                item.Fence.RegisterCallback(_device.Gpu, () =>
                {
                    _device.Gpu.Window.SignalFrameReady();
                    _device.Gpu.GPFifo.Interrupt();
                });
            }

            _device.Gpu.Window.EnqueueFrameThreadSafe(
                frameBufferAddress,
                frameBufferWidth,
                frameBufferHeight,
                0,
                false,
                gobBlocksInY,
                format,
                bytesPerPixel,
                crop,
                AcquireBuffer,
                ReleaseBuffer,
                textureCallbackInformation);
        }

        private void ReleaseBuffer(object obj)
        {
            ReleaseBuffer((TextureCallbackInformation)obj);
        }

        private void ReleaseBuffer(TextureCallbackInformation information)
        {
            AndroidFence fence = AndroidFence.NoFence;

            information.Layer.Consumer.ReleaseBuffer(information.Item, ref fence);
        }

        private void AcquireBuffer(GpuContext ignored, object obj)
        {
            AcquireBuffer((TextureCallbackInformation)obj);
        }

        private void AcquireBuffer(TextureCallbackInformation information)
        {
            information.Item.Fence.WaitForever(_device.Gpu);
        }

        public static Format ConvertColorFormat(ColorFormat colorFormat)
        {
            return colorFormat switch
            {
                ColorFormat.A8B8G8R8 => Format.R8G8B8A8Unorm,
                ColorFormat.X8B8G8R8 => Format.R8G8B8A8Unorm,
                ColorFormat.R5G6B5 => Format.B5G6R5Unorm,
                ColorFormat.A8R8G8B8 => Format.B8G8R8A8Unorm,
                ColorFormat.A4B4G4R4 => Format.R4G4B4A4Unorm,
                _ => throw new NotImplementedException($"Color Format \"{colorFormat}\" not implemented!"),
            };
        }

        public void Dispose()
        {
            _isRunning = false;

            foreach (Layer layer in _layers.Values)
            {
                layer.Core.PrepareForExit();
            }
        }

        public void OnFrameAvailable(ref BufferItem item)
        {
            _device.Statistics.RecordGameFrameTime();
        }

        public void OnFrameReplaced(ref BufferItem item)
        {
            _device.Statistics.RecordGameFrameTime();
        }

        public void OnBuffersReleased() {}
    }
}
