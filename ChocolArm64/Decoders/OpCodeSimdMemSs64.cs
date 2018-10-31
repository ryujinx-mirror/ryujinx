using ChocolArm64.Instructions;
using ChocolArm64.State;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdMemSs64 : OpCodeMemReg64, IOpCodeSimd64
    {
        public int  SElems    { get; private set; }
        public int  Index     { get; private set; }
        public bool Replicate { get; private set; }
        public bool WBack     { get; private set; }

        public OpCodeSimdMemSs64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            int size   = (opCode >> 10) & 3;
            int s      = (opCode >> 12) & 1;
            int sElems = (opCode >> 12) & 2;
            int scale  = (opCode >> 14) & 3;
            int l      = (opCode >> 22) & 1;
            int q      = (opCode >> 30) & 1;

            sElems |= (opCode >> 21) & 1;

            sElems++;

            int index = (q << 3) | (s << 2) | size;

            switch (scale)
            {
                case 1:
                {
                    if ((size & 1) != 0)
                    {
                        inst = Inst.Undefined;

                        return;
                    }

                    index >>= 1;

                    break;
                }

                case 2:
                {
                    if ((size & 2) != 0 ||
                       ((size & 1) != 0 && s != 0))
                    {
                        inst = Inst.Undefined;

                        return;
                    }

                    if ((size & 1) != 0)
                    {
                        index >>= 3;

                        scale = 3;
                    }
                    else
                    {
                        index >>= 2;
                    }

                    break;
                }

                case 3:
                {
                    if (l == 0 || s != 0)
                    {
                        inst = Inst.Undefined;

                        return;
                    }

                    scale = size;

                    Replicate = true;

                    break;
                }
            }

            Index  = index;
            SElems = sElems;
            Size   = scale;

            Extend64 = false;

            WBack = ((opCode >> 23) & 1) != 0;

            RegisterSize = q != 0
                ? State.RegisterSize.Simd128
                : State.RegisterSize.Simd64;
        }
    }
}