namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Transform feedback buffer state.
    /// </summary>
    struct TfBufferState
    {
#pragma warning disable CS0649
        public Boolean32 Enable;
        public GpuVa     Address;
        public int       Size;
        public int       Offset;
        public uint      Padding0;
        public uint      Padding1;
        public uint      Padding2;
#pragma warning restore CS0649
    }
}
