using ARMeilleure.State;
using System;

namespace Ryujinx.Cpu
{
    /// <summary>
    /// CPU register state interface.
    /// </summary>
    public interface IExecutionContext : IDisposable
    {
        /// <summary>
        /// Current Program Counter.
        /// </summary>
        /// <remarks>
        /// In some implementations, this value might not be accurate and might not point to the last instruction executed.
        /// </remarks>
        ulong Pc { get; }

        /// <summary>
        /// Thread ID Register (EL0).
        /// </summary>
        long TpidrEl0 { get; set; }

        /// <summary>
        /// Thread ID Register (read-only) (EL0).
        /// </summary>
        long TpidrroEl0 { get; set; }

        /// <summary>
        /// Processor State Register.
        /// </summary>
        uint Pstate { get; set; }

        /// <summary>
        /// Floating-point Control Register.
        /// </summary>
        uint Fpcr { get; set; }

        /// <summary>
        /// Floating-point Status Register.
        /// </summary>
        uint Fpsr { get; set; }

        /// <summary>
        /// Indicates whenever the CPU is running 64-bit (AArch64 mode) or 32-bit (AArch32 mode) code.
        /// </summary>
        bool IsAarch32 { get; set; }

        /// <summary>
        /// Indicates whenever the CPU is still running code.
        /// </summary>
        /// <remarks>
        /// Even if this is false, the guest code might be still exiting.
        /// One must not assume that the code is no longer running from this property alone.
        /// </remarks>
        bool Running { get; }

        /// <summary>
        /// Gets the value of a general purpose register.
        /// </summary>
        /// <remarks>
        /// The special <paramref name="index"/> of 31 can be used to access the SP (Stack Pointer) register.
        /// </remarks>
        /// <param name="index">Index of the register, in the range 0-31 (inclusive)</param>
        /// <returns>The register value</returns>
        ulong GetX(int index);

        /// <summary>
        /// Sets the value of a general purpose register.
        /// </summary>
        /// <remarks>
        /// The special <paramref name="index"/> of 31 can be used to access the SP (Stack Pointer) register.
        /// </remarks>
        /// <param name="index">Index of the register, in the range 0-31 (inclusive)</param>
        /// <param name="value">Value to be set</param>
        void SetX(int index, ulong value);

        /// <summary>
        /// Gets the value of a FP/SIMD register.
        /// </summary>
        /// <param name="index">Index of the register, in the range 0-31 (inclusive)</param>
        /// <returns>The register value</returns>
        V128 GetV(int index);

        /// <summary>
        /// Sets the value of a FP/SIMD register.
        /// </summary>
        /// <param name="index">Index of the register, in the range 0-31 (inclusive)</param>
        /// <param name="value">Value to be set</param>
        void SetV(int index, V128 value);

        /// <summary>
        /// Requests the thread to stop running temporarily and call <see cref="ExceptionCallbacks.InterruptCallback"/>.
        /// </summary>
        /// <remarks>
        /// The thread might not pause immediately.
        /// One must not assume that guest code is no longer being executed by the thread after calling this function.
        /// </remarks>
        void RequestInterrupt();

        /// <summary>
        /// Requests the thread to stop running guest code and return as soon as possible.
        /// </summary>
        /// <remarks>
        /// The thread might not stop immediately.
        /// One must not assume that guest code is no longer being executed by the thread after calling this function.
        /// After a thread has been stopped, it can't be restarted with the same <see cref="IExecutionContext"/>.
        /// If you only need to pause the thread temporarily, use <see cref="RequestInterrupt"/> instead.
        /// </remarks>
        void StopRunning();
    }
}
