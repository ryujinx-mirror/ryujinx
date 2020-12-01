using System.IO;

namespace Ryujinx.HLE.HOS.Ipc
{
    struct IpcRecvListBuffDesc
    {
        public long Position { get; private set; }
        public long Size     { get; private set; }

        public IpcRecvListBuffDesc(long position, long size)
        {
            Position = position;
            Size = size;
        }

        public IpcRecvListBuffDesc(BinaryReader reader)
        {
            long value = reader.ReadInt64();

            Position = value & 0xffffffffffff;

            Size = (ushort)(value >> 48);
        }
    }
}