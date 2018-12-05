using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class QualifiedName : BaseNode
    {
        private BaseNode Qualifier;
        private BaseNode Name;

        public QualifiedName(BaseNode Qualifier, BaseNode Name) : base(NodeType.QualifiedName)
        {
            this.Qualifier = Qualifier;
            this.Name      = Name;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Qualifier.Print(Writer);
            Writer.Write("::");
            Name.Print(Writer);
        }
    }
}