using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class CastExpression : BaseNode
    {
        private string   _kind;
        private BaseNode _to;
        private BaseNode _from;

        public CastExpression(string kind, BaseNode to, BaseNode from) : base(NodeType.CastExpression)
        {
            _kind = kind;
            _to   = to;
            _from = from;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write(_kind);
            writer.Write("<");
            _to.PrintLeft(writer);
            writer.Write(">(");
            _from.PrintLeft(writer);
            writer.Write(")");
        }
    }
}