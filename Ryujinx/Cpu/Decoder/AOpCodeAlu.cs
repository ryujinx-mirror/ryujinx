using ChocolArm64.Instruction;
using ChocolArm64.State;

namespace ChocolArm64.Decoder
{
    class AOpCodeAlu : AOpCode, IAOpCodeAlu
    {
        public int Rd { get; protected set; }
        public int Rn { get; private   set; }

        public ADataOp DataOp { get; private set; }

        public AOpCodeAlu(AInst Inst, long Position, int OpCode) : base(Inst, Position)
        {
            Rd     =           (OpCode >>  0) & 0x1f;
            Rn     =           (OpCode >>  5) & 0x1f;
            DataOp = (ADataOp)((OpCode >> 24) & 0x3);

            RegisterSize = (OpCode >> 31) != 0
                ? ARegisterSize.Int64
                : ARegisterSize.Int32;
        }
    }
}