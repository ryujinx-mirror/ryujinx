using System;
using System.IO;

namespace Ryujinx.Core.OsHle.Ipc
{
    public static class IpcLog
    {
        public static string Message(byte[] Data, long CmdPtr, bool Domain)
        {
            string IpcMessage = "";

            using (MemoryStream MS = new MemoryStream(Data))
            {
                BinaryReader Reader = new BinaryReader(MS);

                int Word0 = Reader.ReadInt32();
                int Word1 = Reader.ReadInt32();

                int Type = (Word0 & 0xffff);

                int PtrBuffCount = (Word0 >> 16) & 0xf;
                int SendBuffCount = (Word0 >> 20) & 0xf;
                int RecvBuffCount = (Word0 >> 24) & 0xf;
                int XchgBuffCount = (Word0 >> 28) & 0xf;

                int RawDataSize = (Word1 >> 0) & 0x3ff;
                int RecvListFlags = (Word1 >> 10) & 0xf;
                bool HndDescEnable = ((Word1 >> 31) & 0x1) != 0;

                IpcMessage += Environment.NewLine + $" {Logging.GetExecutionTime()} | IpcMessage >" + Environment.NewLine +
                              $"   Type: {Enum.GetName(typeof(IpcMessageType), Type)}" + Environment.NewLine +

                              $"   PtrBuffCount: {PtrBuffCount.ToString()}" + Environment.NewLine +
                              $"   SendBuffCount: {SendBuffCount.ToString()}" + Environment.NewLine +
                              $"   RecvBuffCount: {RecvBuffCount.ToString()}" + Environment.NewLine +
                              $"   XchgBuffCount: {XchgBuffCount.ToString()}" + Environment.NewLine +

                              $"   RawDataSize: {RawDataSize.ToString()}" + Environment.NewLine +
                              $"   RecvListFlags: {RecvListFlags.ToString()}" + Environment.NewLine +
                              $"   HndDescEnable: {HndDescEnable.ToString()}" + Environment.NewLine;

                if (HndDescEnable)
                {
                    int Word = Reader.ReadInt32();

                    bool HasPId = (Word & 1) != 0;

                    int[] ToCopy = new int[(Word >> 1) & 0xf];
                    int[] ToMove = new int[(Word >> 5) & 0xf];

                    long PId = HasPId ? Reader.ReadInt64() : 0;

                    for (int Index = 0; Index < ToCopy.Length; Index++)
                    {
                        ToCopy[Index] = Reader.ReadInt32();
                    }

                    for (int Index = 0; Index < ToMove.Length; Index++)
                    {
                        ToMove[Index] = Reader.ReadInt32();
                    }

                    IpcMessage += Environment.NewLine + "    HndDesc:" + Environment.NewLine +
                                  $"      PId: {PId.ToString()}" + Environment.NewLine +
                                  $"      ToCopy.Length: {ToCopy.Length.ToString()}" + Environment.NewLine +
                                  $"      ToMove.Length: {ToMove.Length.ToString()}" + Environment.NewLine;
                }

                for (int Index = 0; Index < PtrBuffCount; Index++)
                {
                    long IpcPtrBuffDescWord0 = Reader.ReadUInt32();
                    long IpcPtrBuffDescWord1 = Reader.ReadUInt32();

                    long Position = IpcPtrBuffDescWord1;
                    Position |= (IpcPtrBuffDescWord0 << 20) & 0x0f00000000;
                    Position |= (IpcPtrBuffDescWord0 << 30) & 0x7000000000;

                    int IpcPtrBuffDescIndex = ((int)IpcPtrBuffDescWord0 >> 0) & 0x03f;
                    IpcPtrBuffDescIndex |= ((int)IpcPtrBuffDescWord0 >> 3) & 0x1c0;

                    short Size = (short)(IpcPtrBuffDescWord0 >> 16);

                    IpcMessage += Environment.NewLine + $"    PtrBuff[{Index}]:" + Environment.NewLine +
                                  $"      Position: {Position.ToString()}" + Environment.NewLine +
                                  $"      IpcPtrBuffDescIndex: {IpcPtrBuffDescIndex.ToString()}" + Environment.NewLine +
                                  $"      Size: {Size.ToString()}" + Environment.NewLine;
                }

                ReadIpcBuffValues(Reader, SendBuffCount, IpcMessage, "SendBuff");
                ReadIpcBuffValues(Reader, RecvBuffCount, IpcMessage, "RecvBuff");
                ReadIpcBuffValues(Reader, XchgBuffCount, IpcMessage, "XchgBuff");

                RawDataSize *= 4;

                long RecvListPos = Reader.BaseStream.Position + RawDataSize;
                long Pad0 = 0;

                if ((Reader.BaseStream.Position + CmdPtr & 0xf) != 0)
                {
                    Pad0 = 0x10 - (Reader.BaseStream.Position + CmdPtr & 0xf);
                }

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

                if (Domain && (IpcMessageType)Type == IpcMessageType.Request)
                {
                    int DomWord0 = Reader.ReadInt32();

                    int DomCmd = (DomWord0 & 0xff);

                    RawDataSize = (DomWord0 >> 16) & 0xffff;

                    int DomObjId = Reader.ReadInt32();

                    Reader.ReadInt64(); //Padding

                    IpcMessage += Environment.NewLine + $"    Domain:" + Environment.NewLine + Environment.NewLine +
                                  $"      DomObjId: {DomObjId.ToString()}" + Environment.NewLine;
                }

                byte[] RawData = Reader.ReadBytes(RawDataSize);

                IpcMessage += Environment.NewLine + $"    RawData:" + Environment.NewLine + Logging.HexDump(RawData);

                Reader.BaseStream.Seek(RecvListPos, SeekOrigin.Begin);

                for (int Index = 0; Index < RecvListCount; Index++)
                {
                    long RecvListBuffValue = Reader.ReadInt64();
                    long RecvListBuffPosition = RecvListBuffValue & 0xffffffffffff;
                    long RecvListBuffSize = (short)(RecvListBuffValue >> 48);

                    IpcMessage += Environment.NewLine + $"    RecvList[{Index}]:" + Environment.NewLine +
                                  $"      Value: {RecvListBuffValue.ToString()}" + Environment.NewLine +
                                  $"      Position: {RecvListBuffPosition.ToString()}" + Environment.NewLine +
                                  $"      Size: {RecvListBuffSize.ToString()}" + Environment.NewLine;
                }
            }

            return IpcMessage;
        }

        private static void ReadIpcBuffValues(BinaryReader Reader, int Count, string IpcMessage, string BufferName)
        {
            for (int Index = 0; Index < Count; Index++)
            {
                long Word0 = Reader.ReadUInt32();
                long Word1 = Reader.ReadUInt32();
                long Word2 = Reader.ReadUInt32();

                long Position = Word1;
                Position |= (Word2 << 4) & 0x0f00000000;
                Position |= (Word2 << 34) & 0x7000000000;

                long Size = Word0;
                Size |= (Word2 << 8) & 0xf00000000;

                int Flags = (int)Word2 & 3;

                IpcMessage += Environment.NewLine + $"    {BufferName}[{Index}]:" + Environment.NewLine +
                              $"      Position: {Position.ToString()}" + Environment.NewLine +
                              $"      Flags: {Flags.ToString()}" + Environment.NewLine +
                              $"      Size: {Size.ToString()}" + Environment.NewLine;
            }
        }
    }
}
