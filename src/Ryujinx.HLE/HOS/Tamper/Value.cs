using Ryujinx.HLE.HOS.Tamper.Operations;

namespace Ryujinx.HLE.HOS.Tamper
{
    class Value<P> : IOperand where P : unmanaged
    {
        private P _value;

        public Value(P value)
        {
            _value = value;
        }

        public T Get<T>() where T : unmanaged
        {
            return (T)(dynamic)_value;
        }

        public void Set<T>(T value) where T : unmanaged
        {
            _value = (P)(dynamic)value;
        }
    }
}
