using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Tamper.Operations
{
    class OpLog<T> : IOperation where T : unmanaged
    {
        readonly int _logId;
        readonly IOperand _source;

        public OpLog(int logId, IOperand source)
        {
            _logId = logId;
            _source = source;
        }

        public void Execute()
        {
            Logger.Debug?.Print(LogClass.TamperMachine, $"Tamper debug log id={_logId} value={(dynamic)_source.Get<T>():X}");
        }
    }
}
