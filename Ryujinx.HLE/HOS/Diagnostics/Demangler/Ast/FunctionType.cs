using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class FunctionType : BaseNode
    {
        private BaseNode            ReturnType;
        private BaseNode            Params;
        private BaseNode            CVQualifier;
        private SimpleReferenceType ReferenceQualifier;
        private BaseNode            ExceptionSpec;

        public FunctionType(BaseNode ReturnType, BaseNode Params, BaseNode CVQualifier, SimpleReferenceType ReferenceQualifier, BaseNode ExceptionSpec) : base(NodeType.FunctionType)
        {
            this.ReturnType         = ReturnType;
            this.Params             = Params;
            this.CVQualifier        = CVQualifier;
            this.ReferenceQualifier = ReferenceQualifier;
            this.ExceptionSpec      = ExceptionSpec;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            ReturnType.PrintLeft(Writer);
            Writer.Write(" ");
        }

        public override void PrintRight(TextWriter Writer)
        {
            Writer.Write("(");
            Params.Print(Writer);
            Writer.Write(")");

            ReturnType.PrintRight(Writer);

            CVQualifier.Print(Writer);

            if (ReferenceQualifier.Qualifier != Reference.None)
            {
                Writer.Write(" ");
                ReferenceQualifier.PrintQualifier(Writer);
            }

            if (ExceptionSpec != null)
            {
                Writer.Write(" ");
                ExceptionSpec.Print(Writer);
            }
        }

        public override bool HasRightPart()
        {
            return true;
        }

        public override bool HasFunctions()
        {
            return true;
        }
    }
}