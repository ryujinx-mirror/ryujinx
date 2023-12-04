namespace Ryujinx.Common.Configuration.Hid.Controller
{
    public class RumbleConfigController
    {
        /// <summary>
        /// Controller Strong Rumble Multiplier
        /// </summary>
        public float StrongRumble { get; set; }

        /// <summary>
        /// Controller Weak Rumble Multiplier
        /// </summary>
        public float WeakRumble { get; set; }

        /// <summary>
        /// Enable Rumble
        /// </summary>
        public bool EnableRumble { get; set; }
    }
}
