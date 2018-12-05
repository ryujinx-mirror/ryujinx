using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public enum CV
    {
        None,
        Const,
        Volatile,
        Restricted = 4
    }

    public enum Reference
    {
        None,
        RValue,
        LValue
    }

    public class CVType : ParentNode
    {
        public CV Qualifier;

        public CVType(CV Qualifier, BaseNode Child) : base(NodeType.CVQualifierType, Child)
        {
            this.Qualifier = Qualifier;
        }

        public void PrintQualifier(TextWriter Writer)
        {
            if ((Qualifier & CV.Const) != 0)
            {
                Writer.Write(" const");
            }

            if ((Qualifier & CV.Volatile) != 0)
            {
                Writer.Write(" volatile");
            }

            if ((Qualifier & CV.Restricted) != 0)
            {
                Writer.Write(" restrict");
            }
        }

        public override void PrintLeft(TextWriter Writer)
        {
            if (Child != null)
            {
                Child.PrintLeft(Writer);
            }

            PrintQualifier(Writer);
        }

        public override bool HasRightPart()
        {
            return Child != null && Child.HasRightPart();
        }

        public override void PrintRight(TextWriter Writer)
        {
            if (Child != null)
            {
                Child.PrintRight(Writer);
            }
        }
    }

    public class SimpleReferenceType : ParentNode
    {
        public Reference Qualifier;

        public SimpleReferenceType(Reference Qualifier, BaseNode Child) : base(NodeType.SimpleReferenceType, Child)
        {
            this.Qualifier = Qualifier;
        }

        public void PrintQualifier(TextWriter Writer)
        {
            if ((Qualifier & Reference.LValue) != 0)
            {
                Writer.Write("&");
            }

            if ((Qualifier & Reference.RValue) != 0)
            {
                Writer.Write("&&");
            }
        }

        public override void PrintLeft(TextWriter Writer)
        {
            if (Child != null)
            {
                Child.PrintLeft(Writer);
            }
            else if (Qualifier != Reference.None)
            {
                Writer.Write(" ");
            }

            PrintQualifier(Writer);
        }

        public override bool HasRightPart()
        {
            return Child != null && Child.HasRightPart();
        }

        public override void PrintRight(TextWriter Writer)
        {
            if (Child != null)
            {
                Child.PrintRight(Writer);
            }
        }
    }
}