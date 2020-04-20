namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Primitive restart state.
    /// </summary>
    struct PrimitiveRestartState
    {
#pragma warning disable CS0649
        public Boolean32 Enable;
        public int       Index;
#pragma warning restore CS0649
    }
}
