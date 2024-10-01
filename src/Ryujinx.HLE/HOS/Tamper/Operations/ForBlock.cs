using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Tamper.Operations
{
    class ForBlock : IOperation
    {
        private readonly ulong _count;
        private readonly Register _register;
        private readonly IEnumerable<IOperation> _operations;

        public ForBlock(ulong count, Register register, IEnumerable<IOperation> operations)
        {
            _count = count;
            _register = register;
            _operations = operations;
        }

        public ForBlock(ulong count, Register register, params IOperation[] operations)
        {
            _count = count;
            _register = register;
            _operations = operations;
        }

        public void Execute()
        {
            for (ulong i = 0; i < _count; i++)
            {
                // Set the register and execute the operations so that changing the
                // register during runtime does not break iteration.

                _register.Set<ulong>(i);

                foreach (IOperation op in _operations)
                {
                    op.Execute();
                }
            }
        }
    }
}
