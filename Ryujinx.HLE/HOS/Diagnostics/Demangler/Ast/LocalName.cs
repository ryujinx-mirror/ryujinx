using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class LocalName : BaseNode
    {
        private BaseNode Encoding;
        private BaseNode Entity;

        public LocalName(BaseNode Encoding, BaseNode Entity) : base(NodeType.LocalName)
        {
            this.Encoding = Encoding;
            this.Entity   = Entity;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Encoding.Print(Writer);
            Writer.Write("::");
            Entity.Print(Writer);
        }
    }
}