using System;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonArithmetic
    {
        public static void Vaba(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, u ? context.Arm64Assembler.Uaba : context.Arm64Assembler.Saba, null);
        }

        public static void Vabal(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryLong(context, rd, rn, rm, size, u ? context.Arm64Assembler.Uabal : context.Arm64Assembler.Sabal);
        }

        public static void VabdF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FabdV, context.Arm64Assembler.FabdVH);
        }

        public static void VabdI(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, u ? context.Arm64Assembler.Uabd : context.Arm64Assembler.Sabd, null);
        }

        public static void Vabdl(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryLong(context, rd, rn, rm, size, u ? context.Arm64Assembler.Uabdl : context.Arm64Assembler.Sabdl);
        }

        public static void Vabs(CodeGenContext context, uint rd, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FabsSingleAndDouble, context.Arm64Assembler.FabsHalf);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.AbsV);
            }
        }

        public static void VaddF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FaddSingleAndDouble, context.Arm64Assembler.FaddHalf);
        }

        public static void VaddI(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, context.Arm64Assembler.AddV, context.Arm64Assembler.AddS);
        }

        public static void Vaddhn(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryNarrow(context, rd, rn, rm, size, context.Arm64Assembler.Addhn);
        }

        public static void Vaddl(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryLong(context, rd, rn, rm, size, u ? context.Arm64Assembler.Uaddl : context.Arm64Assembler.Saddl);
        }

        public static void Vaddw(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryWide(context, rd, rn, rm, size, u ? context.Arm64Assembler.Uaddw : context.Arm64Assembler.Saddw);
        }

        public static void VfmaF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryRdF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FmlaVecSingleAndDouble, context.Arm64Assembler.FmlaVecHalf);
        }

        public static void VfmsF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryRdF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FmlsVecSingleAndDouble, context.Arm64Assembler.FmlsVecHalf);
        }

        public static void Vhadd(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, u ? context.Arm64Assembler.Uhadd : context.Arm64Assembler.Shadd, null);
        }

        public static void Vhsub(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, u ? context.Arm64Assembler.Uhsub : context.Arm64Assembler.Shsub, null);
        }

        public static void Vmaxnm(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FmaxnmSingleAndDouble, context.Arm64Assembler.FmaxnmHalf);
        }

        public static void VmaxF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FmaxSingleAndDouble, context.Arm64Assembler.FmaxHalf);
        }

        public static void VmaxI(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, u ? context.Arm64Assembler.Umax : context.Arm64Assembler.Smax, null);
        }

        public static void Vminnm(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FminnmSingleAndDouble, context.Arm64Assembler.FminnmHalf);
        }

        public static void VminF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FminSingleAndDouble, context.Arm64Assembler.FminHalf);
        }

        public static void VminI(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, u ? context.Arm64Assembler.Umin : context.Arm64Assembler.Smin, null);
        }

        public static void VmlaF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryMulNegRdF(context, rd, rn, rm, sz, q, negProduct: false);
        }

        public static void VmlaI(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryRd(context, rd, rn, rm, size, q, context.Arm64Assembler.MlaVec);
        }

        public static void VmlaS(CodeGenContext context, uint rd, uint rn, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorTernaryMulNegRdByScalarAnyF(context, rd, rn, rm, size, q, negProduct: false);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorTernaryRdByScalar(context, rd, rn, rm, size, q, context.Arm64Assembler.MlaElt);
            }
        }

        public static void VmlalI(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size)
        {
            InstEmitNeonCommon.EmitVectorTernaryRdLong(context, rd, rn, rm, size, u ? context.Arm64Assembler.UmlalVec : context.Arm64Assembler.SmlalVec);
        }

        public static void VmlalS(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size)
        {
            InstEmitNeonCommon.EmitVectorTernaryRdLongByScalar(context, rd, rn, rm, size, u ? context.Arm64Assembler.UmlalElt : context.Arm64Assembler.SmlalElt);
        }

        public static void VmlsF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryMulNegRdF(context, rd, rn, rm, sz, q, negProduct: true);
        }

        public static void VmlsI(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryRd(context, rd, rn, rm, size, q, context.Arm64Assembler.MlsVec);
        }

        public static void VmlsS(CodeGenContext context, uint rd, uint rn, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorTernaryMulNegRdByScalarAnyF(context, rd, rn, rm, size, q, negProduct: true);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorTernaryRdByScalar(context, rd, rn, rm, size, q, context.Arm64Assembler.MlsElt);
            }
        }

        public static void VmlslI(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size)
        {
            InstEmitNeonCommon.EmitVectorTernaryRdLong(context, rd, rn, rm, size, u ? context.Arm64Assembler.UmlslVec : context.Arm64Assembler.SmlslVec);
        }

        public static void VmlslS(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size)
        {
            InstEmitNeonCommon.EmitVectorTernaryRdLongByScalar(context, rd, rn, rm, size, u ? context.Arm64Assembler.UmlslElt : context.Arm64Assembler.SmlslElt);
        }

        public static void VmulF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FmulVecSingleAndDouble, context.Arm64Assembler.FmulVecHalf);
        }

        public static void VmulI(CodeGenContext context, uint rd, uint rn, uint rm, bool op, uint size, uint q)
        {
            if (op)
            {
                // TODO: Feature check, emulation if not supported.

                InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, context.Arm64Assembler.Pmul, null);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, context.Arm64Assembler.MulVec, null);
            }
        }

        public static void VmulS(CodeGenContext context, uint rd, uint rn, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorBinaryByScalarAnyF(context, rd, rn, rm, size, q, context.Arm64Assembler.FmulElt2regElementSingleAndDouble, context.Arm64Assembler.FmulElt2regElementHalf);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorBinaryByScalar(context, rd, rn, rm, size, q, context.Arm64Assembler.MulElt);
            }
        }

        public static void VmullI(CodeGenContext context, uint rd, uint rn, uint rm, bool op, bool u, uint size)
        {
            if (op)
            {
                // TODO: Feature check, emulation if not supported.

                InstEmitNeonCommon.EmitVectorBinaryLong(context, rd, rn, rm, size == 2 ? 3 : size, context.Arm64Assembler.Pmull);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorBinaryLong(context, rd, rn, rm, size, u ? context.Arm64Assembler.UmullVec : context.Arm64Assembler.SmullVec);
            }
        }

        public static void VmullS(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryLongByScalar(context, rd, rn, rm, size, u ? context.Arm64Assembler.UmullElt : context.Arm64Assembler.SmullElt);
        }

        public static void Vneg(CodeGenContext context, uint rd, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FnegSingleAndDouble, context.Arm64Assembler.FnegHalf);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.NegV);
            }
        }

        public static void Vpadal(CodeGenContext context, uint rd, uint rm, bool op, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryRd(context, rd, rm, size, q, op ? context.Arm64Assembler.Uadalp : context.Arm64Assembler.Sadalp);
        }

        public static void VpaddF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FaddpVecSingleAndDouble, context.Arm64Assembler.FaddpVecHalf);
        }

        public static void VpaddI(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, context.Arm64Assembler.AddpVec, null);
        }

        public static void Vpaddl(CodeGenContext context, uint rd, uint rm, bool op, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, op ? context.Arm64Assembler.Uaddlp : context.Arm64Assembler.Saddlp);
        }

        public static void VpmaxF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FmaxpVecSingleAndDouble, context.Arm64Assembler.FmaxpVecHalf);
        }

        public static void VpmaxI(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, u ? context.Arm64Assembler.Umaxp : context.Arm64Assembler.Smaxp, null);
        }

        public static void VpminF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FminpVecSingleAndDouble, context.Arm64Assembler.FminpVecHalf);
        }

        public static void VpminI(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, u ? context.Arm64Assembler.Uminp : context.Arm64Assembler.Sminp, null);
        }

        public static void Vrecpe(CodeGenContext context, uint rd, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FrecpeV, context.Arm64Assembler.FrecpeVH);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static void Vrecps(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FrecpsV, context.Arm64Assembler.FrecpsVH);
        }

        public static void Vrsqrte(CodeGenContext context, uint rd, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FrsqrteV, context.Arm64Assembler.FrsqrteVH);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static void Vrsqrts(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FrsqrtsV, context.Arm64Assembler.FrsqrtsVH);
        }

        public static void VsubF(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FsubSingleAndDouble, context.Arm64Assembler.FsubHalf);
        }

        public static void VsubI(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, context.Arm64Assembler.SubV, context.Arm64Assembler.SubS);
        }

        public static void Vsubhn(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryNarrow(context, rd, rn, rm, size, context.Arm64Assembler.Subhn);
        }

        public static void Vsubl(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryLong(context, rd, rn, rm, size, u ? context.Arm64Assembler.Usubl : context.Arm64Assembler.Ssubl);
        }

        public static void Vsubw(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryWide(context, rd, rn, rm, size, u ? context.Arm64Assembler.Usubw : context.Arm64Assembler.Ssubw);
        }
    }
}
