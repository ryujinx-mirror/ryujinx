namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    enum OperandType
    {
        Argument,
        Attribute,
        AttributePerPatch,
        Constant,
        ConstantBuffer,
        Label,
        LocalVariable,
        Register,
        Undefined
    }

    static class OperandTypeExtensions
    {
        public static bool IsAttribute(this OperandType type)
        {
            return type == OperandType.Attribute || type == OperandType.AttributePerPatch;
        }
    }
}