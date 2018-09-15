using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class EncodedFunction : BaseNode
    {
        private BaseNode Name;
        private BaseNode Params;
        private BaseNode CV;
        private BaseNode Ref;
        private BaseNode Attrs;
        private BaseNode Ret;

        public EncodedFunction(BaseNode Name, BaseNode Params, BaseNode CV, BaseNode Ref, BaseNode Attrs, BaseNode Ret) : base(NodeType.NameType)
        {
            this.Name   = Name;
            this.Params = Params;
            this.CV     = CV;
            this.Ref    = Ref;
            this.Attrs  = Attrs;
            this.Ret    = Ret;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            if (Ret != null)
            {
                Ret.PrintLeft(Writer);

                if (!Ret.HasRightPart())
                {
                    Writer.Write(" ");
                }
            }

            Name.Print(Writer);

        }

        public override bool HasRightPart()
        {
            return true;
        }

        public override void PrintRight(TextWriter Writer)
        {
            Writer.Write("(");

            if (Params != null)
            {
                Params.Print(Writer);
            }

            Writer.Write(")");

            if (Ret != null)
            {
                Ret.PrintRight(Writer);
            }

            if (CV != null)
            {
                CV.Print(Writer);
            }

            if (Ref != null)
            {
                Ref.Print(Writer);
            }

            if (Attrs != null)
            {
                Attrs.Print(Writer);
            }
        }
    }
}