using System;

namespace Ryujinx.Input
{
    /// <summary>
    /// Represent features supported by a <see cref="IGamepad"/>.
    /// </summary>
    [Flags]
    public enum GamepadFeaturesFlag
    {
        /// <summary>
        /// No features are supported
        /// </summary>
        None,

        /// <summary>
        /// Rumble
        /// </summary>
        /// <remarks>Also named haptic</remarks>
        Rumble,

        /// <summary>
        /// Motion
        /// <remarks>Also named sixaxis</remarks>
        /// </summary>
        Motion,
    }
}
