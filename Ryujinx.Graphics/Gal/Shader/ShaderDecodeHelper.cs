namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderDecodeHelper
    {
        private static readonly ShaderIrOperImmf ImmfZero = new ShaderIrOperImmf(0);
        private static readonly ShaderIrOperImmf ImmfOne  = new ShaderIrOperImmf(1);

        public static ShaderIrNode GetAluFabsFneg(ShaderIrNode Node, bool Abs, bool Neg)
        {
            return GetAluFneg(GetAluFabs(Node, Abs), Neg);
        }

        public static ShaderIrNode GetAluFabs(ShaderIrNode Node, bool Abs)
        {
            return Abs ? new ShaderIrOp(ShaderIrInst.Fabs, Node) : Node;
        }

        public static ShaderIrNode GetAluFneg(ShaderIrNode Node, bool Neg)
        {
            return Neg ? new ShaderIrOp(ShaderIrInst.Fneg, Node) : Node;
        }

        public static ShaderIrNode GetAluFsat(ShaderIrNode Node, bool Sat)
        {
            return Sat ? new ShaderIrOp(ShaderIrInst.Fclamp, Node, ImmfZero, ImmfOne) : Node;
        }

        public static ShaderIrNode GetAluIabsIneg(ShaderIrNode Node, bool Abs, bool Neg)
        {
            return GetAluIneg(GetAluIabs(Node, Abs), Neg);
        }

        public static ShaderIrNode GetAluIabs(ShaderIrNode Node, bool Abs)
        {
            return Abs ? new ShaderIrOp(ShaderIrInst.Abs, Node) : Node;
        }

        public static ShaderIrNode GetAluIneg(ShaderIrNode Node, bool Neg)
        {
            return Neg ? new ShaderIrOp(ShaderIrInst.Neg, Node) : Node;
        }

        public static ShaderIrNode GetAluNot(ShaderIrNode Node, bool Not)
        {
            return Not ? new ShaderIrOp(ShaderIrInst.Not, Node) : Node;
        }

        public static ShaderIrNode ExtendTo32(ShaderIrNode Node, bool Signed, int Size)
        {
            int Shift = 32 - Size;

            ShaderIrInst RightShift = Signed
                ? ShaderIrInst.Asr
                : ShaderIrInst.Lsr;

            Node = new ShaderIrOp(ShaderIrInst.Lsl, Node, new ShaderIrOperImm(Shift));
            Node = new ShaderIrOp(RightShift,       Node, new ShaderIrOperImm(Shift));

            return Node;
        }

        public static ShaderIrNode ExtendTo32(ShaderIrNode Node, bool Signed, ShaderIrNode Size)
        {
            ShaderIrOperImm WordSize = new ShaderIrOperImm(32);

            ShaderIrOp Shift = new ShaderIrOp(ShaderIrInst.Sub, WordSize, Size);

            ShaderIrInst RightShift = Signed
                ? ShaderIrInst.Asr
                : ShaderIrInst.Lsr;

            Node = new ShaderIrOp(ShaderIrInst.Lsl, Node, Shift);
            Node = new ShaderIrOp(RightShift,       Node, Shift);

            return Node;
        }
    }
}