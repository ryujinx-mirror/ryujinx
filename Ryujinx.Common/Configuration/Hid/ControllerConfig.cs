namespace Ryujinx.Common.Configuration.Hid
{
    public class ControllerConfig : InputConfig
    {
        /// <summary>
        /// Controller Left Analog Stick Deadzone
        /// </summary>
        public float DeadzoneLeft { get; set; }

        /// <summary>
        /// Controller Right Analog Stick Deadzone
        /// </summary>
        public float DeadzoneRight { get; set; }

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