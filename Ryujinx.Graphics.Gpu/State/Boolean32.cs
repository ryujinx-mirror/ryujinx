namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Boolean value, stored as a 32-bits integer in memory.
    /// </summary>
    struct Boolean32
    {
#pragma warning disable CS0649
        private uint _value;
#pragma warning restore CS0649

        public static implicit operator bool(Boolean32 value)
        {
            return (value._value & 1) != 0;
        }
    }
}
