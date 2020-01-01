namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Represents a filter used with texture minification linear filtering.
    /// </summary>
    /// <remarks>
    /// This feature is only supported on NVIDIA GPUs.
    /// </remarks>
    enum ReductionFilter
    {
        Average,
        Minimum,
        Maximum
    }
}
