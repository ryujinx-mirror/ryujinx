#define SimdRegElem

using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdRegElem")] // Tested: second half of 2018.
    public sealed class CpuTestSimdRegElem : CpuTest
    {
#if SimdRegElem

#region "ValueSource"
        private static ulong[] _2S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _4H_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0xFFFFFFFFFFFFFFFFul };
        }
#endregion

        private const int RndCnt = 2;

        [Test, Pairwise, Description("MLA <Vd>.<T>, <Vn>.<T>, <Vm>.<Ts>[<index>]")]
        public void Mla_Ve_4H_8H([Values(0u)]     uint Rd,
                                 [Values(1u, 0u)] uint Rn,
                                 [Values(2u, 0u)] uint Rm,
                                 [ValueSource("_4H_")] [Random(RndCnt)] ulong Z,
                                 [ValueSource("_4H_")] [Random(RndCnt)] ulong A,
                                 [ValueSource("_4H_")] [Random(RndCnt)] ulong B,
                                 [Values(0u, 1u, 2u, 3u, 4u, 5u, 6u, 7u)] uint Index,
                                 [Values(0b0u, 0b1u)] uint Q) // <4H, 8H>
        {
            uint H = (Index & 4) >> 2;
            uint L = (Index & 2) >> 1;
            uint M = (Index & 1) >> 0;

            uint Opcode = 0x2F400000; // MLA V0.4H, V0.4H, V0.H[0]
            Opcode |= ((Rm & 15) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (L << 21) | (M << 20) | (H << 11);
            Opcode |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MLA <Vd>.<T>, <Vn>.<T>, <Vm>.<Ts>[<index>]")]
        public void Mla_Ve_2S_4S([Values(0u)]     uint Rd,
                                 [Values(1u, 0u)] uint Rn,
                                 [Values(2u, 0u)] uint Rm,
                                 [ValueSource("_2S_")] [Random(RndCnt)] ulong Z,
                                 [ValueSource("_2S_")] [Random(RndCnt)] ulong A,
                                 [ValueSource("_2S_")] [Random(RndCnt)] ulong B,
                                 [Values(0u, 1u, 2u, 3u)] uint Index,
                                 [Values(0b0u, 0b1u)] uint Q) // <2S, 4S>
        {
            uint H = (Index & 2) >> 1;
            uint L = (Index & 1) >> 0;

            uint Opcode = 0x2F800000; // MLA V0.2S, V0.2S, V0.S[0]
            Opcode |= ((Rm & 15) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (L << 21) | (H << 11);
            Opcode |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MLS <Vd>.<T>, <Vn>.<T>, <Vm>.<Ts>[<index>]")]
        public void Mls_Ve_4H_8H([Values(0u)]     uint Rd,
                                 [Values(1u, 0u)] uint Rn,
                                 [Values(2u, 0u)] uint Rm,
                                 [ValueSource("_4H_")] [Random(RndCnt)] ulong Z,
                                 [ValueSource("_4H_")] [Random(RndCnt)] ulong A,
                                 [ValueSource("_4H_")] [Random(RndCnt)] ulong B,
                                 [Values(0u, 1u, 2u, 3u, 4u, 5u, 6u, 7u)] uint Index,
                                 [Values(0b0u, 0b1u)] uint Q) // <4H, 8H>
        {
            uint H = (Index & 4) >> 2;
            uint L = (Index & 2) >> 1;
            uint M = (Index & 1) >> 0;

            uint Opcode = 0x2F404000; // MLS V0.4H, V0.4H, V0.H[0]
            Opcode |= ((Rm & 15) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (L << 21) | (M << 20) | (H << 11);
            Opcode |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MLS <Vd>.<T>, <Vn>.<T>, <Vm>.<Ts>[<index>]")]
        public void Mls_Ve_2S_4S([Values(0u)]     uint Rd,
                                 [Values(1u, 0u)] uint Rn,
                                 [Values(2u, 0u)] uint Rm,
                                 [ValueSource("_2S_")] [Random(RndCnt)] ulong Z,
                                 [ValueSource("_2S_")] [Random(RndCnt)] ulong A,
                                 [ValueSource("_2S_")] [Random(RndCnt)] ulong B,
                                 [Values(0u, 1u, 2u, 3u)] uint Index,
                                 [Values(0b0u, 0b1u)] uint Q) // <2S, 4S>
        {
            uint H = (Index & 2) >> 1;
            uint L = (Index & 1) >> 0;

            uint Opcode = 0x2F804000; // MLS V0.2S, V0.2S, V0.S[0]
            Opcode |= ((Rm & 15) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (L << 21) | (H << 11);
            Opcode |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            CompareAgainstUnicorn();
        }
#endif
    }
}
