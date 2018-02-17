using ChocolArm64.Instruction;
using ChocolArm64.State;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdMemSs : AOpCodeMemReg, IAOpCodeSimd
    {
        public int  SElems    { get; private set; }
        public int  Index     { get; private set; }
        public bool Replicate { get; private set; }
        public bool WBack     { get; private set; }

        public AOpCodeSimdMemSs(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            int Size   = (OpCode >> 10) & 3;
            int S      = (OpCode >> 12) & 1;
            int SElems = (OpCode >> 12) & 2;
            int Scale  = (OpCode >> 14) & 3;
            int L      = (OpCode >> 22) & 1;
            int Q      = (OpCode >> 30) & 1;
            
            SElems |= (OpCode >> 21) & 1;

            SElems++;

            int Index = (Q << 3) | (S << 2) | Size;

            switch (Scale)
            {
                case 0: Index >>= 0; break;

                case 1:
                {
                    if ((Index & 1) != 0)
                    {
                        Inst = AInst.Undefined;

                        return;
                    }

                    Index >>= 1;

                    break;
                }

                case 2:
                {
                    if ((Index & 2) != 0 ||
                       ((Index & 1) != 0 && S != 0))
                    {
                        Inst = AInst.Undefined;

                        return;
                    }

                    if ((Index & 1) != 0)
                    {
                        Index >>= 3;
                    }
                    else
                    {
                        Index >>= 2;

                        Scale = 3;
                    }

                    break;
                }

                case 3:
                {
                    if (L == 0 || S != 0)
                    {
                        Inst = AInst.Undefined;

                        return;
                    }

                    Scale = Size;

                    Replicate = true;

                    break;
                }
            }

            this.SElems = SElems;
            this.Size   = Scale;

            Extend64 = false;

            WBack = ((OpCode >> 23) & 0x1) != 0;

            RegisterSize = Q != 0
                ? ARegisterSize.SIMD128
                : ARegisterSize.SIMD64;
        }
    }
}