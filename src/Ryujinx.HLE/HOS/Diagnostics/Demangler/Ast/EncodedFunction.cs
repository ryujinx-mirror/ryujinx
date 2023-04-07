using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class EncodedFunction : BaseNode
    {
        private BaseNode _name;
        private BaseNode _params;
        private BaseNode _cv;
        private BaseNode _ref;
        private BaseNode _attrs;
        private BaseNode _ret;

        public EncodedFunction(BaseNode name, BaseNode Params, BaseNode cv, BaseNode Ref, BaseNode attrs, BaseNode ret) : base(NodeType.NameType)
        {
            _name   = name;
            _params = Params;
            _cv     = cv;
            _ref    = Ref;
            _attrs  = attrs;
            _ret    = ret;
        }

        public override void PrintLeft(TextWriter writer)
        {
            if (_ret != null)
            {
                _ret.PrintLeft(writer);

                if (!_ret.HasRightPart())
                {
                    writer.Write(" ");
                }
            }

            _name.Print(writer);

        }

        public override bool HasRightPart()
        {
            return true;
        }

        public override void PrintRight(TextWriter writer)
        {
            writer.Write("(");

            if (_params != null)
            {
                _params.Print(writer);
            }

            writer.Write(")");

            if (_ret != null)
            {
                _ret.PrintRight(writer);
            }

            if (_cv != null)
            {
                _cv.Print(writer);
            }

            if (_ref != null)
            {
                _ref.Print(writer);
            }

            if (_attrs != null)
            {
                _attrs.Print(writer);
            }
        }
    }
}