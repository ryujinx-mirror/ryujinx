using System.IO;

namespace Ryujinx.OsHle.Ipc
{
    struct IpcBuffDesc
    {
        public long Position { get; private set; }
        public long Size     { get; private set; }
        public int  Flags    { get; private set; }

        public IpcBuffDesc(BinaryReader Reader)
        {
            long Word0 = Reader.ReadUInt32();
            long Word1 = Reader.ReadUInt32();
            long Word2 = Reader.ReadUInt32();

            Position  =  Word1;
            Position |= (Word2 <<  4) & 0x0f00000000;
            Position |= (Word2 << 34) & 0x7000000000;

            Size  =  Word0;
            Size |= (Word2 << 8) & 0xf00000000;

            Flags = (int)Word2 & 3;
        }
    }
}