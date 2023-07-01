namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// Early reverb reflection.
    /// </summary>
    public enum ReverbEarlyMode : uint
    {
        /// <summary>
        /// Room early reflection. (small acoustic space, fast reflection)
        /// </summary>
        Room,

        /// <summary>
        /// Chamber early reflection. (bigger than <see cref="Room"/>'s acoustic space, short reflection)
        /// </summary>
        Chamber,

        /// <summary>
        /// Hall early reflection. (large acoustic space, warm reflection)
        /// </summary>
        Hall,

        /// <summary>
        /// Cathedral early reflection. (very large acoustic space, pronounced bright reflection)
        /// </summary>
        Cathedral,

        /// <summary>
        /// No early reflection.
        /// </summary>
        Disabled,
    }
}
