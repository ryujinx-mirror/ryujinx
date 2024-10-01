using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class NameType : BaseNode
    {
        private readonly string _nameValue;

        public NameType(string nameValue, NodeType type) : base(type)
        {
            _nameValue = nameValue;
        }

        public NameType(string nameValue) : base(NodeType.NameType)
        {
            _nameValue = nameValue;
        }

        public override string GetName()
        {
            return _nameValue;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write(_nameValue);
        }
    }
}
