namespace Ryujinx.HLE.HOS.Ipc
{
    struct IpcRecvListBuffDesc
    {
        public ulong Position { get; private set; }
        public ulong Size { get; private set; }

        public IpcRecvListBuffDesc(ulong position, ulong size)
        {
            Position = position;
            Size = size;
        }

        public IpcRecvListBuffDesc(ulong packedValue)
        {
            Position = packedValue & 0xffffffffffff;

            Size = (ushort)(packedValue >> 48);
        }
    }
}
