using ChocolArm64.Memory;
using Ryujinx.OsHle.Handles;
using Ryujinx.OsHle.Ipc;
using Ryujinx.OsHle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using static Ryujinx.OsHle.Objects.Android.Parcel;

namespace Ryujinx.OsHle.Objects
{
    class ViIHOSBinderDriver
    {
        private delegate long ServiceProcessRequest(ServiceCtx Context, byte[] ParcelData);

        private static Dictionary<(string, int), ServiceProcessRequest> InterfaceMthd =
                   new Dictionary<(string, int), ServiceProcessRequest>()
        {
            { ("android.gui.IGraphicBufferProducer", 0x1), GraphicBufferProducerRequestBuffer },
            { ("android.gui.IGraphicBufferProducer", 0x3), GraphicBufferProducerDequeueBuffer },
            { ("android.gui.IGraphicBufferProducer", 0x7), GraphicBufferProducerQueueBuffer   },
            //{ ("android.gui.IGraphicBufferProducer", 0x8), GraphicBufferProducerCancelBuffer  },
            { ("android.gui.IGraphicBufferProducer", 0x9), GraphicBufferProducerQuery         },
            { ("android.gui.IGraphicBufferProducer", 0xa), GraphicBufferProducerConnect       },
            { ("android.gui.IGraphicBufferProducer", 0xe), GraphicBufferPreallocateBuffer     },
        };

        private class BufferObj
        {

        }

        public IdPoolWithObj BufferSlots { get; private set; }

        public byte[] Gbfr;

        public ViIHOSBinderDriver()
        {
            BufferSlots = new IdPoolWithObj();
        }

        public static long TransactParcel(ServiceCtx Context)
        {
            int Id   = Context.RequestData.ReadInt32();
            int Code = Context.RequestData.ReadInt32();

            long DataPos  = Context.Request.SendBuff[0].Position;
            long DataSize = Context.Request.SendBuff[0].Size;

            byte[] Data = AMemoryHelper.ReadBytes(Context.Memory, DataPos, (int)DataSize);

            Data = GetParcelData(Data);

            using (MemoryStream MS = new MemoryStream(Data))
            {
                BinaryReader Reader = new BinaryReader(MS);

                MS.Seek(4, SeekOrigin.Current);

                int StrSize = Reader.ReadInt32();

                string InterfaceName = Encoding.Unicode.GetString(Data, 8, StrSize * 2);

                if (InterfaceMthd.TryGetValue((InterfaceName, Code), out ServiceProcessRequest ProcReq))
                {
                    return ProcReq(Context, Data);
                }
                else
                {
                    throw new NotImplementedException($"{InterfaceName} {Code}");
                }
            }
        }

        private static long GraphicBufferProducerRequestBuffer(ServiceCtx Context, byte[] ParcelData)
        {
            ViIHOSBinderDriver BinderDriver = Context.GetObject<ViIHOSBinderDriver>();

            int GbfrSize = BinderDriver.Gbfr?.Length ?? 0;

            byte[] Data = new byte[GbfrSize + 4];

            if (BinderDriver.Gbfr != null)
            {
                Buffer.BlockCopy(BinderDriver.Gbfr, 0, Data, 0, GbfrSize);
            }

            return MakeReplyParcel(Context, Data);
        }

        private static long GraphicBufferProducerDequeueBuffer(ServiceCtx Context, byte[] ParcelData)
        {
            ViIHOSBinderDriver BinderDriver = Context.GetObject<ViIHOSBinderDriver>();

            //Note: It seems that the maximum number of slots is 64, because if we return
            //a Slot number > 63, it seems to cause a buffer overrun and it reads garbage.
            //Note 2: The size of each object associated with the slot is 0x30.
            int Slot = BinderDriver.BufferSlots.GenerateId(new BufferObj());

            return MakeReplyParcel(Context, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        private static long GraphicBufferProducerQueueBuffer(ServiceCtx Context, byte[] ParcelData)
        {
            return MakeReplyParcel(Context, 1280, 720, 0, 0, 0);
        }

        private static long GraphicBufferProducerCancelBuffer(ServiceCtx Context, byte[] ParcelData)
        {
            ViIHOSBinderDriver BinderDriver = Context.GetObject<ViIHOSBinderDriver>();

            using (MemoryStream MS = new MemoryStream(ParcelData))
            {
                BinaryReader Reader = new BinaryReader(MS);

                MS.Seek(0x50, SeekOrigin.Begin);

                int Slot = Reader.ReadInt32();

                BinderDriver.BufferSlots.Delete(Slot);

                return MakeReplyParcel(Context, 0);
            }
        }

        private static long GraphicBufferProducerQuery(ServiceCtx Context, byte[] ParcelData)
        {
            return MakeReplyParcel(Context, 0, 0);
        }

        private static long GraphicBufferProducerConnect(ServiceCtx Context, byte[] ParcelData)
        {
            return MakeReplyParcel(Context, 1280, 720, 0, 0, 0);
        }

        private static long GraphicBufferPreallocateBuffer(ServiceCtx Context, byte[] ParcelData)
        {
            ViIHOSBinderDriver BinderDriver = Context.GetObject<ViIHOSBinderDriver>();

            int GbfrSize = ParcelData.Length - 0x54;

            BinderDriver.Gbfr = new byte[GbfrSize];

            Buffer.BlockCopy(ParcelData, 0x54, BinderDriver.Gbfr, 0, GbfrSize);

            using (MemoryStream MS = new MemoryStream(ParcelData))
            {
                BinaryReader Reader = new BinaryReader(MS);

                MS.Seek(0xd4, SeekOrigin.Begin);

                int Handle = Reader.ReadInt32();

                HNvMap NvMap = Context.Ns.Os.Handles.GetData<HNvMap>(Handle);

                Context.Ns.Gpu.Renderer.FrameBufferPtr = NvMap.Address;
            }

            return MakeReplyParcel(Context, 0);
        }

        private static long MakeReplyParcel(ServiceCtx Context, params int[] Ints)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                foreach (int Int in Ints)
                {
                    Writer.Write(Int);
                }

                return MakeReplyParcel(Context, MS.ToArray());
            }
        }

        private static long MakeReplyParcel(ServiceCtx Context, byte[] Data)
        {
            long ReplyPos  = Context.Request.ReceiveBuff[0].Position;
            long ReplySize = Context.Request.ReceiveBuff[0].Position;

            byte[] Reply = MakeParcel(Data, new byte[0]);

            AMemoryHelper.WriteBytes(Context.Memory, ReplyPos, Reply);

            return 0;
        }

        public static long AdjustRefcount(ServiceCtx Context)
        {
            int Id     = Context.RequestData.ReadInt32();
            int AddVal = Context.RequestData.ReadInt32();
            int Type   = Context.RequestData.ReadInt32();

            return 0;
        }

        public static long GetNativeHandle(ServiceCtx Context)
        {
            int  Id  = Context.RequestData.ReadInt32();
            uint Unk = Context.RequestData.ReadUInt32();

            Context.Response.HandleDesc = IpcHandleDesc.MakeMove(0xbadcafe);

            return 0;
        }
    }
}