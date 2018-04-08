namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOp : ShaderIrNode
    {
        public ShaderIrInst Inst     { get; private set; }
        public ShaderIrNode OperandA { get; set; }
        public ShaderIrNode OperandB { get; set; }
        public ShaderIrNode OperandC { get; set; }

        public ShaderIrOp(
            ShaderIrInst Inst,
            ShaderIrNode OperandA = null,
            ShaderIrNode OperandB = null,
            ShaderIrNode OperandC = null)
        {
            this.Inst     = Inst;
            this.OperandA = OperandA;
            this.OperandB = OperandB;
            this.OperandC = OperandC;
        }
    }
}