namespace Ryujinx.Common.Configuration.Hid
{
    public class InputConfig
    {
        /// <summary>
        /// Controller Device Index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///  Controller's Type
        /// </summary>
        public ControllerType ControllerType { get; set; }

        /// <summary>
        ///  Player's Index for the controller
        /// </summary>
        public PlayerIndex PlayerIndex { get; set; }
    }
}