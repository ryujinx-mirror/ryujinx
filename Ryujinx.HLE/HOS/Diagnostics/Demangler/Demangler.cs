using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler
{
    class Demangler
    {
        private static readonly string BASE_36   = "0123456789abcdefghijklmnopqrstuvwxyz";
        private List<BaseNode> SubstitutionList  = new List<BaseNode>();
        private List<BaseNode> TemplateParamList = new List<BaseNode>();

        private List<ForwardTemplateReference> ForwardTemplateReferenceList = new List<ForwardTemplateReference>();

        public string Mangled { get; private set; }

        private int Position;
        private int Length;

        private bool CanForwardTemplateReference;
        private bool CanParseTemplateArgs;

        public Demangler(string Mangled)
        {
            this.Mangled         = Mangled;
            Position             = 0;
            Length               = Mangled.Length;
            CanParseTemplateArgs = true;
        }

        private bool ConsumeIf(string ToConsume)
        {
            string MangledPart = Mangled.Substring(Position);

            if (MangledPart.StartsWith(ToConsume))
            {
                Position += ToConsume.Length;

                return true;
            }

            return false;
        }

        private string PeekString(int Offset = 0, int Length = 1)
        {
            if (Position + Offset >= Length)
            {
                return null;
            }

            return Mangled.Substring(Position + Offset, Length);
        }

        private char Peek(int Offset = 0)
        {
            if (Position + Offset >= Length)
            {
                return '\0';
            }

            return Mangled[Position + Offset];
        }

        private char Consume()
        {
            if (Position < Length)
            {
                return Mangled[Position++];
            }

            return '\0';
        }

        private int Count()
        {
            return Length - Position;
        }

        private static int FromBase36(string Encoded)
        {
            char[] ReversedEncoded = Encoded.ToLower().ToCharArray().Reverse().ToArray();

            int Result = 0;

            for (int i = 0; i < ReversedEncoded.Length; i++)
            {
                int Value = BASE_36.IndexOf(ReversedEncoded[i]);
                if (Value == -1)
                {
                    return -1;
                }

                Result += Value * (int)Math.Pow(36, i);
            }

            return Result;
        }

        private int ParseSeqId()
        {
            string Part     = Mangled.Substring(Position);
            int    SeqIdLen = 0;

            for (; SeqIdLen < Part.Length; SeqIdLen++)
            {
                if (!char.IsLetterOrDigit(Part[SeqIdLen]))
                {
                    break;
                }
            }

            Position += SeqIdLen;

            return FromBase36(Part.Substring(0, SeqIdLen));
        }

        //   <substitution> ::= S <seq-id> _
        //                  ::= S_
        //                  ::= St # std::
        //                  ::= Sa # std::allocator
        //                  ::= Sb # std::basic_string
        //                  ::= Ss # std::basic_string<char, std::char_traits<char>, std::allocator<char> >
        //                  ::= Si # std::basic_istream<char, std::char_traits<char> >
        //                  ::= So # std::basic_ostream<char, std::char_traits<char> >
        //                  ::= Sd # std::basic_iostream<char, std::char_traits<char> >
        private BaseNode ParseSubstitution()
        {
            if (!ConsumeIf("S"))
            {
                return null;
            }

            char SubstitutionSecondChar = Peek();
            if (char.IsLower(SubstitutionSecondChar))
            {
                switch (SubstitutionSecondChar)
                {
                    case 'a':
                        Position++;
                        return new SpecialSubstitution(SpecialSubstitution.SpecialType.Allocator);
                    case 'b':
                        Position++;
                        return new SpecialSubstitution(SpecialSubstitution.SpecialType.BasicString);
                    case 's':
                        Position++;
                        return new SpecialSubstitution(SpecialSubstitution.SpecialType.String);
                    case 'i':
                        Position++;
                        return new SpecialSubstitution(SpecialSubstitution.SpecialType.IStream);
                    case 'o':
                        Position++;
                        return new SpecialSubstitution(SpecialSubstitution.SpecialType.OStream);
                    case 'd':
                        Position++;
                        return new SpecialSubstitution(SpecialSubstitution.SpecialType.IOStream);
                    default:
                        return null;
                }
            }

            // ::= S_
            if (ConsumeIf("_"))
            {
                if (SubstitutionList.Count != 0)
                {
                    return SubstitutionList[0];
                }

                return null;
            }

            //                ::= S <seq-id> _
            int SeqId = ParseSeqId();
            if (SeqId < 0)
            {
                return null;
            }

            SeqId++;

            if (!ConsumeIf("_") || SeqId >= SubstitutionList.Count)
            {
                return null;
            }

            return SubstitutionList[SeqId];
        }

        // NOTE: thoses data aren't used in the output
        //  <call-offset> ::= h <nv-offset> _
        //                ::= v <v-offset> _
        //  <nv-offset>   ::= <offset number>
        //                    # non-virtual base override
        //  <v-offset>    ::= <offset number> _ <virtual offset number>
        //                    # virtual base override, with vcall offset
        private bool ParseCallOffset()
        {
            if (ConsumeIf("h"))
            {
                return ParseNumber(true).Length == 0 || !ConsumeIf("_");
            }
            else if (ConsumeIf("v"))
            {
                return ParseNumber(true).Length == 0 || !ConsumeIf("_") || ParseNumber(true).Length == 0 || !ConsumeIf("_");
            }

            return true;
        }


        //   <class-enum-type> ::= <name>     # non-dependent type name, dependent type name, or dependent typename-specifier
        //                     ::= Ts <name>  # dependent elaborated type specifier using 'struct' or 'class'
        //                     ::= Tu <name>  # dependent elaborated type specifier using 'union'
        //                     ::= Te <name>  # dependent elaborated type specifier using 'enum'
        private BaseNode ParseClassEnumType()
        {
            string ElaboratedType = null;

            if (ConsumeIf("Ts"))
            {
                ElaboratedType = "struct";
            }
            else if (ConsumeIf("Tu"))
            {
                ElaboratedType = "union";
            }
            else if (ConsumeIf("Te"))
            {
                ElaboratedType = "enum";
            }

            BaseNode Name = ParseName();
            if (Name == null)
            {
                return null;
            }

            if (ElaboratedType == null)
            {
                return Name;
            }

            return new ElaboratedType(ElaboratedType, Name);
        }

        //  <function-type>         ::= [<CV-qualifiers>] [<exception-spec>] [Dx] F [Y] <bare-function-type> [<ref-qualifier>] E
        //  <bare-function-type>    ::= <signature type>+
        //                              # types are possible return type, then parameter types
        //  <exception-spec>        ::= Do                # non-throwing exception-specification (e.g., noexcept, throw())
        //                          ::= DO <expression> E # computed (instantiation-dependent) noexcept
        //                          ::= Dw <type>+ E      # dynamic exception specification with instantiation-dependent types
        private BaseNode ParseFunctionType()
        {
            CV CVQualifiers = ParseCVQualifiers();

            BaseNode ExceptionSpec = null;

            if (ConsumeIf("Do"))
            {
                ExceptionSpec = new NameType("noexcept");
            }
            else if (ConsumeIf("DO"))
            {
                BaseNode Expression = ParseExpression();
                if (Expression == null || !ConsumeIf("E"))
                {
                    return null;
                }

                ExceptionSpec = new NoexceptSpec(Expression);
            }
            else if (ConsumeIf("Dw"))
            {
                List<BaseNode> Types = new List<BaseNode>();

                while (!ConsumeIf("E"))
                {
                    BaseNode Type = ParseType();
                    if (Type == null)
                    {
                        return null;
                    }

                    Types.Add(Type);
                }

                ExceptionSpec = new DynamicExceptionSpec(new NodeArray(Types));
            }

            // We don't need the transaction
            ConsumeIf("Dx");

            if (!ConsumeIf("F"))
            {
                return null;
            }

            // extern "C"
            ConsumeIf("Y");

            BaseNode ReturnType = ParseType();
            if (ReturnType == null)
            {
                return null;
            }

            Reference ReferenceQualifier = Reference.None;
            List<BaseNode> Params = new List<BaseNode>();

            while (true)
            {
                if (ConsumeIf("E"))
                {
                    break;
                }

                if (ConsumeIf("v"))
                {
                    continue;
                }

                if (ConsumeIf("RE"))
                {
                    ReferenceQualifier = Reference.LValue;
                    break;
                }
                else if (ConsumeIf("OE"))
                {
                    ReferenceQualifier = Reference.RValue;
                    break;
                }

                BaseNode Type = ParseType();
                if (Type == null)
                {
                    return null;
                }

                Params.Add(Type);
            }

            return new FunctionType(ReturnType, new NodeArray(Params), new CVType(CVQualifiers, null), new SimpleReferenceType(ReferenceQualifier, null), ExceptionSpec);
        }

        //   <array-type> ::= A <positive dimension number> _ <element type>
        //                ::= A [<dimension expression>] _ <element type>
        private BaseNode ParseArrayType()
        {
            if (!ConsumeIf("A"))
            {
                return null;
            }

            BaseNode ElementType;
            if (char.IsDigit(Peek()))
            {
                string Dimension = ParseNumber();
                if (Dimension.Length == 0 || !ConsumeIf("_"))
                {
                    return null;
                }

                ElementType = ParseType();
                if (ElementType == null)
                {
                    return null;
                }

                return new ArrayType(ElementType, Dimension);
            }

            if (!ConsumeIf("_"))
            {
                BaseNode DimensionExpression = ParseExpression();
                if (DimensionExpression == null || !ConsumeIf("_"))
                {
                    return null;
                }

                ElementType = ParseType();
                if (ElementType == null)
                {
                    return null;
                }

                return new ArrayType(ElementType, DimensionExpression);
            }

            ElementType = ParseType();
            if (ElementType == null)
            {
                return null;
            }

            return new ArrayType(ElementType);
        }

        // <type>  ::= <builtin-type>
        //         ::= <qualified-type> (PARTIAL)
        //         ::= <function-type>
        //         ::= <class-enum-type>
        //         ::= <array-type> (TODO)
        //         ::= <pointer-to-member-type> (TODO)
        //         ::= <template-param>
        //         ::= <template-template-param> <template-args>
        //         ::= <decltype>
        //         ::= P <type>        # pointer
        //         ::= R <type>        # l-value reference
        //         ::= O <type>        # r-value reference (C++11)
        //         ::= C <type>        # complex pair (C99)
        //         ::= G <type>        # imaginary (C99)
        //         ::= <substitution>  # See Compression below
        private BaseNode ParseType(NameParserContext Context = null)
        {
            // Temporary context
            if (Context == null)
            {
                Context = new NameParserContext();
            }

            BaseNode Result = null;
            switch (Peek())
            {
                case 'r':
                case 'V':
                case 'K':
                    int TypePos = 0;

                    if (Peek(TypePos) == 'r')
                    {
                        TypePos++;
                    }

                    if (Peek(TypePos) == 'V')
                    {
                        TypePos++;
                    }

                    if (Peek(TypePos) == 'K')
                    {
                        TypePos++;
                    }

                    if (Peek(TypePos) == 'F' || (Peek(TypePos) == 'D' && (Peek(TypePos + 1) == 'o' || Peek(TypePos + 1) == 'O' || Peek(TypePos + 1) == 'w' || Peek(TypePos + 1) == 'x')))
                    {
                        Result = ParseFunctionType();
                        break;
                    }

                    CV CV = ParseCVQualifiers();

                    Result = ParseType(Context);

                    if (Result == null)
                    {
                        return null;
                    }

                    Result = new CVType(CV, Result);
                    break;
                case 'U':
                    // TODO: <extended-qualifier>
                    return null;
                case 'v':
                    Position++;
                    return new NameType("void");
                case 'w':
                    Position++;
                    return new NameType("wchar_t");
                case 'b':
                    Position++;
                    return new NameType("bool");
                case 'c':
                    Position++;
                    return new NameType("char");
                case 'a':
                    Position++;
                    return new NameType("signed char");
                case 'h':
                    Position++;
                    return new NameType("unsigned char");
                case 's':
                    Position++;
                    return new NameType("short");
                case 't':
                    Position++;
                    return new NameType("unsigned short");
                case 'i':
                    Position++;
                    return new NameType("int");
                case 'j':
                    Position++;
                    return new NameType("unsigned int");
                case 'l':
                    Position++;
                    return new NameType("long");
                case 'm':
                    Position++;
                    return new NameType("unsigned long");
                case 'x':
                    Position++;
                    return new NameType("long long");
                case 'y':
                    Position++;
                    return new NameType("unsigned long long");
                case 'n':
                    Position++;
                    return new NameType("__int128");
                case 'o':
                    Position++;
                    return new NameType("unsigned __int128");
                case 'f':
                    Position++;
                    return new NameType("float");
                case 'd':
                    Position++;
                    return new NameType("double");
                case 'e':
                    Position++;
                    return new NameType("long double");
                case 'g':
                    Position++;
                    return new NameType("__float128");
                case 'z':
                    Position++;
                    return new NameType("...");
                case 'u':
                    Position++;
                    return ParseSourceName();
                case 'D':
                    switch (Peek(1))
                    {
                        case 'd':
                            Position += 2;
                            return new NameType("decimal64");
                        case 'e':
                            Position += 2;
                            return new NameType("decimal128");
                        case 'f':
                            Position += 2;
                            return new NameType("decimal32");
                        case 'h':
                            Position += 2;
                            // FIXME: GNU c++flit returns this but that is not what is supposed to be returned.
                            return new NameType("half");
                            //return new NameType("decimal16");
                        case 'i':
                            Position += 2;
                            return new NameType("char32_t");
                        case 's':
                            Position += 2;
                            return new NameType("char16_t");
                        case 'a':
                            Position += 2;
                            return new NameType("decltype(auto)");
                        case 'n':
                            Position += 2;
                            // FIXME: GNU c++flit returns this but that is not what is supposed to be returned.
                            return new NameType("decltype(nullptr)");
                            //return new NameType("std::nullptr_t");
                        case 't':
                        case 'T':
                            Position += 2;
                            Result = ParseDecltype();
                            break;
                        case 'o':
                        case 'O':
                        case 'w':
                        case 'x':
                            Result = ParseFunctionType();
                            break;
                        default:
                            return null;
                    }
                    break;
                case 'F':
                    Result = ParseFunctionType();
                    break;
                case 'A':
                    return ParseArrayType();
                case 'M':
                    // TODO: <pointer-to-member-type>
                    Position++;
                    return null;
                case 'T':
                    // might just be a class enum type
                    if (Peek(1) == 's' || Peek(1) == 'u' || Peek(1) == 'e')
                    {
                        Result = ParseClassEnumType();
                        break;
                    }

                    Result = ParseTemplateParam();
                    if (Result == null)
                    {
                        return null;
                    }

                    if (CanParseTemplateArgs && Peek() == 'I')
                    {
                        BaseNode TemplateArguments = ParseTemplateArguments();
                        if (TemplateArguments == null)
                        {
                            return null;
                        }

                        Result = new NameTypeWithTemplateArguments(Result, TemplateArguments);
                    }
                    break;
                case 'P':
                    Position++;
                    Result = ParseType(Context);

                    if (Result == null)
                    {
                        return null;
                    }

                    Result = new PointerType(Result);
                    break;
                case 'R':
                    Position++;
                    Result = ParseType(Context);

                    if (Result == null)
                    {
                        return null;
                    }

                    Result = new ReferenceType("&", Result);
                    break;
                case 'O':
                    Position++;
                    Result = ParseType(Context);

                    if (Result == null)
                    {
                        return null;
                    }

                    Result = new ReferenceType("&&", Result);
                    break;
                case 'C':
                    Position++;
                    Result = ParseType(Context);

                    if (Result == null)
                    {
                        return null;
                    }

                    Result = new PostfixQualifiedType(" complex", Result);
                    break;
                case 'G':
                    Position++;
                    Result = ParseType(Context);

                    if (Result == null)
                    {
                        return null;
                    }

                    Result = new PostfixQualifiedType(" imaginary", Result);
                    break;
                case 'S':
                    if (Peek(1) != 't')
                    {
                        BaseNode Substitution = ParseSubstitution();
                        if (Substitution == null)
                        {
                            return null;
                        }

                        if (CanParseTemplateArgs && Peek() == 'I')
                        {
                            BaseNode TemplateArgument = ParseTemplateArgument();
                            if (TemplateArgument == null)
                            {
                                return null;
                            }

                            Result = new NameTypeWithTemplateArguments(Substitution, TemplateArgument);
                            break;
                        }
                        return Substitution;
                    }
                    else
                    {
                        Result = ParseClassEnumType();
                        break;
                    }
                default:
                    Result = ParseClassEnumType();
                    break;
            }
            if (Result != null)
            {
                SubstitutionList.Add(Result);
            }

            return Result;
        }

        // <special-name> ::= TV <type> # virtual table
        //                ::= TT <type> # VTT structure (construction vtable index)
        //                ::= TI <type> # typeinfo structure
        //                ::= TS <type> # typeinfo name (null-terminated byte string)
        //                ::= Tc <call-offset> <call-offset> <base encoding>
        //                ::= TW <object name> # Thread-local wrapper
        //                ::= TH <object name> # Thread-local initialization
        //                ::= T <call-offset> <base encoding>
        //                              # base is the nominal target function of thunk
        //                ::= GV <object name>	# Guard variable for one-time initialization
        private BaseNode ParseSpecialName(NameParserContext Context = null)
        {
            if (Peek() != 'T')
            {
                if (ConsumeIf("GV"))
                {
                    BaseNode Name = ParseName();
                    if (Name == null)
                    {
                        return null;
                    }

                    return new SpecialName("guard variable for ", Name);
                }
                return null;
            }

            BaseNode Node;
            switch (Peek(1))
            {
                // ::= TV <type>    # virtual table
                case 'V':
                    Position += 2;
                    Node = ParseType(Context);
                    if (Node == null)
                    {
                        return null;
                    }

                    return new SpecialName("vtable for ", Node);
                // ::= TT <type>    # VTT structure (construction vtable index)
                case 'T':
                    Position += 2;
                    Node = ParseType(Context);
                    if (Node == null)
                    {
                        return null;
                    }

                    return new SpecialName("VTT for ", Node);
                // ::= TI <type>    # typeinfo structure
                case 'I':
                    Position += 2;
                    Node = ParseType(Context);
                    if (Node == null)
                    {
                        return null;
                    }

                    return new SpecialName("typeinfo for ", Node);
                // ::= TS <type> # typeinfo name (null-terminated byte string)
                case 'S':
                    Position += 2;
                    Node = ParseType(Context);
                    if (Node == null)
                    {
                        return null;
                    }

                    return new SpecialName("typeinfo name for ", Node);
                // ::= Tc <call-offset> <call-offset> <base encoding>
                case 'c':
                    Position += 2;
                    if (ParseCallOffset() || ParseCallOffset())
                    {
                        return null;
                    }

                    Node = ParseEncoding();
                    if (Node == null)
                    {
                        return null;
                    }

                    return new SpecialName("covariant return thunk to ", Node);
                // extension ::= TC <first type> <number> _ <second type>
                case 'C':
                    Position += 2;
                    BaseNode FirstType = ParseType();
                    if (FirstType == null || ParseNumber(true).Length == 0 || !ConsumeIf("_"))
                    {
                        return null;
                    }

                    BaseNode SecondType = ParseType();

                    return new CtorVtableSpecialName(SecondType, FirstType);
                // ::= TH <object name> # Thread-local initialization
                case 'H':
                    Position += 2;
                    Node = ParseName();
                    if (Node == null)
                    {
                        return null;
                    }

                    return new SpecialName("thread-local initialization routine for ", Node);
                // ::= TW <object name> # Thread-local wrapper
                case 'W':
                    Position += 2;
                    Node = ParseName();
                    if (Node == null)
                    {
                        return null;
                    }

                    return new SpecialName("thread-local wrapper routine for ", Node);
                default:
                    Position++;
                    bool IsVirtual = Peek() == 'v';
                    if (ParseCallOffset())
                    {
                        return null;
                    }

                    Node = ParseEncoding();
                    if (Node == null)
                    {
                        return null;
                    }

                    if (IsVirtual)
                    {
                        return new SpecialName("virtual thunk to ", Node);
                    }

                    return new SpecialName("non-virtual thunk to ", Node);
            }
        }

        // <CV-qualifiers>      ::= [r] [V] [K] # restrict (C99), volatile, const
        private CV ParseCVQualifiers()
        {
            CV Qualifiers = CV.None;

            if (ConsumeIf("r"))
            {
                Qualifiers |= CV.Restricted;
            }
            if (ConsumeIf("V"))
            {
                Qualifiers |= CV.Volatile;
            }
            if (ConsumeIf("K"))
            {
                Qualifiers |= CV.Const;
            }

            return Qualifiers;
        }


        // <ref-qualifier>      ::= R              # & ref-qualifier
        // <ref-qualifier>      ::= O              # && ref-qualifier
        private SimpleReferenceType ParseRefQualifiers()
        {
            Reference Result = Reference.None;
            if (ConsumeIf("O"))
            {
                Result = Reference.RValue;
            }
            else if (ConsumeIf("R"))
            {
                Result = Reference.LValue;
            }
            return new SimpleReferenceType(Result, null);
        }

        private BaseNode CreateNameNode(BaseNode Prev, BaseNode Name, NameParserContext Context)
        {
            BaseNode Result = Name;
            if (Prev != null)
            {
                Result = new NestedName(Name, Prev);
            }

            if (Context != null)
            {
                Context.FinishWithTemplateArguments = false;
            }

            return Result;
        }

        private int ParsePositiveNumber()
        {
            string Part         = Mangled.Substring(Position);
            int    NumberLength = 0;

            for (; NumberLength < Part.Length; NumberLength++)
            {
                if (!char.IsDigit(Part[NumberLength]))
                {
                    break;
                }
            }

            Position += NumberLength;

            if (NumberLength == 0)
            {
                return -1;
            }

            return int.Parse(Part.Substring(0, NumberLength));
        }

        private string ParseNumber(bool IsSigned = false)
        {
            if (IsSigned)
            {
                ConsumeIf("n");
            }

            if (Count() == 0 || !char.IsDigit(Mangled[Position]))
            {
                return null;
            }

            string Part         = Mangled.Substring(Position);
            int    NumberLength = 0;

            for (; NumberLength < Part.Length; NumberLength++)
            {
                if (!char.IsDigit(Part[NumberLength]))
                {
                    break;
                }
            }

            Position += NumberLength;

            return Part.Substring(0, NumberLength);
        }

        // <source-name> ::= <positive length number> <identifier>
        private BaseNode ParseSourceName()
        {
            int Length = ParsePositiveNumber();
            if (Count() < Length || Length <= 0)
            {
                return null;
            }

            string Name = Mangled.Substring(Position, Length);
            Position += Length;
            if (Name.StartsWith("_GLOBAL__N"))
            {
                return new NameType("(anonymous namespace)");
            }

            return new NameType(Name);
        }

        // <operator-name> ::= nw    # new
        //                 ::= na    # new[]
        //                 ::= dl    # delete
        //                 ::= da    # delete[]
        //                 ::= ps    # + (unary)
        //                 ::= ng    # - (unary)
        //                 ::= ad    # & (unary)
        //                 ::= de    # * (unary)
        //                 ::= co    # ~
        //                 ::= pl    # +
        //                 ::= mi    # -
        //                 ::= ml    # *
        //                 ::= dv    # /
        //                 ::= rm    # %
        //                 ::= an    # &
        //                 ::= or    # |
        //                 ::= eo    # ^
        //                 ::= aS    # =
        //                 ::= pL    # +=
        //                 ::= mI    # -=
        //                 ::= mL    # *=
        //                 ::= dV    # /=
        //                 ::= rM    # %=
        //                 ::= aN    # &=
        //                 ::= oR    # |=
        //                 ::= eO    # ^=
        //                 ::= ls    # <<
        //                 ::= rs    # >>
        //                 ::= lS    # <<=
        //                 ::= rS    # >>=
        //                 ::= eq    # ==
        //                 ::= ne    # !=
        //                 ::= lt    # <
        //                 ::= gt    # >
        //                 ::= le    # <=
        //                 ::= ge    # >=
        //                 ::= ss    # <=>
        //                 ::= nt    # !
        //                 ::= aa    # &&
        //                 ::= oo    # ||
        //                 ::= pp    # ++ (postfix in <expression> context)
        //                 ::= mm    # -- (postfix in <expression> context)
        //                 ::= cm    # ,
        //                 ::= pm    # ->*
        //                 ::= pt    # ->
        //                 ::= cl    # ()
        //                 ::= ix    # []
        //                 ::= qu    # ?
        //                 ::= cv <type>    # (cast) (TODO)
        //                 ::= li <source-name>          # operator ""
        //                 ::= v <digit> <source-name>    # vendor extended operator (TODO)
        private BaseNode ParseOperatorName(NameParserContext Context)
        {
            switch (Peek())
            {
                case 'a':
                    switch (Peek(1))
                    {
                        case 'a':
                            Position += 2;
                            return new NameType("operator&&");
                        case 'd':
                        case 'n':
                            Position += 2;
                            return new NameType("operator&");
                        case 'N':
                            Position += 2;
                            return new NameType("operator&=");
                        case 'S':
                            Position += 2;
                            return new NameType("operator=");
                        default:
                            return null;
                    }
                case 'c':
                    switch (Peek(1))
                    {
                        case 'l':
                            Position += 2;
                            return new NameType("operator()");
                        case 'm':
                            Position += 2;
                            return new NameType("operator,");
                        case 'o':
                            Position += 2;
                            return new NameType("operator~");
                        case 'v':
                            Position += 2;

                            bool CanParseTemplateArgsBackup        = CanParseTemplateArgs;
                            bool CanForwardTemplateReferenceBackup = CanForwardTemplateReference;

                            CanParseTemplateArgs        = false;
                            CanForwardTemplateReference = CanForwardTemplateReferenceBackup || Context != null;

                            BaseNode Type = ParseType();

                            CanParseTemplateArgs        = CanParseTemplateArgsBackup;
                            CanForwardTemplateReference = CanForwardTemplateReferenceBackup;

                            if (Type == null)
                            {
                                return null;
                            }

                            if (Context != null)
                            {
                                Context.CtorDtorConversion = true;
                            }

                            return new ConversionOperatorType(Type);
                        default:
                            return null;
                    }
                case 'd':
                    switch (Peek(1))
                    {
                        case 'a':
                            Position += 2;
                            return new NameType("operator delete[]");
                        case 'e':
                            Position += 2;
                            return new NameType("operator*");
                        case 'l':
                            Position += 2;
                            return new NameType("operator delete");
                        case 'v':
                            Position += 2;
                            return new NameType("operator/");
                        case 'V':
                            Position += 2;
                            return new NameType("operator/=");
                        default:
                            return null;
                    }
                case 'e':
                    switch (Peek(1))
                    {
                        case 'o':
                            Position += 2;
                            return new NameType("operator^");
                        case 'O':
                            Position += 2;
                            return new NameType("operator^=");
                        case 'q':
                            Position += 2;
                            return new NameType("operator==");
                        default:
                            return null;
                    }
                case 'g':
                    switch (Peek(1))
                    {
                        case 'e':
                            Position += 2;
                            return new NameType("operator>=");
                        case 't':
                            Position += 2;
                            return new NameType("operator>");
                        default:
                            return null;
                    }
                case 'i':
                    if (Peek(1) == 'x')
                    {
                        Position += 2;
                        return new NameType("operator[]");
                    }
                    return null;
                case 'l':
                    switch (Peek(1))
                    {
                        case 'e':
                            Position += 2;
                            return new NameType("operator<=");
                        case 'i':
                            Position += 2;
                            BaseNode SourceName = ParseSourceName();
                            if (SourceName == null)
                            {
                                return null;
                            }

                            return new LiteralOperator(SourceName);
                        case 's':
                            Position += 2;
                            return new NameType("operator<<");
                        case 'S':
                            Position += 2;
                            return new NameType("operator<<=");
                        case 't':
                            Position += 2;
                            return new NameType("operator<");
                        default:
                            return null;
                    }
                case 'm':
                    switch (Peek(1))
                    {
                        case 'i':
                            Position += 2;
                            return new NameType("operator-");
                        case 'I':
                            Position += 2;
                            return new NameType("operator-=");
                        case 'l':
                            Position += 2;
                            return new NameType("operator*");
                        case 'L':
                            Position += 2;
                            return new NameType("operator*=");
                        case 'm':
                            Position += 2;
                            return new NameType("operator--");
                        default:
                            return null;
                    }
                case 'n':
                    switch (Peek(1))
                    {
                        case 'a':
                            Position += 2;
                            return new NameType("operator new[]");
                        case 'e':
                            Position += 2;
                            return new NameType("operator!=");
                        case 'g':
                            Position += 2;
                            return new NameType("operator-");
                        case 't':
                            Position += 2;
                            return new NameType("operator!");
                        case 'w':
                            Position += 2;
                            return new NameType("operator new");
                        default:
                            return null;
                    }
                case 'o':
                    switch (Peek(1))
                    {
                        case 'o':
                            Position += 2;
                            return new NameType("operator||");
                        case 'r':
                            Position += 2;
                            return new NameType("operator|");
                        case 'R':
                            Position += 2;
                            return new NameType("operator|=");
                        default:
                            return null;
                    }
                case 'p':
                    switch (Peek(1))
                    {
                        case 'm':
                            Position += 2;
                            return new NameType("operator->*");
                        case 's':
                        case 'l':
                            Position += 2;
                            return new NameType("operator+");
                        case 'L':
                            Position += 2;
                            return new NameType("operator+=");
                        case 'p':
                            Position += 2;
                            return new NameType("operator++");
                        case 't':
                            Position += 2;
                            return new NameType("operator->");
                        default:
                            return null;
                    }
                case 'q':
                    if (Peek(1) == 'u')
                    {
                        Position += 2;
                        return new NameType("operator?");
                    }
                    return null;
                case 'r':
                    switch (Peek(1))
                    {
                        case 'm':
                            Position += 2;
                            return new NameType("operator%");
                        case 'M':
                            Position += 2;
                            return new NameType("operator%=");
                        case 's':
                            Position += 2;
                            return new NameType("operator>>");
                        case 'S':
                            Position += 2;
                            return new NameType("operator>>=");
                        default:
                            return null;
                    }
                case 's':
                    if (Peek(1) == 's')
                    {
                        Position += 2;
                        return new NameType("operator<=>");
                    }
                    return null;
                case 'v':
                    // TODO: ::= v <digit> <source-name>    # vendor extended operator
                    return null;
                default:
                    return null;
            }
        }

        // <unqualified-name> ::= <operator-name> [<abi-tags> (TODO)]
        //                    ::= <ctor-dtor-name> (TODO)
        //                    ::= <source-name>
        //                    ::= <unnamed-type-name> (TODO)
        //                    ::= DC <source-name>+ E      # structured binding declaration (TODO)
        private BaseNode ParseUnqualifiedName(NameParserContext Context)
        {
            BaseNode Result = null;
            char C = Peek();
            if (C == 'U')
            {
                // TODO: Unnamed Type Name
                // throw new Exception("Unnamed Type Name not implemented");
            }
            else if (char.IsDigit(C))
            {
                Result = ParseSourceName();
            }
            else if (ConsumeIf("DC"))
            {
                // TODO: Structured Binding Declaration
                // throw new Exception("Structured Binding Declaration not implemented");
            }
            else
            {
                Result = ParseOperatorName(Context);
            }

            if (Result != null)
            {
                // TODO: ABI Tags
                //throw new Exception("ABI Tags not implemented");
            }
            return Result;
        }

        // <ctor-dtor-name> ::= C1  # complete object constructor
        //                  ::= C2  # base object constructor
        //                  ::= C3  # complete object allocating constructor
        //                  ::= D0  # deleting destructor
        //                  ::= D1  # complete object destructor
        //                  ::= D2  # base object destructor 
        private BaseNode ParseCtorDtorName(NameParserContext Context, BaseNode Prev)
        {
            if (Prev.Type == NodeType.SpecialSubstitution && Prev is SpecialSubstitution)
            {
                ((SpecialSubstitution)Prev).SetExtended();
            }

            if (ConsumeIf("C"))
            {
                bool IsInherited  = ConsumeIf("I");

                char CtorDtorType = Peek();
                if (CtorDtorType != '1' && CtorDtorType != '2' && CtorDtorType != '3')
                {
                    return null;
                }

                Position++;

                if (Context != null)
                {
                    Context.CtorDtorConversion = true;
                }

                if (IsInherited && ParseName(Context) == null)
                {
                    return null;
                }

                return new CtorDtorNameType(Prev, false);
            }

            if (ConsumeIf("D"))
            {
                char C = Peek();
                if (C != '0' && C != '1' && C != '2')
                {
                    return null;
                }

                Position++;

                if (Context != null)
                {
                    Context.CtorDtorConversion = true;
                }

                return new CtorDtorNameType(Prev, true);
            }

            return null;
        }

        // <function-param> ::= fp <top-level CV-qualifiers> _                                                                                           # L == 0, first parameter
        //                  ::= fp <top-level CV-qualifiers> <parameter-2 non-negative number> _                                                         # L == 0, second and later parameters
        //                  ::= fL <L-1 non-negative number> p <top-level CV-qualifiers> _                                                               # L > 0, first parameter
        //                  ::= fL <L-1 non-negative number> p <top-level CV-qualifiers> <parameter-2 non-negative number> _                             # L > 0, second and later parameters
        private BaseNode ParseFunctionParameter()
        {
            if (ConsumeIf("fp"))
            {
                // ignored
                ParseCVQualifiers();

                if (!ConsumeIf("_"))
                {
                    return null;
                }

                return new FunctionParameter(ParseNumber());
            }
            else if (ConsumeIf("fL"))
            {
                string L1Number = ParseNumber();
                if (L1Number == null || L1Number.Length == 0)
                {
                    return null;
                }

                if (!ConsumeIf("p"))
                {
                    return null;
                }

                // ignored
                ParseCVQualifiers();

                if (!ConsumeIf("_"))
                {
                    return null;
                }

                return new FunctionParameter(ParseNumber());
            }

            return null;
        }

        // <fold-expr> ::= fL <binary-operator-name> <expression> <expression>
        //             ::= fR <binary-operator-name> <expression> <expression>
        //             ::= fl <binary-operator-name> <expression>
        //             ::= fr <binary-operator-name> <expression>
        private BaseNode ParseFoldExpression()
        {
            if (!ConsumeIf("f"))
            {
                return null;
            }

            char FoldKind       = Peek();
            bool HasInitializer = FoldKind == 'L' || FoldKind == 'R';
            bool IsLeftFold     = FoldKind == 'l' || FoldKind == 'L';

            if (!IsLeftFold && !(FoldKind == 'r' || FoldKind == 'R'))
            {
                return null;
            }

            Position++;

            string OperatorName = null;

            switch (PeekString(0, 2))
            {
                case "aa":
                    OperatorName = "&&";
                    break;
                case "an":
                    OperatorName = "&";
                    break;
                case "aN":
                    OperatorName = "&=";
                    break;
                case "aS":
                    OperatorName = "=";
                    break;
                case "cm":
                    OperatorName = ",";
                    break;
                case "ds":
                    OperatorName = ".*";
                    break;
                case "dv":
                    OperatorName = "/";
                    break;
                case "dV":
                    OperatorName = "/=";
                    break;
                case "eo":
                    OperatorName = "^";
                    break;
                case "eO":
                    OperatorName = "^=";
                    break;
                case "eq":
                    OperatorName = "==";
                    break;
                case "ge":
                    OperatorName = ">=";
                    break;
                case "gt":
                    OperatorName = ">";
                    break;
                case "le":
                    OperatorName = "<=";
                    break;
                case "ls":
                    OperatorName = "<<";
                    break;
                case "lS":
                    OperatorName = "<<=";
                    break;
                case "lt":
                    OperatorName = "<";
                    break;
                case "mi":
                    OperatorName = "-";
                    break;
                case "mI":
                    OperatorName = "-=";
                    break;
                case "ml":
                    OperatorName = "*";
                    break;
                case "mL":
                    OperatorName = "*=";
                    break;
                case "ne":
                    OperatorName = "!=";
                    break;
                case "oo":
                    OperatorName = "||";
                    break;
                case "or":
                    OperatorName = "|";
                    break;
                case "oR":
                    OperatorName = "|=";
                    break;
                case "pl":
                    OperatorName = "+";
                    break;
                case "pL":
                    OperatorName = "+=";
                    break;
                case "rm":
                    OperatorName = "%";
                    break;
                case "rM":
                    OperatorName = "%=";
                    break;
                case "rs":
                    OperatorName = ">>";
                    break;
                case "rS":
                    OperatorName = ">>=";
                    break;
                default:
                    return null;
            }

            Position += 2;

            BaseNode Expression = ParseExpression();
            if (Expression == null)
            {
                return null;
            }

            BaseNode Initializer = null;

            if (HasInitializer)
            {
                Initializer = ParseExpression();
                if (Initializer == null)
                {
                    return null;
                }
            }

            if (IsLeftFold && Initializer != null)
            {
                BaseNode Temp = Expression;
                Expression    = Initializer;
                Initializer   = Temp;
            }

            return new FoldExpression(IsLeftFold, OperatorName, new PackedTemplateParameterExpansion(Expression), Initializer);
        }


        //                ::= cv <type> <expression>                               # type (expression), conversion with one argument
        //                ::= cv <type> _ <expression>* E                          # type (expr-list), conversion with other than one argument
        private BaseNode ParseConversionExpression()
        {
            if (!ConsumeIf("cv"))
            {
                return null;
            }

            bool CanParseTemplateArgsBackup = CanParseTemplateArgs;
            CanParseTemplateArgs            = false;
            BaseNode Type                   = ParseType();
            CanParseTemplateArgs            = CanParseTemplateArgsBackup;

            if (Type == null)
            {
                return null;
            }

            List<BaseNode> Expressions = new List<BaseNode>();
            if (ConsumeIf("_"))
            {
                while (!ConsumeIf("E"))
                {
                    BaseNode Expression = ParseExpression();
                    if (Expression == null)
                    {
                        return null;
                    }

                    Expressions.Add(Expression);
                }
            }
            else
            {
                BaseNode Expression = ParseExpression();
                if (Expression == null)
                {
                    return null;
                }

                Expressions.Add(Expression);
            }

            return new ConversionExpression(Type, new NodeArray(Expressions));
        }

        private BaseNode ParseBinaryExpression(string Name)
        {
            BaseNode LeftPart = ParseExpression();
            if (LeftPart == null)
            {
                return null;
            }

            BaseNode RightPart = ParseExpression();
            if (RightPart == null)
            {
                return null;
            }

            return new BinaryExpression(LeftPart, Name, RightPart);
        }

        private BaseNode ParsePrefixExpression(string Name)
        {
            BaseNode Expression = ParseExpression();
            if (Expression == null)
            {
                return null;
            }

            return new PrefixExpression(Name, Expression);
        }


        // <braced-expression> ::= <expression>
        //                     ::= di <field source-name> <braced-expression>    # .name = expr
        //                     ::= dx <index expression> <braced-expression>     # [expr] = expr
        //                     ::= dX <range begin expression> <range end expression> <braced-expression>
        //                                                                       # [expr ... expr] = expr
        private BaseNode ParseBracedExpression()
        {
            if (Peek() == 'd')
            {
                BaseNode BracedExpressionNode;
                switch (Peek(1))
                {
                    case 'i':
                        Position += 2;
                        BaseNode Field = ParseSourceName();
                        if (Field == null)
                        {
                            return null;
                        }

                        BracedExpressionNode = ParseBracedExpression();
                        if (BracedExpressionNode == null)
                        {
                            return null;
                        }

                        return new BracedExpression(Field, BracedExpressionNode, false);
                    case 'x':
                        Position += 2;
                        BaseNode Index = ParseExpression();
                        if (Index == null)
                        {
                            return null;
                        }

                        BracedExpressionNode = ParseBracedExpression();
                        if (BracedExpressionNode == null)
                        {
                            return null;
                        }

                        return new BracedExpression(Index, BracedExpressionNode, true);
                    case 'X':
                        Position += 2;
                        BaseNode RangeBeginExpression = ParseExpression();
                        if (RangeBeginExpression == null)
                        {
                            return null;
                        }

                        BaseNode RangeEndExpression = ParseExpression();
                        if (RangeEndExpression == null)
                        {
                            return null;
                        }

                        BracedExpressionNode = ParseBracedExpression();
                        if (BracedExpressionNode == null)
                        {
                            return null;
                        }

                        return new BracedRangeExpression(RangeBeginExpression, RangeEndExpression, BracedExpressionNode);
                }
            }

            return ParseExpression();
        }

        //               ::= [gs] nw <expression>* _ <type> E                    # new (expr-list) type
        //               ::= [gs] nw <expression>* _ <type> <initializer>        # new (expr-list) type (init)
        //               ::= [gs] na <expression>* _ <type> E                    # new[] (expr-list) type
        //               ::= [gs] na <expression>* _ <type> <initializer>        # new[] (expr-list) type (init)
        //
        // <initializer> ::= pi <expression>* E                                  # parenthesized initialization
        private BaseNode ParseNewExpression()
        {
            bool IsGlobal = ConsumeIf("gs");
            bool IsArray  = Peek(1) == 'a';

            if (!ConsumeIf("nw") || !ConsumeIf("na"))
            {
                return null;
            }

            List<BaseNode> Expressions  = new List<BaseNode>();
            List<BaseNode> Initializers = new List<BaseNode>();

            while (!ConsumeIf("_"))
            {
                BaseNode Expression = ParseExpression();
                if (Expression == null)
                {
                    return null;
                }

                Expressions.Add(Expression);
            }

            BaseNode TypeNode = ParseType();
            if (TypeNode == null)
            {
                return null;
            }

            if (ConsumeIf("pi"))
            {
                while (!ConsumeIf("E"))
                {
                    BaseNode Initializer = ParseExpression();
                    if (Initializer == null)
                    {
                        return null;
                    }

                    Initializers.Add(Initializer);
                }
            }
            else if (!ConsumeIf("E"))
            {
                return null;
            }

            return new NewExpression(new NodeArray(Expressions), TypeNode, new NodeArray(Initializers), IsGlobal, IsArray);
        }


        // <expression> ::= <unary operator-name> <expression>
        //              ::= <binary operator-name> <expression> <expression>
        //              ::= <ternary operator-name> <expression> <expression> <expression>
        //              ::= pp_ <expression>                                     # prefix ++
        //              ::= mm_ <expression>                                     # prefix --
        //              ::= cl <expression>+ E                                   # expression (expr-list), call
        //              ::= cv <type> <expression>                               # type (expression), conversion with one argument
        //              ::= cv <type> _ <expression>* E                          # type (expr-list), conversion with other than one argument
        //              ::= tl <type> <braced-expression>* E                     # type {expr-list}, conversion with braced-init-list argument
        //              ::= il <braced-expression>* E                            # {expr-list}, braced-init-list in any other context
        //              ::= [gs] nw <expression>* _ <type> E                     # new (expr-list) type
        //              ::= [gs] nw <expression>* _ <type> <initializer>         # new (expr-list) type (init)
        //              ::= [gs] na <expression>* _ <type> E                     # new[] (expr-list) type
        //              ::= [gs] na <expression>* _ <type> <initializer>         # new[] (expr-list) type (init)
        //              ::= [gs] dl <expression>                                 # delete expression
        //              ::= [gs] da <expression>                                 # delete[] expression
        //              ::= dc <type> <expression>                               # dynamic_cast<type> (expression)
        //              ::= sc <type> <expression>                               # static_cast<type> (expression)
        //              ::= cc <type> <expression>                               # const_cast<type> (expression)
        //              ::= rc <type> <expression>                               # reinterpret_cast<type> (expression)
        //              ::= ti <type>                                            # typeid (type)
        //              ::= te <expression>                                      # typeid (expression)
        //              ::= st <type>                                            # sizeof (type)
        //              ::= sz <expression>                                      # sizeof (expression)
        //              ::= at <type>                                            # alignof (type)
        //              ::= az <expression>                                      # alignof (expression)
        //              ::= nx <expression>                                      # noexcept (expression)
        //              ::= <template-param>
        //              ::= <function-param>
        //              ::= dt <expression> <unresolved-name>                    # expr.name
        //              ::= pt <expression> <unresolved-name>                    # expr->name
        //              ::= ds <expression> <expression>                         # expr.*expr
        //              ::= sZ <template-param>                                  # sizeof...(T), size of a template parameter pack
        //              ::= sZ <function-param>                                  # sizeof...(parameter), size of a function parameter pack
        //              ::= sP <template-arg>* E                                 # sizeof...(T), size of a captured template parameter pack from an alias template
        //              ::= sp <expression>                                      # expression..., pack expansion
        //              ::= tw <expression>                                      # throw expression
        //              ::= tr                                                   # throw with no operand (rethrow)
        //              ::= <unresolved-name>                                    # f(p), N::f(p), ::f(p),
        //                                                                       # freestanding dependent name (e.g., T::x),
        //                                                                       # objectless nonstatic member reference
        //              ::= <expr-primary>
        private BaseNode ParseExpression()
        {
            bool IsGlobal = ConsumeIf("gs");
            BaseNode Expression = null;
            if (Count() < 2)
            {
                return null;
            }

            switch (Peek())
            {
                case 'L':
                    return ParseExpressionPrimary();
                case 'T':
                    return ParseTemplateParam();
                case 'f':
                    char C = Peek(1);
                    if (C == 'p' || (C == 'L' && char.IsDigit(Peek(2))))
                    {
                        return ParseFunctionParameter();
                    }

                    return ParseFoldExpression();
                case 'a':
                    switch (Peek(1))
                    {
                        case 'a':
                            Position += 2;
                            return ParseBinaryExpression("&&");
                        case 'd':
                        case 'n':
                            Position += 2;
                            return ParseBinaryExpression("&");
                        case 'N':
                            Position += 2;
                            return ParseBinaryExpression("&=");
                        case 'S':
                            Position += 2;
                            return ParseBinaryExpression("=");
                        case 't':
                            Position += 2;
                            BaseNode Type = ParseType();
                            if (Type == null)
                            {
                                return null;
                            }

                            return new EnclosedExpression("alignof (", Type, ")");
                        case 'z':
                            Position += 2;
                            Expression = ParseExpression();
                            if (Expression == null)
                            {
                                return null;
                            }

                            return new EnclosedExpression("alignof (", Expression, ")");
                    }
                    return null;
                case 'c':
                    switch (Peek(1))
                    {
                        case 'c':
                            Position += 2;
                            BaseNode To = ParseType();
                            if (To == null)
                            {
                                return null;
                            }

                            BaseNode From = ParseExpression();
                            if (From == null)
                            {
                                return null;
                            }

                            return new CastExpression("const_cast", To, From);
                        case 'l':
                            Position += 2;
                            BaseNode Callee = ParseExpression();
                            if (Callee == null)
                            {
                                return null;
                            }

                            List<BaseNode> Names = new List<BaseNode>();
                            while (!ConsumeIf("E"))
                            {
                                Expression = ParseExpression();
                                if (Expression == null)
                                {
                                    return null;
                                }

                                Names.Add(Expression);
                            }
                            return new CallExpression(Callee, Names);
                        case 'm':
                            Position += 2;
                            return ParseBinaryExpression(",");
                        case 'o':
                            Position += 2;
                            return ParsePrefixExpression("~");
                        case 'v':
                            return ParseConversionExpression();
                    }
                    return null;
                case 'd':
                    BaseNode LeftNode = null;
                    BaseNode RightNode = null;
                    switch (Peek(1))
                    {
                        case 'a':
                            Position += 2;
                            Expression = ParseExpression();
                            if (Expression == null)
                            {
                                return Expression;
                            }

                            return new DeleteExpression(Expression, IsGlobal, true);
                        case 'c':
                            Position += 2;
                            BaseNode Type = ParseType();
                            if (Type == null)
                            {
                                return null;
                            }

                            Expression = ParseExpression();
                            if (Expression == null)
                            {
                                return Expression;
                            }

                            return new CastExpression("dynamic_cast", Type, Expression);
                        case 'e':
                            Position += 2;
                            return ParsePrefixExpression("*");
                        case 'l':
                            Position += 2;
                            Expression = ParseExpression();
                            if (Expression == null)
                            {
                                return null;
                            }

                            return new DeleteExpression(Expression, IsGlobal, false);
                        case 'n':
                            return ParseUnresolvedName();
                        case 's':
                            Position += 2;
                            LeftNode = ParseExpression();
                            if (LeftNode == null)
                            {
                                return null;
                            }

                            RightNode = ParseExpression();
                            if (RightNode == null)
                            {
                                return null;
                            }

                            return new MemberExpression(LeftNode, ".*", RightNode);
                        case 't':
                            Position += 2;
                            LeftNode = ParseExpression();
                            if (LeftNode == null)
                            {
                                return null;
                            }

                            RightNode = ParseExpression();
                            if (RightNode == null)
                            {
                                return null;
                            }

                            return new MemberExpression(LeftNode, ".", RightNode);
                        case 'v':
                            Position += 2;
                            return ParseBinaryExpression("/");
                        case 'V':
                            Position += 2;
                            return ParseBinaryExpression("/=");
                    }
                    return null;
                case 'e':
                    switch (Peek(1))
                    {
                        case 'o':
                            Position += 2;
                            return ParseBinaryExpression("^");
                        case 'O':
                            Position += 2;
                            return ParseBinaryExpression("^=");
                        case 'q':
                            Position += 2;
                            return ParseBinaryExpression("==");
                    }
                    return null;
                case 'g':
                    switch (Peek(1))
                    {
                        case 'e':
                            Position += 2;
                            return ParseBinaryExpression(">=");
                        case 't':
                            Position += 2;
                            return ParseBinaryExpression(">");
                    }
                    return null;
                case 'i':
                    switch (Peek(1))
                    {
                        case 'x':
                            Position += 2;
                            BaseNode Base = ParseExpression();
                            if (Base == null)
                            {
                                return null;
                            }

                            BaseNode Subscript = ParseExpression();
                            if (Base == null)
                            {
                                return null;
                            }

                            return new ArraySubscriptingExpression(Base, Subscript);
                        case 'l':
                            Position += 2;

                            List<BaseNode> BracedExpressions = new List<BaseNode>();
                            while (!ConsumeIf("E"))
                            {
                                Expression = ParseBracedExpression();
                                if (Expression == null)
                                {
                                    return null;
                                }

                                BracedExpressions.Add(Expression);
                            }
                            return new InitListExpression(null, BracedExpressions);
                    }
                    return null;
                case 'l':
                    switch (Peek(1))
                    {
                        case 'e':
                            Position += 2;
                            return ParseBinaryExpression("<=");
                        case 's':
                            Position += 2;
                            return ParseBinaryExpression("<<");
                        case 'S':
                            Position += 2;
                            return ParseBinaryExpression("<<=");
                        case 't':
                            Position += 2;
                            return ParseBinaryExpression("<");
                    }
                    return null;
                case 'm':
                    switch (Peek(1))
                    {
                        case 'i':
                            Position += 2;
                            return ParseBinaryExpression("-");
                        case 'I':
                            Position += 2;
                            return ParseBinaryExpression("-=");
                        case 'l':
                            Position += 2;
                            return ParseBinaryExpression("*");
                        case 'L':
                            Position += 2;
                            return ParseBinaryExpression("*=");
                        case 'm':
                            Position += 2;
                            if (ConsumeIf("_"))
                            {
                                return ParsePrefixExpression("--");
                            }

                            Expression = ParseExpression();
                            if (Expression == null)
                            {
                                return null;
                            }

                            return new PostfixExpression(Expression, "--");
                    }
                    return null;
                case 'n':
                    switch (Peek(1))
                    {
                        case 'a':
                        case 'w':
                            Position += 2;
                            return ParseNewExpression();
                        case 'e':
                            Position += 2;
                            return ParseBinaryExpression("!=");
                        case 'g':
                            Position += 2;
                            return ParsePrefixExpression("-");
                        case 't':
                            Position += 2;
                            return ParsePrefixExpression("!");
                        case 'x':
                            Position += 2;
                            Expression = ParseExpression();
                            if (Expression == null)
                            {
                                return null;
                            }

                            return new EnclosedExpression("noexcept (", Expression, ")");
                    }
                    return null;
                case 'o':
                    switch (Peek(1))
                    {
                        case 'n':
                            return ParseUnresolvedName();
                        case 'o':
                            Position += 2;
                            return ParseBinaryExpression("||");
                        case 'r':
                            Position += 2;
                            return ParseBinaryExpression("|");
                        case 'R':
                            Position += 2;
                            return ParseBinaryExpression("|=");
                    }
                    return null;
                case 'p':
                    switch (Peek(1))
                    {
                        case 'm':
                            Position += 2;
                            return ParseBinaryExpression("->*");
                        case 'l':
                        case 's':
                            Position += 2;
                            return ParseBinaryExpression("+");
                        case 'L':
                            Position += 2;
                            return ParseBinaryExpression("+=");
                        case 'p':
                            Position += 2;
                            if (ConsumeIf("_"))
                            {
                                return ParsePrefixExpression("++");
                            }

                            Expression = ParseExpression();
                            if (Expression == null)
                            {
                                return null;
                            }

                            return new PostfixExpression(Expression, "++");
                        case 't':
                            Position += 2;
                            LeftNode = ParseExpression();
                            if (LeftNode == null)
                            {
                                return null;
                            }

                            RightNode = ParseExpression();
                            if (RightNode == null)
                            {
                                return null;
                            }

                            return new MemberExpression(LeftNode, "->", RightNode);
                    }
                    return null;
                case 'q':
                    if (Peek(1) == 'u')
                    {
                        Position += 2;
                        BaseNode Condition = ParseExpression();
                        if (Condition == null)
                        {
                            return null;
                        }

                        LeftNode = ParseExpression();
                        if (LeftNode == null)
                        {
                            return null;
                        }

                        RightNode = ParseExpression();
                        if (RightNode == null)
                        {
                            return null;
                        }

                        return new ConditionalExpression(Condition, LeftNode, RightNode);
                    }
                    return null;
                case 'r':
                    switch (Peek(1))
                    {
                        case 'c':
                            Position += 2;
                            BaseNode To = ParseType();
                            if (To == null)
                            {
                                return null;
                            }

                            BaseNode From = ParseExpression();
                            if (From == null)
                            {
                                return null;
                            }

                            return new CastExpression("reinterpret_cast", To, From);
                        case 'm':
                            Position += 2;
                            return ParseBinaryExpression("%");
                        case 'M':
                            Position += 2;
                            return ParseBinaryExpression("%");
                        case 's':
                            Position += 2;
                            return ParseBinaryExpression(">>");
                        case 'S':
                            Position += 2;
                            return ParseBinaryExpression(">>=");
                    }
                    return null;
                case 's':
                    switch (Peek(1))
                    {
                        case 'c':
                            Position += 2;
                            BaseNode To = ParseType();
                            if (To == null)
                            {
                                return null;
                            }

                            BaseNode From = ParseExpression();
                            if (From == null)
                            {
                                return null;
                            }

                            return new CastExpression("static_cast", To, From);
                        case 'p':
                            Position += 2;
                            Expression = ParseExpression();
                            if (Expression == null)
                            {
                                return null;
                            }

                            return new PackedTemplateParameterExpansion(Expression);
                        case 'r':
                            return ParseUnresolvedName();
                        case 't':
                            Position += 2;
                            BaseNode EnclosedType = ParseType();
                            if (EnclosedType == null)
                            {
                                return null;
                            }

                            return new EnclosedExpression("sizeof (", EnclosedType, ")");
                        case 'z':
                            Position += 2;
                            Expression = ParseExpression();
                            if (Expression == null)
                            {
                                return null;
                            }

                            return new EnclosedExpression("sizeof (", Expression, ")");
                        case 'Z':
                            Position += 2;
                            BaseNode SizeofParamNode = null;
                            switch (Peek())
                            {
                                case 'T':
                                    // FIXME: ??? Not entire sure if it's right
                                    SizeofParamNode = ParseFunctionParameter();
                                    if (SizeofParamNode == null)
                                    {
                                        return null;
                                    }

                                    return new EnclosedExpression("sizeof...(", new PackedTemplateParameterExpansion(SizeofParamNode), ")");
                                case 'f':
                                    SizeofParamNode = ParseFunctionParameter();
                                    if (SizeofParamNode == null)
                                    {
                                        return null;
                                    }

                                    return new EnclosedExpression("sizeof...(", SizeofParamNode, ")");
                            }
                            return null;
                        case 'P':
                            Position += 2;
                            List<BaseNode> Arguments = new List<BaseNode>();
                            while (!ConsumeIf("E"))
                            {
                                BaseNode Argument = ParseTemplateArgument();
                                if (Argument == null)
                                {
                                    return null;
                                }

                                Arguments.Add(Argument);
                            }
                            return new EnclosedExpression("sizeof...(", new NodeArray(Arguments), ")");
                    }
                    return null;
                case 't':
                    switch (Peek(1))
                    {
                        case 'e':
                            Expression = ParseExpression();
                            if (Expression == null)
                            {
                                return null;
                            }

                            return new EnclosedExpression("typeid (", Expression, ")");
                        case 't':
                            BaseNode EnclosedType = ParseExpression();
                            if (EnclosedType == null)
                            {
                                return null;
                            }

                            return new EnclosedExpression("typeid (", EnclosedType, ")");
                        case 'l':
                            Position += 2;
                            BaseNode TypeNode = ParseType();
                            if (TypeNode == null)
                            {
                                return null;
                            }

                            List<BaseNode> BracedExpressions = new List<BaseNode>();
                            while (!ConsumeIf("E"))
                            {
                                Expression = ParseBracedExpression();
                                if (Expression == null)
                                {
                                    return null;
                                }

                                BracedExpressions.Add(Expression);
                            }
                            return new InitListExpression(TypeNode, BracedExpressions);
                        case 'r':
                            Position += 2;
                            return new NameType("throw");
                        case 'w':
                            Position += 2;
                            Expression = ParseExpression();
                            if (Expression == null)
                            {
                                return null;
                            }

                            return new ThrowExpression(Expression);
                    }
                    return null;
            }

            if (char.IsDigit(Peek()))
            {
                return ParseUnresolvedName();
            }

            return null;
        }

        private BaseNode ParseIntegerLiteral(string LiteralName)
        {
            string Number = ParseNumber(true);
            if (Number == null || Number.Length == 0 || !ConsumeIf("E"))
            {
                return null;
            }

            return new IntegerLiteral(LiteralName, Number);
        }

        // <expr-primary> ::= L <type> <value number> E                          # integer literal
        //                ::= L <type> <value float> E                           # floating literal (TODO)
        //                ::= L <string type> E                                  # string literal
        //                ::= L <nullptr type> E                                 # nullptr literal (i.e., "LDnE")
        //                ::= L <pointer type> 0 E                               # null pointer template argument
        //                ::= L <type> <real-part float> _ <imag-part float> E   # complex floating point literal (C 2000)
        //                ::= L _Z <encoding> E                                  # external name
        private BaseNode ParseExpressionPrimary()
        {
            if (!ConsumeIf("L"))
            {
                return null;
            }

            switch (Peek())
            {
                case 'w':
                    Position++;
                    return ParseIntegerLiteral("wchar_t");
                case 'b':
                    if (ConsumeIf("b0E"))
                    {
                        return new NameType("false", NodeType.BooleanExpression);
                    }

                    if (ConsumeIf("b1E"))
                    {
                        return new NameType("true", NodeType.BooleanExpression);
                    }

                    return null;
                case 'c':
                    Position++;
                    return ParseIntegerLiteral("char");
                case 'a':
                    Position++;
                    return ParseIntegerLiteral("signed char");
                case 'h':
                    Position++;
                    return ParseIntegerLiteral("unsigned char");
                case 's':
                    Position++;
                    return ParseIntegerLiteral("short");
                case 't':
                    Position++;
                    return ParseIntegerLiteral("unsigned short");
                case 'i':
                    Position++;
                    return ParseIntegerLiteral("");
                case 'j':
                    Position++;
                    return ParseIntegerLiteral("u");
                case 'l':
                    Position++;
                    return ParseIntegerLiteral("l");
                case 'm':
                    Position++;
                    return ParseIntegerLiteral("ul");
                case 'x':
                    Position++;
                    return ParseIntegerLiteral("ll");
                case 'y':
                    Position++;
                    return ParseIntegerLiteral("ull");
                case 'n':
                    Position++;
                    return ParseIntegerLiteral("__int128");
                case 'o':
                    Position++;
                    return ParseIntegerLiteral("unsigned __int128");
                case 'd':
                case 'e':
                case 'f':
                    // TODO: floating literal
                    return null;
                case '_':
                    if (ConsumeIf("_Z"))
                    {
                        BaseNode Encoding = ParseEncoding();
                        if (Encoding != null && ConsumeIf("E"))
                        {
                            return Encoding;
                        }
                    }
                    return null;
                case 'T':
                    return null;
                default:
                    BaseNode Type = ParseType();
                    if (Type == null)
                    {
                        return null;
                    }

                    string Number = ParseNumber();
                    if (Number == null || Number.Length == 0 || !ConsumeIf("E"))
                    {
                        return null;
                    }

                    return new IntegerCastExpression(Type, Number);
            }
        }

        // <decltype>  ::= Dt <expression> E  # decltype of an id-expression or class member access (C++0x)
        //             ::= DT <expression> E  # decltype of an expression (C++0x)
        private BaseNode ParseDecltype()
        {
            if (!ConsumeIf("D") || (!ConsumeIf("t") && !ConsumeIf("T")))
            {
                return null;
            }

            BaseNode Expression = ParseExpression();
            if (Expression == null)
            {
                return null;
            }

            if (!ConsumeIf("E"))
            {
                return null;
            }

            return new EnclosedExpression("decltype(", Expression, ")");
        }

        // <template-param>          ::= T_ # first template parameter
        //                           ::= T <parameter-2 non-negative number> _
        // <template-template-param> ::= <template-param>
        //                           ::= <substitution>
        private BaseNode ParseTemplateParam()
        {
            if (!ConsumeIf("T"))
            {
                return null;
            }

            int Index = 0;
            if (!ConsumeIf("_"))
            {
                Index = ParsePositiveNumber();
                if (Index < 0)
                {
                    return null;
                }

                Index++;
                if (!ConsumeIf("_"))
                {
                    return null;
                }
            }

            // 5.1.8: TODO: lambda?
            // if (IsParsingLambdaParameters)
            //    return new NameType("auto");

            if (CanForwardTemplateReference)
            {
                ForwardTemplateReference ForwardTemplateReference = new ForwardTemplateReference(Index);
                ForwardTemplateReferenceList.Add(ForwardTemplateReference);
                return ForwardTemplateReference;
            }
            if (Index >= TemplateParamList.Count)
            {
                return null;
            }

            return TemplateParamList[Index];
        }

        // <template-args> ::= I <template-arg>+ E
        private BaseNode ParseTemplateArguments(bool HasContext = false)
        {
            if (!ConsumeIf("I"))
            {
                return null;
            }

            if (HasContext)
            {
                TemplateParamList.Clear();
            }

            List<BaseNode> Args = new List<BaseNode>();
            while (!ConsumeIf("E"))
            {
                if (HasContext)
                {
                    List<BaseNode> TemplateParamListTemp = new List<BaseNode>(TemplateParamList);
                    BaseNode TemplateArgument = ParseTemplateArgument();
                    TemplateParamList = TemplateParamListTemp;
                    if (TemplateArgument == null)
                    {
                        return null;
                    }

                    Args.Add(TemplateArgument);
                    if (TemplateArgument.GetType().Equals(NodeType.PackedTemplateArgument))
                    {
                        TemplateArgument = new PackedTemplateParameter(((NodeArray)TemplateArgument).Nodes);
                    }
                    TemplateParamList.Add(TemplateArgument);
                }
                else
                {
                    BaseNode TemplateArgument = ParseTemplateArgument();
                    if (TemplateArgument == null)
                    {
                        return null;
                    }

                    Args.Add(TemplateArgument);
                }
            }
            return new TemplateArguments(Args);
        }


        // <template-arg> ::= <type>                                             # type or template
        //                ::= X <expression> E                                   # expression
        //                ::= <expr-primary>                                     # simple expressions
        //                ::= J <template-arg>* E                                # argument pack
        private BaseNode ParseTemplateArgument()
        {
            switch (Peek())
            {
                // X <expression> E
                case 'X':
                    Position++;
                    BaseNode Expression = ParseExpression();
                    if (Expression == null || !ConsumeIf("E"))
                    {
                        return null;
                    }

                    return Expression;
                // <expr-primary>
                case 'L':
                    return ParseExpressionPrimary();
                // J <template-arg>* E
                case 'J':
                    Position++;
                    List<BaseNode> TemplateArguments = new List<BaseNode>();
                    while (!ConsumeIf("E"))
                    {
                        BaseNode TemplateArgument = ParseTemplateArgument();
                        if (TemplateArgument == null)
                        {
                            return null;
                        }

                        TemplateArguments.Add(TemplateArgument);
                    }
                    return new NodeArray(TemplateArguments, NodeType.PackedTemplateArgument);
                // <type>
                default:
                    return ParseType();
            }
        }

        class NameParserContext
        {
            public CVType CV;
            public SimpleReferenceType Ref;
            public bool FinishWithTemplateArguments;
            public bool CtorDtorConversion;
        }


        //   <unresolved-type> ::= <template-param> [ <template-args> ]            # T:: or T<X,Y>::
        //                     ::= <decltype>                                      # decltype(p)::
        //                     ::= <substitution>
        private BaseNode ParseUnresolvedType()
        {
            if (Peek() == 'T')
            {
                BaseNode TemplateParam = ParseTemplateParam();
                if (TemplateParam == null)
                {
                    return null;
                }

                SubstitutionList.Add(TemplateParam);
                return TemplateParam;
            }
            else if (Peek() == 'D')
            {
                BaseNode DeclType = ParseDecltype();
                if (DeclType == null)
                {
                    return null;
                }

                SubstitutionList.Add(DeclType);
                return DeclType;
            }
            return ParseSubstitution();
        }

        // <simple-id> ::= <source-name> [ <template-args> ]
        private BaseNode ParseSimpleId()
        {
            BaseNode SourceName = ParseSourceName();
            if (SourceName == null)
            {
                return null;
            }

            if (Peek() == 'I')
            {
                BaseNode TemplateArguments = ParseTemplateArguments();
                if (TemplateArguments == null)
                {
                    return null;
                }

                return new NameTypeWithTemplateArguments(SourceName, TemplateArguments);
            }
            return SourceName;
        }

        //  <destructor-name> ::= <unresolved-type>                               # e.g., ~T or ~decltype(f())
        //                    ::= <simple-id>                                     # e.g., ~A<2*N>
        private BaseNode ParseDestructorName()
        {
            BaseNode Node;
            if (char.IsDigit(Peek()))
            {
                Node = ParseSimpleId();
            }
            else
            {
                Node = ParseUnresolvedType();
            }
            if (Node == null)
            {
                return null;
            }

            return new DtorName(Node);
        }

        //  <base-unresolved-name> ::= <simple-id>                                # unresolved name
        //  extension              ::= <operator-name>                            # unresolved operator-function-id
        //  extension              ::= <operator-name> <template-args>            # unresolved operator template-id
        //                         ::= on <operator-name>                         # unresolved operator-function-id
        //                         ::= on <operator-name> <template-args>         # unresolved operator template-id
        //                         ::= dn <destructor-name>                       # destructor or pseudo-destructor;
        //                                                                        # e.g. ~X or ~X<N-1>
        private BaseNode ParseBaseUnresolvedName()
        {
            if (char.IsDigit(Peek()))
            {
                return ParseSimpleId();
            }
            else if (ConsumeIf("dn"))
            {
                return ParseDestructorName();
            }

            ConsumeIf("on");
            BaseNode OperatorName = ParseOperatorName(null);
            if (OperatorName == null)
            {
                return null;
            }

            if (Peek() == 'I')
            {
                BaseNode TemplateArguments = ParseTemplateArguments();
                if (TemplateArguments == null)
                {
                    return null;
                }

                return new NameTypeWithTemplateArguments(OperatorName, TemplateArguments);
            }
            return OperatorName;
        }

        // <unresolved-name> ::= [gs] <base-unresolved-name>                     # x or (with "gs") ::x
        //                   ::= sr <unresolved-type> <base-unresolved-name>     # T::x / decltype(p)::x
        //                   ::= srN <unresolved-type> <unresolved-qualifier-level>+ E <base-unresolved-name>
        //                                                                       # T::N::x /decltype(p)::N::x
        //                   ::= [gs] sr <unresolved-qualifier-level>+ E <base-unresolved-name>
        //                                                                       # A::x, N::y, A<T>::z; "gs" means leading "::"
        private BaseNode ParseUnresolvedName(NameParserContext Context = null)
        {
            BaseNode Result = null;
            if (ConsumeIf("srN"))
            {
                Result = ParseUnresolvedType();
                if (Result == null)
                {
                    return null;
                }

                if (Peek() == 'I')
                {
                    BaseNode TemplateArguments = ParseTemplateArguments();
                    if (TemplateArguments == null)
                    {
                        return null;
                    }

                    Result = new NameTypeWithTemplateArguments(Result, TemplateArguments);
                    if (Result == null)
                    {
                        return null;
                    }
                }

                while (!ConsumeIf("E"))
                {
                    BaseNode SimpleId = ParseSimpleId();
                    if (SimpleId == null)
                    {
                        return null;
                    }

                    Result = new QualifiedName(Result, SimpleId);
                    if (Result == null)
                    {
                        return null;
                    }
                }

                BaseNode BaseName = ParseBaseUnresolvedName();
                if (BaseName == null)
                {
                    return null;
                }

                return new QualifiedName(Result, BaseName);
            }

            bool IsGlobal = ConsumeIf("gs");

            // ::= [gs] <base-unresolved-name>                     # x or (with "gs") ::x
            if (!ConsumeIf("sr"))
            {
                Result = ParseBaseUnresolvedName();
                if (Result == null)
                {
                    return null;
                }

                if (IsGlobal)
                {
                    Result = new GlobalQualifiedName(Result);
                }

                return Result;
            }

            // ::= [gs] sr <unresolved-qualifier-level>+ E <base-unresolved-name>
            if (char.IsDigit(Peek()))
            {
                do
                {
                    BaseNode Qualifier = ParseSimpleId();
                    if (Qualifier == null)
                    {
                        return null;
                    }

                    if (Result != null)
                    {
                        Result = new QualifiedName(Result, Qualifier);
                    }
                    else if (IsGlobal)
                    {
                        Result = new GlobalQualifiedName(Qualifier);
                    }
                    else
                    {
                        Result = Qualifier;
                    }

                    if (Result == null)
                    {
                        return null;
                    }
                } while (!ConsumeIf("E"));
            }
            // ::= sr <unresolved-type> [tempate-args] <base-unresolved-name>     # T::x / decltype(p)::x
            else
            {
                Result = ParseUnresolvedType();
                if (Result == null)
                {
                    return null;
                }

                if (Peek() == 'I')
                {
                    BaseNode TemplateArguments = ParseTemplateArguments();
                    if (TemplateArguments == null)
                    {
                        return null;
                    }

                    Result = new NameTypeWithTemplateArguments(Result, TemplateArguments);
                    if (Result == null)
                    {
                        return null;
                    }
                }
            }

            if (Result == null)
            {
                return null;
            }

            BaseNode BaseUnresolvedName = ParseBaseUnresolvedName();
            if (BaseUnresolvedName == null)
            {
                return null;
            }

            return new QualifiedName(Result, BaseUnresolvedName);
        }

        //    <unscoped-name> ::= <unqualified-name>
        //                    ::= St <unqualified-name>   # ::std::
        private BaseNode ParseUnscopedName(NameParserContext Context)
        {
            if (ConsumeIf("St"))
            {
                BaseNode UnresolvedName = ParseUnresolvedName(Context);
                if (UnresolvedName == null)
                {
                    return null;
                }

                return new StdQualifiedName(UnresolvedName);
            }
            return ParseUnresolvedName(Context);
        }

        // <nested-name> ::= N [<CV-qualifiers>] [<ref-qualifier>] <prefix (TODO)> <unqualified-name> E
        //               ::= N [<CV-qualifiers>] [<ref-qualifier>] <template-prefix (TODO)> <template-args (TODO)> E
        private BaseNode ParseNestedName(NameParserContext Context)
        {
            // Impossible in theory
            if (Consume() != 'N')
            {
                return null;
            }

            BaseNode Result = null;
            CVType CV = new CVType(ParseCVQualifiers(), null);
            if (Context != null)
            {
                Context.CV = CV;
            }

            SimpleReferenceType Ref = ParseRefQualifiers();
            if (Context != null)
            {
                Context.Ref = Ref;
            }

            if (ConsumeIf("St"))
            {
                Result = new NameType("std");
            }

            while (!ConsumeIf("E"))
            {
                // <data-member-prefix> end
                if (ConsumeIf("M"))
                {
                    if (Result == null)
                    {
                        return null;
                    }

                    continue;
                }
                char C = Peek();

                // TODO: template args
                if (C == 'T')
                {
                    BaseNode TemplateParam = ParseTemplateParam();
                    if (TemplateParam == null)
                    {
                        return null;
                    }

                    Result = CreateNameNode(Result, TemplateParam, Context);
                    SubstitutionList.Add(Result);
                    continue;
                }

                // <template-prefix> <template-args>
                if (C == 'I')
                {
                    BaseNode TemplateArgument = ParseTemplateArguments(Context != null);
                    if (TemplateArgument == null || Result == null)
                    {
                        return null;
                    }

                    Result = new NameTypeWithTemplateArguments(Result, TemplateArgument);
                    if (Context != null)
                    {
                        Context.FinishWithTemplateArguments = true;
                    }

                    SubstitutionList.Add(Result);
                    continue;
                }

                // <decltype>
                if (C == 'D' && (Peek(1) == 't' || Peek(1) == 'T'))
                {
                    BaseNode Decltype = ParseDecltype();
                    if (Decltype == null)
                    {
                        return null;
                    }

                    Result = CreateNameNode(Result, Decltype, Context);
                    SubstitutionList.Add(Result);
                    continue;
                }

                // <substitution>
                if (C == 'S' && Peek(1) != 't')
                {
                    BaseNode Substitution = ParseSubstitution();
                    if (Substitution == null)
                    {
                        return null;
                    }

                    Result = CreateNameNode(Result, Substitution, Context);
                    if (Result != Substitution)
                    {
                        SubstitutionList.Add(Substitution);
                    }

                    continue;
                }

                // <ctor-dtor-name> of ParseUnqualifiedName
                if (C == 'C' || (C == 'D' && Peek(1) != 'C'))
                {
                    // We cannot have nothing before this
                    if (Result == null)
                    {
                        return null;
                    }

                    BaseNode CtOrDtorName = ParseCtorDtorName(Context, Result);

                    if (CtOrDtorName == null)
                    {
                        return null;
                    }

                    Result = CreateNameNode(Result, CtOrDtorName, Context);

                    // TODO: ABI Tags (before)
                    if (Result == null)
                    {
                        return null;
                    }

                    SubstitutionList.Add(Result);
                    continue;
                }

                BaseNode UnqualifiedName = ParseUnqualifiedName(Context);
                if (UnqualifiedName == null)
                {
                    return null;
                }
                Result = CreateNameNode(Result, UnqualifiedName, Context);

                SubstitutionList.Add(Result);
            }
            if (Result == null || SubstitutionList.Count == 0)
            {
                return null;
            }

            SubstitutionList.RemoveAt(SubstitutionList.Count - 1);
            return Result;
        }

        //   <discriminator> ::= _ <non-negative number>      # when number < 10
        //                   ::= __ <non-negative number> _   # when number >= 10
        private void ParseDiscriminator()
        {
            if (Count() == 0)
            {
                return;
            }
            // We ignore the discriminator, we don't need it.
            if (ConsumeIf("_"))
            {
                ConsumeIf("_");
                while (char.IsDigit(Peek()) && Count() != 0)
                {
                    Consume();
                }
                ConsumeIf("_");
            }
        }

        //   <local-name> ::= Z <function encoding> E <entity name> [<discriminator>]
        //                ::= Z <function encoding> E s [<discriminator>]
        //                ::= Z <function encoding> Ed [ <parameter number> ] _ <entity name>
        private BaseNode ParseLocalName(NameParserContext Context)
        {
            if (!ConsumeIf("Z"))
            {
                return null;
            }

            BaseNode Encoding = ParseEncoding();
            if (Encoding == null || !ConsumeIf("E"))
            {
                return null;
            }

            BaseNode EntityName;
            if (ConsumeIf("s"))
            {
                ParseDiscriminator();
                return new LocalName(Encoding, new NameType("string literal"));
            }
            else if (ConsumeIf("d"))
            {
                ParseNumber(true);
                if (!ConsumeIf("_"))
                {
                    return null;
                }

                EntityName = ParseName(Context);
                if (EntityName == null)
                {
                    return null;
                }

                return new LocalName(Encoding, EntityName);
            }

            EntityName = ParseName(Context);
            if (EntityName == null)
            {
                return null;
            }

            ParseDiscriminator();
            return new LocalName(Encoding, EntityName);
        }

        // <name> ::= <nested-name>
        //        ::= <unscoped-name>
        //        ::= <unscoped-template-name> <template-args>
        //        ::= <local-name>  # See Scope Encoding below (TODO)
        private BaseNode ParseName(NameParserContext Context = null)
        {
            ConsumeIf("L");

            if (Peek() == 'N')
            {
                return ParseNestedName(Context);
            }

            if (Peek() == 'Z')
            {
                return ParseLocalName(Context);
            }

            if (Peek() == 'S' && Peek(1) != 't')
            {
                BaseNode Substitution = ParseSubstitution();
                if (Substitution == null)
                {
                    return null;
                }

                if (Peek() != 'I')
                {
                    return null;
                }

                BaseNode TemplateArguments = ParseTemplateArguments(Context != null);
                if (TemplateArguments == null)
                {
                    return null;
                }

                if (Context != null)
                {
                    Context.FinishWithTemplateArguments = true;
                }

                return new NameTypeWithTemplateArguments(Substitution, TemplateArguments);
            }

            BaseNode Result = ParseUnscopedName(Context);
            if (Result == null)
            {
                return null;
            }

            if (Peek() == 'I')
            {
                SubstitutionList.Add(Result);
                BaseNode TemplateArguments = ParseTemplateArguments(Context != null);
                if (TemplateArguments == null)
                {
                    return null;
                }

                if (Context != null)
                {
                    Context.FinishWithTemplateArguments = true;
                }

                return new NameTypeWithTemplateArguments(Result, TemplateArguments);
            }

            return Result;
        }

        private bool IsEncodingEnd()
        {
            char C = Peek();
            return Count() == 0 || C == 'E' || C == '.' || C == '_';
        }

        // <encoding> ::= <function name> <bare-function-type>
        //            ::= <data name>
        //            ::= <special-name>
        private BaseNode ParseEncoding()
        {
            NameParserContext Context = new NameParserContext();
            if (Peek() == 'T' || (Peek() == 'G' && Peek(1) == 'V'))
            {
                return ParseSpecialName(Context);
            }

            BaseNode Name = ParseName(Context);
            if (Name == null)
            {
                return null;
            }

            // TODO: compute template refs here

            if (IsEncodingEnd())
            {
                return Name;
            }

            // TODO: Ua9enable_ifI

            BaseNode ReturnType = null;
            if (!Context.CtorDtorConversion && Context.FinishWithTemplateArguments)
            {
                ReturnType = ParseType();
                if (ReturnType == null)
                {
                    return null;
                }
            }

            if (ConsumeIf("v"))
            {
                return new EncodedFunction(Name, null, Context.CV, Context.Ref, null, ReturnType);
            }

            List<BaseNode> Params = new List<BaseNode>();

            // backup because that can be destroyed by parseType
            CVType CV = Context.CV;
            SimpleReferenceType Ref = Context.Ref;

            while (!IsEncodingEnd())
            {
                BaseNode Param = ParseType();
                if (Param == null)
                {
                    return null;
                }

                Params.Add(Param);
            }

            return new EncodedFunction(Name, new NodeArray(Params), CV, Ref, null, ReturnType);
        }

        // <mangled-name> ::= _Z <encoding>
        //                ::= <type>
        private BaseNode Parse()
        {
            if (ConsumeIf("_Z"))
            {
                BaseNode Encoding = ParseEncoding();
                if (Encoding != null && Count() == 0)
                {
                    return Encoding;
                }
                return null;
            }
            else
            {
                BaseNode Type = ParseType();
                if (Type != null && Count() == 0)
                {
                    return Type;
                }
                return null;
            }
        }

        public static string Parse(string OriginalMangled)
        {
            Demangler Instance = new Demangler(OriginalMangled);
            BaseNode ResNode   = Instance.Parse();

            if (ResNode != null)
            {
                StringWriter Writer = new StringWriter();
                ResNode.Print(Writer);
                return Writer.ToString();
            }

            return OriginalMangled;
        }
    }
}
