using Ryujinx.HLE.HOS.Tamper.Operations;

namespace Ryujinx.HLE.HOS.Tamper
{
    class Value<TP> : IOperand where TP : unmanaged
    {
        private TP _value;

        public Value(TP value)
        {
            _value = value;
        }

        public T Get<T>() where T : unmanaged
        {
            return (T)(dynamic)_value;
        }

        public void Set<T>(T value) where T : unmanaged
        {
            _value = (TP)(dynamic)value;
        }
    }
}
