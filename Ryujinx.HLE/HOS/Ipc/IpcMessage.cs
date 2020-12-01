using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Ipc
{
    class IpcMessage
    {
        public IpcMessageType Type { get; set; }

        public IpcHandleDesc HandleDesc { get; set; }

        public List<IpcPtrBuffDesc>      PtrBuff      { get; private set; }
        public List<IpcBuffDesc>         SendBuff     { get; private set; }
        public List<IpcBuffDesc>         ReceiveBuff  { get; private set; }
        public List<IpcBuffDesc>         ExchangeBuff { get; private set; }
        public List<IpcRecvListBuffDesc> RecvListBuff { get; private set; }

        public List<int> ObjectIds { get; private set; }

        public byte[] RawData { get; set; }

        public IpcMessage()
        {
            PtrBuff      = new List<IpcPtrBuffDesc>();
            SendBuff     = new List<IpcBuffDesc>();
            ReceiveBuff  = new List<IpcBuffDesc>();
            ExchangeBuff = new List<IpcBuffDesc>();
            RecvListBuff = new List<IpcRecvListBuffDesc>();

            ObjectIds = new List<int>();
        }

        public IpcMessage(byte[] data, long cmdPtr) : this()
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(ms);

                Initialize(reader, cmdPtr);
            }
        }

        private void Initialize(BinaryReader reader, long cmdPtr)
        {
            int word0 = reader.ReadInt32();
            int word1 = reader.ReadInt32();

            Type = (IpcMessageType)(word0 & 0xffff);

            int  ptrBuffCount  = (word0 >> 16) & 0xf;
            int  sendBuffCount = (word0 >> 20) & 0xf;
            int  recvBuffCount = (word0 >> 24) & 0xf;
            int  xchgBuffCount = (word0 >> 28) & 0xf;

            int  rawDataSize   =  (word1 >>  0) & 0x3ff;
            int  recvListFlags =  (word1 >> 10) & 0xf;
            bool hndDescEnable = ((word1 >> 31) & 0x1) != 0;

            if (hndDescEnable)
            {
                HandleDesc = new IpcHandleDesc(reader);
            }

            for (int index = 0; index < ptrBuffCount; index++)
            {
                PtrBuff.Add(new IpcPtrBuffDesc(reader));
            }

            void ReadBuff(List<IpcBuffDesc> buff, int count)
            {
                for (int index = 0; index < count; index++)
                {
                    buff.Add(new IpcBuffDesc(reader));
                }
            }

            ReadBuff(SendBuff,     sendBuffCount);
            ReadBuff(ReceiveBuff,  recvBuffCount);
            ReadBuff(ExchangeBuff, xchgBuffCount);

            rawDataSize *= 4;

            long recvListPos = reader.BaseStream.Position + rawDataSize;

            long pad0 = GetPadSize16(reader.BaseStream.Position + cmdPtr);

            if (rawDataSize != 0)
            {
                rawDataSize -= (int)pad0;
            }

            reader.BaseStream.Seek(pad0, SeekOrigin.Current);

            int recvListCount = recvListFlags - 2;

            if (recvListCount == 0)
            {
                recvListCount = 1;
            }
            else if (recvListCount < 0)
            {
                recvListCount = 0;
            }

            RawData = reader.ReadBytes(rawDataSize);

            reader.BaseStream.Seek(recvListPos, SeekOrigin.Begin);

            for (int index = 0; index < recvListCount; index++)
            {
                RecvListBuff.Add(new IpcRecvListBuffDesc(reader));
            }
        }

        public byte[] GetBytes(long cmdPtr, ulong recvListAddr)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                int word0;
                int word1;

                word0  = (int)Type;
                word0 |= (PtrBuff.Count      & 0xf) << 16;
                word0 |= (SendBuff.Count     & 0xf) << 20;
                word0 |= (ReceiveBuff.Count  & 0xf) << 24;
                word0 |= (ExchangeBuff.Count & 0xf) << 28;

                byte[] handleData = new byte[0];

                if (HandleDesc != null)
                {
                    handleData = HandleDesc.GetBytes();
                }

                int dataLength = RawData?.Length ?? 0;

                dataLength = (dataLength + 3) & ~3;

                int rawLength = dataLength;

                int pad0 = (int)GetPadSize16(cmdPtr + 8 + handleData.Length + PtrBuff.Count * 8);

                // Apparently, padding after Raw Data is 16 bytes, however when there is
                // padding before Raw Data too, we need to subtract the size of this padding.
                // This is the weirdest padding I've seen so far...
                int pad1 = 0x10 - pad0;

                dataLength = (dataLength + pad0 + pad1) / 4;

                word1 = (dataLength & 0x3ff) | (2 << 10);

                if (HandleDesc != null)
                {
                    word1 |= 1 << 31;
                }

                writer.Write(word0);
                writer.Write(word1);
                writer.Write(handleData);

                for (int index = 0; index < PtrBuff.Count; index++)
                {
                    writer.Write(PtrBuff[index].GetWord0());
                    writer.Write(PtrBuff[index].GetWord1());
                }

                ms.Seek(pad0, SeekOrigin.Current);

                if (RawData != null)
                {
                    writer.Write(RawData);
                    ms.Seek(rawLength - RawData.Length, SeekOrigin.Current);
                }

                writer.Write(new byte[pad1]);
                writer.Write(recvListAddr);

                return ms.ToArray();
            }
        }

        private long GetPadSize16(long position)
        {
            if ((position & 0xf) != 0)
            {
                return 0x10 - (position & 0xf);
            }

            return 0;
        }

        // ReSharper disable once InconsistentNaming
        public (long Position, long Size) GetBufferType0x21(int index = 0)
        {
            if (PtrBuff.Count > index &&
                PtrBuff[index].Position != 0 &&
                PtrBuff[index].Size     != 0)
            {
                return (PtrBuff[index].Position, PtrBuff[index].Size);
            }

            if (SendBuff.Count > index &&
                SendBuff[index].Position != 0 &&
                SendBuff[index].Size     != 0)
            {
                return (SendBuff[index].Position, SendBuff[index].Size);
            }

            return (0, 0);
        }

        // ReSharper disable once InconsistentNaming
        public (long Position, long Size) GetBufferType0x22(int index = 0)
        {
            if (RecvListBuff.Count > index &&
                RecvListBuff[index].Position != 0 &&
                RecvListBuff[index].Size     != 0)
            {
                return (RecvListBuff[index].Position, RecvListBuff[index].Size);
            }

            if (ReceiveBuff.Count > index &&
                ReceiveBuff[index].Position != 0 &&
                ReceiveBuff[index].Size     != 0)
            {
                return (ReceiveBuff[index].Position, ReceiveBuff[index].Size);
            }

            return (0, 0);
        }
    }
}
