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

        public static void F2f_C(ShaderIrBlock block, long opCode, int position)
        {
            EmitF2F(block, opCode, ShaderOper.Cr);
        }

        public static void F2f_I(ShaderIrBlock block, long opCode, int position)
        {
            EmitF2F(block, opCode, ShaderOper.Immf);
        }

        public static void F2f_R(ShaderIrBlock block, long opCode, int position)
        {
            EmitF2F(block, opCode, ShaderOper.Rr);
        }

        public static void F2i_C(ShaderIrBlock block, long opCode, int position)
        {
            EmitF2I(block, opCode, ShaderOper.Cr);
        }

        public static void F2i_I(ShaderIrBlock block, long opCode, int position)
        {
            EmitF2I(block, opCode, ShaderOper.Immf);
        }

        public static void F2i_R(ShaderIrBlock block, long opCode, int position)
        {
            EmitF2I(block, opCode, ShaderOper.Rr);
        }

        public static void I2f_C(ShaderIrBlock block, long opCode, int position)
        {
            EmitI2F(block, opCode, ShaderOper.Cr);
        }

        public static void I2f_I(ShaderIrBlock block, long opCode, int position)
        {
            EmitI2F(block, opCode, ShaderOper.Imm);
        }

        public static void I2f_R(ShaderIrBlock block, long opCode, int position)
        {
            EmitI2F(block, opCode, ShaderOper.Rr);
        }

        public static void I2i_C(ShaderIrBlock block, long opCode, int position)
        {
            EmitI2I(block, opCode, ShaderOper.Cr);
        }

        public static void I2i_I(ShaderIrBlock block, long opCode, int position)
        {
            EmitI2I(block, opCode, ShaderOper.Imm);
        }

        public static void I2i_R(ShaderIrBlock block, long opCode, int position)
        {
            EmitI2I(block, opCode, ShaderOper.Rr);
        }

        public static void Isberd(ShaderIrBlock block, long opCode, int position)
        {
            //This instruction seems to be used to translate from an address to a vertex index in a GS
            //Stub it as such

            block.AddNode(new ShaderIrCmnt("Stubbed."));

            block.AddNode(opCode.PredNode(new ShaderIrAsg(opCode.Gpr0(), opCode.Gpr8())));
        }

        public static void Mov_C(ShaderIrBlock block, long opCode, int position)
        {
            ShaderIrOperCbuf cbuf = opCode.Cbuf34();

            block.AddNode(opCode.PredNode(new ShaderIrAsg(opCode.Gpr0(), cbuf)));
        }

        public static void Mov_I(ShaderIrBlock block, long opCode, int position)
        {
            ShaderIrOperImm imm = opCode.Imm19_20();

            block.AddNode(opCode.PredNode(new ShaderIrAsg(opCode.Gpr0(), imm)));
        }

        public static void Mov_I32(ShaderIrBlock block, long opCode, int position)
        {
            ShaderIrOperImm imm = opCode.Imm32_20();

            block.AddNode(opCode.PredNode(new ShaderIrAsg(opCode.Gpr0(), imm)));
        }

        public static void Mov_R(ShaderIrBlock block, long opCode, int position)
        {
            ShaderIrOperGpr gpr = opCode.Gpr20();

            block.AddNode(opCode.PredNode(new ShaderIrAsg(opCode.Gpr0(), gpr)));
        }

        public static void Sel_C(ShaderIrBlock block, long opCode, int position)
        {
            EmitSel(block, opCode, ShaderOper.Cr);
        }

        public static void Sel_I(ShaderIrBlock block, long opCode, int position)
        {
            EmitSel(block, opCode, ShaderOper.Imm);
        }

        public static void Sel_R(ShaderIrBlock block, long opCode, int position)
        {
            EmitSel(block, opCode, ShaderOper.Rr);
        }

        public static void Mov_S(ShaderIrBlock block, long opCode, int position)
        {
            block.AddNode(new ShaderIrCmnt("Stubbed."));

            //Zero is used as a special number to get a valid "0 * 0 + VertexIndex" in a GS
            ShaderIrNode source = new ShaderIrOperImm(0);

            block.AddNode(opCode.PredNode(new ShaderIrAsg(opCode.Gpr0(), source)));
        }

        private static void EmitF2F(ShaderIrBlock block, long opCode, ShaderOper oper)
        {
            bool negA = opCode.Read(45);
            bool absA = opCode.Read(49);

            ShaderIrNode operA;

            switch (oper)
            {
                case ShaderOper.Cr:   operA = opCode.Cbuf34();    break;
                case ShaderOper.Immf: operA = opCode.Immf19_20(); break;
                case ShaderOper.Rr:   operA = opCode.Gpr20();     break;

                default: throw new ArgumentException(nameof(oper));
            }

            operA = GetAluFabsFneg(operA, absA, negA);

            ShaderIrInst roundInst = GetRoundInst(opCode);

            if (roundInst != ShaderIrInst.Invalid)
            {
                operA = new ShaderIrOp(roundInst, operA);
            }

            block.AddNode(opCode.PredNode(new ShaderIrAsg(opCode.Gpr0(), operA)));
        }

        private static void EmitF2I(ShaderIrBlock block, long opCode, ShaderOper oper)
        {
            IntType type = GetIntType(opCode);

            if (type == IntType.U64 ||
                type == IntType.S64)
            {
                //TODO: 64-bits support.
                //Note: GLSL doesn't support 64-bits integers.
                throw new NotImplementedException();
            }

            bool negA = opCode.Read(45);
            bool absA = opCode.Read(49);

            ShaderIrNode operA;

            switch (oper)
            {
                case ShaderOper.Cr:   operA = opCode.Cbuf34();    break;
                case ShaderOper.Immf: operA = opCode.Immf19_20(); break;
                case ShaderOper.Rr:   operA = opCode.Gpr20();     break;

                default: throw new ArgumentException(nameof(oper));
            }

            operA = GetAluFabsFneg(operA, absA, negA);

            ShaderIrInst roundInst = GetRoundInst(opCode);

            if (roundInst != ShaderIrInst.Invalid)
            {
                operA = new ShaderIrOp(roundInst, operA);
            }

            bool signed = type >= IntType.S8;

            int size = 8 << ((int)type & 3);

            if (size < 32)
            {
                uint mask = uint.MaxValue >> (32 - size);

                float cMin = 0;
                float cMax = mask;

                if (signed)
                {
                    uint halfMask = mask >> 1;

                    cMin -= halfMask + 1;
                    cMax  = halfMask;
                }

                ShaderIrOperImmf min = new ShaderIrOperImmf(cMin);
                ShaderIrOperImmf max = new ShaderIrOperImmf(cMax);

                operA = new ShaderIrOp(ShaderIrInst.Fclamp, operA, min, max);
            }

            ShaderIrInst inst = signed
                ? ShaderIrInst.Ftos
                : ShaderIrInst.Ftou;

            ShaderIrNode op = new ShaderIrOp(inst, operA);

            block.AddNode(opCode.PredNode(new ShaderIrAsg(opCode.Gpr0(), op)));
        }

        private static void EmitI2F(ShaderIrBlock block, long opCode, ShaderOper oper)
        {
            IntType type = GetIntType(opCode);

            if (type == IntType.U64 ||
                type == IntType.S64)
            {
                //TODO: 64-bits support.
                //Note: GLSL doesn't support 64-bits integers.
                throw new NotImplementedException();
            }

            int sel = opCode.Read(41, 3);

            bool negA = opCode.Read(45);
            bool absA = opCode.Read(49);

            ShaderIrNode operA;

            switch (oper)
            {
                case ShaderOper.Cr:  operA = opCode.Cbuf34();   break;
                case ShaderOper.Imm: operA = opCode.Imm19_20(); break;
                case ShaderOper.Rr:  operA = opCode.Gpr20();    break;

                default: throw new ArgumentException(nameof(oper));
            }

            operA = GetAluIabsIneg(operA, absA, negA);

            bool signed = type >= IntType.S8;

            int shift = sel * 8;

            int size = 8 << ((int)type & 3);

            if (shift != 0)
            {
                operA = new ShaderIrOp(ShaderIrInst.Asr, operA, new ShaderIrOperImm(shift));
            }

            if (size < 32)
            {
                operA = ExtendTo32(operA, signed, size);
            }

            ShaderIrInst inst = signed
                ? ShaderIrInst.Stof
                : ShaderIrInst.Utof;

            ShaderIrNode op = new ShaderIrOp(inst, operA);

            block.AddNode(opCode.PredNode(new ShaderIrAsg(opCode.Gpr0(), op)));
        }

        private static void EmitI2I(ShaderIrBlock block, long opCode, ShaderOper oper)
        {
            IntType type = GetIntType(opCode);

            if (type == IntType.U64 ||
                type == IntType.S64)
            {
                //TODO: 64-bits support.
                //Note: GLSL doesn't support 64-bits integers.
                throw new NotImplementedException();
            }

            int sel = opCode.Read(41, 3);

            bool negA = opCode.Read(45);
            bool absA = opCode.Read(49);
            bool satA = opCode.Read(50);

            ShaderIrNode operA;

            switch (oper)
            {
                case ShaderOper.Cr:   operA = opCode.Cbuf34();    break;
                case ShaderOper.Immf: operA = opCode.Immf19_20(); break;
                case ShaderOper.Rr:   operA = opCode.Gpr20();     break;

                default: throw new ArgumentException(nameof(oper));
            }

            operA = GetAluIabsIneg(operA, absA, negA);

            bool signed = type >= IntType.S8;

            int shift = sel * 8;

            int size = 8 << ((int)type & 3);

            if (shift != 0)
            {
                operA = new ShaderIrOp(ShaderIrInst.Asr, operA, new ShaderIrOperImm(shift));
            }

            if (size < 32)
            {
                uint mask = uint.MaxValue >> (32 - size);

                if (satA)
                {
                    uint cMin = 0;
                    uint cMax = mask;

                    if (signed)
                    {
                        uint halfMask = mask >> 1;

                        cMin -= halfMask + 1;
                        cMax  = halfMask;
                    }

                    ShaderIrOperImm min = new ShaderIrOperImm((int)cMin);
                    ShaderIrOperImm max = new ShaderIrOperImm((int)cMax);

                    operA = new ShaderIrOp(signed
                        ? ShaderIrInst.Clamps
                        : ShaderIrInst.Clampu, operA, min, max);
                }
                else
                {
                    operA = ExtendTo32(operA, signed, size);
                }
            }

            block.AddNode(opCode.PredNode(new ShaderIrAsg(opCode.Gpr0(), operA)));
        }

        private static void EmitSel(ShaderIrBlock block, long opCode, ShaderOper oper)
        {
            ShaderIrOperGpr dst  = opCode.Gpr0();
            ShaderIrNode    pred = opCode.Pred39N();

            ShaderIrNode resultA = opCode.Gpr8();
            ShaderIrNode resultB;

            switch (oper)
            {
                case ShaderOper.Cr:  resultB = opCode.Cbuf34();   break;
                case ShaderOper.Imm: resultB = opCode.Imm19_20(); break;
                case ShaderOper.Rr:  resultB = opCode.Gpr20();    break;

                default: throw new ArgumentException(nameof(oper));
            }

            block.AddNode(opCode.PredNode(new ShaderIrCond(pred, new ShaderIrAsg(dst, resultA), false)));

            block.AddNode(opCode.PredNode(new ShaderIrCond(pred, new ShaderIrAsg(dst, resultB), true)));
        }

        private static IntType GetIntType(long opCode)
        {
            bool signed = opCode.Read(13);

            IntType type = (IntType)(opCode.Read(10, 3));

            if (signed)
            {
                type += (int)IntType.S8;
            }

            return type;
        }

        private static FloatType GetFloatType(long opCode)
        {
            return (FloatType)(opCode.Read(8, 3));
        }

        private static ShaderIrInst GetRoundInst(long opCode)
        {
            switch (opCode.Read(39, 3))
            {
                case 1: return ShaderIrInst.Floor;
                case 2: return ShaderIrInst.Ceil;
                case 3: return ShaderIrInst.Trunc;
            }

            return ShaderIrInst.Invalid;
        }
    }
}