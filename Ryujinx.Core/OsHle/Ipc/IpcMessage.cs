using System.Collections.Generic;
using System.IO;

namespace Ryujinx.Core.OsHle.Ipc
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

        public List<int> ResponseObjIds { get; private set; }    

        public bool      IsDomain { get; private set; }
        public IpcDomCmd DomCmd   { get; private set; }
        public int       DomObjId { get; private set; }

        public byte[] RawData { get; set; }

        public IpcMessage()
        {
            PtrBuff      = new List<IpcPtrBuffDesc>();
            SendBuff     = new List<IpcBuffDesc>();
            ReceiveBuff  = new List<IpcBuffDesc>();
            ExchangeBuff = new List<IpcBuffDesc>();
            RecvListBuff = new List<IpcRecvListBuffDesc>();

            ResponseObjIds = new List<int>();
        }

        public IpcMessage(bool Domain) : this()
        {
            IsDomain = Domain;
        }

        public IpcMessage(byte[] Data, long CmdPtr, bool Domain) : this()
        {
            using (MemoryStream MS = new MemoryStream(Data))
            {
                BinaryReader Reader = new BinaryReader(MS);

                Initialize(Reader, CmdPtr, Domain);
            }
        }

        private void Initialize(BinaryReader Reader, long CmdPtr, bool Domain)
        {
            IsDomain = Domain;

            int Word0 = Reader.ReadInt32();
            int Word1 = Reader.ReadInt32();

            Type = (IpcMessageType)(Word0 & 0xffff);

            int  PtrBuffCount  = (Word0 >> 16) & 0xf;
            int  SendBuffCount = (Word0 >> 20) & 0xf;
            int  RecvBuffCount = (Word0 >> 24) & 0xf;
            int  XchgBuffCount = (Word0 >> 28) & 0xf;

            int  RawDataSize   =  (Word1 >>  0) & 0x3ff;
            int  RecvListFlags =  (Word1 >> 10) & 0xf;
            bool HndDescEnable = ((Word1 >> 31) & 0x1) != 0;

            if (HndDescEnable)
            {
                HandleDesc = new IpcHandleDesc(Reader);
            }

            for (int Index = 0; Index < PtrBuffCount; Index++)
            {
                PtrBuff.Add(new IpcPtrBuffDesc(Reader));
            }

            void ReadBuff(List<IpcBuffDesc> Buff, int Count)
            {
                for (int Index = 0; Index < Count; Index++)
                {
                    Buff.Add(new IpcBuffDesc(Reader));
                }
            }

            ReadBuff(SendBuff,     SendBuffCount);
            ReadBuff(ReceiveBuff,  RecvBuffCount);
            ReadBuff(ExchangeBuff, XchgBuffCount);

            RawDataSize *= 4;

            long RecvListPos = Reader.BaseStream.Position + RawDataSize;

            long Pad0 = GetPadSize16(Reader.BaseStream.Position + CmdPtr);

            Reader.BaseStream.Seek(Pad0, SeekOrigin.Current);            

            int RecvListCount = RecvListFlags - 2;

            if (RecvListCount == 0)
            {
                RecvListCount = 1;
            }
            else if (RecvListCount < 0)
            {
                RecvListCount = 0;
            }

            if (Domain)
            {
                int DomWord0 = Reader.ReadInt32();

                DomCmd = (IpcDomCmd)(DomWord0 & 0xff);

                RawDataSize = (DomWord0 >> 16) & 0xffff;

                DomObjId = Reader.ReadInt32();

                Reader.ReadInt64(); //Padding
            }

            RawData = Reader.ReadBytes(RawDataSize);

            Reader.BaseStream.Seek(RecvListPos, SeekOrigin.Begin);

            for (int Index = 0; Index < RecvListCount; Index++)
            {
                RecvListBuff.Add(new IpcRecvListBuffDesc(Reader));
            }
        }

        public byte[] GetBytes(long CmdPtr)
        {
            //todo
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                int Word0;
                int Word1;

                Word0  = (int)Type;
                Word0 |= (PtrBuff.Count      & 0xf) << 16;
                Word0 |= (SendBuff.Count     & 0xf) << 20;
                Word0 |= (ReceiveBuff.Count  & 0xf) << 24;
                Word0 |= (ExchangeBuff.Count & 0xf) << 28;

                byte[] HandleData = new byte[0];

                if (HandleDesc != null)
                {
                    HandleData = HandleDesc.GetBytes();
                }

                int DataLength = RawData?.Length ?? 0;

                int Pad0 = (int)GetPadSize16(CmdPtr + 8 + HandleData.Length);

                //Apparently, padding after Raw Data is 16 bytes, however when there is
                //padding before Raw Data too, we need to subtract the size of this padding.
                //This is the weirdest padding I've seen so far...
                int Pad1 = 0x10 - Pad0;

                DataLength = (DataLength + Pad0 + Pad1 + (IsDomain ? 0x10 : 0)) / 4;

                DataLength += ResponseObjIds.Count;

                Word1 = DataLength & 0x3ff;

                if (HandleDesc != null)
                {
                    Word1 |= 1 << 31;
                }

                Writer.Write(Word0);
                Writer.Write(Word1);
                Writer.Write(HandleData);

                MS.Seek(Pad0, SeekOrigin.Current);

                if (IsDomain)
                {
                    Writer.Write(ResponseObjIds.Count);
                    Writer.Write(0);
                    Writer.Write(0L);
                }

                if (RawData != null)
                {
                    Writer.Write(RawData);
                }

                foreach (int Id in ResponseObjIds)
                {
                    Writer.Write(Id);
                }

                Writer.Write(new byte[Pad1]);

                return MS.ToArray();
            }
        }

        private long GetPadSize16(long Position)
        {
            if ((Position & 0xf) != 0)
            {
                return 0x10 - (Position & 0xf);
            }

            return 0;
        }

        public long GetSendBuffPtr()
        {
            if (SendBuff.Count > 0 && SendBuff[0].Position != 0)
            {
                return SendBuff[0].Position;
            }

            if (PtrBuff.Count > 0 && PtrBuff[0].Position != 0)
            {
                return PtrBuff[0].Position;
            }

            return -1;
        }
    }
}