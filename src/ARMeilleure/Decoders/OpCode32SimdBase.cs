using System;

namespace ARMeilleure.Decoders
{
    abstract class OpCode32SimdBase : OpCode32, IOpCode32Simd
    {
        public int Vd { get; protected set; }
        public int Vm { get; protected set; }
        public int Size { get; protected set; }

        // Helpers to index doublewords within quad words. Essentially, looping over the vector starts at quadword Q and index Fx or Ix within it,
        // depending on instruction type.
        //
        // Qx: The quadword register that the target vector is contained in.
        // Ix: The starting index of the target vector within the quadword, with size treated as integer.
        // Fx: The starting index of the target vector within the quadword, with size treated as floating point. (16 or 32)
        public int Qd => GetQuadwordIndex(Vd);
        public int Id => GetQuadwordSubindex(Vd) << (3 - Size);
        public int Fd => GetQuadwordSubindex(Vd) << (1 - (Size & 1)); // When the top bit is truncated, 1 is fp16 which is an optional extension in ARMv8.2. We always assume 64.

        public int Qm => GetQuadwordIndex(Vm);
        public int Im => GetQuadwordSubindex(Vm) << (3 - Size);
        public int Fm => GetQuadwordSubindex(Vm) << (1 - (Size & 1));

        protected int GetQuadwordIndex(int index)
        {
            return RegisterSize switch
            {
                RegisterSize.Simd128 or RegisterSize.Simd64 => index >> 1,
                _ => throw new InvalidOperationException(),
            };
        }

        protected int GetQuadwordSubindex(int index)
        {
            return RegisterSize switch
            {
                RegisterSize.Simd128 => 0,
                RegisterSize.Simd64 => index & 1,
                _ => throw new InvalidOperationException(),
            };
        }

        protected OpCode32SimdBase(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode)
        {
            IsThumb = isThumb;
        }
    }
}
