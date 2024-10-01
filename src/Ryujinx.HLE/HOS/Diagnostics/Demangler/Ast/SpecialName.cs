using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class SpecialName : ParentNode
    {
        private readonly string _specialValue;

        public SpecialName(string specialValue, BaseNode type) : base(NodeType.SpecialName, type)
        {
            _specialValue = specialValue;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write(_specialValue);
            Child.Print(writer);
        }
    }
}
