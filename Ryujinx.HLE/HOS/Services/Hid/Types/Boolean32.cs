namespace Ryujinx.HLE.HOS.Services.Hid
{
    struct Boolean32
    {
        private uint _value;
        public static implicit operator bool(Boolean32 value) => (value._value & 1) != 0;
        public static implicit operator Boolean32(bool value) => new Boolean32() { _value = value ? 1u : 0u };
    }
}