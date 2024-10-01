using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class CtorVtableSpecialName : BaseNode
    {
        private readonly BaseNode _firstType;
        private readonly BaseNode _secondType;

        public CtorVtableSpecialName(BaseNode firstType, BaseNode secondType) : base(NodeType.CtorVtableSpecialName)
        {
            _firstType = firstType;
            _secondType = secondType;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write("construction vtable for ");
            _firstType.Print(writer);
            writer.Write("-in-");
            _secondType.Print(writer);
        }
    }
}
