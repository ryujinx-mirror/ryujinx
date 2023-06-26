namespace ARMeilleure.State
{
    /// <summary>
    /// CPU Counter interface.
    /// </summary>
    public interface ICounter
    {
        /// <summary>
        /// Counter frequency in Hertz.
        /// </summary>
        ulong Frequency { get; }

        /// <summary>
        /// Current counter value.
        /// </summary>
        ulong Counter { get; }
    }
}
