using System;
using System.Collections.Generic;

namespace ARMeilleure.IntermediateRepresentation
{
    class Node : IIntrusiveListNode<Node>
    {
        public Node ListPrevious { get; set; }
        public Node ListNext { get; set; }

        public Operand Destination
        {
            get => _destinations.Count != 0 ? GetDestination(0) : null;
            set => SetDestination(value);
        }

        private readonly List<Operand> _destinations;
        private readonly List<Operand> _sources;
        private bool _clearedDest;

        public int DestinationsCount => _destinations.Count;
        public int SourcesCount      => _sources.Count;

        private void Resize(List<Operand> list, int size)
        {
            if (list.Count > size)
            {
                list.RemoveRange(size, list.Count - size);
            } 
            else
            {
                while (list.Count < size)
                {
                    list.Add(null);
                }
            }
        }

        public Node()
        {
            _destinations = new List<Operand>();
            _sources = new List<Operand>();
        }

        public Node(Operand destination, int sourcesCount) : this()
        {
            Destination = destination;

            Resize(_sources, sourcesCount);
        }

        private void Reset(int sourcesCount)
        {
            _clearedDest = true;
            _sources.Clear();
            ListPrevious = null;
            ListNext = null;

            Resize(_sources, sourcesCount);
        }

        public Node With(Operand destination, int sourcesCount)
        {
            Reset(sourcesCount);
            Destination = destination;

            return this;
        }

        public Node With(Operand[] destinations, int sourcesCount)
        {
            Reset(sourcesCount);
            SetDestinations(destinations ?? throw new ArgumentNullException(nameof(destinations)));

            return this;
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
            if (!_clearedDest) 
            {
                RemoveAssignment(_destinations[index]);
            }

            AddAssignment(destination);

            _clearedDest = false;

            _destinations[index] = destination;
        }

        public void SetSource(int index, Operand source)
        {
            RemoveUse(_sources[index]);

            AddUse(source);

            _sources[index] = source;
        }

        private void RemoveOldDestinations()
        {
            if (!_clearedDest)
            {
                for (int index = 0; index < _destinations.Count; index++)
                {
                    RemoveAssignment(_destinations[index]);
                }
            }

            _clearedDest = false;
        }

        public void SetDestination(Operand destination)
        {
            RemoveOldDestinations();

            if (destination == null)
            {
                _destinations.Clear();
                _clearedDest = true;
            }
            else
            {
                Resize(_destinations, 1);

                _destinations[0] = destination;

                AddAssignment(destination);
            }
        }

        public void SetDestinations(Operand[] destinations)
        {
            RemoveOldDestinations();

            Resize(_destinations, destinations.Length);

            for (int index = 0; index < destinations.Length; index++)
            {
                Operand newOp = destinations[index];

                _destinations[index] = newOp;

                AddAssignment(newOp);
            }
        }

        private void RemoveOldSources()
        {
            for (int index = 0; index < _sources.Count; index++)
            {
                RemoveUse(_sources[index]);
            }
        }

        public void SetSource(Operand source)
        {
            RemoveOldSources();

            if (source == null)
            {
                _sources.Clear();
            }
            else
            {
                Resize(_sources, 1);

                _sources[0] = source;

                AddUse(source);
            }
        }

        public void SetSources(Operand[] sources)
        {
            RemoveOldSources();

            Resize(_sources, sources.Length);

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