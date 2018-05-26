using ChocolArm64.Decoder;
using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder32
{
    class A32OpCode : AOpCode
    {
        public ACond Cond { get; private set; }

        public A32OpCode(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Cond = (ACond)((uint)OpCode >> 28);
        }
    }
}