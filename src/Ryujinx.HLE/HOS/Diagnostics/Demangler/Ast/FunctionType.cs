using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class FunctionType : BaseNode
    {
        private readonly BaseNode _returnType;
        private readonly BaseNode _params;
        private readonly BaseNode _cvQualifier;
        private readonly SimpleReferenceType _referenceQualifier;
        private readonly BaseNode _exceptionSpec;

        public FunctionType(BaseNode returnType, BaseNode Params, BaseNode cvQualifier, SimpleReferenceType referenceQualifier, BaseNode exceptionSpec) : base(NodeType.FunctionType)
        {
            _returnType = returnType;
            _params = Params;
            _cvQualifier = cvQualifier;
            _referenceQualifier = referenceQualifier;
            _exceptionSpec = exceptionSpec;
        }

        public override void PrintLeft(TextWriter writer)
        {
            _returnType.PrintLeft(writer);
            writer.Write(" ");
        }

        public override void PrintRight(TextWriter writer)
        {
            writer.Write("(");
            _params.Print(writer);
            writer.Write(")");

            _returnType.PrintRight(writer);

            _cvQualifier.Print(writer);

            if (_referenceQualifier.Qualifier != Reference.None)
            {
                writer.Write(" ");
                _referenceQualifier.PrintQualifier(writer);
            }

            if (_exceptionSpec != null)
            {
                writer.Write(" ");
                _exceptionSpec.Print(writer);
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
