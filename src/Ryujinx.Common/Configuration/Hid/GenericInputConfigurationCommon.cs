namespace Ryujinx.Common.Configuration.Hid
{
    public class GenericInputConfigurationCommon<TButton> : InputConfig where TButton : unmanaged
    {
        /// <summary>
        /// Left JoyCon Controller Bindings
        /// </summary>
        public LeftJoyconCommonConfig<TButton> LeftJoycon { get; set; }

        /// <summary>
        /// Right JoyCon Controller Bindings
        /// </summary>
        public RightJoyconCommonConfig<TButton> RightJoycon { get; set; }
    }
}
