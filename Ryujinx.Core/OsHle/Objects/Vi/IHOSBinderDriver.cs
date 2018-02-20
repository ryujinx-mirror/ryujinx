using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using Ryujinx.Core.OsHle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using static Ryujinx.Core.OsHle.Objects.Android.Parcel;

namespace Ryujinx.Core.OsHle.Objects.Vi
{
    class IHOSBinderDriver : IIpcInterface
    {
        private delegate long ServiceProcessParcel(ServiceCtx Context, byte[] ParcelData);

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        private Dictionary<(string, int), ServiceProcessParcel> m_Methods;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;       

        private class BufferObj
        {

        }

        private IdPoolWithObj BufferSlots;

        private byte[] Gbfr;

        public IHOSBinderDriver()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, TransactParcel  },
                { 1, AdjustRefcount  },
                { 2, GetNativeHandle }
            };

            m_Methods = new Dictionary<(string, int), ServiceProcessParcel>()
            {
                { ("android.gui.IGraphicBufferProducer", 0x1), GraphicBufferProducerRequestBuffer },
                { ("android.gui.IGraphicBufferProducer", 0x3), GraphicBufferProducerDequeueBuffer },
                { ("android.gui.IGraphicBufferProducer", 0x7), GraphicBufferProducerQueueBuffer   },
                { ("android.gui.IGraphicBufferProducer", 0x8), GraphicBufferProducerCancelBuffer  },
                { ("android.gui.IGraphicBufferProducer", 0x9), GraphicBufferProducerQuery         },
                { ("android.gui.IGraphicBufferProducer", 0xa), GraphicBufferProducerConnect       },
                { ("android.gui.IGraphicBufferProducer", 0xe), GraphicBufferPreallocateBuffer     }
            };

            BufferSlots = new IdPoolWithObj();
        }

        public long TransactParcel(ServiceCtx Context)
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

                if (m_Methods.TryGetValue((InterfaceName, Code), out ServiceProcessParcel ProcReq))
                {
                    return ProcReq(Context, Data);
                }
                else
                {
                    throw new NotImplementedException($"{InterfaceName} {Code}");
                }
            }
        }

        private long GraphicBufferProducerRequestBuffer(ServiceCtx Context, byte[] ParcelData)
        {
            int GbfrSize = Gbfr?.Length ?? 0;

            byte[] Data = new byte[GbfrSize + 4];

            if (Gbfr != null)
            {
                Buffer.BlockCopy(Gbfr, 0, Data, 0, GbfrSize);
            }

            return MakeReplyParcel(Context, Data);
        }

        private long GraphicBufferProducerDequeueBuffer(ServiceCtx Context, byte[] ParcelData)
        {
            //Note: It seems that the maximum number of slots is 64, because if we return
            //a Slot number > 63, it seems to cause a buffer overrun and it reads garbage.
            //Note 2: The size of each object associated with the slot is 0x30.
            int Slot = BufferSlots.GenerateId(new BufferObj());

            return MakeReplyParcel(Context, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        private long GraphicBufferProducerQueueBuffer(ServiceCtx Context, byte[] ParcelData)
        {
            return MakeReplyParcel(Context, 1280, 720, 0, 0, 0);
        }

        private long GraphicBufferProducerCancelBuffer(ServiceCtx Context, byte[] ParcelData)
        {
            using (MemoryStream MS = new MemoryStream(ParcelData))
            {
                BinaryReader Reader = new BinaryReader(MS);

                MS.Seek(0x50, SeekOrigin.Begin);

                int Slot = Reader.ReadInt32();

                BufferSlots.Delete(Slot);

                return MakeReplyParcel(Context, 0);
            }
        }

        private long GraphicBufferProducerQuery(ServiceCtx Context, byte[] ParcelData)
        {
            return MakeReplyParcel(Context, 0, 0);
        }

        private long GraphicBufferProducerConnect(ServiceCtx Context, byte[] ParcelData)
        {
            return MakeReplyParcel(Context, 1280, 720, 0, 0, 0);
        }

        private long GraphicBufferPreallocateBuffer(ServiceCtx Context, byte[] ParcelData)
        {
            int GbfrSize = ParcelData.Length - 0x54;

            Gbfr = new byte[GbfrSize];

            Buffer.BlockCopy(ParcelData, 0x54, Gbfr, 0, GbfrSize);

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

        private long MakeReplyParcel(ServiceCtx Context, params int[] Ints)
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

        private long MakeReplyParcel(ServiceCtx Context, byte[] Data)
        {
            long ReplyPos  = Context.Request.ReceiveBuff[0].Position;
            long ReplySize = Context.Request.ReceiveBuff[0].Position;

            byte[] Reply = MakeParcel(Data, new byte[0]);

            AMemoryHelper.WriteBytes(Context.Memory, ReplyPos, Reply);

            return 0;
        }

        public long AdjustRefcount(ServiceCtx Context)
        {
            int Id     = Context.RequestData.ReadInt32();
            int AddVal = Context.RequestData.ReadInt32();
            int Type   = Context.RequestData.ReadInt32();

            return 0;
        }

        public long GetNativeHandle(ServiceCtx Context)
        {
            int  Id  = Context.RequestData.ReadInt32();
            uint Unk = Context.RequestData.ReadUInt32();

            Context.Response.HandleDesc = IpcHandleDesc.MakeMove(0xbadcafe);

            return 0;
        }
    }
}