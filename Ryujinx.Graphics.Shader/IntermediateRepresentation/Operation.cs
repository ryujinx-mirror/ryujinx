using System;
using System.Diagnostics;

namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class Operation : INode
    {
        public Instruction Inst { get; private set; }

        private Operand[] _dests;

        public Operand Dest
        {
            get
            {
                return _dests.Length != 0 ? _dests[0] : null;
            }
            set
            {
                if (value != null && value.Type == OperandType.LocalVariable)
                {
                    value.AsgOp = this;
                }

                if (value != null)
                {
                    _dests = new[] { value };
                }
                else
                {
                    _dests = Array.Empty<Operand>();
                }
            }
        }

        public int DestsCount => _dests.Length;

        private Operand[] _sources;

        public int SourcesCount => _sources.Length;

        public int Index { get; }

        private Operation(Operand[] sources)
        {
            // The array may be modified externally, so we store a copy.
            _sources = (Operand[])sources.Clone();

            for (int index = 0; index < _sources.Length; index++)
            {
                Operand source = _sources[index];

                if (source.Type == OperandType.LocalVariable)
                {
                    source.UseOps.Add(this);
                }
            }
        }

        public Operation(Instruction inst, int index, Operand[] dests, Operand[] sources) : this(sources)
        {
            Inst  = inst;
            Index = index;

            // The array may be modified externally, so we store a copy.
            _dests = (Operand[])dests.Clone();

            for (int dstIndex = 0; dstIndex < dests.Length; dstIndex++)
            {
                Operand dest = dests[dstIndex];

                if (dest != null && dest.Type == OperandType.LocalVariable)
                {
                    dest.AsgOp = this;
                }
            }
        }

        public Operation(Instruction inst, Operand dest, params Operand[] sources) : this(sources)
        {
            Inst = inst;

            if (dest != null)
            {
                dest.AsgOp = this;

                _dests = new[] { dest };
            }
            else
            {
                _dests = Array.Empty<Operand>();
            }
        }

        public Operation(Instruction inst, int index, Operand dest, params Operand[] sources) : this(inst, dest, sources)
        {
            Index = index;
        }

        public void AppendDests(Operand[] operands)
        {
            int startIndex = _dests.Length;

            Array.Resize(ref _dests, startIndex + operands.Length);

            for (int index = 0; index < operands.Length; index++)
            {
                Operand dest = operands[index];

                if (dest != null && dest.Type == OperandType.LocalVariable)
                {
                    Debug.Assert(dest.AsgOp == null);
                    dest.AsgOp = this;
                }

                _dests[startIndex + index] = dest;
            }
        }

        public void AppendSources(Operand[] operands)
        {
            int startIndex = _sources.Length;

            Array.Resize(ref _sources, startIndex + operands.Length);

            for (int index = 0; index < operands.Length; index++)
            {
                Operand source = operands[index];

                if (source.Type == OperandType.LocalVariable)
                {
                    source.UseOps.Add(this);
                }

                _sources[startIndex + index] = source;
            }
        }

        public Operand GetDest(int index)
        {
            return _dests[index];
        }

        public Operand GetSource(int index)
        {
            return _sources[index];
        }

        public void SetDest(int index, Operand dest)
        {
            Operand oldDest = _dests[index];

            if (oldDest != null && oldDest.Type == OperandType.LocalVariable)
            {
                oldDest.AsgOp = null;
            }

            if (dest != null && dest.Type == OperandType.LocalVariable)
            {
                dest.AsgOp = this;
            }

            _dests[index] = dest;
        }

        public void SetSource(int index, Operand source)
        {
            Operand oldSrc = _sources[index];

            if (oldSrc != null && oldSrc.Type == OperandType.LocalVariable)
            {
                oldSrc.UseOps.Remove(this);
            }

            if (source != null && source.Type == OperandType.LocalVariable)
            {
                source.UseOps.Add(this);
            }

            _sources[index] = source;
        }

        protected void RemoveSource(int index)
        {
            SetSource(index, null);

            Operand[] newSources = new Operand[_sources.Length - 1];

            Array.Copy(_sources, 0, newSources, 0, index);
            Array.Copy(_sources, index + 1, newSources, index, _sources.Length - (index + 1));

            _sources = newSources;
        }

        public void TurnIntoCopy(Operand source)
        {
            TurnInto(Instruction.Copy, source);
        }

        public void TurnInto(Instruction newInst, Operand source)
        {
            Inst = newInst;

            foreach (Operand oldSrc in _sources)
            {
                if (oldSrc != null && oldSrc.Type == OperandType.LocalVariable)
                {
                    oldSrc.UseOps.Remove(this);
                }
            }

            if (source.Type == OperandType.LocalVariable)
            {
                source.UseOps.Add(this);
            }

            _sources = new Operand[] { source };
        }
    }
}