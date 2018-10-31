using ChocolArm64.Instructions;
using ChocolArm64.State;

namespace ChocolArm64.Decoders
{
    class OpCodeAlu64 : OpCode64, IOpCodeAlu64
    {
        public int Rd { get; protected set; }
        public int Rn { get; private   set; }

        public DataOp DataOp { get; private set; }

        public OpCodeAlu64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rd     =           (opCode >>  0) & 0x1f;
            Rn     =           (opCode >>  5) & 0x1f;
            DataOp = (DataOp)((opCode >> 24) & 0x3);

            RegisterSize = (opCode >> 31) != 0
                ? State.RegisterSize.Int64
                : State.RegisterSize.Int32;
        }
    }
}