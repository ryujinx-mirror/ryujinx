using ARMeilleure.State;
using System;

namespace Ryujinx.Cpu
{
    /// <summary>
    /// Tick source interface.
    /// </summary>
    public interface ITickSource : ICounter
    {
        /// <summary>
        /// Time elapsed since the counter was created.
        /// </summary>
        TimeSpan ElapsedTime { get; }

        /// <summary>
        /// Time elapsed since the counter was created, in seconds.
        /// </summary>
        double ElapsedSeconds { get; }

        /// <summary>
        /// Stops counting.
        /// </summary>
        void Suspend();

        /// <summary>
        /// Resumes counting after a call to <see cref="Suspend"/>.
        /// </summary>
        void Resume();
    }
}
