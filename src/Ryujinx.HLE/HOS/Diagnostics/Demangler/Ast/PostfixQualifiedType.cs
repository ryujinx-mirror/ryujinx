using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PostfixQualifiedType : ParentNode
    {
        private readonly string _postfixQualifier;

        public PostfixQualifiedType(string postfixQualifier, BaseNode type) : base(NodeType.PostfixQualifiedType, type)
        {
            _postfixQualifier = postfixQualifier;
        }

        public override void PrintLeft(TextWriter writer)
        {
            Child.Print(writer);
            writer.Write(_postfixQualifier);
        }
    }
}
