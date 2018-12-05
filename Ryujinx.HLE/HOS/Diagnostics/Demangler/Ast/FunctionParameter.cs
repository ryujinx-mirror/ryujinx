using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class FunctionParameter : BaseNode
    {
        private string Number;

        public FunctionParameter(string Number) : base(NodeType.FunctionParameter)
        {
            this.Number = Number;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("fp ");

            if (Number != null)
            {
                Writer.Write(Number);
            }
        }
    }
}