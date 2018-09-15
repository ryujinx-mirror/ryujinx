using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class NameType : BaseNode
    {
        private string NameValue;

        public NameType(string NameValue, NodeType Type) : base(Type)
        {
            this.NameValue = NameValue;
        }

        public NameType(string NameValue) : base(NodeType.NameType)
        {
            this.NameValue = NameValue;
        }

        public override string GetName()
        {
            return NameValue;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write(NameValue);
        }
    }
}