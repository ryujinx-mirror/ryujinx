using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class CtorVtableSpecialName : BaseNode
    {
        private BaseNode FirstType;
        private BaseNode SecondType;

        public CtorVtableSpecialName(BaseNode FirstType, BaseNode SecondType) : base(NodeType.CtorVtableSpecialName)
        {
            this.FirstType  = FirstType;
            this.SecondType = SecondType;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("construction vtable for ");
            FirstType.Print(Writer);
            Writer.Write("-in-");
            SecondType.Print(Writer);
        }
    }
}