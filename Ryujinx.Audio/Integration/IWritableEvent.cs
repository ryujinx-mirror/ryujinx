namespace Ryujinx.Audio.Integration
{
    /// <summary>
    /// Represent a writable event with manual clear.
    /// </summary>
    public interface IWritableEvent
    {
        /// <summary>
        /// Signal the event.
        /// </summary>
        void Signal();

        /// <summary>
        /// Clear the signaled state of the event.
        /// </summary>
        void Clear();
    }
}
