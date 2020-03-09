using System;

namespace ARMeilleure.IntermediateRepresentation
{
    class Node : IIntrusiveListNode<Node>
    {
        public Node ListPrevious { get; set; }
        public Node ListNext { get; set; }

        public Operand Destination
        {
            get
            {
                return _destinations.Length != 0 ? GetDestination(0) : null;
            }
            set
            {
                if (value != null)
                {
                    SetDestinations(new Operand[] { value });
                }
                else
                {
                    SetDestinations(new Operand[0]);
                }
            }
        }

        private Operand[] _destinations;
        private Operand[] _sources;

        public int DestinationsCount => _destinations.Length;
        public int SourcesCount      => _sources.Length;

        public Node(Operand destination, int sourcesCount)
        {
            Destination = destination;

            _sources = new Operand[sourcesCount];
        }

        public Node(Operand[] destinations, int sourcesCount)
        {
            SetDestinations(destinations ?? throw new ArgumentNullException(nameof(destinations)));

            _sources = new Operand[sourcesCount];
        }

        public Operand GetDestination(int index)
        {
            return _destinations[index];
        }

        public Operand GetSource(int index)
        {
            return _sources[index];
        }

        public void SetDestination(int index, Operand destination)
        {
            RemoveAssignment(_destinations[index]);

            AddAssignment(destination);

            _destinations[index] = destination;
        }

        public void SetSource(int index, Operand source)
        {
            RemoveUse(_sources[index]);

            AddUse(source);

            _sources[index] = source;
        }

        public void SetDestinations(Operand[] destinations)
        {
            if (_destinations != null)
            {
                for (int index = 0; index < _destinations.Length; index++)
                {
                    RemoveAssignment(_destinations[index]);
                }

                _destinations = destinations;
            }
            else
            {
                _destinations = new Operand[destinations.Length];
            }

            for (int index = 0; index < destinations.Length; index++)
            {
                Operand newOp = destinations[index];

                _destinations[index] = newOp;

                AddAssignment(newOp);
            }
        }

        public void SetSources(Operand[] sources)
        {
            for (int index = 0; index < _sources.Length; index++)
            {
                RemoveUse(_sources[index]);
            }

            _sources = new Operand[sources.Length];

            for (int index = 0; index < sources.Length; index++)
            {
                Operand newOp = sources[index];

                _sources[index] = newOp;

                AddUse(newOp);
            }
        }

        private void AddAssignment(Operand op)
        {
            if (op == null)
            {
                return;
            }

            if (op.Kind == OperandKind.LocalVariable)
            {
                op.Assignments.Add(this);
            }
            else if (op.Kind == OperandKind.Memory)
            {
                MemoryOperand memOp = (MemoryOperand)op;

                if (memOp.BaseAddress != null)
                {
                    memOp.BaseAddress.Assignments.Add(this);
                }
                
                if (memOp.Index != null)
                {
                    memOp.Index.Assignments.Add(this);
                }
            }
        }

        private void RemoveAssignment(Operand op)
        {
            if (op == null)
            {
                return;
            }

            if (op.Kind == OperandKind.LocalVariable)
            {
                op.Assignments.Remove(this);
            }
            else if (op.Kind == OperandKind.Memory)
            {
                MemoryOperand memOp = (MemoryOperand)op;

                if (memOp.BaseAddress != null)
                {
                    memOp.BaseAddress.Assignments.Remove(this);
                }

                if (memOp.Index != null)
                {
                    memOp.Index.Assignments.Remove(this);
                }
            }
        }

        private void AddUse(Operand op)
        {
            if (op == null)
            {
                return;
            }

            if (op.Kind == OperandKind.LocalVariable)
            {
                op.Uses.Add(this);
            }
            else if (op.Kind == OperandKind.Memory)
            {
                MemoryOperand memOp = (MemoryOperand)op;

                if (memOp.BaseAddress != null)
                {
                    memOp.BaseAddress.Uses.Add(this);
                }

                if (memOp.Index != null)
                {
                    memOp.Index.Uses.Add(this);
                }
            }
        }

        private void RemoveUse(Operand op)
        {
            if (op == null)
            {
                return;
            }

            if (op.Kind == OperandKind.LocalVariable)
            {
                op.Uses.Remove(this);
            }
            else if (op.Kind == OperandKind.Memory)
            {
                MemoryOperand memOp = (MemoryOperand)op;

                if (memOp.BaseAddress != null)
                {
                    memOp.BaseAddress.Uses.Remove(this);
                }

                if (memOp.Index != null)
                {
                    memOp.Index.Uses.Remove(this);
                }
            }
        }
    }
}