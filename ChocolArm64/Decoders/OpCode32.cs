using ChocolArm64.Instructions;
using ChocolArm64.State;

namespace ChocolArm64.Decoders
{
    class OpCode32 : OpCode64
    {
        public Condition Cond { get; protected set; }

        public OpCode32(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            RegisterSize = RegisterSize.Int32;

            Cond = (Condition)((uint)opCode >> 28);
        }

        public uint GetPc()
        {
            //Due to backwards compatibility and legacy behavior of ARMv4 CPUs pipeline,
            //the PC actually points 2 instructions ahead.
            return (uint)Position + (uint)OpCodeSizeInBytes * 2;
        }
    }
}