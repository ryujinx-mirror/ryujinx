using System.IO;

namespace Ryujinx.HLE.HOS.Ipc
{
    struct IpcPtrBuffDesc
    {
        public long  Position { get; private set; }
        public int   Index    { get; private set; }
        public long  Size     { get; private set; }

        public IpcPtrBuffDesc(BinaryReader Reader)
        {
            long Word0 = Reader.ReadUInt32();
            long Word1 = Reader.ReadUInt32();

            Position  =  Word1;
            Position |= (Word0 << 20) & 0x0f00000000;
            Position |= (Word0 << 30) & 0x7000000000;

            Index  = ((int)Word0 >> 0) & 0x03f;
            Index |= ((int)Word0 >> 3) & 0x1c0;

            Size = (ushort)(Word0 >> 16);
        }
    }
}