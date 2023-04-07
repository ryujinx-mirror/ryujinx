namespace Ryujinx.HLE.HOS.Tamper.Operations
{
    class OpNot<T> : IOperation where T : unmanaged
    {
        IOperand _destination;
        IOperand _source;

        public OpNot(IOperand destination, IOperand source)
        {
            _destination = destination;
            _source = source;
        }

        public void Execute()
        {
            _destination.Set((T)(~(dynamic)_source.Get<T>()));
        }
    }
}
