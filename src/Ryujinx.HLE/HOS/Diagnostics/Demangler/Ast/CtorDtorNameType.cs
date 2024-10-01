using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class CtorDtorNameType : ParentNode
    {
        private readonly bool _isDestructor;

        public CtorDtorNameType(BaseNode name, bool isDestructor) : base(NodeType.CtorDtorNameType, name)
        {
            _isDestructor = isDestructor;
        }

        public override void PrintLeft(TextWriter writer)
        {
            if (_isDestructor)
            {
                writer.Write("~");
            }

            writer.Write(Child.GetName());
        }
    }
}
