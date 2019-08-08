using System;
using System.Collections.Generic;

namespace ARMeilleure.IntermediateRepresentation
{
    class Node
    {
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

        private LinkedListNode<Node>[] _asgUseNodes;
        private LinkedListNode<Node>[] _srcUseNodes;

        public int DestinationsCount => _destinations.Length;
        public int SourcesCount      => _sources.Length;

        public Node(Operand destination, int sourcesCount)
        {
            Destination = destination;

            _sources = new Operand[sourcesCount];

            _srcUseNodes = new LinkedListNode<Node>[sourcesCount];
        }

        public Node(Operand[] destinations, int sourcesCount)
        {
            SetDestinations(destinations ?? throw new ArgumentNullException(nameof(destinations)));

            _sources = new Operand[sourcesCount];

            _srcUseNodes = new LinkedListNode<Node>[sourcesCount];
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
            Operand oldOp = _destinations[index];

            if (oldOp != null && oldOp.Kind == OperandKind.LocalVariable)
            {
                oldOp.Assignments.Remove(_asgUseNodes[index]);
            }

            if (destination != null && destination.Kind == OperandKind.LocalVariable)
            {
                _asgUseNodes[index] = destination.Assignments.AddLast(this);
            }

            _destinations[index] = destination;
        }

        public void SetSource(int index, Operand source)
        {
            Operand oldOp = _sources[index];

            if (oldOp != null && oldOp.Kind == OperandKind.LocalVariable)
            {
                oldOp.Uses.Remove(_srcUseNodes[index]);
            }

            if (source != null && source.Kind == OperandKind.LocalVariable)
            {
                _srcUseNodes[index] = source.Uses.AddLast(this);
            }

            _sources[index] = source;
        }

        public void SetDestinations(Operand[] destinations)
        {
            if (_destinations != null)
            {
                for (int index = 0; index < _destinations.Length; index++)
                {
                    Operand oldOp = _destinations[index];

                    if (oldOp != null && oldOp.Kind == OperandKind.LocalVariable)
                    {
                        oldOp.Assignments.Remove(_asgUseNodes[index]);
                    }
                }

                _destinations = destinations;
            }
            else
            {
                _destinations = new Operand[destinations.Length];
            }

            _asgUseNodes = new LinkedListNode<Node>[destinations.Length];

            for (int index = 0; index < destinations.Length; index++)
            {
                Operand newOp = destinations[index];

                _destinations[index] = newOp;

                if (newOp.Kind == OperandKind.LocalVariable)
                {
                    _asgUseNodes[index] = newOp.Assignments.AddLast(this);
                }
            }
        }

        public void SetSources(Operand[] sources)
        {
            for (int index = 0; index < _sources.Length; index++)
            {
                Operand oldOp = _sources[index];

                if (oldOp != null && oldOp.Kind == OperandKind.LocalVariable)
                {
                    oldOp.Uses.Remove(_srcUseNodes[index]);
                }
            }

            _sources = new Operand[sources.Length];

            _srcUseNodes = new LinkedListNode<Node>[sources.Length];

            for (int index = 0; index < sources.Length; index++)
            {
                Operand newOp = sources[index];

                _sources[index] = newOp;

                if (newOp.Kind == OperandKind.LocalVariable)
                {
                    _srcUseNodes[index] = newOp.Uses.AddLast(this);
                }
            }
        }
    }
}