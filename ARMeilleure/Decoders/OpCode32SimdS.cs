namespace ARMeilleure.Decoders
{
    class OpCode32SimdS : OpCode32, IOpCode32Simd
    {
        public int Vd { get; protected set; }
        public int Vm { get; protected set; }
        public int Opc { get; protected set; } // "with_zero" (Opc<1>) [Vcmp, Vcmpe].
        public int Opc2 { get; } // opc2 or RM (opc2<1:0>) [Vcvt, Vrint].
        public int Size { get; protected set; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdS(inst, address, opCode);

        public OpCode32SimdS(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Opc = (opCode >> 15) & 0x3;
            Opc2 = (opCode >> 16) & 0x7;

            Size = (opCode >> 8) & 0x3;

            bool single = Size != 3;

            RegisterSize = single ? RegisterSize.Int32 : RegisterSize.Int64;

            if (single)
            {
                Vm = ((opCode >> 5) & 0x1) | ((opCode << 1) & 0x1e);
                Vd = ((opCode >> 22) & 0x1) | ((opCode >> 11) & 0x1e);
            }
            else
            {
                Vm = ((opCode >> 1) & 0x10) | ((opCode >> 0) & 0xf);
                Vd = ((opCode >> 18) & 0x10) | ((opCode >> 12) & 0xf);
            }
        }
    }
}
