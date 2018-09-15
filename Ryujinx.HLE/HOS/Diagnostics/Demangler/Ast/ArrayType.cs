using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ArrayType : BaseNode
    {
        private BaseNode Base;
        private BaseNode DimensionExpression;
        private string   DimensionString;

        public ArrayType(BaseNode Base, BaseNode DimensionExpression = null) : base(NodeType.ArrayType)
        {
            this.Base                = Base;
            this.DimensionExpression = DimensionExpression;
        }

        public ArrayType(BaseNode Base, string DimensionString) : base(NodeType.ArrayType)
        {
            this.Base            = Base;
            this.DimensionString = DimensionString;
        }

        public override bool HasRightPart()
        {
            return true;
        }

        public override bool IsArray()
        {
            return true;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Base.PrintLeft(Writer);
        }

        public override void PrintRight(TextWriter Writer)
        {
            // FIXME: detect if previous char was a ].
            Writer.Write(" ");

            Writer.Write("[");

            if (DimensionString != null)
            {
                Writer.Write(DimensionString);
            }
            else if (DimensionExpression != null)
            {
                DimensionExpression.Print(Writer);
            }

            Writer.Write("]");

            Base.PrintRight(Writer);
        }
    }
}