using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Nv.NvGpuAS;
using Ryujinx.HLE.HOS.Services.Nv.NvMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using static Ryujinx.HLE.HOS.Services.Android.Parcel;

namespace Ryujinx.HLE.HOS.Services.Android
{
    class NvFlinger : IDisposable
    {
        private delegate long ServiceProcessParcel(ServiceCtx context, BinaryReader parcelReader);

        private Dictionary<(string, int), ServiceProcessParcel> _commands;

        private KEvent _binderEvent;

        private IGalRenderer _renderer;

        private const int BufferQueueCount = 0x40;
        private const int BufferQueueMask  = BufferQueueCount - 1;

        [Flags]
        private enum HalTransform
        {
            FlipX     = 1,
            FlipY     = 2,
            Rotate90  = 4,
            Rotate180 = FlipX | FlipY,
            Rotate270 = Rotate90 | Rotate180,
        }

        private enum BufferState
        {
            Free,
            Dequeued,
            Queued,
            Acquired
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x8)]
        private struct Fence
        {
            public int id;
            public int value;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x24)]
        private struct MultiFence
        {
            [FieldOffset(0x0)]
            public int FenceCount;

            [FieldOffset(0x4)]
            public Fence Fence0;

            [FieldOffset(0xC)]
            public Fence Fence1;

            [FieldOffset(0x14)]
            public Fence Fence2;

            [FieldOffset(0x1C)]
            public Fence Fence3;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x10)]
        private struct Rect
        {
            public int Top;
            public int Left;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct QueueBufferObject
        {
            [FieldOffset(0x0)]
            public long Timestamp;

            [FieldOffset(0x8)]
            public int IsAutoTimestamp;

            [FieldOffset(0xC)]
            public Rect Crop;

            [FieldOffset(0x1C)]
            public int ScalingMode;

            [FieldOffset(0x20)]
            public HalTransform Transform;

            [FieldOffset(0x24)]
            public int StickyTransform;

            [FieldOffset(0x28)]
            public int Unknown;

            [FieldOffset(0x2C)]
            public int SwapInterval;

            [FieldOffset(0x30)]
            public MultiFence Fence;
        }

        private struct BufferEntry
        {
            public BufferState State;

            public HalTransform Transform;

            public Rect Crop;

            public GbpBuffer Data;
        }

        private BufferEntry[] _bufferQueue;

        private AutoResetEvent _waitBufferFree;

        private bool _disposed;

        public NvFlinger(IGalRenderer renderer, KEvent binderEvent)
        {
            _commands = new Dictionary<(string, int), ServiceProcessParcel>
            {
                { ("android.gui.IGraphicBufferProducer", 0x1), GbpRequestBuffer  },
                { ("android.gui.IGraphicBufferProducer", 0x3), GbpDequeueBuffer  },
                { ("android.gui.IGraphicBufferProducer", 0x4), GbpDetachBuffer   },
                { ("android.gui.IGraphicBufferProducer", 0x7), GbpQueueBuffer    },
                { ("android.gui.IGraphicBufferProducer", 0x8), GbpCancelBuffer   },
                { ("android.gui.IGraphicBufferProducer", 0x9), GbpQuery          },
                { ("android.gui.IGraphicBufferProducer", 0xa), GbpConnect        },
                { ("android.gui.IGraphicBufferProducer", 0xb), GbpDisconnect     },
                { ("android.gui.IGraphicBufferProducer", 0xe), GbpPreallocBuffer }
            };

            _renderer    = renderer;
            _binderEvent = binderEvent;

            _bufferQueue = new BufferEntry[0x40];

            _waitBufferFree = new AutoResetEvent(false);
        }

        public long ProcessParcelRequest(ServiceCtx context, byte[] parcelData, int code)
        {
            using (MemoryStream ms = new MemoryStream(parcelData))
            {
                BinaryReader reader = new BinaryReader(ms);

                ms.Seek(4, SeekOrigin.Current);

                int strSize = reader.ReadInt32();

                string interfaceName = Encoding.Unicode.GetString(reader.ReadBytes(strSize * 2));

                long remainder = ms.Position & 0xf;

                if (remainder != 0)
                {
                    ms.Seek(0x10 - remainder, SeekOrigin.Current);
                }

                ms.Seek(0x50, SeekOrigin.Begin);

                if (_commands.TryGetValue((interfaceName, code), out ServiceProcessParcel procReq))
                {
                    Logger.PrintDebug(LogClass.ServiceVi, $"{interfaceName} {procReq.Method.Name}");

                    return procReq(context, reader);
                }
                else
                {
                    throw new NotImplementedException($"{interfaceName} {code}");
                }
            }
        }

        private long GbpRequestBuffer(ServiceCtx context, BinaryReader parcelReader)
        {
            int slot = parcelReader.ReadInt32();

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                BufferEntry entry = _bufferQueue[slot];

                int  bufferCount = 1; //?
                long bufferSize  = entry.Data.Size;

                writer.Write(bufferCount);
                writer.Write(bufferSize);

                entry.Data.Write(writer);

                writer.Write(0);

                return MakeReplyParcel(context, ms.ToArray());
            }
        }

        private long GbpDequeueBuffer(ServiceCtx context, BinaryReader parcelReader)
        {
            //TODO: Errors.
            int format        = parcelReader.ReadInt32();
            int width         = parcelReader.ReadInt32();
            int height        = parcelReader.ReadInt32();
            int getTimestamps = parcelReader.ReadInt32();
            int usage         = parcelReader.ReadInt32();

            int slot = GetFreeSlotBlocking(width, height);

            return MakeReplyParcel(context, slot, 1, 0x24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        private long GbpQueueBuffer(ServiceCtx context, BinaryReader parcelReader)
        {
            context.Device.Statistics.RecordGameFrameTime();

            //TODO: Errors.
            int slot            = parcelReader.ReadInt32();

            long Position = parcelReader.BaseStream.Position;

            QueueBufferObject queueBufferObject = ReadFlattenedObject<QueueBufferObject>(parcelReader);

            parcelReader.BaseStream.Position = Position;

            _bufferQueue[slot].Transform = queueBufferObject.Transform;
            _bufferQueue[slot].Crop      = queueBufferObject.Crop;

            _bufferQueue[slot].State = BufferState.Queued;

            SendFrameBuffer(context, slot);

            if (context.Device.EnableDeviceVsync)
            {
                context.Device.VsyncEvent.WaitOne();
            }

            return MakeReplyParcel(context, 1280, 720, 0, 0, 0);
        }

        private long GbpDetachBuffer(ServiceCtx context, BinaryReader parcelReader)
        {
            return MakeReplyParcel(context, 0);
        }

        private long GbpCancelBuffer(ServiceCtx context, BinaryReader parcelReader)
        {
            //TODO: Errors.
            int slot = parcelReader.ReadInt32();

            MultiFence fence = ReadFlattenedObject<MultiFence>(parcelReader);

            _bufferQueue[slot].State = BufferState.Free;

            _waitBufferFree.Set();

            return MakeReplyParcel(context, 0);
        }

        private long GbpQuery(ServiceCtx context, BinaryReader parcelReader)
        {
            return MakeReplyParcel(context, 0, 0);
        }

        private long GbpConnect(ServiceCtx context, BinaryReader parcelReader)
        {
            return MakeReplyParcel(context, 1280, 720, 0, 0, 0);
        }

        private long GbpDisconnect(ServiceCtx context, BinaryReader parcelReader)
        {
            return MakeReplyParcel(context, 0);
        }

        private long GbpPreallocBuffer(ServiceCtx context, BinaryReader parcelReader)
        {
            int slot = parcelReader.ReadInt32();

            bool hasInput = parcelReader.ReadInt32() == 1;

            if (hasInput)
            {
                byte[] graphicBuffer = ReadFlattenedObject(parcelReader);

                _bufferQueue[slot].State = BufferState.Free;

                using (BinaryReader graphicBufferReader = new BinaryReader(new MemoryStream(graphicBuffer)))
                {
                    _bufferQueue[slot].Data = new GbpBuffer(graphicBufferReader);
                }

            }

            return MakeReplyParcel(context, 0);
        }

        private byte[] ReadFlattenedObject(BinaryReader reader)
        {
            long flattenedObjectSize = reader.ReadInt64();

            return reader.ReadBytes((int)flattenedObjectSize);
        }

        private unsafe T ReadFlattenedObject<T>(BinaryReader reader) where T: struct
        {
            byte[] data = ReadFlattenedObject(reader);

            fixed (byte* ptr = data)
            {
                return Marshal.PtrToStructure<T>((IntPtr)ptr);
            }
        }

        private long MakeReplyParcel(ServiceCtx context, params int[] ints)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                foreach (int Int in ints)
                {
                    writer.Write(Int);
                }

                return MakeReplyParcel(context, ms.ToArray());
            }
        }

        private long MakeReplyParcel(ServiceCtx context, byte[] data)
        {
            (long replyPos, long replySize) = context.Request.GetBufferType0x22();

            byte[] reply = MakeParcel(data, new byte[0]);

            context.Memory.WriteBytes(replyPos, reply);

            return 0;
        }

        private GalImageFormat ConvertColorFormat(ColorFormat colorFormat)
        {
            switch (colorFormat)
            {
                case ColorFormat.A8B8G8R8:
                    return GalImageFormat.Rgba8  | GalImageFormat.Unorm;
                case ColorFormat.X8B8G8R8:
                    return GalImageFormat.Rgbx8  | GalImageFormat.Unorm;
                case ColorFormat.R5G6B5:
                    return GalImageFormat.Bgr565 | GalImageFormat.Unorm;
                case ColorFormat.A8R8G8B8:
                    return GalImageFormat.Bgra8  | GalImageFormat.Unorm;
                case ColorFormat.A4B4G4R4:
                    return GalImageFormat.Rgba4  | GalImageFormat.Unorm;
                default:
                    throw new NotImplementedException($"Color Format \"{colorFormat}\" not implemented!");
            }
        }

        // TODO: support multi surface
        private void SendFrameBuffer(ServiceCtx context, int slot)
        {
            int fbWidth  = _bufferQueue[slot].Data.Header.Width;
            int fbHeight = _bufferQueue[slot].Data.Header.Height;

            int nvMapHandle = _bufferQueue[slot].Data.Buffer.Surfaces[0].NvMapHandle;

            if (nvMapHandle == 0)
            {
                nvMapHandle = _bufferQueue[slot].Data.Buffer.NvMapId;
            }

            int bufferOffset = _bufferQueue[slot].Data.Buffer.Surfaces[0].Offset;

            NvMapHandle map = NvMapIoctl.GetNvMap(context, nvMapHandle);

            long fbAddr = map.Address + bufferOffset;

            _bufferQueue[slot].State = BufferState.Acquired;

            Rect crop = _bufferQueue[slot].Crop;

            bool flipX = _bufferQueue[slot].Transform.HasFlag(HalTransform.FlipX);
            bool flipY = _bufferQueue[slot].Transform.HasFlag(HalTransform.FlipY);

            GalImageFormat imageFormat = ConvertColorFormat(_bufferQueue[slot].Data.Buffer.Surfaces[0].ColorFormat);

            int BlockHeight = 1 << _bufferQueue[slot].Data.Buffer.Surfaces[0].BlockHeightLog2;

            //Note: Rotation is being ignored.

            int top    = crop.Top;
            int left   = crop.Left;
            int right  = crop.Right;
            int bottom = crop.Bottom;

            NvGpuVmm vmm = NvGpuASIoctl.GetASCtx(context).Vmm;

            _renderer.QueueAction(() =>
            {
                if (!_renderer.Texture.TryGetImage(fbAddr, out GalImage image))
                {
                    image = new GalImage(
                        fbWidth,
                        fbHeight, 1, 1, 1, BlockHeight, 1,
                        GalMemoryLayout.BlockLinear,
                        imageFormat,
                        GalTextureTarget.TwoD);
                }

                context.Device.Gpu.ResourceManager.ClearPbCache();
                context.Device.Gpu.ResourceManager.SendTexture(vmm, fbAddr, image);

                _renderer.RenderTarget.SetTransform(flipX, flipY, top, left, right, bottom);
                _renderer.RenderTarget.Present(fbAddr);

                ReleaseBuffer(slot);
            });
        }

        private void ReleaseBuffer(int slot)
        {
            _bufferQueue[slot].State = BufferState.Free;

            _binderEvent.ReadableEvent.Signal();

            _waitBufferFree.Set();
        }

        private int GetFreeSlotBlocking(int width, int height)
        {
            int slot;

            do
            {
                if ((slot = GetFreeSlot(width, height)) != -1)
                {
                    break;
                }

                if (_disposed)
                {
                    break;
                }

                _waitBufferFree.WaitOne();
            }
            while (!_disposed);

            return slot;
        }

        private int GetFreeSlot(int width, int height)
        {
            lock (_bufferQueue)
            {
                for (int slot = 0; slot < _bufferQueue.Length; slot++)
                {
                    if (_bufferQueue[slot].State != BufferState.Free)
                    {
                        continue;
                    }

                    GbpBuffer data = _bufferQueue[slot].Data;

                    if (data.Header.Width  == width &&
                        data.Header.Height == height)
                    {
                        _bufferQueue[slot].State = BufferState.Dequeued;

                        return slot;
                    }
                }
            }

            return -1;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;

                _waitBufferFree.Set();
                _waitBufferFree.Dispose();
            }
        }
    }
}