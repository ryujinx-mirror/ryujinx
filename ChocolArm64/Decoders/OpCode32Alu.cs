using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCode32Alu : OpCode32, IOpCode32Alu
    {
        public int Rd { get; private set; }
        public int Rn { get; private set; }

        public bool SetFlags { get; private set; }

        public OpCode32Alu(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rd = (opCode >> 12) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            SetFlags = ((opCode >> 20) & 1) != 0;
        }
    }
}