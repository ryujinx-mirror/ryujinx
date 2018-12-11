using System.Collections.Generic;

namespace ChocolArm64.Translation
{
    class ILBlock : IILEmit
    {
        public long IntInputs    { get; private set; }
        public long IntOutputs   { get; private set; }
        public long IntAwOutputs { get; private set; }

        public long VecInputs    { get; private set; }
        public long VecOutputs   { get; private set; }
        public long VecAwOutputs { get; private set; }

        public bool HasStateStore { get; private set; }

        private List<IILEmit> _emitters;

        public int Count => _emitters.Count;

        public ILBlock Next   { get; set; }
        public ILBlock Branch { get; set; }

        public ILBlock()
        {
            _emitters = new List<IILEmit>();
        }

        public void Add(IILEmit emitter)
        {
            if (emitter is ILBarrier)
            {
                //Those barriers are used to separate the groups of CIL
                //opcodes emitted by each ARM instruction.
                //We can only consider the new outputs for doing input elimination
                //after all the CIL opcodes used by the instruction being emitted.
                IntAwOutputs = IntOutputs;
                VecAwOutputs = VecOutputs;
            }
            else if (emitter is ILOpCodeLoad ld && ILMethodBuilder.IsRegIndex(ld.Index))
            {
                switch (ld.IoType)
                {
                    case IoType.Flag:   IntInputs |= ((1L << ld.Index) << 32) & ~IntAwOutputs; break;
                    case IoType.Int:    IntInputs |=  (1L << ld.Index)        & ~IntAwOutputs; break;
                    case IoType.Vector: VecInputs |=  (1L << ld.Index)        & ~VecAwOutputs; break;
                }
            }
            else if (emitter is ILOpCodeStore st && ILMethodBuilder.IsRegIndex(st.Index))
            {
                switch (st.IoType)
                {
                    case IoType.Flag:   IntOutputs |= (1L << st.Index) << 32; break;
                    case IoType.Int:    IntOutputs |=  1L << st.Index;        break;
                    case IoType.Vector: VecOutputs |=  1L << st.Index;        break;
                }
            }
            else if (emitter is ILOpCodeStoreState)
            {
                HasStateStore = true;
            }

            _emitters.Add(emitter);
        }

        public void Emit(ILMethodBuilder context)
        {
            foreach (IILEmit ilEmitter in _emitters)
            {
                ilEmitter.Emit(context);
            }
        }
    }
}