using ARMeilleure.Instructions;
using ARMeilleure.State;
using System;
using System.Numerics;

namespace ARMeilleure.Decoders
{
    class OpCodeT16MemStack : OpCodeT16, IOpCode32MemMult
    {
        public int Rn => RegisterAlias.Aarch32Sp;
        public int RegisterMask { get; }
        public int PostOffset { get; }
        public bool IsLoad { get; }
        public int Offset { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16MemStack(inst, address, opCode);

        public OpCodeT16MemStack(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int extra = (opCode >> 8) & 1;
            int regCount = BitOperations.PopCount((uint)opCode & 0x1ff);

            switch (inst.Name)
            {
                case InstName.Push:
                    RegisterMask = (opCode & 0xff) | (extra << 14);
                    IsLoad = false;
                    Offset = -4 * regCount;
                    PostOffset = -4 * regCount;
                    break;
                case InstName.Pop:
                    RegisterMask = (opCode & 0xff) | (extra << 15);
                    IsLoad = true;
                    Offset = 0;
                    PostOffset = 4 * regCount;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
