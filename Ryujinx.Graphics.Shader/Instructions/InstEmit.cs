using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void AtomCas(EmitterContext context)
        {
            InstAtomCas op = context.GetOp<InstAtomCas>();

            context.Config.GpuAccessor.Log("Shader instruction AtomCas is not implemented.");
        }

        public static void AtomsCas(EmitterContext context)
        {
            InstAtomsCas op = context.GetOp<InstAtomsCas>();

            context.Config.GpuAccessor.Log("Shader instruction AtomsCas is not implemented.");
        }

        public static void B2r(EmitterContext context)
        {
            InstB2r op = context.GetOp<InstB2r>();

            context.Config.GpuAccessor.Log("Shader instruction B2r is not implemented.");
        }

        public static void Bpt(EmitterContext context)
        {
            InstBpt op = context.GetOp<InstBpt>();

            context.Config.GpuAccessor.Log("Shader instruction Bpt is not implemented.");
        }

        public static void Cctl(EmitterContext context)
        {
            InstCctl op = context.GetOp<InstCctl>();

            context.Config.GpuAccessor.Log("Shader instruction Cctl is not implemented.");
        }

        public static void Cctll(EmitterContext context)
        {
            InstCctll op = context.GetOp<InstCctll>();

            context.Config.GpuAccessor.Log("Shader instruction Cctll is not implemented.");
        }

        public static void Cctlt(EmitterContext context)
        {
            InstCctlt op = context.GetOp<InstCctlt>();

            context.Config.GpuAccessor.Log("Shader instruction Cctlt is not implemented.");
        }

        public static void Cont(EmitterContext context)
        {
            InstCont op = context.GetOp<InstCont>();

            context.Config.GpuAccessor.Log("Shader instruction ContUnsup is not implemented.");
        }

        public static void Cset(EmitterContext context)
        {
            InstCset op = context.GetOp<InstCset>();

            context.Config.GpuAccessor.Log("Shader instruction CsetUnsup is not implemented.");
        }

        public static void Cs2r(EmitterContext context)
        {
            InstCs2r op = context.GetOp<InstCs2r>();

            context.Config.GpuAccessor.Log("Shader instruction Cs2r is not implemented.");
        }

        public static void DmnmxR(EmitterContext context)
        {
            InstDmnmxR op = context.GetOp<InstDmnmxR>();

            context.Config.GpuAccessor.Log("Shader instruction DmnmxR is not implemented.");
        }

        public static void DmnmxI(EmitterContext context)
        {
            InstDmnmxI op = context.GetOp<InstDmnmxI>();

            context.Config.GpuAccessor.Log("Shader instruction DmnmxI is not implemented.");
        }

        public static void DmnmxC(EmitterContext context)
        {
            InstDmnmxC op = context.GetOp<InstDmnmxC>();

            context.Config.GpuAccessor.Log("Shader instruction DmnmxC is not implemented.");
        }

        public static void DsetR(EmitterContext context)
        {
            InstDsetR op = context.GetOp<InstDsetR>();

            context.Config.GpuAccessor.Log("Shader instruction DsetR is not implemented.");
        }

        public static void DsetI(EmitterContext context)
        {
            InstDsetI op = context.GetOp<InstDsetI>();

            context.Config.GpuAccessor.Log("Shader instruction DsetI is not implemented.");
        }

        public static void DsetC(EmitterContext context)
        {
            InstDsetC op = context.GetOp<InstDsetC>();

            context.Config.GpuAccessor.Log("Shader instruction DsetC is not implemented.");
        }

        public static void DsetpR(EmitterContext context)
        {
            InstDsetpR op = context.GetOp<InstDsetpR>();

            context.Config.GpuAccessor.Log("Shader instruction DsetpR is not implemented.");
        }

        public static void DsetpI(EmitterContext context)
        {
            InstDsetpI op = context.GetOp<InstDsetpI>();

            context.Config.GpuAccessor.Log("Shader instruction DsetpI is not implemented.");
        }

        public static void DsetpC(EmitterContext context)
        {
            InstDsetpC op = context.GetOp<InstDsetpC>();

            context.Config.GpuAccessor.Log("Shader instruction DsetpC is not implemented.");
        }

        public static void FchkR(EmitterContext context)
        {
            InstFchkR op = context.GetOp<InstFchkR>();

            context.Config.GpuAccessor.Log("Shader instruction FchkR is not implemented.");
        }

        public static void FchkI(EmitterContext context)
        {
            InstFchkI op = context.GetOp<InstFchkI>();

            context.Config.GpuAccessor.Log("Shader instruction FchkI is not implemented.");
        }

        public static void FchkC(EmitterContext context)
        {
            InstFchkC op = context.GetOp<InstFchkC>();

            context.Config.GpuAccessor.Log("Shader instruction FchkC is not implemented.");
        }

        public static void Getcrsptr(EmitterContext context)
        {
            InstGetcrsptr op = context.GetOp<InstGetcrsptr>();

            context.Config.GpuAccessor.Log("Shader instruction Getcrsptr is not implemented.");
        }

        public static void Getlmembase(EmitterContext context)
        {
            InstGetlmembase op = context.GetOp<InstGetlmembase>();

            context.Config.GpuAccessor.Log("Shader instruction Getlmembase is not implemented.");
        }

        public static void Ide(EmitterContext context)
        {
            InstIde op = context.GetOp<InstIde>();

            context.Config.GpuAccessor.Log("Shader instruction Ide is not implemented.");
        }

        public static void IdpR(EmitterContext context)
        {
            InstIdpR op = context.GetOp<InstIdpR>();

            context.Config.GpuAccessor.Log("Shader instruction IdpR is not implemented.");
        }

        public static void IdpC(EmitterContext context)
        {
            InstIdpC op = context.GetOp<InstIdpC>();

            context.Config.GpuAccessor.Log("Shader instruction IdpC is not implemented.");
        }

        public static void ImadspR(EmitterContext context)
        {
            InstImadspR op = context.GetOp<InstImadspR>();

            context.Config.GpuAccessor.Log("Shader instruction ImadspR is not implemented.");
        }

        public static void ImadspI(EmitterContext context)
        {
            InstImadspI op = context.GetOp<InstImadspI>();

            context.Config.GpuAccessor.Log("Shader instruction ImadspI is not implemented.");
        }

        public static void ImadspC(EmitterContext context)
        {
            InstImadspC op = context.GetOp<InstImadspC>();

            context.Config.GpuAccessor.Log("Shader instruction ImadspC is not implemented.");
        }

        public static void ImadspRc(EmitterContext context)
        {
            InstImadspRc op = context.GetOp<InstImadspRc>();

            context.Config.GpuAccessor.Log("Shader instruction ImadspRc is not implemented.");
        }

        public static void ImulR(EmitterContext context)
        {
            InstImulR op = context.GetOp<InstImulR>();

            context.Config.GpuAccessor.Log("Shader instruction ImulR is not implemented.");
        }

        public static void ImulI(EmitterContext context)
        {
            InstImulI op = context.GetOp<InstImulI>();

            context.Config.GpuAccessor.Log("Shader instruction ImulI is not implemented.");
        }

        public static void ImulC(EmitterContext context)
        {
            InstImulC op = context.GetOp<InstImulC>();

            context.Config.GpuAccessor.Log("Shader instruction ImulC is not implemented.");
        }

        public static void Imul32i(EmitterContext context)
        {
            InstImul32i op = context.GetOp<InstImul32i>();

            context.Config.GpuAccessor.Log("Shader instruction Imul32i is not implemented.");
        }

        public static void Jcal(EmitterContext context)
        {
            InstJcal op = context.GetOp<InstJcal>();

            context.Config.GpuAccessor.Log("Shader instruction Jcal is not implemented.");
        }

        public static void Jmp(EmitterContext context)
        {
            InstJmp op = context.GetOp<InstJmp>();

            context.Config.GpuAccessor.Log("Shader instruction Jmp is not implemented.");
        }

        public static void Jmx(EmitterContext context)
        {
            InstJmx op = context.GetOp<InstJmx>();

            context.Config.GpuAccessor.Log("Shader instruction Jmx is not implemented.");
        }

        public static void Ld(EmitterContext context)
        {
            InstLd op = context.GetOp<InstLd>();

            context.Config.GpuAccessor.Log("Shader instruction Ld is not implemented.");
        }

        public static void Lepc(EmitterContext context)
        {
            InstLepc op = context.GetOp<InstLepc>();

            context.Config.GpuAccessor.Log("Shader instruction Lepc is not implemented.");
        }

        public static void Longjmp(EmitterContext context)
        {
            InstLongjmp op = context.GetOp<InstLongjmp>();

            context.Config.GpuAccessor.Log("Shader instruction Longjmp is not implemented.");
        }

        public static void P2rR(EmitterContext context)
        {
            InstP2rR op = context.GetOp<InstP2rR>();

            context.Config.GpuAccessor.Log("Shader instruction P2rR is not implemented.");
        }

        public static void P2rI(EmitterContext context)
        {
            InstP2rI op = context.GetOp<InstP2rI>();

            context.Config.GpuAccessor.Log("Shader instruction P2rI is not implemented.");
        }

        public static void P2rC(EmitterContext context)
        {
            InstP2rC op = context.GetOp<InstP2rC>();

            context.Config.GpuAccessor.Log("Shader instruction P2rC is not implemented.");
        }

        public static void Pcnt(EmitterContext context)
        {
            InstPcnt op = context.GetOp<InstPcnt>();

            context.Config.GpuAccessor.Log("Shader instruction Pcnt is not implemented.");
        }

        public static void Pexit(EmitterContext context)
        {
            InstPexit op = context.GetOp<InstPexit>();

            context.Config.GpuAccessor.Log("Shader instruction Pexit is not implemented.");
        }

        public static void Pixld(EmitterContext context)
        {
            InstPixld op = context.GetOp<InstPixld>();

            context.Config.GpuAccessor.Log("Shader instruction Pixld is not implemented.");
        }

        public static void Plongjmp(EmitterContext context)
        {
            InstPlongjmp op = context.GetOp<InstPlongjmp>();

            context.Config.GpuAccessor.Log("Shader instruction Plongjmp is not implemented.");
        }

        public static void Pret(EmitterContext context)
        {
            InstPret op = context.GetOp<InstPret>();

            context.Config.GpuAccessor.Log("Shader instruction Pret is not implemented.");
        }

        public static void PrmtR(EmitterContext context)
        {
            InstPrmtR op = context.GetOp<InstPrmtR>();

            context.Config.GpuAccessor.Log("Shader instruction PrmtR is not implemented.");
        }

        public static void PrmtI(EmitterContext context)
        {
            InstPrmtI op = context.GetOp<InstPrmtI>();

            context.Config.GpuAccessor.Log("Shader instruction PrmtI is not implemented.");
        }

        public static void PrmtC(EmitterContext context)
        {
            InstPrmtC op = context.GetOp<InstPrmtC>();

            context.Config.GpuAccessor.Log("Shader instruction PrmtC is not implemented.");
        }

        public static void PrmtRc(EmitterContext context)
        {
            InstPrmtRc op = context.GetOp<InstPrmtRc>();

            context.Config.GpuAccessor.Log("Shader instruction PrmtRc is not implemented.");
        }

        public static void R2b(EmitterContext context)
        {
            InstR2b op = context.GetOp<InstR2b>();

            context.Config.GpuAccessor.Log("Shader instruction R2b is not implemented.");
        }

        public static void Ram(EmitterContext context)
        {
            InstRam op = context.GetOp<InstRam>();

            context.Config.GpuAccessor.Log("Shader instruction Ram is not implemented.");
        }

        public static void Rtt(EmitterContext context)
        {
            InstRtt op = context.GetOp<InstRtt>();

            context.Config.GpuAccessor.Log("Shader instruction Rtt is not implemented.");
        }

        public static void Sam(EmitterContext context)
        {
            InstSam op = context.GetOp<InstSam>();

            context.Config.GpuAccessor.Log("Shader instruction Sam is not implemented.");
        }

        public static void Setcrsptr(EmitterContext context)
        {
            InstSetcrsptr op = context.GetOp<InstSetcrsptr>();

            context.Config.GpuAccessor.Log("Shader instruction Setcrsptr is not implemented.");
        }

        public static void Setlmembase(EmitterContext context)
        {
            InstSetlmembase op = context.GetOp<InstSetlmembase>();

            context.Config.GpuAccessor.Log("Shader instruction Setlmembase is not implemented.");
        }

        public static void St(EmitterContext context)
        {
            InstSt op = context.GetOp<InstSt>();

            context.Config.GpuAccessor.Log("Shader instruction St is not implemented.");
        }

        public static void Stp(EmitterContext context)
        {
            InstStp op = context.GetOp<InstStp>();

            context.Config.GpuAccessor.Log("Shader instruction Stp is not implemented.");
        }

        public static void Txa(EmitterContext context)
        {
            InstTxa op = context.GetOp<InstTxa>();

            context.Config.GpuAccessor.Log("Shader instruction Txa is not implemented.");
        }

        public static void Vabsdiff(EmitterContext context)
        {
            InstVabsdiff op = context.GetOp<InstVabsdiff>();

            context.Config.GpuAccessor.Log("Shader instruction Vabsdiff is not implemented.");
        }

        public static void Vabsdiff4(EmitterContext context)
        {
            InstVabsdiff4 op = context.GetOp<InstVabsdiff4>();

            context.Config.GpuAccessor.Log("Shader instruction Vabsdiff4 is not implemented.");
        }

        public static void Vadd(EmitterContext context)
        {
            InstVadd op = context.GetOp<InstVadd>();

            context.Config.GpuAccessor.Log("Shader instruction Vadd is not implemented.");
        }

        public static void Votevtg(EmitterContext context)
        {
            InstVotevtg op = context.GetOp<InstVotevtg>();

            context.Config.GpuAccessor.Log("Shader instruction Votevtg is not implemented.");
        }

        public static void Vset(EmitterContext context)
        {
            InstVset op = context.GetOp<InstVset>();

            context.Config.GpuAccessor.Log("Shader instruction Vset is not implemented.");
        }

        public static void Vshl(EmitterContext context)
        {
            InstVshl op = context.GetOp<InstVshl>();

            context.Config.GpuAccessor.Log("Shader instruction Vshl is not implemented.");
        }

        public static void Vshr(EmitterContext context)
        {
            InstVshr op = context.GetOp<InstVshr>();

            context.Config.GpuAccessor.Log("Shader instruction Vshr is not implemented.");
        }
    }
}