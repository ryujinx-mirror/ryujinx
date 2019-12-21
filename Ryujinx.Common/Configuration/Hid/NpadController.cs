namespace Ryujinx.Common.Configuration.Hid
{
    public class NpadController
    {
        /// <summary>
        /// Enables or disables controller support
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// Controller Device Index
        /// </summary>
        public int Index;

        /// <summary>
        /// Controller Analog Stick Deadzone
        /// </summary>
        public float Deadzone;

        /// <summary>
        /// Controller Trigger Threshold
        /// </summary>
        public float TriggerThreshold;

        /// <summary>
        /// Left JoyCon Controller Bindings
        /// </summary>
        public NpadControllerLeft LeftJoycon;

        /// <summary>
        /// Right JoyCon Controller Bindings
        /// </summary>
        public NpadControllerRight RightJoycon;
    }
}
