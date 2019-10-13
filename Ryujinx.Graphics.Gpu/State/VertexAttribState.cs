namespace Ryujinx.Graphics.Gpu.State
{
    struct VertexAttribState
    {
        public uint Attribute;

        public int UnpackBufferIndex()
        {
            return (int)(Attribute & 0x1f);
        }

        public int UnpackOffset()
        {
            return (int)((Attribute >> 7) & 0x3fff);
        }

        public uint UnpackFormat()
        {
            return Attribute & 0x3fe00000;
        }
    }
}
