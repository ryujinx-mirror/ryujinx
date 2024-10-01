using Ryujinx.HLE.HOS.Tamper.Conditions;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Tamper.Operations
{
    class IfBlock : IOperation
    {
        private readonly ICondition _condition;
        private readonly IEnumerable<IOperation> _operationsThen;
        private readonly IEnumerable<IOperation> _operationsElse;

        public IfBlock(ICondition condition, IEnumerable<IOperation> operationsThen, IEnumerable<IOperation> operationsElse)
        {
            _condition = condition;
            _operationsThen = operationsThen;
            _operationsElse = operationsElse;
        }

        public void Execute()
        {
            IEnumerable<IOperation> operations = _condition.Evaluate() ? _operationsThen : _operationsElse;

            if (operations == null)
            {
                return;
            }

            foreach (IOperation op in operations)
            {
                op.Execute();
            }
        }
    }
}
