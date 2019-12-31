namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Condition for conditional rendering.
    /// </summary>
    enum Condition
    {
        Never,
        Always,
        ResultNonZero,
        Equal,
        NotEqual
    }
}
