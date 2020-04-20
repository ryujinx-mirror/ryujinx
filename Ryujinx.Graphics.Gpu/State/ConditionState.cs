namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Condition parameters for conditional rendering.
    /// </summary>
    struct ConditionState
    {
#pragma warning disable CS0649
        public GpuVa     Address;
        public Condition Condition;
#pragma warning restore CS0649
    }
}
