using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ArraySubscriptingExpression : BaseNode
    {
        private BaseNode LeftNode;
        private BaseNode Subscript;

        public ArraySubscriptingExpression(BaseNode LeftNode, BaseNode Subscript) : base(NodeType.ArraySubscriptingExpression)
        {
            this.LeftNode  = LeftNode;
            this.Subscript = Subscript;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("(");
            LeftNode.Print(Writer);
            Writer.Write(")[");
            Subscript.Print(Writer);
            Writer.Write("]");            
        }
    }
}