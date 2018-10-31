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

        public List<IILEmit> IlEmitters { get; private set; }

        public ILBlock Next   { get; set; }
        public ILBlock Branch { get; set; }

        public ILBlock()
        {
            IlEmitters = new List<IILEmit>();
        }

        public void Add(IILEmit ilEmitter)
        {
            if (ilEmitter is ILBarrier)
            {
                //Those barriers are used to separate the groups of CIL
                //opcodes emitted by each ARM instruction.
                //We can only consider the new outputs for doing input elimination
                //after all the CIL opcodes used by the instruction being emitted.
                IntAwOutputs = IntOutputs;
                VecAwOutputs = VecOutputs;
            }
            else if (ilEmitter is IlOpCodeLoad ld && ILEmitter.IsRegIndex(ld.Index))
            {
                switch (ld.IoType)
                {
                    case IoType.Flag:   IntInputs |= ((1L << ld.Index) << 32) & ~IntAwOutputs; break;
                    case IoType.Int:    IntInputs |=  (1L << ld.Index)        & ~IntAwOutputs; break;
                    case IoType.Vector: VecInputs |=  (1L << ld.Index)        & ~VecAwOutputs; break;
                }
            }
            else if (ilEmitter is IlOpCodeStore st)
            {
                if (ILEmitter.IsRegIndex(st.Index))
                {
                    switch (st.IoType)
                    {
                        case IoType.Flag:   IntOutputs |= (1L << st.Index) << 32; break;
                        case IoType.Int:    IntOutputs |=  1L << st.Index;        break;
                        case IoType.Vector: VecOutputs |=  1L << st.Index;        break;
                    }
                }

                if (st.IoType == IoType.Fields)
                {
                    HasStateStore = true;
                }
            }

            IlEmitters.Add(ilEmitter);
        }

        public void Emit(ILEmitter context)
        {
            foreach (IILEmit ilEmitter in IlEmitters)
            {
                ilEmitter.Emit(context);
            }
        }
    }
}