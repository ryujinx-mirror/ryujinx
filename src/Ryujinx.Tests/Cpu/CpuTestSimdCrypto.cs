// https://www.intel.com/content/dam/doc/white-paper/advanced-encryption-standard-new-instructions-set-paper.pdf

using ARMeilleure.State;
using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    public class CpuTestSimdCrypto : CpuTest
    {
        [Test, Description("AESD <Vd>.16B, <Vn>.16B")]
        public void Aesd_V([Values(0u)] uint rd,
                           [Values(1u)] uint rn,
                           [Values(0x7B5B546573745665ul)] ulong valueH,
                           [Values(0x63746F725D53475Dul)] ulong valueL,
                           [Random(2)] ulong roundKeyH,
                           [Random(2)] ulong roundKeyL,
                           [Values(0x8DCAB9BC035006BCul)] ulong resultH,
                           [Values(0x8F57161E00CAFD8Dul)] ulong resultL)
        {
            uint opcode = 0x4E285800; // AESD V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            V128 v0 = MakeVectorE0E1(roundKeyL ^ valueL, roundKeyH ^ valueH);
            V128 v1 = MakeVectorE0E1(roundKeyL, roundKeyH);

            ExecutionContext context = SingleOpcode(opcode, v0: v0, v1: v1);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(context.GetV(0)), Is.EqualTo(resultL));
                Assert.That(GetVectorE1(context.GetV(0)), Is.EqualTo(resultH));
            });
            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(context.GetV(1)), Is.EqualTo(roundKeyL));
                Assert.That(GetVectorE1(context.GetV(1)), Is.EqualTo(roundKeyH));
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("AESE <Vd>.16B, <Vn>.16B")]
        public void Aese_V([Values(0u)] uint rd,
                           [Values(1u)] uint rn,
                           [Values(0x7B5B546573745665ul)] ulong valueH,
                           [Values(0x63746F725D53475Dul)] ulong valueL,
                           [Random(2)] ulong roundKeyH,
                           [Random(2)] ulong roundKeyL,
                           [Values(0x8F92A04DFBED204Dul)] ulong resultH,
                           [Values(0x4C39B1402192A84Cul)] ulong resultL)
        {
            uint opcode = 0x4E284800; // AESE V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            V128 v0 = MakeVectorE0E1(roundKeyL ^ valueL, roundKeyH ^ valueH);
            V128 v1 = MakeVectorE0E1(roundKeyL, roundKeyH);

            ExecutionContext context = SingleOpcode(opcode, v0: v0, v1: v1);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(context.GetV(0)), Is.EqualTo(resultL));
                Assert.That(GetVectorE1(context.GetV(0)), Is.EqualTo(resultH));
            });
            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(context.GetV(1)), Is.EqualTo(roundKeyL));
                Assert.That(GetVectorE1(context.GetV(1)), Is.EqualTo(roundKeyH));
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("AESIMC <Vd>.16B, <Vn>.16B")]
        public void Aesimc_V([Values(0u)] uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(0x8DCAB9DC035006BCul)] ulong valueH,
                             [Values(0x8F57161E00CAFD8Dul)] ulong valueL,
                             [Values(0xD635A667928B5EAEul)] ulong resultH,
                             [Values(0xEEC9CC3BC55F5777ul)] ulong resultL)
        {
            uint opcode = 0x4E287800; // AESIMC V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            V128 v = MakeVectorE0E1(valueL, valueH);

            ExecutionContext context = SingleOpcode(
                opcode,
                v0: rn == 0u ? v : default,
                v1: rn == 1u ? v : default);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(context.GetV(0)), Is.EqualTo(resultL));
                Assert.That(GetVectorE1(context.GetV(0)), Is.EqualTo(resultH));
            });
            if (rn == 1u)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(GetVectorE0(context.GetV(1)), Is.EqualTo(valueL));
                    Assert.That(GetVectorE1(context.GetV(1)), Is.EqualTo(valueH));
                });
            }

            CompareAgainstUnicorn();
        }

        [Test, Description("AESMC <Vd>.16B, <Vn>.16B")]
        public void Aesmc_V([Values(0u)] uint rd,
                            [Values(1u, 0u)] uint rn,
                            [Values(0x627A6F6644B109C8ul)] ulong valueH,
                            [Values(0x2B18330A81C3B3E5ul)] ulong valueL,
                            [Values(0x7B5B546573745665ul)] ulong resultH,
                            [Values(0x63746F725D53475Dul)] ulong resultL)
        {
            uint opcode = 0x4E286800; // AESMC V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            V128 v = MakeVectorE0E1(valueL, valueH);

            ExecutionContext context = SingleOpcode(
                opcode,
                v0: rn == 0u ? v : default,
                v1: rn == 1u ? v : default);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(context.GetV(0)), Is.EqualTo(resultL));
                Assert.That(GetVectorE1(context.GetV(0)), Is.EqualTo(resultH));
            });
            if (rn == 1u)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(GetVectorE0(context.GetV(1)), Is.EqualTo(valueL));
                    Assert.That(GetVectorE1(context.GetV(1)), Is.EqualTo(valueH));
                });
            }

            CompareAgainstUnicorn();
        }
    }
}
