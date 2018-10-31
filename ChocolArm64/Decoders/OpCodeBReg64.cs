using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeBReg64 : OpCode64
    {
        public int Rn { get; private set; }

        public OpCodeBReg64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            int op4 = (opCode >>  0) & 0x1f;
            int op2 = (opCode >> 16) & 0x1f;

            if (op2 != 0b11111 || op4 != 0b00000)
            {
                Emitter = InstEmit.Und;

                return;
            }

            Rn = (opCode >> 5) & 0x1f;
        }
    }
}