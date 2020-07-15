namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Transform feedback state.
    /// </summary>
    struct TfState
    {
#pragma warning disable CS0649
        public int BufferIndex;
        public int VaryingsCount;
        public int Stride;
        public uint Padding;
#pragma warning restore CS0649
    }
}
