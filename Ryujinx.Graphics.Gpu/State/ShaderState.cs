namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Graphics shader stage state.
    /// </summary>
    struct ShaderState
    {
#pragma warning disable CS0649
        public uint       Control;
        public uint       Offset;
        public uint       Unknown0x8;
        public int        MaxRegisters;
        public ShaderType Type;
        public uint       Unknown0x14;
        public uint       Unknown0x18;
        public uint       Unknown0x1c;
        public uint       Unknown0x20;
        public uint       Unknown0x24;
        public uint       Unknown0x28;
        public uint       Unknown0x2c;
        public uint       Unknown0x30;
        public uint       Unknown0x34;
        public uint       Unknown0x38;
        public uint       Unknown0x3c;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks shader enable information.
        /// Must be ignored for vertex shaders, those are always enabled.
        /// </summary>
        /// <returns>True if the stage is enabled, false otherwise</returns>
        public bool UnpackEnable()
        {
            return (Control & 1) != 0;
        }
    }
}
