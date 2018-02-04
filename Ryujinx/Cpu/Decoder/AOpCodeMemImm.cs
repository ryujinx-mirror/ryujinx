using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeMemImm : AOpCodeMem
    {
        public    long Imm      { get; protected set; }
        public    bool WBack    { get; protected set; }
        public    bool PostIdx  { get; protected set; }
        protected bool Unscaled { get; private   set; }

        private enum MemOp
        {
            Unscaled     = 0,
            PostIndexed  = 1,
            Unprivileged = 2,
            PreIndexed   = 3,
            Unsigned
        }

        public AOpCodeMemImm(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Extend64 = ((OpCode >> 22) & 3) == 2;
            WBack    = ((OpCode >> 24) & 1) == 0;

            //The type is not valid for the Unsigned Immediate 12-bits encoding,
            //because the bits 11:10 are used for the larger Immediate offset.
            MemOp Type = WBack ? (MemOp)((OpCode >> 10) & 3) : MemOp.Unsigned;

            PostIdx  = Type == MemOp.PostIndexed;
            Unscaled = Type == MemOp.Unscaled ||
                       Type == MemOp.Unprivileged;

            //Unscaled and Unprivileged doesn't write back,
            //but they do use the 9-bits Signed Immediate.
            if (Unscaled)
            {
                WBack = false;
            }

            if (WBack || Unscaled)
            {
                //9-bits Signed Immediate.
                Imm = (OpCode << 43) >> 55;
            }
            else
            {
                //12-bits Unsigned Immediate.
                Imm = ((OpCode >> 10) & 0xfff) << Size;
            }
        }
    }
}