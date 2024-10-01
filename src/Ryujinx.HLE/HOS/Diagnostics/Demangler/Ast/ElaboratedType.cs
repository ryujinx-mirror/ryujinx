using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ElaboratedType : ParentNode
    {
        private readonly string _elaborated;

        public ElaboratedType(string elaborated, BaseNode type) : base(NodeType.ElaboratedType, type)
        {
            _elaborated = elaborated;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write(_elaborated);
            writer.Write(" ");
            Child.Print(writer);
        }
    }
}
