using System.IO;

namespace Ryujinx.HLE.HOS.Ipc
{
    struct IpcPtrBuffDesc
    {
        public long  Position { get; private set; }
        public int   Index    { get; private set; }
        public long  Size     { get; private set; }

        public IpcPtrBuffDesc(long position, int index, long size)
        {
            Position = position;
            Index = index;
            Size = size;
        }

        public IpcPtrBuffDesc(BinaryReader reader)
        {
            long word0 = reader.ReadUInt32();
            long word1 = reader.ReadUInt32();

            Position  =  word1;
            Position |= (word0 << 20) & 0x0f00000000;
            Position |= (word0 << 30) & 0x7000000000;

            Index  = ((int)word0 >> 0) & 0x03f;
            Index |= ((int)word0 >> 3) & 0x1c0;

            Size = (ushort)(word0 >> 16);
        }

        public IpcPtrBuffDesc WithSize(long size)
        {
            return new IpcPtrBuffDesc(Position, Index, size);
        }

        public uint GetWord0()
        {
            uint word0;

            word0  = (uint)((Position & 0x0f00000000) >> 20);
            word0 |= (uint)((Position & 0x7000000000) >> 30);

            word0 |= (uint)(Index & 0x03f) << 0;
            word0 |= (uint)(Index & 0x1c0) << 3;

            word0 |= (uint)Size << 16;

            return word0;
        }

        public uint GetWord1()
        {
            return (uint)Position;
        }
    }
}