namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOp : ShaderIrNode
    {
        public ShaderIrInst Inst     { get; private set; }
        public ShaderIrNode OperandA { get; set; }
        public ShaderIrNode OperandB { get; set; }
        public ShaderIrNode OperandC { get; set; }
        public ShaderIrMeta MetaData { get; set; }

        public ShaderIrOp(
            ShaderIrInst inst,
            ShaderIrNode operandA = null,
            ShaderIrNode operandB = null,
            ShaderIrNode operandC = null,
            ShaderIrMeta metaData = null)
        {
            Inst     = inst;
            OperandA = operandA;
            OperandB = operandB;
            OperandC = operandC;
            MetaData = metaData;
        }
    }
}