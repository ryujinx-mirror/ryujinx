using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ArrayType : BaseNode
    {
        private readonly BaseNode _base;
        private readonly BaseNode _dimensionExpression;
        private readonly string _dimensionString;

        public ArrayType(BaseNode Base, BaseNode dimensionExpression = null) : base(NodeType.ArrayType)
        {
            _base = Base;
            _dimensionExpression = dimensionExpression;
        }

        public ArrayType(BaseNode Base, string dimensionString) : base(NodeType.ArrayType)
        {
            _base = Base;
            _dimensionString = dimensionString;
        }

        public override bool HasRightPart()
        {
            return true;
        }

        public override bool IsArray()
        {
            return true;
        }

        public override void PrintLeft(TextWriter writer)
        {
            _base.PrintLeft(writer);
        }

        public override void PrintRight(TextWriter writer)
        {
            // FIXME: detect if previous char was a ].
            writer.Write(" ");

            writer.Write("[");

            if (_dimensionString != null)
            {
                writer.Write(_dimensionString);
            }
            else
            {
                _dimensionExpression?.Print(writer);
            }

            writer.Write("]");

            _base.PrintRight(writer);
        }
    }
}
