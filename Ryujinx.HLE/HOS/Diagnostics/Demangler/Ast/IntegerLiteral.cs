using System.IO;
using System;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class IntegerLiteral : BaseNode
    {
        private string _literalName;
        private string _literalValue;

        public IntegerLiteral(string literalName, string literalValue) : base(NodeType.IntegerLiteral)
        {
            _literalValue = literalValue;
            _literalName  = literalName;
        }

        public override void PrintLeft(TextWriter writer)
        {
            if (_literalName.Length > 3)
            {
                writer.Write("(");
                writer.Write(_literalName);
                writer.Write(")");
            }

            if (_literalValue[0] == 'n')
            {
                writer.Write("-");
                writer.Write(_literalValue.AsSpan(1));
            }
            else
            {
                writer.Write(_literalValue);
            }

            if (_literalName.Length <= 3)
            {
                writer.Write(_literalName);
            }
        }
    }
}