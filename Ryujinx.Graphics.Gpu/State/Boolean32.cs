namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Boolean value, stored as a 32-bits integer in memory.
    /// </summary>
    struct Boolean32
    {
        private uint _value;

        public static implicit operator bool(Boolean32 value)
        {
            return (value._value & 1) != 0;
        }
    }
}
