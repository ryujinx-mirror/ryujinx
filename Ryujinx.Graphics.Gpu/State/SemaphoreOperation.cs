namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// GPU semaphore operation.
    /// </summary>
    enum SemaphoreOperation
    {
        Release = 0,
        Acquire = 1,
        Counter = 2
    }
}