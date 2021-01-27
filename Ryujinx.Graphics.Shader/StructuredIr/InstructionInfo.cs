using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    static class InstructionInfo
    {
        private struct InstInfo
        {
            public VariableType DestType { get; }

            public VariableType[] SrcTypes { get; }

            public InstInfo(VariableType destType, params VariableType[] srcTypes)
            {
                DestType = destType;
                SrcTypes = srcTypes;
            }
        }

        private static InstInfo[] _infoTbl;

        static InstructionInfo()
        {
            _infoTbl = new InstInfo[(int)Instruction.Count];

            //  Inst                                  Destination type     Source 1 type        Source 2 type        Source 3 type        Source 4 type
            Add(Instruction.AtomicAdd,                VariableType.U32,    VariableType.S32,    VariableType.S32,    VariableType.U32);
            Add(Instruction.AtomicAnd,                VariableType.U32,    VariableType.S32,    VariableType.S32,    VariableType.U32);
            Add(Instruction.AtomicCompareAndSwap,     VariableType.U32,    VariableType.S32,    VariableType.S32,    VariableType.U32,    VariableType.U32);
            Add(Instruction.AtomicMaxS32,             VariableType.S32,    VariableType.S32,    VariableType.S32,    VariableType.S32);
            Add(Instruction.AtomicMaxU32,             VariableType.U32,    VariableType.S32,    VariableType.S32,    VariableType.U32);
            Add(Instruction.AtomicMinS32,             VariableType.S32,    VariableType.S32,    VariableType.S32,    VariableType.S32);
            Add(Instruction.AtomicMinU32,             VariableType.U32,    VariableType.S32,    VariableType.S32,    VariableType.U32);
            Add(Instruction.AtomicOr,                 VariableType.U32,    VariableType.S32,    VariableType.S32,    VariableType.U32);
            Add(Instruction.AtomicSwap,               VariableType.U32,    VariableType.S32,    VariableType.S32,    VariableType.U32);
            Add(Instruction.AtomicXor,                VariableType.U32,    VariableType.S32,    VariableType.S32,    VariableType.U32);
            Add(Instruction.Absolute,                 VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.Add,                      VariableType.Scalar, VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.Ballot,                   VariableType.U32,    VariableType.Bool);
            Add(Instruction.BitCount,                 VariableType.Int,    VariableType.Int);
            Add(Instruction.BitfieldExtractS32,       VariableType.S32,    VariableType.S32,    VariableType.S32,    VariableType.S32);
            Add(Instruction.BitfieldExtractU32,       VariableType.U32,    VariableType.U32,    VariableType.S32,    VariableType.S32);
            Add(Instruction.BitfieldInsert,           VariableType.Int,    VariableType.Int,    VariableType.Int,    VariableType.S32,    VariableType.S32);
            Add(Instruction.BitfieldReverse,          VariableType.Int,    VariableType.Int);
            Add(Instruction.BitwiseAnd,               VariableType.Int,    VariableType.Int,    VariableType.Int);
            Add(Instruction.BitwiseExclusiveOr,       VariableType.Int,    VariableType.Int,    VariableType.Int);
            Add(Instruction.BitwiseNot,               VariableType.Int,    VariableType.Int);
            Add(Instruction.BitwiseOr,                VariableType.Int,    VariableType.Int,    VariableType.Int);
            Add(Instruction.BranchIfTrue,             VariableType.None,   VariableType.Bool);
            Add(Instruction.BranchIfFalse,            VariableType.None,   VariableType.Bool);
            Add(Instruction.Call,                     VariableType.Scalar);
            Add(Instruction.Ceiling,                  VariableType.Scalar, VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.Clamp,                    VariableType.Scalar, VariableType.Scalar, VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.ClampU32,                 VariableType.U32,    VariableType.U32,    VariableType.U32,    VariableType.U32);
            Add(Instruction.CompareEqual,             VariableType.Bool,   VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.CompareGreater,           VariableType.Bool,   VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.CompareGreaterOrEqual,    VariableType.Bool,   VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.CompareGreaterOrEqualU32, VariableType.Bool,   VariableType.U32,    VariableType.U32);
            Add(Instruction.CompareGreaterU32,        VariableType.Bool,   VariableType.U32,    VariableType.U32);
            Add(Instruction.CompareLess,              VariableType.Bool,   VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.CompareLessOrEqual,       VariableType.Bool,   VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.CompareLessOrEqualU32,    VariableType.Bool,   VariableType.U32,    VariableType.U32);
            Add(Instruction.CompareLessU32,           VariableType.Bool,   VariableType.U32,    VariableType.U32);
            Add(Instruction.CompareNotEqual,          VariableType.Bool,   VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.ConditionalSelect,        VariableType.Scalar, VariableType.Bool,   VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.ConvertFP32ToFP64,        VariableType.F64,    VariableType.F32);
            Add(Instruction.ConvertFP64ToFP32,        VariableType.F32,    VariableType.F64);
            Add(Instruction.ConvertFPToS32,           VariableType.S32,    VariableType.F32);
            Add(Instruction.ConvertFPToU32,           VariableType.U32,    VariableType.F32);
            Add(Instruction.ConvertS32ToFP,           VariableType.F32,    VariableType.S32);
            Add(Instruction.ConvertU32ToFP,           VariableType.F32,    VariableType.U32);
            Add(Instruction.Cosine,                   VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.Ddx,                      VariableType.F32,    VariableType.F32);
            Add(Instruction.Ddy,                      VariableType.F32,    VariableType.F32);
            Add(Instruction.Divide,                   VariableType.Scalar, VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.ExponentB2,               VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.FindFirstSetS32,          VariableType.S32,    VariableType.S32);
            Add(Instruction.FindFirstSetU32,          VariableType.S32,    VariableType.U32);
            Add(Instruction.Floor,                    VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.FusedMultiplyAdd,         VariableType.Scalar, VariableType.Scalar, VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.ImageLoad,                VariableType.F32);
            Add(Instruction.ImageStore,               VariableType.None);
            Add(Instruction.IsNan,                    VariableType.Bool,   VariableType.F32);
            Add(Instruction.LoadAttribute,            VariableType.F32,    VariableType.S32,    VariableType.S32);
            Add(Instruction.LoadConstant,             VariableType.F32,    VariableType.S32,    VariableType.S32);
            Add(Instruction.LoadGlobal,               VariableType.U32,    VariableType.S32,    VariableType.S32);
            Add(Instruction.LoadLocal,                VariableType.U32,    VariableType.S32);
            Add(Instruction.LoadShared,               VariableType.U32,    VariableType.S32);
            Add(Instruction.LoadStorage,              VariableType.U32,    VariableType.S32,    VariableType.S32);
            Add(Instruction.Lod,                      VariableType.F32);
            Add(Instruction.LogarithmB2,              VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.LogicalAnd,               VariableType.Bool,   VariableType.Bool,   VariableType.Bool);
            Add(Instruction.LogicalExclusiveOr,       VariableType.Bool,   VariableType.Bool,   VariableType.Bool);
            Add(Instruction.LogicalNot,               VariableType.Bool,   VariableType.Bool);
            Add(Instruction.LogicalOr,                VariableType.Bool,   VariableType.Bool,   VariableType.Bool);
            Add(Instruction.Maximum,                  VariableType.Scalar, VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.MaximumU32,               VariableType.U32,    VariableType.U32,    VariableType.U32);
            Add(Instruction.Minimum,                  VariableType.Scalar, VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.MinimumU32,               VariableType.U32,    VariableType.U32,    VariableType.U32);
            Add(Instruction.Multiply,                 VariableType.Scalar, VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.MultiplyHighS32,          VariableType.S32,    VariableType.S32,    VariableType.S32);
            Add(Instruction.MultiplyHighU32,          VariableType.U32,    VariableType.U32,    VariableType.U32);
            Add(Instruction.Negate,                   VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.PackDouble2x32,           VariableType.F64,    VariableType.U32,    VariableType.U32);
            Add(Instruction.PackHalf2x16,             VariableType.U32,    VariableType.F32,    VariableType.F32);
            Add(Instruction.ReciprocalSquareRoot,     VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.Round,                    VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.ShiftLeft,                VariableType.Int,    VariableType.Int,    VariableType.Int);
            Add(Instruction.ShiftRightS32,            VariableType.S32,    VariableType.S32,    VariableType.Int);
            Add(Instruction.ShiftRightU32,            VariableType.U32,    VariableType.U32,    VariableType.Int);
            Add(Instruction.Shuffle,                  VariableType.F32,    VariableType.F32,    VariableType.U32,    VariableType.U32,    VariableType.Bool);
            Add(Instruction.ShuffleDown,              VariableType.F32,    VariableType.F32,    VariableType.U32,    VariableType.U32,    VariableType.Bool);
            Add(Instruction.ShuffleUp,                VariableType.F32,    VariableType.F32,    VariableType.U32,    VariableType.U32,    VariableType.Bool);
            Add(Instruction.ShuffleXor,               VariableType.F32,    VariableType.F32,    VariableType.U32,    VariableType.U32,    VariableType.Bool);
            Add(Instruction.Sine,                     VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.SquareRoot,               VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.StoreGlobal,              VariableType.None,   VariableType.S32,    VariableType.S32,    VariableType.U32);
            Add(Instruction.StoreLocal,               VariableType.None,   VariableType.S32,    VariableType.U32);
            Add(Instruction.StoreShared,              VariableType.None,   VariableType.S32,    VariableType.U32);
            Add(Instruction.StoreStorage,             VariableType.None,   VariableType.S32,    VariableType.S32,    VariableType.U32);
            Add(Instruction.Subtract,                 VariableType.Scalar, VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.SwizzleAdd,               VariableType.F32,    VariableType.F32,    VariableType.F32,    VariableType.S32);
            Add(Instruction.TextureSample,            VariableType.F32);
            Add(Instruction.TextureSize,              VariableType.S32,    VariableType.S32,    VariableType.S32);
            Add(Instruction.Truncate,                 VariableType.Scalar, VariableType.Scalar);
            Add(Instruction.UnpackDouble2x32,         VariableType.U32,    VariableType.F64);
            Add(Instruction.UnpackHalf2x16,           VariableType.F32,    VariableType.U32);
            Add(Instruction.VoteAll,                  VariableType.Bool,   VariableType.Bool);
            Add(Instruction.VoteAllEqual,             VariableType.Bool,   VariableType.Bool);
            Add(Instruction.VoteAny,                  VariableType.Bool,   VariableType.Bool);
        }

        private static void Add(Instruction inst, VariableType destType, params VariableType[] srcTypes)
        {
            _infoTbl[(int)inst] = new InstInfo(destType, srcTypes);
        }

        public static VariableType GetDestVarType(Instruction inst)
        {
            return GetFinalVarType(_infoTbl[(int)(inst & Instruction.Mask)].DestType, inst);
        }

        public static VariableType GetSrcVarType(Instruction inst, int index)
        {
            // TODO: Return correct type depending on source index,
            // that can improve the decompiler output.
            if (inst == Instruction.ImageLoad  ||
                inst == Instruction.ImageStore ||
                inst == Instruction.Lod        ||
                inst == Instruction.TextureSample)
            {
                return VariableType.F32;
            }
            else if (inst == Instruction.Call)
            {
                return VariableType.S32;
            }

            return GetFinalVarType(_infoTbl[(int)(inst & Instruction.Mask)].SrcTypes[index], inst);
        }

        private static VariableType GetFinalVarType(VariableType type, Instruction inst)
        {
            if (type == VariableType.Scalar)
            {
                if ((inst & Instruction.FP32) != 0)
                {
                    return VariableType.F32;
                }
                else if ((inst & Instruction.FP64) != 0)
                {
                    return VariableType.F64;
                }
                else
                {
                    return VariableType.S32;
                }
            }
            else if (type == VariableType.Int)
            {
                return VariableType.S32;
            }
            else if (type == VariableType.None)
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