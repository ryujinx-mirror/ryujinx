using System;

namespace Ryujinx.Common.PreciseSleep
{
    /// <summary>
    /// An event which works similarly to an AutoResetEvent, but is backed by a
    /// more precise timer that allows waits of less than a millisecond.
    /// </summary>
    public interface IPreciseSleepEvent : IDisposable
    {
        /// <summary>
        /// Adjust a timepoint to better fit the host clock.
        /// When no adjustment is made, the input timepoint will be returned.
        /// </summary>
        /// <param name="timePoint">Timepoint to adjust</param>
        /// <param name="timeoutNs">Requested timeout in nanoseconds</param>
        /// <returns>Adjusted timepoint</returns>
        long AdjustTimePoint(long timePoint, long timeoutNs);

        /// <summary>
        /// Sleep until a timepoint, or a signal is received.
        /// Given no signal, may wake considerably before, or slightly after the timeout.
        /// </summary>
        /// <param name="timePoint">Timepoint to sleep until</param>
        /// <returns>True if signalled or waited, false if a wait could not be performed</returns>
        bool SleepUntil(long timePoint);

        /// <summary>
        /// Sleep until a signal is received.
        /// </summary>
        void Sleep();

        /// <summary>
        /// Signal the event, waking any sleeping thread or the next attempted sleep.
        /// </summary>
        void Signal();
    }
}
