using System.IO;

namespace Ryujinx.HLE.HOS.Ipc
{
    struct IpcRecvListBuffDesc
    {
        public ulong Position { get; private set; }
        public ulong Size     { get; private set; }

        public IpcRecvListBuffDesc(ulong position, ulong size)
        {
            Position = position;
            Size = size;
        }

        public IpcRecvListBuffDesc(BinaryReader reader)
        {
            ulong value = reader.ReadUInt64();

            Position = value & 0xffffffffffff;

            Size = (ushort)(value >> 48);
        }
    }
}