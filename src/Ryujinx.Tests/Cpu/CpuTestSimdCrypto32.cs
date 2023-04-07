// https://www.intel.com/content/dam/doc/white-paper/advanced-encryption-standard-new-instructions-set-paper.pdf

using ARMeilleure.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    public class CpuTestSimdCrypto32 : CpuTest32
    {
        [Test, Description("AESD.8 <Qd>, <Qm>")]
        public void Aesd_V([Values(0u)] uint rd,
                           [Values(2u)] uint rm,
                           [Values(0x7B5B546573745665ul)] ulong valueH,
                           [Values(0x63746F725D53475Dul)] ulong valueL,
                           [Random(2)]                    ulong roundKeyH,
                           [Random(2)]                    ulong roundKeyL,
                           [Values(0x8DCAB9BC035006BCul)] ulong resultH,
                           [Values(0x8F57161E00CAFD8Dul)] ulong resultL)
        {
            uint opcode = 0xf3b00340; // AESD.8 Q0, Q0
            opcode |= ((rm & 0xf) << 0) | ((rm & 0x10) << 1);
            opcode |= ((rd & 0xf) << 12) | ((rd & 0x10) << 18);

            V128 v0 = MakeVectorE0E1(roundKeyL ^ valueL, roundKeyH ^ valueH);
            V128 v1 = MakeVectorE0E1(roundKeyL, roundKeyH);

            ExecutionContext context = SingleOpcode(opcode, v0: v0, v1: v1, runUnicorn: false);

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

            // Unicorn does not yet support crypto instructions in A32.
            // CompareAgainstUnicorn();
        }

        [Test, Description("AESE.8 <Qd>, <Qm>")]
        public void Aese_V([Values(0u)] uint rd,
                           [Values(2u)] uint rm,
                           [Values(0x7B5B546573745665ul)] ulong valueH,
                           [Values(0x63746F725D53475Dul)] ulong valueL,
                           [Random(2)]                    ulong roundKeyH,
                           [Random(2)]                    ulong roundKeyL,
                           [Values(0x8F92A04DFBED204Dul)] ulong resultH,
                           [Values(0x4C39B1402192A84Cul)] ulong resultL)
        {
            uint opcode = 0xf3b00300; // AESE.8 Q0, Q0
            opcode |= ((rm & 0xf) << 0) | ((rm & 0x10) << 1);
            opcode |= ((rd & 0xf) << 12) | ((rd & 0x10) << 18);

            V128 v0 = MakeVectorE0E1(roundKeyL ^ valueL, roundKeyH ^ valueH);
            V128 v1 = MakeVectorE0E1(roundKeyL, roundKeyH);

            ExecutionContext context = SingleOpcode(opcode, v0: v0, v1: v1, runUnicorn: false);

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

            // Unicorn does not yet support crypto instructions in A32.
            // CompareAgainstUnicorn();
        }

        [Test, Description("AESIMC.8 <Qd>, <Qm>")]
        public void Aesimc_V([Values(0u)]     uint rd,
                             [Values(2u, 0u)] uint rm,
                             [Values(0x8DCAB9DC035006BCul)] ulong valueH,
                             [Values(0x8F57161E00CAFD8Dul)] ulong valueL,
                             [Values(0xD635A667928B5EAEul)] ulong resultH,
                             [Values(0xEEC9CC3BC55F5777ul)] ulong resultL)
        {
            uint opcode = 0xf3b003c0; // AESIMC.8 Q0, Q0
            opcode |= ((rm & 0xf) << 0) | ((rm & 0x10) << 1);
            opcode |= ((rd & 0xf) << 12) | ((rd & 0x10) << 18);

            V128 v = MakeVectorE0E1(valueL, valueH);

            ExecutionContext context = SingleOpcode(
                opcode,
                v0: rm == 0u ? v : default(V128),
                v1: rm == 2u ? v : default(V128),
                runUnicorn: false);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(context.GetV(0)), Is.EqualTo(resultL));
                Assert.That(GetVectorE1(context.GetV(0)), Is.EqualTo(resultH));
            });
            if (rm == 2u)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(GetVectorE0(context.GetV(1)), Is.EqualTo(valueL));
                    Assert.That(GetVectorE1(context.GetV(1)), Is.EqualTo(valueH));
                });
            }

            // Unicorn does not yet support crypto instructions in A32.
            // CompareAgainstUnicorn();
        }

        [Test, Description("AESMC.8 <Qd>, <Qm>")]
        public void Aesmc_V([Values(0u)]     uint rd,
                            [Values(2u, 0u)] uint rm,
                            [Values(0x627A6F6644B109C8ul)] ulong valueH,
                            [Values(0x2B18330A81C3B3E5ul)] ulong valueL,
                            [Values(0x7B5B546573745665ul)] ulong resultH,
                            [Values(0x63746F725D53475Dul)] ulong resultL)
        {
            uint opcode = 0xf3b00380; // AESMC.8 Q0, Q0
            opcode |= ((rm & 0xf) << 0) | ((rm & 0x10) << 1);
            opcode |= ((rd & 0xf) << 12) | ((rd & 0x10) << 18);

            V128 v = MakeVectorE0E1(valueL, valueH);

            ExecutionContext context = SingleOpcode(
                opcode,
                v0: rm == 0u ? v : default(V128),
                v1: rm == 2u ? v : default(V128),
                runUnicorn: false);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(context.GetV(0)), Is.EqualTo(resultL));
                Assert.That(GetVectorE1(context.GetV(0)), Is.EqualTo(resultH));
            });
            if (rm == 2u)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(GetVectorE0(context.GetV(1)), Is.EqualTo(valueL));
                    Assert.That(GetVectorE1(context.GetV(1)), Is.EqualTo(valueH));
                });
            }

            // Unicorn does not yet support crypto instructions in A32.
            // CompareAgainstUnicorn();
        }
    }
}
