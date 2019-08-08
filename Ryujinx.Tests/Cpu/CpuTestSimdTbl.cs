#define SimdTbl

using ARMeilleure.State;

using NUnit.Framework;

using System.Collections.Generic;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdTbl")]
    public sealed class CpuTestSimdTbl : CpuTest
    {
#if SimdTbl

#region "Helper methods"
        private static ulong GenIdxsForTbls(int regs)
        {
            const byte idxInRngMin  = (byte)0;
                  byte idxInRngMax  = (byte)((16 * regs) - 1);
                  byte idxOutRngMin = (byte) (16 * regs);
            const byte idxOutRngMax = (byte)255;

            ulong idxs = 0ul;

            for (int cnt = 1; cnt <= 8; cnt++)
            {
                ulong idxInRng  = (ulong)TestContext.CurrentContext.Random.NextByte(idxInRngMin,  idxInRngMax);
                ulong idxOutRng = (ulong)TestContext.CurrentContext.Random.NextByte(idxOutRngMin, idxOutRngMax);

                ulong idx = TestContext.CurrentContext.Random.NextBool() ? idxInRng : idxOutRng;

                idxs = (idxs << 8) | idx;
            }

            return idxs;
        }
#endregion

#region "ValueSource (Types)"
        private static ulong[] _8B_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static IEnumerable<ulong> _GenIdxsForTbl1_()
        {
            yield return 0x0000000000000000ul;
            yield return 0x7F7F7F7F7F7F7F7Ful;
            yield return 0x8080808080808080ul;
            yield return 0xFFFFFFFFFFFFFFFFul;

            for (int cnt = 1; cnt <= RndCntIdxs; cnt++)
            {
                yield return GenIdxsForTbls(regs: 1);
            }
        }

        private static IEnumerable<ulong> _GenIdxsForTbl2_()
        {
            yield return 0x0000000000000000ul;
            yield return 0x7F7F7F7F7F7F7F7Ful;
            yield return 0x8080808080808080ul;
            yield return 0xFFFFFFFFFFFFFFFFul;

            for (int cnt = 1; cnt <= RndCntIdxs; cnt++)
            {
                yield return GenIdxsForTbls(regs: 2);
            }
        }

        private static IEnumerable<ulong> _GenIdxsForTbl3_()
        {
            yield return 0x0000000000000000ul;
            yield return 0x7F7F7F7F7F7F7F7Ful;
            yield return 0x8080808080808080ul;
            yield return 0xFFFFFFFFFFFFFFFFul;

            for (int cnt = 1; cnt <= RndCntIdxs; cnt++)
            {
                yield return GenIdxsForTbls(regs: 3);
            }
        }

        private static IEnumerable<ulong> _GenIdxsForTbl4_()
        {
            yield return 0x0000000000000000ul;
            yield return 0x7F7F7F7F7F7F7F7Ful;
            yield return 0x8080808080808080ul;
            yield return 0xFFFFFFFFFFFFFFFFul;

            for (int cnt = 1; cnt <= RndCntIdxs; cnt++)
            {
                yield return GenIdxsForTbls(regs: 4);
            }
        }
#endregion

#region "ValueSource (Opcodes)"
        private static uint[] _SingleRegTbl_V_8B_16B_()
        {
            return new uint[]
            {
                0x0E000000u, // TBL V0.8B, { V0.16B }, V0.8B
            };
        }

        private static uint[] _TwoRegTbl_V_8B_16B_()
        {
            return new uint[]
            {
                0x0E002000u, // TBL V0.8B, { V0.16B, V1.16B }, V0.8B
            };
        }

        private static uint[] _ThreeRegTbl_V_8B_16B_()
        {
            return new uint[]
            {
                0x0E004000u, // TBL V0.8B, { V0.16B, V1.16B, V2.16B }, V0.8B
            };
        }

        private static uint[] _FourRegTbl_V_8B_16B_()
        {
            return new uint[]
            {
                0x0E006000u, // TBL V0.8B, { V0.16B, V1.16B, V2.16B, V3.16B }, V0.8B
            };
        }
#endregion

        private const int RndCntTbls = 2;
        private const int RndCntIdxs = 2;

        [Test, Pairwise, Description("TBL <Vd>.<Ta>, { <Vn>.16B }, <Vm>.<Ta>")]
        public void SingleRegTbl_V_8B_16B([ValueSource("_SingleRegTbl_V_8B_16B_")] uint opcodes,
                                          [Values(0u)] uint rd,
                                          [Values(1u)] uint rn,
                                          [Values(2u)] uint rm,
                                          [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table0,
                                          [ValueSource("_GenIdxsForTbl1_")] ulong indexes,
                                          [Values(0b0u, 0b1u)] uint q) // <8B, 16B>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            V128 v0 = MakeVectorE0E1(z, z);
            V128 v1 = MakeVectorE0E1(table0, table0);
            V128 v2 = MakeVectorE0E1(indexes, q == 1u ? indexes : 0ul);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("TBL <Vd>.<Ta>, { <Vn>.16B, <Vn+1>.16B }, <Vm>.<Ta>")]
        public void TwoRegTbl_V_8B_16B([ValueSource("_TwoRegTbl_V_8B_16B_")] uint opcodes,
                                       [Values(0u)] uint rd,
                                       [Values(1u)] uint rn,
                                       [Values(3u)] uint rm,
                                       [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table0,
                                       [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table1,
                                       [ValueSource("_GenIdxsForTbl2_")] ulong indexes,
                                       [Values(0b0u, 0b1u)] uint q) // <8B, 16B>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            V128 v0 = MakeVectorE0E1(z, z);
            V128 v1 = MakeVectorE0E1(table0, table0);
            V128 v2 = MakeVectorE0E1(table1, table1);
            V128 v3 = MakeVectorE0E1(indexes, q == 1u ? indexes : 0ul);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, v3: v3);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("TBL <Vd>.<Ta>, { <Vn>.16B, <Vn+1>.16B }, <Vm>.<Ta>")]
        public void Mod_TwoRegTbl_V_8B_16B([ValueSource("_TwoRegTbl_V_8B_16B_")] uint opcodes,
                                           [Values(30u, 1u)] uint rd,
                                           [Values(31u)]     uint rn,
                                           [Values(1u, 30u)] uint rm,
                                           [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table0,
                                           [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table1,
                                           [ValueSource("_GenIdxsForTbl2_")] ulong indexes,
                                           [Values(0b0u, 0b1u)] uint q) // <8B, 16B>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            V128 v30 = MakeVectorE0E1(z, z);
            V128 v31 = MakeVectorE0E1(table0, table0);
            V128 v0  = MakeVectorE0E1(table1, table1);
            V128 v1  = MakeVectorE0E1(indexes, indexes);

            SingleOpcode(opcodes, v0: v0, v1: v1, v30: v30, v31: v31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("TBL <Vd>.<Ta>, { <Vn>.16B, <Vn+1>.16B, <Vn+2>.16B }, <Vm>.<Ta>")]
        public void ThreeRegTbl_V_8B_16B([ValueSource("_ThreeRegTbl_V_8B_16B_")] uint opcodes,
                                         [Values(0u)] uint rd,
                                         [Values(1u)] uint rn,
                                         [Values(4u)] uint rm,
                                         [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table0,
                                         [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table1,
                                         [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table2,
                                         [ValueSource("_GenIdxsForTbl3_")] ulong indexes,
                                         [Values(0b0u, 0b1u)] uint q) // <8B, 16B>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            V128 v0 = MakeVectorE0E1(z, z);
            V128 v1 = MakeVectorE0E1(table0, table0);
            V128 v2 = MakeVectorE0E1(table1, table1);
            V128 v3 = MakeVectorE0E1(table2, table2);
            V128 v4 = MakeVectorE0E1(indexes, q == 1u ? indexes : 0ul);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, v3: v3, v4: v4);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("TBL <Vd>.<Ta>, { <Vn>.16B, <Vn+1>.16B, <Vn+2>.16B }, <Vm>.<Ta>")]
        public void Mod_ThreeRegTbl_V_8B_16B([ValueSource("_ThreeRegTbl_V_8B_16B_")] uint opcodes,
                                             [Values(30u, 2u)] uint rd,
                                             [Values(31u)]     uint rn,
                                             [Values(2u, 30u)] uint rm,
                                             [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table0,
                                             [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table1,
                                             [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table2,
                                             [ValueSource("_GenIdxsForTbl3_")] ulong indexes,
                                             [Values(0b0u, 0b1u)] uint q) // <8B, 16B>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            V128 v30 = MakeVectorE0E1(z, z);
            V128 v31 = MakeVectorE0E1(table0, table0);
            V128 v0  = MakeVectorE0E1(table1, table1);
            V128 v1  = MakeVectorE0E1(table2, table2);
            V128 v2  = MakeVectorE0E1(indexes, indexes);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, v30: v30, v31: v31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("TBL <Vd>.<Ta>, { <Vn>.16B, <Vn+1>.16B, <Vn+2>.16B, <Vn+3>.16B }, <Vm>.<Ta>")]
        public void FourRegTbl_V_8B_16B([ValueSource("_FourRegTbl_V_8B_16B_")] uint opcodes,
                                        [Values(0u)] uint rd,
                                        [Values(1u)] uint rn,
                                        [Values(5u)] uint rm,
                                        [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table0,
                                        [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table1,
                                        [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table2,
                                        [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table3,
                                        [ValueSource("_GenIdxsForTbl4_")] ulong indexes,
                                        [Values(0b0u, 0b1u)] uint q) // <8B, 16B>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            V128 v0 = MakeVectorE0E1(z, z);
            V128 v1 = MakeVectorE0E1(table0, table0);
            V128 v2 = MakeVectorE0E1(table1, table1);
            V128 v3 = MakeVectorE0E1(table2, table2);
            V128 v4 = MakeVectorE0E1(table3, table3);
            V128 v5 = MakeVectorE0E1(indexes, q == 1u ? indexes : 0ul);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, v3: v3, v4: v4, v5: v5);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("TBL <Vd>.<Ta>, { <Vn>.16B, <Vn+1>.16B, <Vn+2>.16B, <Vn+3>.16B }, <Vm>.<Ta>")]
        public void Mod_FourRegTbl_V_8B_16B([ValueSource("_FourRegTbl_V_8B_16B_")] uint opcodes,
                                            [Values(30u, 3u)] uint rd,
                                            [Values(31u)]     uint rn,
                                            [Values(3u, 30u)] uint rm,
                                            [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table0,
                                            [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table1,
                                            [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table2,
                                            [ValueSource("_8B_")] [Random(RndCntTbls)] ulong table3,
                                            [ValueSource("_GenIdxsForTbl4_")] ulong indexes,
                                            [Values(0b0u, 0b1u)] uint q) // <8B, 16B>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            V128 v30 = MakeVectorE0E1(z, z);
            V128 v31 = MakeVectorE0E1(table0, table0);
            V128 v0  = MakeVectorE0E1(table1, table1);
            V128 v1  = MakeVectorE0E1(table2, table2);
            V128 v2  = MakeVectorE0E1(table3, table3);
            V128 v3  = MakeVectorE0E1(indexes, indexes);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, v3: v3, v30: v30, v31: v31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
