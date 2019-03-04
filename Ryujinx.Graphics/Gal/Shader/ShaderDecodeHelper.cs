namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderDecodeHelper
    {
        private static readonly ShaderIrOperImmf ImmfZero = new ShaderIrOperImmf(0);
        private static readonly ShaderIrOperImmf ImmfOne  = new ShaderIrOperImmf(1);

        public static ShaderIrNode GetAluFabsFneg(ShaderIrNode node, bool abs, bool neg)
        {
            return GetAluFneg(GetAluFabs(node, abs), neg);
        }

        public static ShaderIrNode GetAluFabs(ShaderIrNode node, bool abs)
        {
            return abs ? new ShaderIrOp(ShaderIrInst.Fabs, node) : node;
        }

        public static ShaderIrNode GetAluFneg(ShaderIrNode node, bool neg)
        {
            return neg ? new ShaderIrOp(ShaderIrInst.Fneg, node) : node;
        }

        public static ShaderIrNode GetAluFsat(ShaderIrNode node, bool sat)
        {
            return sat ? new ShaderIrOp(ShaderIrInst.Fclamp, node, ImmfZero, ImmfOne) : node;
        }

        public static ShaderIrNode GetAluIabsIneg(ShaderIrNode node, bool abs, bool neg)
        {
            return GetAluIneg(GetAluIabs(node, abs), neg);
        }

        public static ShaderIrNode GetAluIabs(ShaderIrNode node, bool abs)
        {
            return abs ? new ShaderIrOp(ShaderIrInst.Abs, node) : node;
        }

        public static ShaderIrNode GetAluIneg(ShaderIrNode node, bool neg)
        {
            return neg ? new ShaderIrOp(ShaderIrInst.Neg, node) : node;
        }

        public static ShaderIrNode GetAluNot(ShaderIrNode node, bool not)
        {
            return not ? new ShaderIrOp(ShaderIrInst.Not, node) : node;
        }

        public static ShaderIrNode ExtendTo32(ShaderIrNode node, bool signed, int size)
        {
            int shift = 32 - size;

            ShaderIrInst rightShift = signed
                ? ShaderIrInst.Asr
                : ShaderIrInst.Lsr;

            node = new ShaderIrOp(ShaderIrInst.Lsl, node, new ShaderIrOperImm(shift));
            node = new ShaderIrOp(rightShift,       node, new ShaderIrOperImm(shift));

            return node;
        }

        public static ShaderIrNode ExtendTo32(ShaderIrNode node, bool signed, ShaderIrNode size)
        {
            ShaderIrOperImm wordSize = new ShaderIrOperImm(32);

            ShaderIrOp shift = new ShaderIrOp(ShaderIrInst.Sub, wordSize, size);

            ShaderIrInst rightShift = signed
                ? ShaderIrInst.Asr
                : ShaderIrInst.Lsr;

            node = new ShaderIrOp(ShaderIrInst.Lsl, node, shift);
            node = new ShaderIrOp(rightShift,       node, shift);

            return node;
        }
    }
}