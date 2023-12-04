namespace Ryujinx.Common.Configuration.Hid.Controller.Motion
{
    public class CemuHookMotionConfigController : MotionConfigController
    {
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
    }
}
