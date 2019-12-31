namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// GPU counter report state.
    /// </summary>
    struct ReportState
    {
        public GpuVa Address;
        public int   Payload;
        public uint  Control;
    }
}
