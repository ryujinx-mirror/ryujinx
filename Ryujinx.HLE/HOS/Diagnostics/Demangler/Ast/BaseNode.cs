using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public enum NodeType
    {
        CVQualifierType,
        SimpleReferenceType,
        NameType,
        EncodedFunction,
        NestedName,
        SpecialName,
        LiteralOperator,
        NodeArray,
        ElaboratedType,
        PostfixQualifiedType,
        SpecialSubstitution,
        ExpandedSpecialSubstitution,
        CtorDtorNameType,
        EnclosedExpression,
        ForwardTemplateReference,
        NameTypeWithTemplateArguments,
        PackedTemplateArgument,
        TemplateArguments,
        BooleanExpression,
        CastExpression,
        CallExpression,
        IntegerCastExpression,
        PackedTemplateParameter,
        PackedTemplateParameterExpansion,
        IntegerLiteral,
        DeleteExpression,
        MemberExpression,
        ArraySubscriptingExpression,
        InitListExpression,
        PostfixExpression,
        ConditionalExpression,
        ThrowExpression,
        FunctionParameter,
        ConversionExpression,
        BinaryExpression,
        PrefixExpression,
        BracedExpression,
        BracedRangeExpression,
        NewExpression,
        QualifiedName,
        StdQualifiedName,
        DtOrName,
        GlobalQualifiedName,
        NoexceptSpec,
        DynamicExceptionSpec,
        FunctionType,
        PointerType,
        ReferenceType,
        ConversionOperatorType,
        LocalName,
        CtorVtableSpecialName,
        ArrayType
    }

    public abstract class BaseNode
    {
        public NodeType Type { get; protected set; }

        public BaseNode(NodeType Type)
        {
            this.Type = Type;
        }

        public virtual void Print(TextWriter Writer)
        {
            PrintLeft(Writer);

            if (HasRightPart())
            {
                PrintRight(Writer);
            }
        }

        public abstract void PrintLeft(TextWriter Writer);

        public virtual bool HasRightPart()
        {
            return false;
        }

        public virtual bool IsArray()
        {
            return false;
        }

        public virtual bool HasFunctions()
        {
            return false;
        }

        public virtual string GetName()
        {
            return null;
        }

        public virtual void PrintRight(TextWriter Writer) {}

        public override string ToString()
        {
            StringWriter Writer = new StringWriter();

            Print(Writer);

            return Writer.ToString();
        }
    }
}