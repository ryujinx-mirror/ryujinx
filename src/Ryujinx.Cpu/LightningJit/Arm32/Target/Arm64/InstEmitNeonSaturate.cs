using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonSaturate
    {
        public static void Vqabs(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.SqabsV);
        }

        public static void Vqadd(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(
                context,
                rd,
                rn,
                rm,
                size,
                q,
                u ? context.Arm64Assembler.UqaddV : context.Arm64Assembler.SqaddV,
                u ? context.Arm64Assembler.UqaddS : context.Arm64Assembler.SqaddS);
        }

        public static void Vqdmlal(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryLong(context, rd, rn, rm, size, context.Arm64Assembler.SqdmlalVecV);
        }

        public static void VqdmlalS(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryLongByScalar(context, rd, rn, rm, size, context.Arm64Assembler.SqdmlalElt2regElement);
        }

        public static void Vqdmlsl(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryLong(context, rd, rn, rm, size, context.Arm64Assembler.SqdmlslVecV);
        }

        public static void VqdmlslS(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryLongByScalar(context, rd, rn, rm, size, context.Arm64Assembler.SqdmlslElt2regElement);
        }

        public static void Vqdmulh(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, context.Arm64Assembler.SqdmulhVecV, context.Arm64Assembler.SqdmulhVecS);
        }

        public static void VqdmulhS(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryByScalar(context, rd, rn, rm, size, q, context.Arm64Assembler.SqdmulhElt2regElement);
        }

        public static void Vqdmull(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryLong(context, rd, rn, rm, size, context.Arm64Assembler.SqdmullVecV);
        }

        public static void VqdmullS(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryLongByScalar(context, rd, rn, rm, size, context.Arm64Assembler.SqdmullElt2regElement);
        }

        public static void Vqmovn(CodeGenContext context, uint rd, uint rm, uint op, uint size)
        {
            if (op == 3)
            {
                InstEmitNeonCommon.EmitVectorUnaryNarrow(context, rd, rm, size, context.Arm64Assembler.UqxtnV);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnaryNarrow(context, rd, rm, size, op == 1 ? context.Arm64Assembler.SqxtunV : context.Arm64Assembler.SqxtnV);
            }
        }

        public static void Vqneg(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.SqnegV);
        }

        public static void Vqrdmlah(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryRd(context, rd, rn, rm, size, q, context.Arm64Assembler.SqrdmlahVecV);
        }

        public static void VqrdmlahS(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryRdByScalar(context, rd, rn, rm, size, q, context.Arm64Assembler.SqrdmlahElt2regElement);
        }

        public static void Vqrdmlsh(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryRd(context, rd, rn, rm, size, q, context.Arm64Assembler.SqrdmlshVecV);
        }

        public static void VqrdmlshS(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryRdByScalar(context, rd, rn, rm, size, q, context.Arm64Assembler.SqrdmlshElt2regElement);
        }

        public static void Vqrdmulh(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, context.Arm64Assembler.SqrdmulhVecV, context.Arm64Assembler.SqrdmulhVecS);
        }

        public static void VqrdmulhS(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryByScalar(context, rd, rn, rm, size, q, context.Arm64Assembler.SqrdmulhElt2regElement);
        }

        public static void Vqrshl(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rm, rn, size, q, context.Arm64Assembler.SqrshlV, context.Arm64Assembler.SqrshlS);
        }

        public static void Vqrshrn(CodeGenContext context, uint rd, uint rm, bool u, uint op, uint imm6)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm6(imm6);
            uint shift = InstEmitNeonShift.GetShiftRight(imm6, size);

            if (u && op == 0)
            {
                InstEmitNeonCommon.EmitVectorBinaryNarrowShift(context, rd, rm, shift, size, isShl: false, context.Arm64Assembler.SqrshrunV);
            }
            else if (!u && op == 1)
            {
                InstEmitNeonCommon.EmitVectorBinaryNarrowShift(context, rd, rm, shift, size, isShl: false, context.Arm64Assembler.SqrshrnV);
            }
            else
            {
                Debug.Assert(u && op == 1); // !u && op == 0 is the encoding for another instruction.

                InstEmitNeonCommon.EmitVectorBinaryNarrowShift(context, rd, rm, shift, size, isShl: false, context.Arm64Assembler.UqrshrnV);
            }
        }

        public static void VqshlI(CodeGenContext context, uint rd, uint rm, bool u, uint op, uint l, uint imm6, uint q)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm7(imm6 | (l << 6));
            uint shift = InstEmitNeonShift.GetShiftLeft(imm6, size);

            if (u && op == 0)
            {
                InstEmitNeonCommon.EmitVectorBinaryShift(context, rd, rm, shift, size, q, isShl: true, context.Arm64Assembler.SqshluV, context.Arm64Assembler.SqshluS);
            }
            else if (!u && op == 1)
            {
                InstEmitNeonCommon.EmitVectorBinaryShift(context, rd, rm, shift, size, q, isShl: true, context.Arm64Assembler.SqshlImmV, context.Arm64Assembler.SqshlImmS);
            }
            else
            {
                Debug.Assert(u && op == 1); // !u && op == 0 is the encoding for another instruction.

                InstEmitNeonCommon.EmitVectorBinaryShift(context, rd, rm, shift, size, q, isShl: true, context.Arm64Assembler.UqshlImmV, context.Arm64Assembler.UqshlImmS);
            }
        }

        public static void VqshlR(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            if (u)
            {
                InstEmitNeonCommon.EmitVectorBinary(context, rd, rm, rn, size, q, context.Arm64Assembler.UqshlRegV, context.Arm64Assembler.UqshlRegS);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorBinary(context, rd, rm, rn, size, q, context.Arm64Assembler.SqshlRegV, context.Arm64Assembler.SqshlRegS);
            }
        }

        public static void Vqshrn(CodeGenContext context, uint rd, uint rm, bool u, uint op, uint imm6)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm6(imm6);
            uint shift = InstEmitNeonShift.GetShiftRight(imm6, size);

            if (u && op == 0)
            {
                InstEmitNeonCommon.EmitVectorBinaryNarrowShift(context, rd, rm, shift, size, isShl: false, context.Arm64Assembler.SqshrunV);
            }
            else if (!u && op == 1)
            {
                InstEmitNeonCommon.EmitVectorBinaryNarrowShift(context, rd, rm, shift, size, isShl: false, context.Arm64Assembler.SqshrnV);
            }
            else
            {
                Debug.Assert(u && op == 1); // !u && op == 0 is the encoding for another instruction.

                InstEmitNeonCommon.EmitVectorBinaryNarrowShift(context, rd, rm, shift, size, isShl: false, context.Arm64Assembler.UqshrnV);
            }
        }

        public static void Vqsub(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(
                context,
                rd,
                rn,
                rm,
                size,
                q,
                u ? context.Arm64Assembler.UqsubV : context.Arm64Assembler.SqsubV,
                u ? context.Arm64Assembler.UqsubS : context.Arm64Assembler.SqsubS);
        }
    }
}
