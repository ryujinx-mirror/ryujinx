using System;

using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        private enum IntType
        {
            U8  = 0,
            U16 = 1,
            U32 = 2,
            U64 = 3,
            S8  = 4,
            S16 = 5,
            S32 = 6,
            S64 = 7
        }

        private enum FloatType
        {
            F16 = 1,
            F32 = 2,
            F64 = 3
        }

        public static void F2f_C(ShaderIrBlock Block, long OpCode)
        {
            EmitF2f(Block, OpCode, ShaderOper.CR);
        }

        public static void F2f_I(ShaderIrBlock Block, long OpCode)
        {
            EmitF2f(Block, OpCode, ShaderOper.Immf);
        }

        public static void F2f_R(ShaderIrBlock Block, long OpCode)
        {
            EmitF2f(Block, OpCode, ShaderOper.RR);
        }

        public static void F2i_C(ShaderIrBlock Block, long OpCode)
        {
            EmitF2i(Block, OpCode, ShaderOper.CR);
        }

        public static void F2i_I(ShaderIrBlock Block, long OpCode)
        {
            EmitF2i(Block, OpCode, ShaderOper.Immf);
        }

        public static void F2i_R(ShaderIrBlock Block, long OpCode)
        {
            EmitF2i(Block, OpCode, ShaderOper.RR);
        }

        public static void I2f_C(ShaderIrBlock Block, long OpCode)
        {
            EmitI2f(Block, OpCode, ShaderOper.CR);
        }

        public static void I2f_I(ShaderIrBlock Block, long OpCode)
        {
            EmitI2f(Block, OpCode, ShaderOper.Imm);
        }

        public static void I2f_R(ShaderIrBlock Block, long OpCode)
        {
            EmitI2f(Block, OpCode, ShaderOper.RR);
        }

        public static void I2i_C(ShaderIrBlock Block, long OpCode)
        {
            EmitI2i(Block, OpCode, ShaderOper.CR);
        }

        public static void I2i_I(ShaderIrBlock Block, long OpCode)
        {
            EmitI2i(Block, OpCode, ShaderOper.Imm);
        }

        public static void I2i_R(ShaderIrBlock Block, long OpCode)
        {
            EmitI2i(Block, OpCode, ShaderOper.RR);
        }

        public static void Mov_C(ShaderIrBlock Block, long OpCode)
        {
            ShaderIrOperCbuf Cbuf = GetOperCbuf34(OpCode);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Cbuf), OpCode));
        }

        public static void Mov_I(ShaderIrBlock Block, long OpCode)
        {
            ShaderIrOperImm Imm = GetOperImm19_20(OpCode);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Imm), OpCode));
        }

        public static void Mov_R(ShaderIrBlock Block, long OpCode)
        {
            ShaderIrOperGpr Gpr = GetOperGpr20(OpCode);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Gpr), OpCode));
        }

        public static void Mov32i(ShaderIrBlock Block, long OpCode)
        {
            ShaderIrOperImm Imm = GetOperImm32_20(OpCode);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Imm), OpCode));
        }

        private static void EmitF2f(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            bool Na = ((OpCode >> 45) & 1) != 0;
            bool Aa = ((OpCode >> 49) & 1) != 0;

            ShaderIrNode OperA;

            switch (Oper)
            {
                case ShaderOper.CR:   OperA = GetOperCbuf34   (OpCode); break;
                case ShaderOper.Immf: OperA = GetOperImmf19_20(OpCode); break;
                case ShaderOper.RR:   OperA = GetOperGpr20    (OpCode); break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperA = GetAluAbsNeg(OperA, Aa, Na);

            ShaderIrInst RoundInst = GetRoundInst(OpCode);

            if (RoundInst != ShaderIrInst.Invalid)
            {
                OperA = new ShaderIrOp(RoundInst, OperA);
            }

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), OperA), OpCode));
        }

        private static void EmitF2i(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            IntType Type = GetIntType(OpCode);

            if (Type == IntType.U64 ||
                Type == IntType.S64)
            {
                //TODO: 64-bits support.
                //Note: GLSL doesn't support 64-bits integers.
                throw new NotImplementedException();
            }

            bool Na = ((OpCode >> 45) & 1) != 0;
            bool Aa = ((OpCode >> 49) & 1) != 0;

            ShaderIrNode OperA;

            switch (Oper)
            {
                case ShaderOper.CR:   OperA = GetOperCbuf34   (OpCode); break;
                case ShaderOper.Immf: OperA = GetOperImmf19_20(OpCode); break;
                case ShaderOper.RR:   OperA = GetOperGpr20    (OpCode); break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperA = GetAluAbsNeg(OperA, Aa, Na);

            ShaderIrInst RoundInst = GetRoundInst(OpCode);

            if (RoundInst != ShaderIrInst.Invalid)
            {
                OperA = new ShaderIrOp(RoundInst, OperA);
            }

            bool Signed = Type >= IntType.S8;

            int Size = 8 << ((int)Type & 3);

            if (Size < 32)
            {
                uint Mask = uint.MaxValue >> (32 - Size);

                float CMin = 0;
                float CMax = Mask;

                if (Signed)
                {
                    uint HalfMask = Mask >> 1;

                    CMin -= HalfMask + 1;
                    CMax  = HalfMask;
                }

                ShaderIrOperImmf IMin = new ShaderIrOperImmf(CMin);
                ShaderIrOperImmf IMax = new ShaderIrOperImmf(CMax);

                OperA = new ShaderIrOp(ShaderIrInst.Fclamp, OperA, IMin, IMax);
            }

            ShaderIrInst Inst = Signed
                ? ShaderIrInst.Ftos
                : ShaderIrInst.Ftou;

            ShaderIrNode Op = new ShaderIrOp(Inst, OperA);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Op), OpCode));
        }

        private static void EmitI2f(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            IntType Type = GetIntType(OpCode);

            if (Type == IntType.U64 ||
                Type == IntType.S64)
            {
                //TODO: 64-bits support.
                //Note: GLSL doesn't support 64-bits integers.
                throw new NotImplementedException();
            }

            int Sel = (int)(OpCode >> 41) & 3;

            bool Na = ((OpCode >> 45) & 1) != 0;
            bool Aa = ((OpCode >> 49) & 1) != 0;

            ShaderIrNode OperA;

            switch (Oper)
            {
                case ShaderOper.CR:  OperA = GetOperCbuf34  (OpCode); break;
                case ShaderOper.Imm: OperA = GetOperImm19_20(OpCode); break;
                case ShaderOper.RR:  OperA = GetOperGpr20   (OpCode); break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperA = GetAluAbsNeg(OperA, Aa, Na);

            bool Signed = Type >= IntType.S8;

            int Shift = Sel * 8;

            int Size = 8 << ((int)Type & 3);

            if (Shift != 0)
            {
                OperA = new ShaderIrOp(ShaderIrInst.Asr, OperA, new ShaderIrOperImm(Shift));
            }

            if (Size < 32)
            {
                uint Mask = uint.MaxValue >> (32 - Size);

                OperA = new ShaderIrOp(ShaderIrInst.And, OperA, new ShaderIrOperImm((int)Mask));
            }

            ShaderIrInst Inst = Signed
                ? ShaderIrInst.Stof
                : ShaderIrInst.Utof;

            ShaderIrNode Op = new ShaderIrOp(Inst, OperA);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Op), OpCode));
        }

        private static void EmitI2i(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            IntType Type = GetIntType(OpCode);

            if (Type == IntType.U64 ||
                Type == IntType.S64)
            {
                //TODO: 64-bits support.
                //Note: GLSL doesn't support 64-bits integers.
                throw new NotImplementedException();
            }

            int Sel = (int)(OpCode >> 41) & 3;

            bool NegA = ((OpCode >> 45) & 1) != 0;
            bool AbsA = ((OpCode >> 49) & 1) != 0;
            bool SatA = ((OpCode >> 50) & 1) != 0;

            ShaderIrNode OperA;

            switch (Oper)
            {
                case ShaderOper.CR:   OperA = GetOperCbuf34   (OpCode); break;
                case ShaderOper.Immf: OperA = GetOperImmf19_20(OpCode); break;
                case ShaderOper.RR:   OperA = GetOperGpr20    (OpCode); break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperA = GetAluAbsNeg(OperA, AbsA, NegA);

            bool Signed = Type >= IntType.S8;

            int Shift = Sel * 8;

            int Size = 8 << ((int)Type & 3);

            if (Shift != 0)
            {
                OperA = new ShaderIrOp(ShaderIrInst.Asr, OperA, new ShaderIrOperImm(Shift));
            }

            if (Size < 32)
            {
                uint Mask = uint.MaxValue >> (32 - Size);

                if (SatA)
                {
                    uint CMin = 0;
                    uint CMax = Mask;

                    if (Signed)
                    {
                        uint HalfMask = Mask >> 1;

                        CMin -= HalfMask + 1;
                        CMax  = HalfMask;
                    }

                    ShaderIrOperImm IMin = new ShaderIrOperImm((int)CMin);
                    ShaderIrOperImm IMax = new ShaderIrOperImm((int)CMax);

                    OperA = new ShaderIrOp(Signed
                        ? ShaderIrInst.Clamps
                        : ShaderIrInst.Clampu, OperA, IMin, IMax);
                }
                else
                {
                    OperA = new ShaderIrOp(ShaderIrInst.And, OperA, new ShaderIrOperImm((int)Mask));
                }
            }

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), OperA), OpCode));
        }

        private static IntType GetIntType(long OpCode)
        {
            bool Signed = ((OpCode >> 13) & 1) != 0;

            IntType Type = (IntType)((OpCode >> 10) & 3);

            if (Signed)
            {
                Type += (int)IntType.S8;
            }

            return Type;
        }

        private static FloatType GetFloatType(long OpCode)
        {
            return (FloatType)((OpCode >> 8) & 3);
        }

        private static ShaderIrInst GetRoundInst(long OpCode)
        {
            switch ((OpCode >> 39) & 3)
            {
                case 1: return ShaderIrInst.Floor;
                case 2: return ShaderIrInst.Ceil;
                case 3: return ShaderIrInst.Trunc;
            }

            return ShaderIrInst.Invalid;
        }
    }
}