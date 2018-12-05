using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class CtorDtorNameType : ParentNode
    {
        private bool IsDestructor;

        public CtorDtorNameType(BaseNode Name, bool IsDestructor) : base(NodeType.CtorDtorNameType, Name)
        {
            this.IsDestructor = IsDestructor;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            if (IsDestructor)
            {
                Writer.Write("~");
            }

            Writer.Write(Child.GetName());
        }
    }
}