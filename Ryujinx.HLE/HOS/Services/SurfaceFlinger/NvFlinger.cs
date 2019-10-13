using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using static Ryujinx.HLE.HOS.Services.SurfaceFlinger.Parcel;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    class NvFlinger : IDisposable
    {
        private delegate ResultCode ServiceProcessParcel(ServiceCtx context, BinaryReader parcelReader);

        private Dictionary<(string, int), ServiceProcessParcel> _commands;

        private KEvent _binderEvent;

        private IRenderer _renderer;

        private const int BufferQueueCount = 0x40;
        private const int BufferQueueMask  = BufferQueueCount - 1;

        private BufferEntry[] _bufferQueue;

        private AutoResetEvent _waitBufferFree;

        private bool _disposed;

        public NvFlinger(IRenderer renderer, KEvent binderEvent)
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

        public ResultCode ProcessParcelRequest(ServiceCtx context, byte[] parcelData, int code)
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

        private ResultCode GbpRequestBuffer(ServiceCtx context, BinaryReader parcelReader)
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

        private ResultCode GbpDequeueBuffer(ServiceCtx context, BinaryReader parcelReader)
        {
            // TODO: Errors.
            int format        = parcelReader.ReadInt32();
            int width         = parcelReader.ReadInt32();
            int height        = parcelReader.ReadInt32();
            int getTimestamps = parcelReader.ReadInt32();
            int usage         = parcelReader.ReadInt32();

            int slot = GetFreeSlotBlocking(width, height);

            return MakeReplyParcel(context, slot, 1, 0x24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        private ResultCode GbpQueueBuffer(ServiceCtx context, BinaryReader parcelReader)
        {
            context.Device.Statistics.RecordGameFrameTime();

            // TODO: Errors.
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

        private ResultCode GbpDetachBuffer(ServiceCtx context, BinaryReader parcelReader)
        {
            return MakeReplyParcel(context, 0);
        }

        private ResultCode GbpCancelBuffer(ServiceCtx context, BinaryReader parcelReader)
        {
            // TODO: Errors.
            int slot = parcelReader.ReadInt32();

            MultiFence fence = ReadFlattenedObject<MultiFence>(parcelReader);

            _bufferQueue[slot].State = BufferState.Free;

            _waitBufferFree.Set();

            return MakeReplyParcel(context, 0);
        }

        private ResultCode GbpQuery(ServiceCtx context, BinaryReader parcelReader)
        {
            return MakeReplyParcel(context, 0, 0);
        }

        private ResultCode GbpConnect(ServiceCtx context, BinaryReader parcelReader)
        {
            return MakeReplyParcel(context, 1280, 720, 0, 0, 0);
        }

        private ResultCode GbpDisconnect(ServiceCtx context, BinaryReader parcelReader)
        {
            return MakeReplyParcel(context, 0);
        }

        private ResultCode GbpPreallocBuffer(ServiceCtx context, BinaryReader parcelReader)
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

        private ResultCode MakeReplyParcel(ServiceCtx context, params int[] ints)
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

        private ResultCode MakeReplyParcel(ServiceCtx context, byte[] data)
        {
            (long replyPos, long replySize) = context.Request.GetBufferType0x22();

            byte[] reply = MakeParcel(data, new byte[0]);

            context.Memory.WriteBytes(replyPos, reply);

            return ResultCode.Success;
        }

        private Format ConvertColorFormat(ColorFormat colorFormat)
        {
            switch (colorFormat)
            {
                case ColorFormat.A8B8G8R8:
                    return Format.R8G8B8A8Unorm;
                case ColorFormat.X8B8G8R8:
                    return Format.R8G8B8A8Unorm;
                case ColorFormat.R5G6B5:
                    return Format.R5G6B5Unorm;
                case ColorFormat.A8R8G8B8:
                    return Format.B8G8R8A8Unorm;
                case ColorFormat.A4B4G4R4:
                    return Format.R4G4B4A4Unorm;
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

            NvMapHandle map = NvMapDeviceFile.GetMapFromHandle(context.Process, nvMapHandle);

            ulong fbAddr = (ulong)(map.Address + bufferOffset);

            _bufferQueue[slot].State = BufferState.Acquired;

            Rect crop = _bufferQueue[slot].Crop;

            bool flipX = _bufferQueue[slot].Transform.HasFlag(HalTransform.FlipX);
            bool flipY = _bufferQueue[slot].Transform.HasFlag(HalTransform.FlipY);

            Format format = ConvertColorFormat(_bufferQueue[slot].Data.Buffer.Surfaces[0].ColorFormat);

            int bytesPerPixel =
                format == Format.R5G6B5Unorm ||
                format == Format.R4G4B4A4Unorm ? 2 : 4;

            int gobBlocksInY = 1 << _bufferQueue[slot].Data.Buffer.Surfaces[0].BlockHeightLog2;

            // Note: Rotation is being ignored.

            ITexture texture = context.Device.Gpu.GetTexture(
                fbAddr,
                fbWidth,
                fbHeight,
                0,
                false,
                gobBlocksInY,
                format,
                bytesPerPixel);

            _renderer.Window.RegisterTextureReleaseCallback(ReleaseBuffer);

            ImageCrop imageCrop = new ImageCrop(
                crop.Left,
                crop.Right,
                crop.Top,
                crop.Bottom,
                flipX,
                flipY);

            _renderer.Window.QueueTexture(texture, imageCrop, slot);
        }

        private void ReleaseBuffer(object context)
        {
            ReleaseBuffer((int)context);
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