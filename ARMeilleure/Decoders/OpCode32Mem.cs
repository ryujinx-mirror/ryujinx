using ARMeilleure.Instructions;

namespace ARMeilleure.Decoders
{
    class OpCode32Mem : OpCode32, IOpCode32Mem
    {
        public int Rt { get; protected set; }
        public int Rn { get; private set; }

        public int Immediate { get; protected set; }

        public bool Index        { get; private set; }
        public bool Add          { get; private set; }
        public bool WBack        { get; private set; }
        public bool Unprivileged { get; private set; }

        public bool IsLoad { get; private set; }

        public OpCode32Mem(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rt = (opCode >> 12) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            bool isLoad = (opCode & (1 << 20)) != 0;
            bool w      = (opCode & (1 << 21)) != 0;
            bool u      = (opCode & (1 << 23)) != 0;
            bool p      = (opCode & (1 << 24)) != 0;

            Index        = p;
            Add          = u;
            WBack        = !p || w;
            Unprivileged = !p && w;

            IsLoad = isLoad || inst.Name == InstName.Ldrd;
        }
    }
}