using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeMem64 : OpCode64
    {
        public int  Rt       { get; protected set; }
        public int  Rn       { get; protected set; }
        public int  Size     { get; protected set; }
        public bool Extend64 { get; protected set; }

        public OpCodeMem64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rt   = (opCode >>  0) & 0x1f;
            Rn   = (opCode >>  5) & 0x1f;
            Size = (opCode >> 30) & 0x3;
        }
    }
}