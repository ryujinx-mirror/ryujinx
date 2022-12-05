namespace Ryujinx.Cpu
{
    /// <summary>
    /// Exception callback without any additional arguments.
    /// </summary>
    /// <param name="context">Context for the thread where the exception was triggered</param>
    public delegate void ExceptionCallbackNoArgs(IExecutionContext context);

    /// <summary>
    /// Exception callback.
    /// </summary>
    /// <param name="context">Context for the thread where the exception was triggered</param>
    /// <param name="address">Address of the instruction that caused the exception</param>
    /// <param name="imm">Immediate value of the instruction that caused the exception, or for undefined instruction, the instruction itself</param>
    public delegate void ExceptionCallback(IExecutionContext context, ulong address, int imm);

    /// <summary>
    /// Stores handlers for the various CPU exceptions.
    /// </summary>
    public readonly struct ExceptionCallbacks
    {
        /// <summary>
        /// Handler for CPU interrupts triggered using <see cref="IExecutionContext.RequestInterrupt"/>.
        /// </summary>
        public readonly ExceptionCallbackNoArgs InterruptCallback;

        /// <summary>
        /// Handler for CPU software interrupts caused by the Arm BRK instruction.
        /// </summary>
        public readonly ExceptionCallback BreakCallback;

        /// <summary>
        /// Handler for CPU software interrupts caused by the Arm SVC instruction.
        /// </summary>
        public readonly ExceptionCallback SupervisorCallback;

        /// <summary>
        /// Handler for CPU software interrupts caused by any undefined Arm instruction.
        /// </summary>
        public readonly ExceptionCallback UndefinedCallback;

        /// <summary>
        /// Creates a new exception callbacks structure.
        /// </summary>
        /// <remarks>
        /// All handlers are optional, and if null, the CPU will just continue executing as if nothing happened.
        /// </remarks>
        /// <param name="interruptCallback">Handler for CPU interrupts triggered using <see cref="IExecutionContext.RequestInterrupt"/></param>
        /// <param name="breakCallback">Handler for CPU software interrupts caused by the Arm BRK instruction</param>
        /// <param name="supervisorCallback">Handler for CPU software interrupts caused by the Arm SVC instruction</param>
        /// <param name="undefinedCallback">Handler for CPU software interrupts caused by any undefined Arm instruction</param>
        public ExceptionCallbacks(
            ExceptionCallbackNoArgs interruptCallback = null,
            ExceptionCallback breakCallback = null,
            ExceptionCallback supervisorCallback = null,
            ExceptionCallback undefinedCallback = null)
        {
            InterruptCallback = interruptCallback;
            BreakCallback = breakCallback;
            SupervisorCallback = supervisorCallback;
            UndefinedCallback = undefinedCallback;
        }
    }
}
