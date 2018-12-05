using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class NameTypeWithTemplateArguments : BaseNode
    {
        private BaseNode Prev;
        private BaseNode TemplateArgument;

        public NameTypeWithTemplateArguments(BaseNode Prev, BaseNode TemplateArgument) : base(NodeType.NameTypeWithTemplateArguments)
        {
            this.Prev             = Prev;
            this.TemplateArgument = TemplateArgument;
        }

        public override string GetName()
        {
            return Prev.GetName();
        }
        
        public override void PrintLeft(TextWriter Writer)
        {
            Prev.Print(Writer);
            TemplateArgument.Print(Writer);
        }
    }
}