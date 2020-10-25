using System;

namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class Operation : INode
    {
        public Instruction Inst { get; private set; }

        private Operand _dest;

        public Operand Dest
        {
            get => _dest;
            set => _dest = AssignDest(value);
        }

        private Operand[] _sources;

        public int SourcesCount => _sources.Length;

        public int Index { get; }

        public Operation(Instruction inst, Operand dest, params Operand[] sources)
        {
            Inst = inst;
            Dest = dest;

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

        public Operation(
            Instruction      inst,
            int              index,
            Operand          dest,
            params Operand[] sources) : this(inst, dest, sources)
        {
            Index = index;
        }

        public void AppendOperands(params Operand[] operands)
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

        private Operand AssignDest(Operand dest)
        {
            if (dest != null && dest.Type == OperandType.LocalVariable)
            {
                dest.AsgOp = this;
            }

            return dest;
        }

        public Operand GetSource(int index)
        {
            return _sources[index];
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