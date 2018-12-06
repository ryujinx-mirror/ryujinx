using System.IO;

namespace Ryujinx.HLE.HOS.Ipc
{
    struct IpcBuffDesc
    {
        public long Position { get; private set; }
        public long Size     { get; private set; }
        public int  Flags    { get; private set; }

        public IpcBuffDesc(BinaryReader reader)
        {
            long word0 = reader.ReadUInt32();
            long word1 = reader.ReadUInt32();
            long word2 = reader.ReadUInt32();

            Position  =  word1;
            Position |= (word2 <<  4) & 0x0f00000000;
            Position |= (word2 << 34) & 0x7000000000;

            Size  =  word0;
            Size |= (word2 << 8) & 0xf00000000;

            Flags = (int)word2 & 3;
        }
    }
}