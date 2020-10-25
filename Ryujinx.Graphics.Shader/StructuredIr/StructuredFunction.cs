using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class StructuredFunction
    {
        public AstBlock MainBlock { get; }

        public string Name { get; }

        public VariableType ReturnType { get; }

        public VariableType[] InArguments { get; }
        public VariableType[] OutArguments { get; }

        public HashSet<AstOperand> Locals { get; }

        public StructuredFunction(
            AstBlock mainBlock,
            string name,
            VariableType returnType,
            VariableType[] inArguments,
            VariableType[] outArguments)
        {
            MainBlock = mainBlock;
            Name = name;
            ReturnType = returnType;
            InArguments = inArguments;
            OutArguments = outArguments;

            Locals = new HashSet<AstOperand>();
        }

        public VariableType GetArgumentType(int index)
        {
            return index >= InArguments.Length
                ? OutArguments[index - InArguments.Length]
                : InArguments[index];
        }
    }
}