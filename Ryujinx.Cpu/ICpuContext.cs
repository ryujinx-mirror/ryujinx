namespace Ryujinx.Cpu
{
    /// <summary>
    /// CPU context interface.
    /// </summary>
    public interface ICpuContext
    {
        /// <summary>
        /// Creates a new execution context that will store thread CPU register state when executing guest code.
        /// </summary>
        /// <param name="exceptionCallbacks">Optional functions to be called when the CPU receives an interrupt</param>
        /// <returns>Execution context</returns>
        IExecutionContext CreateExecutionContext(ExceptionCallbacks exceptionCallbacks);

        /// <summary>
        /// Starts executing code at a specified entry point address.
        /// </summary>
        /// <remarks>
        /// This function only returns when the execution is stopped, by calling <see cref="IExecutionContext.StopRunning"/>.
        /// </remarks>
        /// <param name="context">Execution context to be used for this run</param>
        /// <param name="address">Entry point address</param>
        void Execute(IExecutionContext context, ulong address);

        /// <summary>
        /// Invalidates the instruction cache for a given memory region.
        /// </summary>
        /// <remarks>
        /// This should be called if code is modified to make the CPU emulator aware of the modifications,
        /// otherwise it might run stale code which will lead to errors and crashes.
        /// Calling this function is not necessary if the code memory was modified by guest code,
        /// as the expectation is that it will do it on its own using the appropriate cache invalidation instructions,
        /// except on Arm32 where those instructions can't be used in unprivileged mode.
        /// </remarks>
        /// <param name="address">Address of the region to be invalidated</param>
        /// <param name="size">Size of the region to be invalidated</param>
        void InvalidateCacheRegion(ulong address, ulong size);
    }
}
