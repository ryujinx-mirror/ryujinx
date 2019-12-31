namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Condition parameters for conditional rendering.
    /// </summary>
    struct ConditionState
    {
        public GpuVa     Address;
        public Condition Condition;
    }
}
