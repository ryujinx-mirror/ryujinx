namespace Ryujinx.Input
{
    /// <summary>
    /// Represent a motion sensor on a gamepad.
    /// </summary>
    public enum MotionInputId : byte
    {
        /// <summary>
        /// Invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// Accelerometer.
        /// </summary>
        /// <remarks>Values are in m/s^2</remarks>
        Accelerometer,

        /// <summary>
        /// Gyroscope.
        /// </summary>
        /// <remarks>Values are in degrees</remarks>
        Gyroscope,
    }
}
