namespace Ryujinx.Graphics.Gpu.Engine.Types
{
    /// <summary>
    /// Boolean value, stored as a 32-bits integer in memory.
    /// </summary>
    readonly struct Boolean32
    {
        private readonly uint _value;

        public Boolean32(uint value)
        {
            _value = value;
        }

        public static implicit operator bool(Boolean32 value)
        {
            return (value._value & 1) != 0;
        }
    }
}
