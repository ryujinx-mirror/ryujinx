namespace ARMeilleure.Decoders
{
    class OpCode32MemMult : OpCode32, IOpCode32MemMult
    {
        public int Rn { get; }

        public int RegisterMask { get; }
        public int Offset       { get; }
        public int PostOffset   { get; }

        public bool IsLoad { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32MemMult(inst, address, opCode);

        public OpCode32MemMult(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rn = (opCode >> 16) & 0xf;

            bool isLoad = (opCode & (1 << 20)) != 0;
            bool w      = (opCode & (1 << 21)) != 0;
            bool u      = (opCode & (1 << 23)) != 0;
            bool p      = (opCode & (1 << 24)) != 0;

            RegisterMask = opCode & 0xffff;

            int regsSize = 0;

            for (int index = 0; index < 16; index++)
            {
                regsSize += (RegisterMask >> index) & 1;
            }

            regsSize *= 4;

            if (!u)
            {
                Offset -= regsSize;
            }

            if (u == p)
            {
                Offset += 4;
            }

            if (w)
            {
                PostOffset = u ? regsSize : -regsSize;
            }
            else
            {
                PostOffset = 0;
            }

            IsLoad = isLoad;
        }
    }
}