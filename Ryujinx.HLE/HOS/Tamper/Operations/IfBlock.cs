using Ryujinx.HLE.HOS.Tamper.Conditions;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Tamper.Operations
{
    class IfBlock : IOperation
    {
        private ICondition _condition;
        private IEnumerable<IOperation> _operations;

        public IfBlock(ICondition condition, IEnumerable<IOperation> operations)
        {
            _condition = condition;
            _operations = operations;
        }

        public IfBlock(ICondition condition, params IOperation[] operations)
        {
            _operations = operations;
        }

        public void Execute()
        {
            if (!_condition.Evaluate())
            {
                return;
            }

            foreach (IOperation op in _operations)
            {
                op.Execute();
            }
        }
    }
}
