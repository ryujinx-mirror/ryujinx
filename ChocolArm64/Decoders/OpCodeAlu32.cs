using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeAlu32 : OpCode32, IOpCodeAlu32
    {
        public int Rd { get; private set; }
        public int Rn { get; private set; }

        public bool SetFlags { get; private set; }

        public OpCodeAlu32(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rd = (opCode >> 12) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            SetFlags = ((opCode >> 20) & 1) != 0;
        }
    }
}