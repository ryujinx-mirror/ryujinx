using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Tamper.Operations
{
    class Block : IOperation
    {
        private readonly IEnumerable<IOperation> _operations;

        public Block(IEnumerable<IOperation> operations)
        {
            _operations = operations;
        }

        public Block(params IOperation[] operations)
        {
            _operations = operations;
        }

        public void Execute()
        {
            foreach (IOperation op in _operations)
            {
                op.Execute();
            }
        }
    }
}
