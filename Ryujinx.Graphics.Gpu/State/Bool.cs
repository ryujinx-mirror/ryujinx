namespace Ryujinx.Graphics.Gpu.State
{
    struct Bool
    {
        private uint _value;

        public bool IsTrue()
        {
            return (_value & 1) != 0;
        }

        public bool IsFalse()
        {
            return (_value & 1) == 0;
        }
    }
}
