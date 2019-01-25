using ChocolArm64.Instructions;
using ChocolArm64.State;
using System;

namespace ChocolArm64.Decoders
{
    class OpCode64 : IOpCode64
    {
        public long Position  { get; private set; }
        public int  RawOpCode { get; private set; }

        public int OpCodeSizeInBytes { get; protected set; } = 4;

        public InstEmitter  Emitter      { get; protected set; }
        public RegisterSize RegisterSize { get; protected set; }

        public OpCode64(Inst inst, long position, int opCode)
        {
            Position  = position;
            RawOpCode = opCode;

            RegisterSize = RegisterSize.Int64;

            Emitter = inst.Emitter;
        }

        public int GetBitsCount()
        {
            switch (RegisterSize)
            {
                case RegisterSize.Int32:   return 32;
                case RegisterSize.Int64:   return 64;
                case RegisterSize.Simd64:  return 64;
                case RegisterSize.Simd128: return 128;
            }

            throw new InvalidOperationException();
        }
    }
}