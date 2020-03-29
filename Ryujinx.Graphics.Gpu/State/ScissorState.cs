namespace Ryujinx.Graphics.Gpu.State
{
    struct ScissorState
    {
        public Boolean32 Enable;
        public ushort X1;
        public ushort X2;
        public ushort Y1;
        public ushort Y2;
        public uint Padding;
    }
}
