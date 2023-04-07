namespace Ryujinx.Common.Configuration.Hid
{
    public class GenericInputConfigurationCommon<Button> : InputConfig where Button : unmanaged
    {
        /// <summary>
        /// Left JoyCon Controller Bindings
        /// </summary>
        public LeftJoyconCommonConfig<Button> LeftJoycon { get; set; }

        /// <summary>
        /// Right JoyCon Controller Bindings
        /// </summary>
        public RightJoyconCommonConfig<Button> RightJoycon { get; set; }
    }
}
