using System;

namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    [Flags]
    enum Instruction
    {
        Absolute = 1,
        Add,
        AtomicAdd,
        AtomicAnd,
        AtomicCompareAndSwap,
        AtomicMinS32,
        AtomicMinU32,
        AtomicMaxS32,
        AtomicMaxU32,
        AtomicOr,
        AtomicSwap,
        AtomicXor,
        Ballot,
        Barrier,
        BitCount,
        BitfieldExtractS32,
        BitfieldExtractU32,
        BitfieldInsert,
        BitfieldReverse,
        BitwiseAnd,
        BitwiseExclusiveOr,
        BitwiseNot,
        BitwiseOr,
        Branch,
        BranchIfFalse,
        BranchIfTrue,
        Call,
        Ceiling,
        Clamp,
        ClampU32,
        Comment,
        CompareEqual,
        CompareGreater,
        CompareGreaterOrEqual,
        CompareGreaterOrEqualU32,
        CompareGreaterU32,
        CompareLess,
        CompareLessOrEqual,
        CompareLessOrEqualU32,
        CompareLessU32,
        CompareNotEqual,
        ConditionalSelect,
        ConvertFP32ToFP64,
        ConvertFP64ToFP32,
        ConvertFPToS32,
        ConvertFPToU32,
        ConvertS32ToFP,
        ConvertU32ToFP,
        Copy,
        Cosine,
        Ddx,
        Ddy,
        Discard,
        Divide,
        EmitVertex,
        EndPrimitive,
        ExponentB2,
        FindFirstSetS32,
        FindFirstSetU32,
        Floor,
        FusedMultiplyAdd,
        GroupMemoryBarrier,
        ImageLoad,
        ImageStore,
        IsNan,
        LoadAttribute,
        LoadConstant,
        LoadGlobal,
        LoadLocal,
        LoadShared,
        LoadStorage,
        Lod,
        LogarithmB2,
        LogicalAnd,
        LogicalExclusiveOr,
        LogicalNot,
        LogicalOr,
        LoopBreak,
        LoopContinue,
        MarkLabel,
        Maximum,
        MaximumU32,
        MemoryBarrier,
        Minimum,
        MinimumU32,
        Multiply,
        MultiplyHighS32,
        MultiplyHighU32,
        Negate,
        PackDouble2x32,
        PackHalf2x16,
        ReciprocalSquareRoot,
        Return,
        Round,
        ShiftLeft,
        ShiftRightS32,
        ShiftRightU32,
        Shuffle,
        ShuffleDown,
        ShuffleUp,
        ShuffleXor,
        Sine,
        SquareRoot,
        StoreGlobal,
        StoreLocal,
        StoreShared,
        StoreStorage,
        Subtract,
        SwizzleAdd,
        TextureSample,
        TextureSize,
        Truncate,
        UnpackDouble2x32,
        UnpackHalf2x16,
        VoteAll,
        VoteAllEqual,
        VoteAny,

        Count,

        FP32 = 1 << 16,
        FP64 = 1 << 17,

        MrShift = 18,

        MrGlobal  = 0 << MrShift,
        MrShared  = 1 << MrShift,
        MrStorage = 2 << MrShift,
        MrMask    = 3 << MrShift,

        Mask = 0xffff
    }

    static class InstructionExtensions
    {
        public static bool IsAtomic(this Instruction inst)
        {
            switch (inst & Instruction.Mask)
            {
                case Instruction.AtomicAdd:
                case Instruction.AtomicAnd:
                case Instruction.AtomicCompareAndSwap:
                case Instruction.AtomicMaxS32:
                case Instruction.AtomicMaxU32:
                case Instruction.AtomicMinS32:
                case Instruction.AtomicMinU32:
                case Instruction.AtomicOr:
                case Instruction.AtomicSwap:
                case Instruction.AtomicXor:
                    return true;
            }

            return false;
        }
    }
}