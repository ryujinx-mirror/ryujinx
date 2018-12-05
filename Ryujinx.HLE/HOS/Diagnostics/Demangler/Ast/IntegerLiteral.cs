using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class IntegerLiteral : BaseNode
    {
        private string LitteralName;
        private string LitteralValue;

        public IntegerLiteral(string LitteralName, string LitteralValue) : base(NodeType.IntegerLiteral)
        {
            this.LitteralValue = LitteralValue;
            this.LitteralName  = LitteralName;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            if (LitteralName.Length > 3)
            {
                Writer.Write("(");
                Writer.Write(LitteralName);
                Writer.Write(")");
            }

            if (LitteralValue[0] == 'n')
            {
                Writer.Write("-");
                Writer.Write(LitteralValue.Substring(1));
            }
            else
            {
                Writer.Write(LitteralValue);
            }

            if (LitteralName.Length <= 3)
            {
                Writer.Write(LitteralName);
            }
        }
    }
}