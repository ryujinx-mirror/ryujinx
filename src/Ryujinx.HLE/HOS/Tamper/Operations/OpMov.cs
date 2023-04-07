namespace Ryujinx.HLE.HOS.Tamper.Operations
{
    class OpMov<T> : IOperation where T : unmanaged
    {
        IOperand _destination;
        IOperand _source;

        public OpMov(IOperand destination, IOperand source)
        {
            _destination = destination;
            _source = source;
        }

        public void Execute()
        {
            _destination.Set(_source.Get<T>());
        }
    }
}
