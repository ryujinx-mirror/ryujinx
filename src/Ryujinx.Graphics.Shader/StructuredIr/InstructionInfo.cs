using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    static class InstructionInfo
    {
        private readonly struct InstInfo
        {
            public AggregateType DestType { get; }

            public AggregateType[] SrcTypes { get; }

            public InstInfo(AggregateType destType, params AggregateType[] srcTypes)
            {
                DestType = destType;
                SrcTypes = srcTypes;
            }
        }

        private static readonly InstInfo[] _infoTbl;

        static InstructionInfo()
        {
            _infoTbl = new InstInfo[(int)Instruction.Count];

#pragma warning disable IDE0055 // Disable formatting
            //  Inst                                  Destination type      Source 1 type          Source 2 type          Source 3 type          Source 4 type
            Add(Instruction.AtomicAdd,                AggregateType.U32,    AggregateType.S32,     AggregateType.S32,     AggregateType.U32);
            Add(Instruction.AtomicAnd,                AggregateType.U32,    AggregateType.S32,     AggregateType.S32,     AggregateType.U32);
            Add(Instruction.AtomicCompareAndSwap,     AggregateType.U32,    AggregateType.S32,     AggregateType.S32,     AggregateType.U32,     AggregateType.U32);
            Add(Instruction.AtomicMaxS32,             AggregateType.S32,    AggregateType.S32,     AggregateType.S32,     AggregateType.S32);
            Add(Instruction.AtomicMaxU32,             AggregateType.U32,    AggregateType.S32,     AggregateType.S32,     AggregateType.U32);
            Add(Instruction.AtomicMinS32,             AggregateType.S32,    AggregateType.S32,     AggregateType.S32,     AggregateType.S32);
            Add(Instruction.AtomicMinU32,             AggregateType.U32,    AggregateType.S32,     AggregateType.S32,     AggregateType.U32);
            Add(Instruction.AtomicOr,                 AggregateType.U32,    AggregateType.S32,     AggregateType.S32,     AggregateType.U32);
            Add(Instruction.AtomicSwap,               AggregateType.U32,    AggregateType.S32,     AggregateType.S32,     AggregateType.U32);
            Add(Instruction.AtomicXor,                AggregateType.U32,    AggregateType.S32,     AggregateType.S32,     AggregateType.U32);
            Add(Instruction.Absolute,                 AggregateType.Scalar, AggregateType.Scalar);
            Add(Instruction.Add,                      AggregateType.Scalar, AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.Ballot,                   AggregateType.U32,    AggregateType.Bool);
            Add(Instruction.BitCount,                 AggregateType.S32,    AggregateType.S32);
            Add(Instruction.BitfieldExtractS32,       AggregateType.S32,    AggregateType.S32,     AggregateType.S32,     AggregateType.S32);
            Add(Instruction.BitfieldExtractU32,       AggregateType.U32,    AggregateType.U32,     AggregateType.S32,     AggregateType.S32);
            Add(Instruction.BitfieldInsert,           AggregateType.S32,    AggregateType.S32,     AggregateType.S32,     AggregateType.S32,     AggregateType.S32);
            Add(Instruction.BitfieldReverse,          AggregateType.S32,    AggregateType.S32);
            Add(Instruction.BitwiseAnd,               AggregateType.S32,    AggregateType.S32,     AggregateType.S32);
            Add(Instruction.BitwiseExclusiveOr,       AggregateType.S32,    AggregateType.S32,     AggregateType.S32);
            Add(Instruction.BitwiseNot,               AggregateType.S32,    AggregateType.S32);
            Add(Instruction.BitwiseOr,                AggregateType.S32,    AggregateType.S32,     AggregateType.S32);
            Add(Instruction.BranchIfTrue,             AggregateType.Void,   AggregateType.Bool);
            Add(Instruction.BranchIfFalse,            AggregateType.Void,   AggregateType.Bool);
            Add(Instruction.Call,                     AggregateType.Scalar);
            Add(Instruction.Ceiling,                  AggregateType.Scalar, AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.Clamp,                    AggregateType.Scalar, AggregateType.Scalar,  AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.ClampU32,                 AggregateType.U32,    AggregateType.U32,     AggregateType.U32,     AggregateType.U32);
            Add(Instruction.CompareEqual,             AggregateType.Bool,   AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.CompareGreater,           AggregateType.Bool,   AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.CompareGreaterOrEqual,    AggregateType.Bool,   AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.CompareGreaterOrEqualU32, AggregateType.Bool,   AggregateType.U32,     AggregateType.U32);
            Add(Instruction.CompareGreaterU32,        AggregateType.Bool,   AggregateType.U32,     AggregateType.U32);
            Add(Instruction.CompareLess,              AggregateType.Bool,   AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.CompareLessOrEqual,       AggregateType.Bool,   AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.CompareLessOrEqualU32,    AggregateType.Bool,   AggregateType.U32,     AggregateType.U32);
            Add(Instruction.CompareLessU32,           AggregateType.Bool,   AggregateType.U32,     AggregateType.U32);
            Add(Instruction.CompareNotEqual,          AggregateType.Bool,   AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.ConditionalSelect,        AggregateType.Scalar, AggregateType.Bool,    AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.ConvertFP32ToFP64,        AggregateType.FP64,   AggregateType.FP32);
            Add(Instruction.ConvertFP64ToFP32,        AggregateType.FP32,   AggregateType.FP64);
            Add(Instruction.ConvertFP32ToS32,         AggregateType.S32,    AggregateType.FP32);
            Add(Instruction.ConvertFP32ToU32,         AggregateType.U32,    AggregateType.FP32);
            Add(Instruction.ConvertFP64ToS32,         AggregateType.S32,    AggregateType.FP64);
            Add(Instruction.ConvertFP64ToU32,         AggregateType.U32,    AggregateType.FP64);
            Add(Instruction.ConvertS32ToFP32,         AggregateType.FP32,   AggregateType.S32);
            Add(Instruction.ConvertS32ToFP64,         AggregateType.FP64,   AggregateType.S32);
            Add(Instruction.ConvertU32ToFP32,         AggregateType.FP32,   AggregateType.U32);
            Add(Instruction.ConvertU32ToFP64,         AggregateType.FP64,   AggregateType.U32);
            Add(Instruction.Cosine,                   AggregateType.Scalar, AggregateType.Scalar);
            Add(Instruction.Ddx,                      AggregateType.FP32,   AggregateType.FP32);
            Add(Instruction.Ddy,                      AggregateType.FP32,   AggregateType.FP32);
            Add(Instruction.Divide,                   AggregateType.Scalar, AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.ExponentB2,               AggregateType.Scalar, AggregateType.Scalar);
            Add(Instruction.FindLSB,                  AggregateType.S32,    AggregateType.S32);
            Add(Instruction.FindMSBS32,               AggregateType.S32,    AggregateType.S32);
            Add(Instruction.FindMSBU32,               AggregateType.S32,    AggregateType.U32);
            Add(Instruction.Floor,                    AggregateType.Scalar, AggregateType.Scalar);
            Add(Instruction.FusedMultiplyAdd,         AggregateType.Scalar, AggregateType.Scalar,  AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.ImageLoad,                AggregateType.FP32);
            Add(Instruction.ImageStore,               AggregateType.Void);
            Add(Instruction.ImageAtomic,              AggregateType.S32);
            Add(Instruction.IsNan,                    AggregateType.Bool,   AggregateType.Scalar);
            Add(Instruction.Load,                     AggregateType.FP32);
            Add(Instruction.Lod,                      AggregateType.FP32);
            Add(Instruction.LogarithmB2,              AggregateType.Scalar, AggregateType.Scalar);
            Add(Instruction.LogicalAnd,               AggregateType.Bool,   AggregateType.Bool,    AggregateType.Bool);
            Add(Instruction.LogicalExclusiveOr,       AggregateType.Bool,   AggregateType.Bool,    AggregateType.Bool);
            Add(Instruction.LogicalNot,               AggregateType.Bool,   AggregateType.Bool);
            Add(Instruction.LogicalOr,                AggregateType.Bool,   AggregateType.Bool,    AggregateType.Bool);
            Add(Instruction.Maximum,                  AggregateType.Scalar, AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.MaximumU32,               AggregateType.U32,    AggregateType.U32,     AggregateType.U32);
            Add(Instruction.Minimum,                  AggregateType.Scalar, AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.MinimumU32,               AggregateType.U32,    AggregateType.U32,     AggregateType.U32);
            Add(Instruction.Modulo,                   AggregateType.Scalar, AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.Multiply,                 AggregateType.Scalar, AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.MultiplyHighS32,          AggregateType.S32,    AggregateType.S32,     AggregateType.S32);
            Add(Instruction.MultiplyHighU32,          AggregateType.U32,    AggregateType.U32,     AggregateType.U32);
            Add(Instruction.Negate,                   AggregateType.Scalar, AggregateType.Scalar);
            Add(Instruction.PackDouble2x32,           AggregateType.FP64,   AggregateType.U32,     AggregateType.U32);
            Add(Instruction.PackHalf2x16,             AggregateType.U32,    AggregateType.FP32,    AggregateType.FP32);
            Add(Instruction.ReciprocalSquareRoot,     AggregateType.Scalar, AggregateType.Scalar);
            Add(Instruction.Return,                   AggregateType.Void,   AggregateType.U32);
            Add(Instruction.Round,                    AggregateType.Scalar, AggregateType.Scalar);
            Add(Instruction.ShiftLeft,                AggregateType.S32,    AggregateType.S32,     AggregateType.S32);
            Add(Instruction.ShiftRightS32,            AggregateType.S32,    AggregateType.S32,     AggregateType.S32);
            Add(Instruction.ShiftRightU32,            AggregateType.U32,    AggregateType.U32,     AggregateType.S32);
            Add(Instruction.Shuffle,                  AggregateType.FP32,   AggregateType.FP32,    AggregateType.U32);
            Add(Instruction.ShuffleDown,              AggregateType.FP32,   AggregateType.FP32,    AggregateType.U32);
            Add(Instruction.ShuffleUp,                AggregateType.FP32,   AggregateType.FP32,    AggregateType.U32);
            Add(Instruction.ShuffleXor,               AggregateType.FP32,   AggregateType.FP32,    AggregateType.U32);
            Add(Instruction.Sine,                     AggregateType.Scalar, AggregateType.Scalar);
            Add(Instruction.SquareRoot,               AggregateType.Scalar, AggregateType.Scalar);
            Add(Instruction.Store,                    AggregateType.Void);
            Add(Instruction.Subtract,                 AggregateType.Scalar, AggregateType.Scalar,  AggregateType.Scalar);
            Add(Instruction.SwizzleAdd,               AggregateType.FP32,   AggregateType.FP32,    AggregateType.FP32,    AggregateType.S32);
            Add(Instruction.TextureSample,            AggregateType.FP32);
            Add(Instruction.TextureQuerySamples,      AggregateType.S32,    AggregateType.S32);
            Add(Instruction.TextureQuerySize,         AggregateType.S32,    AggregateType.S32,     AggregateType.S32);
            Add(Instruction.Truncate,                 AggregateType.Scalar, AggregateType.Scalar);
            Add(Instruction.UnpackDouble2x32,         AggregateType.U32,    AggregateType.FP64);
            Add(Instruction.UnpackHalf2x16,           AggregateType.FP32,   AggregateType.U32);
            Add(Instruction.VectorExtract,            AggregateType.Scalar, AggregateType.Vector4, AggregateType.S32);
            Add(Instruction.VoteAll,                  AggregateType.Bool,   AggregateType.Bool);
            Add(Instruction.VoteAllEqual,             AggregateType.Bool,   AggregateType.Bool);
            Add(Instruction.VoteAny,                  AggregateType.Bool,   AggregateType.Bool);
#pragma warning restore IDE0055
        }

        private static void Add(Instruction inst, AggregateType destType, params AggregateType[] srcTypes)
        {
            _infoTbl[(int)inst] = new InstInfo(destType, srcTypes);
        }

        public static AggregateType GetDestVarType(Instruction inst)
        {
            return GetFinalVarType(_infoTbl[(int)(inst & Instruction.Mask)].DestType, inst);
        }

        public static AggregateType GetSrcVarType(Instruction inst, int index)
        {
            // TODO: Return correct type depending on source index,
            // that can improve the decompiler output.
            if (inst == Instruction.ImageLoad ||
                inst == Instruction.ImageStore ||
                inst == Instruction.ImageAtomic ||
                inst == Instruction.Lod ||
                inst == Instruction.TextureSample)
            {
                return AggregateType.FP32;
            }
            else if (inst == Instruction.Call || inst == Instruction.Load || inst == Instruction.Store || inst.IsAtomic())
            {
                return AggregateType.S32;
            }

            return GetFinalVarType(_infoTbl[(int)(inst & Instruction.Mask)].SrcTypes[index], inst);
        }

        private static AggregateType GetFinalVarType(AggregateType type, Instruction inst)
        {
            if (type == AggregateType.Scalar)
            {
                if ((inst & Instruction.FP32) != 0)
                {
                    return AggregateType.FP32;
                }
                else if ((inst & Instruction.FP64) != 0)
                {
                    return AggregateType.FP64;
                }
                else
                {
                    return AggregateType.S32;
                }
            }
            else if (type == AggregateType.Void)
            {
                throw new ArgumentException($"Invalid operand for instruction \"{inst}\".");
            }

            return type;
        }

        public static bool IsUnary(Instruction inst)
        {
            if (inst == Instruction.Copy)
            {
                return true;
            }
            else if (inst == Instruction.TextureSample)
            {
                return false;
            }

            return _infoTbl[(int)(inst & Instruction.Mask)].SrcTypes.Length == 1;
        }
    }
}
