using System;
using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonConvert
    {
        public static void Vcvta(CodeGenContext context, uint rd, uint rm, bool op, uint size, uint q)
        {
            if (op)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcvtauV, context.Arm64Assembler.FcvtauVH);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcvtasV, context.Arm64Assembler.FcvtasVH);
            }
        }

        public static void Vcvtm(CodeGenContext context, uint rd, uint rm, bool op, uint size, uint q)
        {
            if (op)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcvtmuV, context.Arm64Assembler.FcvtmuVH);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcvtmsV, context.Arm64Assembler.FcvtmsVH);
            }
        }

        public static void Vcvtn(CodeGenContext context, uint rd, uint rm, bool op, uint size, uint q)
        {
            if (op)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcvtnuV, context.Arm64Assembler.FcvtnuVH);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcvtnsV, context.Arm64Assembler.FcvtnsVH);
            }
        }

        public static void Vcvtp(CodeGenContext context, uint rd, uint rm, bool op, uint size, uint q)
        {
            if (op)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcvtpuV, context.Arm64Assembler.FcvtpuVH);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcvtpsV, context.Arm64Assembler.FcvtpsVH);
            }
        }

        public static void VcvtHs(CodeGenContext context, uint rd, uint rm, bool op)
        {
            bool halfToSingle = op;
            if (halfToSingle)
            {
                // Half to single.

                InstEmitNeonCommon.EmitVectorUnaryLong(context, rd, rm, 0, context.Arm64Assembler.Fcvtl);
            }
            else
            {
                // Single to half.

                InstEmitNeonCommon.EmitVectorUnaryNarrow(context, rd, rm, 0, context.Arm64Assembler.Fcvtn);
            }
        }

        public static void VcvtIs(CodeGenContext context, uint rd, uint rm, uint op, uint size, uint q)
        {
            Debug.Assert(op >> 2 == 0);

            bool unsigned = (op & 1) != 0;
            bool toInteger = (op >> 1) != 0;

            if (toInteger)
            {
                if (unsigned)
                {
                    InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcvtzuIntV, context.Arm64Assembler.FcvtzuIntVH);
                }
                else
                {
                    InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcvtzsIntV, context.Arm64Assembler.FcvtzsIntVH);
                }
            }
            else
            {
                if (unsigned)
                {
                    InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.UcvtfIntV, context.Arm64Assembler.UcvtfIntVH);
                }
                else
                {
                    InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.ScvtfIntV, context.Arm64Assembler.ScvtfIntVH);
                }
            }
        }

        public static void VcvtXs(CodeGenContext context, uint rd, uint rm, uint imm6, uint op, bool u, uint q)
        {
            Debug.Assert(op >> 2 == 0);

            bool unsigned = u;
            bool toFixed = (op & 1) != 0;
            uint size = 1 + (op >> 1);
            uint fbits = Math.Clamp(64u - imm6, 1, 8u << (int)size);

            if (toFixed)
            {
                if (unsigned)
                {
                    InstEmitNeonCommon.EmitVectorUnaryFixedAnyF(context, rd, rm, fbits, size, q, context.Arm64Assembler.FcvtzuFixV);
                }
                else
                {
                    InstEmitNeonCommon.EmitVectorUnaryFixedAnyF(context, rd, rm, fbits, size, q, context.Arm64Assembler.FcvtzsFixV);
                }
            }
            else
            {
                if (unsigned)
                {
                    InstEmitNeonCommon.EmitVectorUnaryFixedAnyF(context, rd, rm, fbits, size, q, context.Arm64Assembler.UcvtfFixV);
                }
                else
                {
                    InstEmitNeonCommon.EmitVectorUnaryFixedAnyF(context, rd, rm, fbits, size, q, context.Arm64Assembler.ScvtfFixV);
                }
            }
        }
    }
}
