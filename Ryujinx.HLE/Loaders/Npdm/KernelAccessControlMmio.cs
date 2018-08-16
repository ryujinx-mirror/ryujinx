namespace Ryujinx.HLE.Loaders.Npdm
{
    struct KernelAccessControlMmio
    {
        public ulong Address  { get; private set; }
        public ulong Size     { get; private set; }
        public bool  IsRo     { get; private set; }
        public bool  IsNormal { get; private set; }

        public KernelAccessControlMmio(
            ulong Address,
            ulong Size,
            bool  IsRo,
            bool  IsNormal)
        {
            this.Address  = Address;
            this.Size     = Size;
            this.IsRo     = IsRo;
            this.IsNormal = IsNormal;
        }
    }
}