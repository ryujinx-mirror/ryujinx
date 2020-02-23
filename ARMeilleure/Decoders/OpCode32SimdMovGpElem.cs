namespace ARMeilleure.Decoders
{
    class OpCode32SimdMovGpElem : OpCode32, IOpCode32Simd
    {
        public int Size { get; private set; }

        public int Vd { get; private set; }
        public int Rt { get; private set; }
        public int Op { get; private set; }
        public bool U { get; private set; }

        public int Index { get; private set; }

        public OpCode32SimdMovGpElem(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Op = (opCode >> 20) & 0x1;
            U = ((opCode >> 23) & 1) != 0;

            var opc = (((opCode >> 23) & 1) << 4) | (((opCode >> 21) & 0x3) << 2) | ((opCode >> 5) & 0x3);

            if ((opc & 0b01000) == 0b01000)
            {
                Size = 0;
                Index = opc & 0x7;
            } 
            else if ((opc & 0b01001) == 0b00001)
            {
                Size = 1;
                Index = (opc >> 1) & 0x3;
            }
            else if ((opc & 0b11011) == 0)
            {
                Size = 2;
                Index = (opc >> 2) & 0x1;
            } 
            else
            {
                Instruction = InstDescriptor.Undefined;
                return;
            }

            Vd = ((opCode >> 3) & 0x10) | ((opCode >> 16) & 0xf);
            Rt = (opCode >> 12) & 0xf;
        }
    }
}
