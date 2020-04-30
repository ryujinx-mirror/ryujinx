namespace Ryujinx.Common.Configuration.Hid
{
    public class NpadController
    {
        /// <summary>
        /// Enables or disables controller support
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Controller Device Index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Controller Analog Stick Deadzone
        /// </summary>
        public float Deadzone { get; set; }

        /// <summary>
        /// Controller Trigger Threshold
        /// </summary>
        public float TriggerThreshold { get; set; }

        /// <summary>
        /// Left JoyCon Controller Bindings
        /// </summary>
        public NpadControllerLeft LeftJoycon { get; set; }

        /// <summary>
        /// Right JoyCon Controller Bindings
        /// </summary>
        public NpadControllerRight RightJoycon { get; set; }
    }
}
