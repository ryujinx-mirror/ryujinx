namespace Ryujinx.Graphics.Gpu.State
{
    struct Boolean32
    {
        private uint _value;

        public static implicit operator bool(Boolean32 value)
        {
            return (value._value & 1) != 0;
        }
    }
}
