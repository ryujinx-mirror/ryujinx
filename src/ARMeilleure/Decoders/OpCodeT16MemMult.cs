using ARMeilleure.Instructions;
using System;
using System.Numerics;

namespace ARMeilleure.Decoders
{
    class OpCodeT16MemMult : OpCodeT16, IOpCode32MemMult
    {
        public int Rn { get; }
        public int RegisterMask { get; }
        public int PostOffset { get; }
        public bool IsLoad { get; }
        public int Offset { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16MemMult(inst, address, opCode);

        public OpCodeT16MemMult(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            RegisterMask = opCode & 0xff;
            Rn = (opCode >> 8) & 7;

            int regCount = BitOperations.PopCount((uint)RegisterMask);

            Offset = 0;
            PostOffset = 4 * regCount;
            IsLoad = inst.Name switch
            {
                InstName.Ldm => true,
                InstName.Stm => false,
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
