namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Depth bias (also called polygon offset) parameters.
    /// </summary>
    struct DepthBiasState
    {
        public Boolean32 PointEnable;
        public Boolean32 LineEnable;
        public Boolean32 FillEnable;
    }
}
