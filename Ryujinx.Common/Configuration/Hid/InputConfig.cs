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

        /// <summary>
        /// Motion Controller Slot
        /// </summary>
        public int Slot { get; set; }
        
        /// <summary>
        /// Motion Controller Alternative Slot, for RightJoyCon in Pair mode
        /// </summary>
        public int AltSlot { get; set; }

        /// <summary>
        /// Mirror motion input in Pair mode
        /// </summary>
        public bool MirrorInput { get; set; }

        /// <summary>
        /// Host address of the DSU Server
        /// </summary>
        public string DsuServerHost { get; set; }

        /// <summary>
        /// Port of the DSU Server
        /// </summary>
        public int DsuServerPort { get; set; }

        /// <summary>
        /// Gyro Sensitivity
        /// </summary>
        public int Sensitivity { get; set; }

        /// <summary>
        /// Gyro Deadzone
        /// </summary>
        public double GyroDeadzone { get; set; }
        
        /// <summary>
        /// Enable Motion Controls
        /// </summary>
        public bool EnableMotion { get; set; }
    }
}