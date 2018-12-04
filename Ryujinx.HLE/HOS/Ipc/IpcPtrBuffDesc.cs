using System.IO;

namespace Ryujinx.HLE.HOS.Ipc
{
    struct IpcPtrBuffDesc
    {
        public long  Position { get; }
        public int   Index    { get; }
        public long  Size     { get; }

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
    }
}