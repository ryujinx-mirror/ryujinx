// https://www.intel.com/content/dam/doc/white-paper/advanced-encryption-standard-new-instructions-set-paper.pdf

using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    public class CpuTestSimdCrypto : CpuTest
    {
        [Test, Description("AESD <Vd>.16B, <Vn>.16B")]
        public void Aesd_V([Values(0u)] uint Rd,
                           [Values(1u)] uint Rn,
                           [Values(0x7B5B546573745665ul)] ulong ValueH,
                           [Values(0x63746F725D53475Dul)] ulong ValueL,
                           [Random(2)]                    ulong RoundKeyH,
                           [Random(2)]                    ulong RoundKeyL,
                           [Values(0x8DCAB9BC035006BCul)] ulong ResultH,
                           [Values(0x8F57161E00CAFD8Dul)] ulong ResultL)
        {
            uint Opcode = 0x4E285800; // AESD V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(RoundKeyL ^ ValueL, RoundKeyH ^ ValueH);
            Vector128<float> V1 = MakeVectorE0E1(RoundKeyL,          RoundKeyH);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(ResultL));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(ResultH));
            });
            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V1), Is.EqualTo(RoundKeyL));
                Assert.That(GetVectorE1(ThreadState.V1), Is.EqualTo(RoundKeyH));
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("AESE <Vd>.16B, <Vn>.16B")]
        public void Aese_V([Values(0u)] uint Rd,
                           [Values(1u)] uint Rn,
                           [Values(0x7B5B546573745665ul)] ulong ValueH,
                           [Values(0x63746F725D53475Dul)] ulong ValueL,
                           [Random(2)]                    ulong RoundKeyH,
                           [Random(2)]                    ulong RoundKeyL,
                           [Values(0x8F92A04DFBED204Dul)] ulong ResultH,
                           [Values(0x4C39B1402192A84Cul)] ulong ResultL)
        {
            uint Opcode = 0x4E284800; // AESE V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(RoundKeyL ^ ValueL, RoundKeyH ^ ValueH);
            Vector128<float> V1 = MakeVectorE0E1(RoundKeyL,          RoundKeyH);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(ResultL));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(ResultH));
            });
            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V1), Is.EqualTo(RoundKeyL));
                Assert.That(GetVectorE1(ThreadState.V1), Is.EqualTo(RoundKeyH));
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("AESIMC <Vd>.16B, <Vn>.16B")]
        public void Aesimc_V([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(0x8DCAB9DC035006BCul)] ulong ValueH,
                             [Values(0x8F57161E00CAFD8Dul)] ulong ValueL,
                             [Values(0xD635A667928B5EAEul)] ulong ResultH,
                             [Values(0xEEC9CC3BC55F5777ul)] ulong ResultL)
        {
            uint Opcode = 0x4E287800; // AESIMC V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V = MakeVectorE0E1(ValueL, ValueH);

            AThreadState ThreadState = SingleOpcode(
                Opcode,
                V0: Rn == 0u ? V : default(Vector128<float>),
                V1: Rn == 1u ? V : default(Vector128<float>));

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(ResultL));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(ResultH));
            });
            if (Rn == 1u)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(GetVectorE0(ThreadState.V1), Is.EqualTo(ValueL));
                    Assert.That(GetVectorE1(ThreadState.V1), Is.EqualTo(ValueH));
                });
            }

            CompareAgainstUnicorn();
        }

        [Test, Description("AESMC <Vd>.16B, <Vn>.16B")]
        public void Aesmc_V([Values(0u)]     uint Rd,
                            [Values(1u, 0u)] uint Rn,
                            [Values(0x627A6F6644B109C8ul)] ulong ValueH,
                            [Values(0x2B18330A81C3B3E5ul)] ulong ValueL,
                            [Values(0x7B5B546573745665ul)] ulong ResultH,
                            [Values(0x63746F725D53475Dul)] ulong ResultL)
        {
            uint Opcode = 0x4E286800; // AESMC V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V = MakeVectorE0E1(ValueL, ValueH);

            AThreadState ThreadState = SingleOpcode(
                Opcode,
                V0: Rn == 0u ? V : default(Vector128<float>),
                V1: Rn == 1u ? V : default(Vector128<float>));

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(ResultL));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(ResultH));
            });
            if (Rn == 1u)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(GetVectorE0(ThreadState.V1), Is.EqualTo(ValueL));
                    Assert.That(GetVectorE1(ThreadState.V1), Is.EqualTo(ValueH));
                });
            }

            CompareAgainstUnicorn();
        }
    }
}
