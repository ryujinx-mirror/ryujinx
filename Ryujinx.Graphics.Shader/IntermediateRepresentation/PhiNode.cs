using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class PhiNode : INode
    {
        private Operand _dest;

        public Operand Dest
        {
            get => _dest;
            set => _dest = AssignDest(value);
        }

        public int DestsCount => _dest != null ? 1 : 0;

        private HashSet<BasicBlock> _blocks;

        private class PhiSource
        {
            public BasicBlock Block   { get; }
            public Operand    Operand { get; set; }

            public PhiSource(BasicBlock block, Operand operand)
            {
                Block   = block;
                Operand = operand;
            }
        }

        private List<PhiSource> _sources;

        public int SourcesCount => _sources.Count;

        public PhiNode(Operand dest)
        {
            _blocks = new HashSet<BasicBlock>();

            _sources = new List<PhiSource>();

            dest.AsgOp = this;

            Dest = dest;
        }

        private Operand AssignDest(Operand dest)
        {
            if (dest != null && dest.Type == OperandType.LocalVariable)
            {
                dest.AsgOp = this;
            }

            return dest;
        }

        public void AddSource(BasicBlock block, Operand operand)
        {
            if (_blocks.Add(block))
            {
                if (operand.Type == OperandType.LocalVariable)
                {
                    operand.UseOps.Add(this);
                }

                _sources.Add(new PhiSource(block, operand));
            }
        }

        public Operand GetDest(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _dest;
        }

        public Operand GetSource(int index)
        {
            return _sources[index].Operand;
        }

        public BasicBlock GetBlock(int index)
        {
            return _sources[index].Block;
        }

        public void SetSource(int index, Operand source)
        {
            Operand oldSrc = _sources[index].Operand;

            if (oldSrc != null && oldSrc.Type == OperandType.LocalVariable)
            {
                oldSrc.UseOps.Remove(this);
            }

            if (source.Type == OperandType.LocalVariable)
            {
                source.UseOps.Add(this);
            }

            _sources[index].Operand = source;
        }
    }
}