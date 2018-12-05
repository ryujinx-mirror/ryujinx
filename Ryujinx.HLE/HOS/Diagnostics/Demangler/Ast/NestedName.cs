using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class NestedName : ParentNode
    {
        private BaseNode Name;

        public NestedName(BaseNode Name, BaseNode Type) : base(NodeType.NestedName, Type)
        {
            this.Name = Name;
        }

        public override string GetName()
        {
            return Name.GetName();
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Child.Print(Writer);
            Writer.Write("::");
            Name.Print(Writer);
        }
    }
}