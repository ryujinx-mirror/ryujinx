using System.Collections.Generic;

namespace ChocolArm64.Translation
{
    class ILBlock : IILEmit
    {
        public  long IntInputs  { get; private set; }
        public  long IntOutputs { get; private set; }
        private long _intAwOutputs;

        public  long VecInputs  { get; private set; }
        public  long VecOutputs { get; private set; }
        private long _vecAwOutputs;

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
                _intAwOutputs = IntOutputs;
                _vecAwOutputs = VecOutputs;
            }
            else if (emitter is ILOpCodeLoad ld && ILMethodBuilder.IsRegIndex(ld.Index))
            {
                switch (ld.VarType)
                {
                    case VarType.Flag:   IntInputs |= ((1L << ld.Index) << 32) & ~_intAwOutputs; break;
                    case VarType.Int:    IntInputs |=  (1L << ld.Index)        & ~_intAwOutputs; break;
                    case VarType.Vector: VecInputs |=  (1L << ld.Index)        & ~_vecAwOutputs; break;
                }
            }
            else if (emitter is ILOpCodeStore st && ILMethodBuilder.IsRegIndex(st.Index))
            {
                switch (st.VarType)
                {
                    case VarType.Flag:   IntOutputs |= (1L << st.Index) << 32; break;
                    case VarType.Int:    IntOutputs |=  1L << st.Index;        break;
                    case VarType.Vector: VecOutputs |=  1L << st.Index;        break;
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