using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class NameTypeWithTemplateArguments : BaseNode
    {
        private readonly BaseNode _prev;
        private readonly BaseNode _templateArgument;

        public NameTypeWithTemplateArguments(BaseNode prev, BaseNode templateArgument) : base(NodeType.NameTypeWithTemplateArguments)
        {
            _prev = prev;
            _templateArgument = templateArgument;
        }

        public override string GetName()
        {
            return _prev.GetName();
        }

        public override void PrintLeft(TextWriter writer)
        {
            _prev.Print(writer);
            _templateArgument.Print(writer);
        }
    }
}
