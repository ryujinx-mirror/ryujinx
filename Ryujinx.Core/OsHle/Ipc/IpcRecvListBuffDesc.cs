using System.IO;

namespace Ryujinx.Core.OsHle.Ipc
{
    struct IpcRecvListBuffDesc
    {
        public long Position { get; private set; }
        public long Size     { get; private set; }

        public IpcRecvListBuffDesc(BinaryReader Reader)
        {
            long Value = Reader.ReadInt64();

            Position = Value & 0xffffffffffff;

            Size = (ushort)(Value >> 48);
        }
    }
}