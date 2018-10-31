using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeMemEx64 : OpCodeMem64
    {
        public int Rt2 { get; private set; }
        public int Rs  { get; private set; }

        public OpCodeMemEx64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rt2 = (opCode >> 10) & 0x1f;
            Rs  = (opCode >> 16) & 0x1f;
        }
    }
}