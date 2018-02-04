using ChocolArm64.Instruction;
using ChocolArm64.State;
using System;

namespace ChocolArm64.Decoder
{
    class AOpCode : IAOpCode
    {
        public long Position { get; private set; }

        public AInstEmitter  Emitter      { get; protected set; }
        public ARegisterSize RegisterSize { get; protected set; }

        public AOpCode(AInst Inst, long Position)
        {
            this.Position = Position;

            RegisterSize = ARegisterSize.Int64;

            Emitter = Inst.Emitter;
        }

        public int GetBitsCount()
        {
            switch (RegisterSize)
            {
                case ARegisterSize.Int32:   return 32;
                case ARegisterSize.Int64:   return 64;
                case ARegisterSize.SIMD64:  return 64;
                case ARegisterSize.SIMD128: return 128;
            }

            throw new InvalidOperationException();
        }
    }
}