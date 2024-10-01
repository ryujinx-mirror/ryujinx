using FuncBinaryInstruction = System.Func<Spv.Generator.Instruction, Spv.Generator.Instruction, Spv.Generator.Instruction, Spv.Generator.Instruction>;
using FuncQuaternaryInstruction = System.Func<Spv.Generator.Instruction, Spv.Generator.Instruction, Spv.Generator.Instruction, Spv.Generator.Instruction, Spv.Generator.Instruction, Spv.Generator.Instruction>;
using FuncTernaryInstruction = System.Func<Spv.Generator.Instruction, Spv.Generator.Instruction, Spv.Generator.Instruction, Spv.Generator.Instruction, Spv.Generator.Instruction>;
using FuncUnaryInstruction = System.Func<Spv.Generator.Instruction, Spv.Generator.Instruction, Spv.Generator.Instruction>;

namespace Ryujinx.Graphics.Shader.CodeGen.Spirv
{
    /// <summary>
    /// Delegate cache for SPIR-V instruction generators. Avoids delegate allocation when passing generators as arguments.
    /// </summary>
    internal readonly struct SpirvDelegates
    {
        // Unary
        public readonly FuncUnaryInstruction GlslFAbs;
        public readonly FuncUnaryInstruction GlslSAbs;
        public readonly FuncUnaryInstruction GlslCeil;
        public readonly FuncUnaryInstruction GlslCos;
        public readonly FuncUnaryInstruction GlslExp2;
        public readonly FuncUnaryInstruction GlslFloor;
        public readonly FuncUnaryInstruction GlslLog2;
        public readonly FuncUnaryInstruction FNegate;
        public readonly FuncUnaryInstruction SNegate;
        public readonly FuncUnaryInstruction GlslInverseSqrt;
        public readonly FuncUnaryInstruction GlslRoundEven;
        public readonly FuncUnaryInstruction GlslSin;
        public readonly FuncUnaryInstruction GlslSqrt;
        public readonly FuncUnaryInstruction GlslTrunc;

        // UnaryBool
        public readonly FuncUnaryInstruction LogicalNot;

        // UnaryFP32
        public readonly FuncUnaryInstruction DPdx;
        public readonly FuncUnaryInstruction DPdy;

        // UnaryS32
        public readonly FuncUnaryInstruction BitCount;
        public readonly FuncUnaryInstruction BitReverse;
        public readonly FuncUnaryInstruction Not;

        // Compare
        public readonly FuncBinaryInstruction FOrdEqual;
        public readonly FuncBinaryInstruction IEqual;
        public readonly FuncBinaryInstruction FOrdGreaterThan;
        public readonly FuncBinaryInstruction SGreaterThan;
        public readonly FuncBinaryInstruction FOrdGreaterThanEqual;
        public readonly FuncBinaryInstruction SGreaterThanEqual;
        public readonly FuncBinaryInstruction FOrdLessThan;
        public readonly FuncBinaryInstruction SLessThan;
        public readonly FuncBinaryInstruction FOrdLessThanEqual;
        public readonly FuncBinaryInstruction SLessThanEqual;
        public readonly FuncBinaryInstruction FOrdNotEqual;
        public readonly FuncBinaryInstruction INotEqual;

        // CompareU32
        public readonly FuncBinaryInstruction UGreaterThanEqual;
        public readonly FuncBinaryInstruction UGreaterThan;
        public readonly FuncBinaryInstruction ULessThanEqual;
        public readonly FuncBinaryInstruction ULessThan;

        // Binary
        public readonly FuncBinaryInstruction FAdd;
        public readonly FuncBinaryInstruction IAdd;
        public readonly FuncBinaryInstruction FDiv;
        public readonly FuncBinaryInstruction SDiv;
        public readonly FuncBinaryInstruction GlslFMax;
        public readonly FuncBinaryInstruction GlslSMax;
        public readonly FuncBinaryInstruction GlslFMin;
        public readonly FuncBinaryInstruction GlslSMin;
        public readonly FuncBinaryInstruction FMod;
        public readonly FuncBinaryInstruction FMul;
        public readonly FuncBinaryInstruction IMul;
        public readonly FuncBinaryInstruction FSub;
        public readonly FuncBinaryInstruction ISub;

        // BinaryBool
        public readonly FuncBinaryInstruction LogicalAnd;
        public readonly FuncBinaryInstruction LogicalNotEqual;
        public readonly FuncBinaryInstruction LogicalOr;

        // BinaryS32
        public readonly FuncBinaryInstruction BitwiseAnd;
        public readonly FuncBinaryInstruction BitwiseXor;
        public readonly FuncBinaryInstruction BitwiseOr;
        public readonly FuncBinaryInstruction ShiftLeftLogical;
        public readonly FuncBinaryInstruction ShiftRightArithmetic;
        public readonly FuncBinaryInstruction ShiftRightLogical;

        // BinaryU32
        public readonly FuncBinaryInstruction GlslUMax;
        public readonly FuncBinaryInstruction GlslUMin;

        // AtomicMemoryBinary
        public readonly FuncQuaternaryInstruction AtomicIAdd;
        public readonly FuncQuaternaryInstruction AtomicAnd;
        public readonly FuncQuaternaryInstruction AtomicSMin;
        public readonly FuncQuaternaryInstruction AtomicUMin;
        public readonly FuncQuaternaryInstruction AtomicSMax;
        public readonly FuncQuaternaryInstruction AtomicUMax;
        public readonly FuncQuaternaryInstruction AtomicOr;
        public readonly FuncQuaternaryInstruction AtomicExchange;
        public readonly FuncQuaternaryInstruction AtomicXor;

        // Ternary
        public readonly FuncTernaryInstruction GlslFClamp;
        public readonly FuncTernaryInstruction GlslSClamp;
        public readonly FuncTernaryInstruction GlslFma;

        // TernaryS32
        public readonly FuncTernaryInstruction BitFieldSExtract;
        public readonly FuncTernaryInstruction BitFieldUExtract;

        // TernaryU32
        public readonly FuncTernaryInstruction GlslUClamp;

        // QuaternaryS32
        public readonly FuncQuaternaryInstruction BitFieldInsert;

        public SpirvDelegates(CodeGenContext context)
        {
            // Unary
            GlslFAbs = context.GlslFAbs;
            GlslSAbs = context.GlslSAbs;
            GlslCeil = context.GlslCeil;
            GlslCos = context.GlslCos;
            GlslExp2 = context.GlslExp2;
            GlslFloor = context.GlslFloor;
            GlslLog2 = context.GlslLog2;
            FNegate = context.FNegate;
            SNegate = context.SNegate;
            GlslInverseSqrt = context.GlslInverseSqrt;
            GlslRoundEven = context.GlslRoundEven;
            GlslSin = context.GlslSin;
            GlslSqrt = context.GlslSqrt;
            GlslTrunc = context.GlslTrunc;

            // UnaryBool
            LogicalNot = context.LogicalNot;

            // UnaryFP32
            DPdx = context.DPdx;
            DPdy = context.DPdy;

            // UnaryS32
            BitCount = context.BitCount;
            BitReverse = context.BitReverse;
            Not = context.Not;

            // Compare
            FOrdEqual = context.FOrdEqual;
            IEqual = context.IEqual;
            FOrdGreaterThan = context.FOrdGreaterThan;
            SGreaterThan = context.SGreaterThan;
            FOrdGreaterThanEqual = context.FOrdGreaterThanEqual;
            SGreaterThanEqual = context.SGreaterThanEqual;
            FOrdLessThan = context.FOrdLessThan;
            SLessThan = context.SLessThan;
            FOrdLessThanEqual = context.FOrdLessThanEqual;
            SLessThanEqual = context.SLessThanEqual;
            FOrdNotEqual = context.FOrdNotEqual;
            INotEqual = context.INotEqual;

            // CompareU32
            UGreaterThanEqual = context.UGreaterThanEqual;
            UGreaterThan = context.UGreaterThan;
            ULessThanEqual = context.ULessThanEqual;
            ULessThan = context.ULessThan;

            // Binary
            FAdd = context.FAdd;
            IAdd = context.IAdd;
            FDiv = context.FDiv;
            SDiv = context.SDiv;
            GlslFMax = context.GlslFMax;
            GlslSMax = context.GlslSMax;
            GlslFMin = context.GlslFMin;
            GlslSMin = context.GlslSMin;
            FMod = context.FMod;
            FMul = context.FMul;
            IMul = context.IMul;
            FSub = context.FSub;
            ISub = context.ISub;

            // BinaryBool
            LogicalAnd = context.LogicalAnd;
            LogicalNotEqual = context.LogicalNotEqual;
            LogicalOr = context.LogicalOr;

            // BinaryS32
            BitwiseAnd = context.BitwiseAnd;
            BitwiseXor = context.BitwiseXor;
            BitwiseOr = context.BitwiseOr;
            ShiftLeftLogical = context.ShiftLeftLogical;
            ShiftRightArithmetic = context.ShiftRightArithmetic;
            ShiftRightLogical = context.ShiftRightLogical;

            // BinaryU32
            GlslUMax = context.GlslUMax;
            GlslUMin = context.GlslUMin;

            // AtomicMemoryBinary
            AtomicIAdd = context.AtomicIAdd;
            AtomicAnd = context.AtomicAnd;
            AtomicSMin = context.AtomicSMin;
            AtomicUMin = context.AtomicUMin;
            AtomicSMax = context.AtomicSMax;
            AtomicUMax = context.AtomicUMax;
            AtomicOr = context.AtomicOr;
            AtomicExchange = context.AtomicExchange;
            AtomicXor = context.AtomicXor;

            // Ternary
            GlslFClamp = context.GlslFClamp;
            GlslSClamp = context.GlslSClamp;
            GlslFma = context.GlslFma;

            // TernaryS32
            BitFieldSExtract = context.BitFieldSExtract;
            BitFieldUExtract = context.BitFieldUExtract;

            // TernaryU32
            GlslUClamp = context.GlslUClamp;

            // QuaternaryS32
            BitFieldInsert = context.BitFieldInsert;
        }
    }
}
