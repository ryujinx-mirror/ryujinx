using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;

using static Ryujinx.Graphics.Shader.CodeGen.Glsl.TypeConversion;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions
{
    static class InstGenHelper
    {
        private static InstInfo[] _infoTbl;

        static InstGenHelper()
        {
            _infoTbl = new InstInfo[(int)Instruction.Count];

            Add(Instruction.AtomicAdd,                InstType.AtomicBinary,   "atomicAdd");
            Add(Instruction.AtomicAnd,                InstType.AtomicBinary,   "atomicAnd");
            Add(Instruction.AtomicCompareAndSwap,     InstType.AtomicTernary,  "atomicCompSwap");
            Add(Instruction.AtomicMaxS32,             InstType.CallTernary,    HelperFunctionNames.AtomicMaxS32);
            Add(Instruction.AtomicMaxU32,             InstType.AtomicBinary,   "atomicMax");
            Add(Instruction.AtomicMinS32,             InstType.CallTernary,    HelperFunctionNames.AtomicMinS32);
            Add(Instruction.AtomicMinU32,             InstType.AtomicBinary,   "atomicMin");
            Add(Instruction.AtomicOr,                 InstType.AtomicBinary,   "atomicOr");
            Add(Instruction.AtomicSwap,               InstType.AtomicBinary,   "atomicExchange");
            Add(Instruction.AtomicXor,                InstType.AtomicBinary,   "atomicXor");
            Add(Instruction.Absolute,                 InstType.CallUnary,      "abs");
            Add(Instruction.Add,                      InstType.OpBinaryCom,    "+",               2);
            Add(Instruction.Ballot,                   InstType.CallUnary,      "ballotARB");
            Add(Instruction.Barrier,                  InstType.CallNullary,    "barrier");
            Add(Instruction.BitCount,                 InstType.CallUnary,      "bitCount");
            Add(Instruction.BitfieldExtractS32,       InstType.CallTernary,    "bitfieldExtract");
            Add(Instruction.BitfieldExtractU32,       InstType.CallTernary,    "bitfieldExtract");
            Add(Instruction.BitfieldInsert,           InstType.CallQuaternary, "bitfieldInsert");
            Add(Instruction.BitfieldReverse,          InstType.CallUnary,      "bitfieldReverse");
            Add(Instruction.BitwiseAnd,               InstType.OpBinaryCom,    "&",               6);
            Add(Instruction.BitwiseExclusiveOr,       InstType.OpBinaryCom,    "^",               7);
            Add(Instruction.BitwiseNot,               InstType.OpUnary,        "~",               0);
            Add(Instruction.BitwiseOr,                InstType.OpBinaryCom,    "|",               8);
            Add(Instruction.Call,                     InstType.Special);
            Add(Instruction.Ceiling,                  InstType.CallUnary,      "ceil");
            Add(Instruction.Clamp,                    InstType.CallTernary,    "clamp");
            Add(Instruction.ClampU32,                 InstType.CallTernary,    "clamp");
            Add(Instruction.CompareEqual,             InstType.OpBinaryCom,    "==",              5);
            Add(Instruction.CompareGreater,           InstType.OpBinary,       ">",               4);
            Add(Instruction.CompareGreaterOrEqual,    InstType.OpBinary,       ">=",              4);
            Add(Instruction.CompareGreaterOrEqualU32, InstType.OpBinary,       ">=",              4);
            Add(Instruction.CompareGreaterU32,        InstType.OpBinary,       ">",               4);
            Add(Instruction.CompareLess,              InstType.OpBinary,       "<",               4);
            Add(Instruction.CompareLessOrEqual,       InstType.OpBinary,       "<=",              4);
            Add(Instruction.CompareLessOrEqualU32,    InstType.OpBinary,       "<=",              4);
            Add(Instruction.CompareLessU32,           InstType.OpBinary,       "<",               4);
            Add(Instruction.CompareNotEqual,          InstType.OpBinaryCom,    "!=",              5);
            Add(Instruction.ConditionalSelect,        InstType.OpTernary,      "?:",              12);
            Add(Instruction.ConvertFP32ToFP64,        InstType.CallUnary,      "double");
            Add(Instruction.ConvertFP64ToFP32,        InstType.CallUnary,      "float");
            Add(Instruction.ConvertFPToS32,           InstType.CallUnary,      "int");
            Add(Instruction.ConvertFPToU32,           InstType.CallUnary,      "uint");
            Add(Instruction.ConvertS32ToFP,           InstType.CallUnary,      "float");
            Add(Instruction.ConvertU32ToFP,           InstType.CallUnary,      "float");
            Add(Instruction.Cosine,                   InstType.CallUnary,      "cos");
            Add(Instruction.Ddx,                      InstType.CallUnary,      "dFdx");
            Add(Instruction.Ddy,                      InstType.CallUnary,      "dFdy");
            Add(Instruction.Discard,                  InstType.OpNullary,      "discard");
            Add(Instruction.Divide,                   InstType.OpBinary,       "/",               1);
            Add(Instruction.EmitVertex,               InstType.CallNullary,    "EmitVertex");
            Add(Instruction.EndPrimitive,             InstType.CallNullary,    "EndPrimitive");
            Add(Instruction.ExponentB2,               InstType.CallUnary,      "exp2");
            Add(Instruction.FindFirstSetS32,          InstType.CallUnary,      "findMSB");
            Add(Instruction.FindFirstSetU32,          InstType.CallUnary,      "findMSB");
            Add(Instruction.Floor,                    InstType.CallUnary,      "floor");
            Add(Instruction.FusedMultiplyAdd,         InstType.CallTernary,    "fma");
            Add(Instruction.GroupMemoryBarrier,       InstType.CallNullary,    "groupMemoryBarrier");
            Add(Instruction.ImageLoad,                InstType.Special);
            Add(Instruction.ImageStore,               InstType.Special);
            Add(Instruction.IsNan,                    InstType.CallUnary,      "isnan");
            Add(Instruction.LoadAttribute,            InstType.Special);
            Add(Instruction.LoadConstant,             InstType.Special);
            Add(Instruction.LoadLocal,                InstType.Special);
            Add(Instruction.LoadShared,               InstType.Special);
            Add(Instruction.LoadStorage,              InstType.Special);
            Add(Instruction.Lod,                      InstType.Special);
            Add(Instruction.LogarithmB2,              InstType.CallUnary,      "log2");
            Add(Instruction.LogicalAnd,               InstType.OpBinaryCom,    "&&",              9);
            Add(Instruction.LogicalExclusiveOr,       InstType.OpBinaryCom,    "^^",              10);
            Add(Instruction.LogicalNot,               InstType.OpUnary,        "!",               0);
            Add(Instruction.LogicalOr,                InstType.OpBinaryCom,    "||",              11);
            Add(Instruction.LoopBreak,                InstType.OpNullary,      "break");
            Add(Instruction.LoopContinue,             InstType.OpNullary,      "continue");
            Add(Instruction.PackDouble2x32,           InstType.Special);
            Add(Instruction.PackHalf2x16,             InstType.Special);
            Add(Instruction.Maximum,                  InstType.CallBinary,     "max");
            Add(Instruction.MaximumU32,               InstType.CallBinary,     "max");
            Add(Instruction.MemoryBarrier,            InstType.CallNullary,    "memoryBarrier");
            Add(Instruction.Minimum,                  InstType.CallBinary,     "min");
            Add(Instruction.MinimumU32,               InstType.CallBinary,     "min");
            Add(Instruction.Multiply,                 InstType.OpBinaryCom,    "*",               1);
            Add(Instruction.MultiplyHighS32,          InstType.CallBinary,     HelperFunctionNames.MultiplyHighS32);
            Add(Instruction.MultiplyHighU32,          InstType.CallBinary,     HelperFunctionNames.MultiplyHighU32);
            Add(Instruction.Negate,                   InstType.OpUnary,        "-",               0);
            Add(Instruction.ReciprocalSquareRoot,     InstType.CallUnary,      "inversesqrt");
            Add(Instruction.Return,                   InstType.OpNullary,      "return");
            Add(Instruction.Round,                    InstType.CallUnary,      "roundEven");
            Add(Instruction.ShiftLeft,                InstType.OpBinary,       "<<",              3);
            Add(Instruction.ShiftRightS32,            InstType.OpBinary,       ">>",              3);
            Add(Instruction.ShiftRightU32,            InstType.OpBinary,       ">>",              3);
            Add(Instruction.Shuffle,                  InstType.CallQuaternary, HelperFunctionNames.Shuffle);
            Add(Instruction.ShuffleDown,              InstType.CallQuaternary, HelperFunctionNames.ShuffleDown);
            Add(Instruction.ShuffleUp,                InstType.CallQuaternary, HelperFunctionNames.ShuffleUp);
            Add(Instruction.ShuffleXor,               InstType.CallQuaternary, HelperFunctionNames.ShuffleXor);
            Add(Instruction.Sine,                     InstType.CallUnary,      "sin");
            Add(Instruction.SquareRoot,               InstType.CallUnary,      "sqrt");
            Add(Instruction.StoreLocal,               InstType.Special);
            Add(Instruction.StoreShared,              InstType.Special);
            Add(Instruction.StoreStorage,             InstType.Special);
            Add(Instruction.Subtract,                 InstType.OpBinary,       "-",               2);
            Add(Instruction.SwizzleAdd,               InstType.CallTernary,    HelperFunctionNames.SwizzleAdd);
            Add(Instruction.TextureSample,            InstType.Special);
            Add(Instruction.TextureSize,              InstType.Special);
            Add(Instruction.Truncate,                 InstType.CallUnary,      "trunc");
            Add(Instruction.UnpackDouble2x32,         InstType.Special);
            Add(Instruction.UnpackHalf2x16,           InstType.Special);
            Add(Instruction.VoteAll,                  InstType.CallUnary,      "allInvocationsARB");
            Add(Instruction.VoteAllEqual,             InstType.CallUnary,      "allInvocationsEqualARB");
            Add(Instruction.VoteAny,                  InstType.CallUnary,      "anyInvocationARB");
        }

        private static void Add(Instruction inst, InstType flags, string opName = null, int precedence = 0)
        {
            _infoTbl[(int)inst] = new InstInfo(flags, opName, precedence);
        }

        public static InstInfo GetInstructionInfo(Instruction inst)
        {
            return _infoTbl[(int)(inst & Instruction.Mask)];
        }

        public static string GetSoureExpr(CodeGenContext context, IAstNode node, VariableType dstType)
        {
            return ReinterpretCast(context, node, OperandManager.GetNodeDestType(context, node), dstType);
        }

        public static string Enclose(string expr, IAstNode node, Instruction pInst, bool isLhs)
        {
            InstInfo pInfo = GetInstructionInfo(pInst);

            return Enclose(expr, node, pInst, pInfo, isLhs);
        }

        public static string Enclose(string expr, IAstNode node, Instruction pInst, InstInfo pInfo, bool isLhs = false)
        {
            if (NeedsParenthesis(node, pInst, pInfo, isLhs))
            {
                expr = "(" + expr + ")";
            }

            return expr;
        }

        public static bool NeedsParenthesis(IAstNode node, Instruction pInst, InstInfo pInfo, bool isLhs)
        {
            // If the node isn't a operation, then it can only be a operand,
            // and those never needs to be surrounded in parenthesis.
            if (!(node is AstOperation operation))
            {
                // This is sort of a special case, if this is a negative constant,
                // and it is consumed by a unary operation, we need to put on the parenthesis,
                // as in GLSL a sequence like --2 or ~-1 is not valid.
                if (IsNegativeConst(node) && pInfo.Type == InstType.OpUnary)
                {
                    return true;
                }

                return false;
            }

            if ((pInfo.Type & (InstType.Call | InstType.Special)) != 0)
            {
                return false;
            }

            InstInfo info = _infoTbl[(int)(operation.Inst & Instruction.Mask)];

            if ((info.Type & (InstType.Call | InstType.Special)) != 0)
            {
                return false;
            }

            if (info.Precedence < pInfo.Precedence)
            {
                return false;
            }

            if (info.Precedence == pInfo.Precedence && isLhs)
            {
                return false;
            }

            if (pInst == operation.Inst && info.Type == InstType.OpBinaryCom)
            {
                return false;
            }

            return true;
        }

        private static bool IsNegativeConst(IAstNode node)
        {
            if (!(node is AstOperand operand))
            {
                return false;
            }

            return operand.Type == OperandType.Constant && operand.Value < 0;
        }
    }
}