using ChocolArm64.Decoders;
using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders32
{
    class A32OpCode : OpCode64
    {
        public Cond Cond { get; private set; }

        public A32OpCode(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Cond = (Cond)((uint)opCode >> 28);
        }
    }
}