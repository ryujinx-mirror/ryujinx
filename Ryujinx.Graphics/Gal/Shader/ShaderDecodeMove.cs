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

            ulong Mask = ulong.MaxValue >> (64 - Size);

            int Mask32 = (int)Mask;

            if (Shift != 0)
            {
                OperA = new ShaderIrOp(ShaderIrInst.Asr, OperA, new ShaderIrOperImm(Shift));
            }

            if (Mask != uint.MaxValue)
            {
                OperA = new ShaderIrOp(ShaderIrInst.And, OperA, new ShaderIrOperImm(Mask32));
            }

            ShaderIrInst Inst = Signed
                ? ShaderIrInst.Stof
                : ShaderIrInst.Utof;

            ShaderIrNode Op = new ShaderIrOp(Inst, OperA);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Op), OpCode));
        }

        public static void Mov32i(ShaderIrBlock Block, long OpCode)
        {
            ShaderIrOperImm Imm = GetOperImm32_20(OpCode);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Imm), OpCode));
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
    }
}