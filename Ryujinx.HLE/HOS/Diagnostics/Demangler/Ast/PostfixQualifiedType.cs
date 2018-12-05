using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PostfixQualifiedType : ParentNode
    {
        private string PostfixQualifier;

        public PostfixQualifiedType(string PostfixQualifier, BaseNode Type) : base(NodeType.PostfixQualifiedType, Type)
        {
            this.PostfixQualifier = PostfixQualifier;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Child.Print(Writer);
            Writer.Write(PostfixQualifier);
        }
    }
}