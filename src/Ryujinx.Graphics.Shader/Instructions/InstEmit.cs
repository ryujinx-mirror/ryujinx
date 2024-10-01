using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void AtomCas(EmitterContext context)
        {
            context.GetOp<InstAtomCas>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction AtomCas is not implemented.");
        }

        public static void AtomsCas(EmitterContext context)
        {
            context.GetOp<InstAtomsCas>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction AtomsCas is not implemented.");
        }

        public static void B2r(EmitterContext context)
        {
            context.GetOp<InstB2r>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction B2r is not implemented.");
        }

        public static void Bpt(EmitterContext context)
        {
            context.GetOp<InstBpt>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Bpt is not implemented.");
        }

        public static void Cctl(EmitterContext context)
        {
            context.GetOp<InstCctl>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Cctl is not implemented.");
        }

        public static void Cctll(EmitterContext context)
        {
            context.GetOp<InstCctll>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Cctll is not implemented.");
        }

        public static void Cctlt(EmitterContext context)
        {
            context.GetOp<InstCctlt>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Cctlt is not implemented.");
        }

        public static void Cs2r(EmitterContext context)
        {
            context.GetOp<InstCs2r>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Cs2r is not implemented.");
        }

        public static void FchkR(EmitterContext context)
        {
            context.GetOp<InstFchkR>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction FchkR is not implemented.");
        }

        public static void FchkI(EmitterContext context)
        {
            context.GetOp<InstFchkI>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction FchkI is not implemented.");
        }

        public static void FchkC(EmitterContext context)
        {
            context.GetOp<InstFchkC>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction FchkC is not implemented.");
        }

        public static void Getcrsptr(EmitterContext context)
        {
            context.GetOp<InstGetcrsptr>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Getcrsptr is not implemented.");
        }

        public static void Getlmembase(EmitterContext context)
        {
            context.GetOp<InstGetlmembase>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Getlmembase is not implemented.");
        }

        public static void Ide(EmitterContext context)
        {
            context.GetOp<InstIde>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Ide is not implemented.");
        }

        public static void IdpR(EmitterContext context)
        {
            context.GetOp<InstIdpR>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction IdpR is not implemented.");
        }

        public static void IdpC(EmitterContext context)
        {
            context.GetOp<InstIdpC>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction IdpC is not implemented.");
        }

        public static void ImadspR(EmitterContext context)
        {
            context.GetOp<InstImadspR>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction ImadspR is not implemented.");
        }

        public static void ImadspI(EmitterContext context)
        {
            context.GetOp<InstImadspI>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction ImadspI is not implemented.");
        }

        public static void ImadspC(EmitterContext context)
        {
            context.GetOp<InstImadspC>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction ImadspC is not implemented.");
        }

        public static void ImadspRc(EmitterContext context)
        {
            context.GetOp<InstImadspRc>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction ImadspRc is not implemented.");
        }

        public static void Jcal(EmitterContext context)
        {
            context.GetOp<InstJcal>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Jcal is not implemented.");
        }

        public static void Jmp(EmitterContext context)
        {
            context.GetOp<InstJmp>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Jmp is not implemented.");
        }

        public static void Jmx(EmitterContext context)
        {
            context.GetOp<InstJmx>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Jmx is not implemented.");
        }

        public static void Ld(EmitterContext context)
        {
            context.GetOp<InstLd>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Ld is not implemented.");
        }

        public static void Lepc(EmitterContext context)
        {
            context.GetOp<InstLepc>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Lepc is not implemented.");
        }

        public static void Longjmp(EmitterContext context)
        {
            context.GetOp<InstLongjmp>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Longjmp is not implemented.");
        }

        public static void Pexit(EmitterContext context)
        {
            context.GetOp<InstPexit>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Pexit is not implemented.");
        }

        public static void Pixld(EmitterContext context)
        {
            context.GetOp<InstPixld>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Pixld is not implemented.");
        }

        public static void Plongjmp(EmitterContext context)
        {
            context.GetOp<InstPlongjmp>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Plongjmp is not implemented.");
        }

        public static void Pret(EmitterContext context)
        {
            context.GetOp<InstPret>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Pret is not implemented.");
        }

        public static void PrmtR(EmitterContext context)
        {
            context.GetOp<InstPrmtR>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction PrmtR is not implemented.");
        }

        public static void PrmtI(EmitterContext context)
        {
            context.GetOp<InstPrmtI>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction PrmtI is not implemented.");
        }

        public static void PrmtC(EmitterContext context)
        {
            context.GetOp<InstPrmtC>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction PrmtC is not implemented.");
        }

        public static void PrmtRc(EmitterContext context)
        {
            context.GetOp<InstPrmtRc>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction PrmtRc is not implemented.");
        }

        public static void R2b(EmitterContext context)
        {
            context.GetOp<InstR2b>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction R2b is not implemented.");
        }

        public static void Ram(EmitterContext context)
        {
            context.GetOp<InstRam>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Ram is not implemented.");
        }

        public static void Rtt(EmitterContext context)
        {
            context.GetOp<InstRtt>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Rtt is not implemented.");
        }

        public static void Sam(EmitterContext context)
        {
            context.GetOp<InstSam>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Sam is not implemented.");
        }

        public static void Setcrsptr(EmitterContext context)
        {
            context.GetOp<InstSetcrsptr>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Setcrsptr is not implemented.");
        }

        public static void Setlmembase(EmitterContext context)
        {
            context.GetOp<InstSetlmembase>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Setlmembase is not implemented.");
        }

        public static void St(EmitterContext context)
        {
            context.GetOp<InstSt>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction St is not implemented.");
        }

        public static void Stp(EmitterContext context)
        {
            context.GetOp<InstStp>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Stp is not implemented.");
        }

        public static void Txa(EmitterContext context)
        {
            context.GetOp<InstTxa>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Txa is not implemented.");
        }

        public static void Vabsdiff(EmitterContext context)
        {
            context.GetOp<InstVabsdiff>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Vabsdiff is not implemented.");
        }

        public static void Vabsdiff4(EmitterContext context)
        {
            context.GetOp<InstVabsdiff4>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Vabsdiff4 is not implemented.");
        }

        public static void Vadd(EmitterContext context)
        {
            context.GetOp<InstVadd>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Vadd is not implemented.");
        }

        public static void Votevtg(EmitterContext context)
        {
            context.GetOp<InstVotevtg>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Votevtg is not implemented.");
        }

        public static void Vset(EmitterContext context)
        {
            context.GetOp<InstVset>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Vset is not implemented.");
        }

        public static void Vshl(EmitterContext context)
        {
            context.GetOp<InstVshl>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Vshl is not implemented.");
        }

        public static void Vshr(EmitterContext context)
        {
            context.GetOp<InstVshr>();

            context.TranslatorContext.GpuAccessor.Log("Shader instruction Vshr is not implemented.");
        }
    }
}
