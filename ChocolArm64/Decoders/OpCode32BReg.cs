using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCode32BReg : OpCode32, IOpCode32BReg
    {
        public int Rm { get; private set; }

        public OpCode32BReg(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rm = opCode & 0xf;
        }
    }
}