using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class CastExpression : BaseNode
    {
        private readonly string _kind;
        private readonly BaseNode _to;
        private readonly BaseNode _from;

        public CastExpression(string kind, BaseNode to, BaseNode from) : base(NodeType.CastExpression)
        {
            _kind = kind;
            _to = to;
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
