namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// Late reverb reflection.
    /// </summary>
    public enum ReverbLateMode : uint
    {
        /// <summary>
        /// Room late reflection. (small acoustic space, fast reflection)
        /// </summary>
        Room,

        /// <summary>
        /// Hall late reflection. (large acoustic space, warm reflection)
        /// </summary>
        Hall,

        /// <summary>
        /// Classic plate late reflection. (clean distinctive reverb)
        /// </summary>
        Plate,

        /// <summary>
        /// Cathedral late reflection. (very large acoustic space, pronounced bright reflection)
        /// </summary>
        Cathedral,

        /// <summary>
        /// Do not apply any delay. (max delay)
        /// </summary>
        NoDelay,

        /// <summary>
        /// Max delay. (used for delay line limits)
        /// </summary>
        Limit = NoDelay,
    }
}
