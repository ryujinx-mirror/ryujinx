using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ElaboratedType : ParentNode
    {
        private string Elaborated;

        public ElaboratedType(string Elaborated, BaseNode Type) : base(NodeType.ElaboratedType, Type)
        {
            this.Elaborated = Elaborated;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write(Elaborated);
            Writer.Write(" ");
            Child.Print(Writer);
        }
    }
}