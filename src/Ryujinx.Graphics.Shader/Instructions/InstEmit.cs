using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void AtomCas(EmitterContext context)
        {
            context.GetOp<InstAtomCas>();

            context.Config.GpuAccessor.Log("Shader instruction AtomCas is not implemented.");
        }

        public static void AtomsCas(EmitterContext context)
        {
            context.GetOp<InstAtomsCas>();

            context.Config.GpuAccessor.Log("Shader instruction AtomsCas is not implemented.");
        }

        public static void B2r(EmitterContext context)
        {
            context.GetOp<InstB2r>();

            context.Config.GpuAccessor.Log("Shader instruction B2r is not implemented.");
        }

        public static void Bpt(EmitterContext context)
        {
            context.GetOp<InstBpt>();

            context.Config.GpuAccessor.Log("Shader instruction Bpt is not implemented.");
        }

        public static void Cctl(EmitterContext context)
        {
            context.GetOp<InstCctl>();

            context.Config.GpuAccessor.Log("Shader instruction Cctl is not implemented.");
        }

        public static void Cctll(EmitterContext context)
        {
            context.GetOp<InstCctll>();

            context.Config.GpuAccessor.Log("Shader instruction Cctll is not implemented.");
        }

        public static void Cctlt(EmitterContext context)
        {
            context.GetOp<InstCctlt>();

            context.Config.GpuAccessor.Log("Shader instruction Cctlt is not implemented.");
        }

        public static void Cs2r(EmitterContext context)
        {
            context.GetOp<InstCs2r>();

            context.Config.GpuAccessor.Log("Shader instruction Cs2r is not implemented.");
        }

        public static void FchkR(EmitterContext context)
        {
            context.GetOp<InstFchkR>();

            context.Config.GpuAccessor.Log("Shader instruction FchkR is not implemented.");
        }

        public static void FchkI(EmitterContext context)
        {
            context.GetOp<InstFchkI>();

            context.Config.GpuAccessor.Log("Shader instruction FchkI is not implemented.");
        }

        public static void FchkC(EmitterContext context)
        {
            context.GetOp<InstFchkC>();

            context.Config.GpuAccessor.Log("Shader instruction FchkC is not implemented.");
        }

        public static void Getcrsptr(EmitterContext context)
        {
            context.GetOp<InstGetcrsptr>();

            context.Config.GpuAccessor.Log("Shader instruction Getcrsptr is not implemented.");
        }

        public static void Getlmembase(EmitterContext context)
        {
            context.GetOp<InstGetlmembase>();

            context.Config.GpuAccessor.Log("Shader instruction Getlmembase is not implemented.");
        }

        public static void Ide(EmitterContext context)
        {
            context.GetOp<InstIde>();

            context.Config.GpuAccessor.Log("Shader instruction Ide is not implemented.");
        }

        public static void IdpR(EmitterContext context)
        {
            context.GetOp<InstIdpR>();

            context.Config.GpuAccessor.Log("Shader instruction IdpR is not implemented.");
        }

        public static void IdpC(EmitterContext context)
        {
            context.GetOp<InstIdpC>();

            context.Config.GpuAccessor.Log("Shader instruction IdpC is not implemented.");
        }

        public static void ImadspR(EmitterContext context)
        {
            context.GetOp<InstImadspR>();

            context.Config.GpuAccessor.Log("Shader instruction ImadspR is not implemented.");
        }

        public static void ImadspI(EmitterContext context)
        {
            context.GetOp<InstImadspI>();

            context.Config.GpuAccessor.Log("Shader instruction ImadspI is not implemented.");
        }

        public static void ImadspC(EmitterContext context)
        {
            context.GetOp<InstImadspC>();

            context.Config.GpuAccessor.Log("Shader instruction ImadspC is not implemented.");
        }

        public static void ImadspRc(EmitterContext context)
        {
            context.GetOp<InstImadspRc>();

            context.Config.GpuAccessor.Log("Shader instruction ImadspRc is not implemented.");
        }

        public static void Jcal(EmitterContext context)
        {
            context.GetOp<InstJcal>();

            context.Config.GpuAccessor.Log("Shader instruction Jcal is not implemented.");
        }

        public static void Jmp(EmitterContext context)
        {
            context.GetOp<InstJmp>();

            context.Config.GpuAccessor.Log("Shader instruction Jmp is not implemented.");
        }

        public static void Jmx(EmitterContext context)
        {
            context.GetOp<InstJmx>();

            context.Config.GpuAccessor.Log("Shader instruction Jmx is not implemented.");
        }

        public static void Ld(EmitterContext context)
        {
            context.GetOp<InstLd>();

            context.Config.GpuAccessor.Log("Shader instruction Ld is not implemented.");
        }

        public static void Lepc(EmitterContext context)
        {
            context.GetOp<InstLepc>();

            context.Config.GpuAccessor.Log("Shader instruction Lepc is not implemented.");
        }

        public static void Longjmp(EmitterContext context)
        {
            context.GetOp<InstLongjmp>();

            context.Config.GpuAccessor.Log("Shader instruction Longjmp is not implemented.");
        }

        public static void Pexit(EmitterContext context)
        {
            context.GetOp<InstPexit>();

            context.Config.GpuAccessor.Log("Shader instruction Pexit is not implemented.");
        }

        public static void Pixld(EmitterContext context)
        {
            context.GetOp<InstPixld>();

            context.Config.GpuAccessor.Log("Shader instruction Pixld is not implemented.");
        }

        public static void Plongjmp(EmitterContext context)
        {
            context.GetOp<InstPlongjmp>();

            context.Config.GpuAccessor.Log("Shader instruction Plongjmp is not implemented.");
        }

        public static void Pret(EmitterContext context)
        {
            context.GetOp<InstPret>();

            context.Config.GpuAccessor.Log("Shader instruction Pret is not implemented.");
        }

        public static void PrmtR(EmitterContext context)
        {
            context.GetOp<InstPrmtR>();

            context.Config.GpuAccessor.Log("Shader instruction PrmtR is not implemented.");
        }

        public static void PrmtI(EmitterContext context)
        {
            context.GetOp<InstPrmtI>();

            context.Config.GpuAccessor.Log("Shader instruction PrmtI is not implemented.");
        }

        public static void PrmtC(EmitterContext context)
        {
            context.GetOp<InstPrmtC>();

            context.Config.GpuAccessor.Log("Shader instruction PrmtC is not implemented.");
        }

        public static void PrmtRc(EmitterContext context)
        {
            context.GetOp<InstPrmtRc>();

            context.Config.GpuAccessor.Log("Shader instruction PrmtRc is not implemented.");
        }

        public static void R2b(EmitterContext context)
        {
            context.GetOp<InstR2b>();

            context.Config.GpuAccessor.Log("Shader instruction R2b is not implemented.");
        }

        public static void Ram(EmitterContext context)
        {
            context.GetOp<InstRam>();

            context.Config.GpuAccessor.Log("Shader instruction Ram is not implemented.");
        }

        public static void Rtt(EmitterContext context)
        {
            context.GetOp<InstRtt>();

            context.Config.GpuAccessor.Log("Shader instruction Rtt is not implemented.");
        }

        public static void Sam(EmitterContext context)
        {
            context.GetOp<InstSam>();

            context.Config.GpuAccessor.Log("Shader instruction Sam is not implemented.");
        }

        public static void Setcrsptr(EmitterContext context)
        {
            context.GetOp<InstSetcrsptr>();

            context.Config.GpuAccessor.Log("Shader instruction Setcrsptr is not implemented.");
        }

        public static void Setlmembase(EmitterContext context)
        {
            context.GetOp<InstSetlmembase>();

            context.Config.GpuAccessor.Log("Shader instruction Setlmembase is not implemented.");
        }

        public static void St(EmitterContext context)
        {
            context.GetOp<InstSt>();

            context.Config.GpuAccessor.Log("Shader instruction St is not implemented.");
        }

        public static void Stp(EmitterContext context)
        {
            context.GetOp<InstStp>();

            context.Config.GpuAccessor.Log("Shader instruction Stp is not implemented.");
        }

        public static void Txa(EmitterContext context)
        {
            context.GetOp<InstTxa>();

            context.Config.GpuAccessor.Log("Shader instruction Txa is not implemented.");
        }

        public static void Vabsdiff(EmitterContext context)
        {
            context.GetOp<InstVabsdiff>();

            context.Config.GpuAccessor.Log("Shader instruction Vabsdiff is not implemented.");
        }

        public static void Vabsdiff4(EmitterContext context)
        {
            context.GetOp<InstVabsdiff4>();

            context.Config.GpuAccessor.Log("Shader instruction Vabsdiff4 is not implemented.");
        }

        public static void Vadd(EmitterContext context)
        {
            context.GetOp<InstVadd>();

            context.Config.GpuAccessor.Log("Shader instruction Vadd is not implemented.");
        }

        public static void Votevtg(EmitterContext context)
        {
            context.GetOp<InstVotevtg>();

            context.Config.GpuAccessor.Log("Shader instruction Votevtg is not implemented.");
        }

        public static void Vset(EmitterContext context)
        {
            context.GetOp<InstVset>();

            context.Config.GpuAccessor.Log("Shader instruction Vset is not implemented.");
        }

        public static void Vshl(EmitterContext context)
        {
            context.GetOp<InstVshl>();

            context.Config.GpuAccessor.Log("Shader instruction Vshl is not implemented.");
        }

        public static void Vshr(EmitterContext context)
        {
            context.GetOp<InstVshr>();

            context.Config.GpuAccessor.Log("Shader instruction Vshr is not implemented.");
        }
    }
}
