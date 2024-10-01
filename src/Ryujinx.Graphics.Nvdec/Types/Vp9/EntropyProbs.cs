using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Video;

namespace Ryujinx.Graphics.Nvdec.Types.Vp9
{
    struct EntropyProbs
    {
#pragma warning disable CS0649 // Field is never assigned to
        public Array10<Array10<Array8<byte>>> KfYModeProbE0ToE7;
        public Array10<Array10<byte>> KfYModeProbE8;
        public Array3<byte> Padding384;
        public Array7<byte> SegTreeProbs;
        public Array3<byte> SegPredProbs;
        public Array15<byte> Padding391;
        public Array10<Array8<byte>> KfUvModeProbE0ToE7;
        public Array10<byte> KfUvModeProbE8;
        public Array6<byte> Padding3FA;
        public Array7<Array4<byte>> InterModeProb;
        public Array4<byte> IntraInterProb;
        public Array10<Array8<byte>> UvModeProbE0ToE7;
        public Array2<Array1<byte>> Tx8x8Prob;
        public Array2<Array2<byte>> Tx16x16Prob;
        public Array2<Array3<byte>> Tx32x32Prob;
        public Array4<byte> YModeProbE8;
        public Array4<Array8<byte>> YModeProbE0ToE7;
        public Array16<Array4<byte>> KfPartitionProb;
        public Array16<Array4<byte>> PartitionProb;
        public Array10<byte> UvModeProbE8;
        public Array4<Array2<byte>> SwitchableInterpProb;
        public Array5<byte> CompInterProb;
        public Array4<byte> SkipProbs;
        public Array3<byte> Joints;
        public Array2<byte> Sign;
        public Array2<Array1<byte>> Class0;
        public Array2<Array3<byte>> Fp;
        public Array2<byte> Class0Hp;
        public Array2<byte> Hp;
        public Array2<Array10<byte>> Classes;
        public Array2<Array2<Array3<byte>>> Class0Fp;
        public Array2<Array10<byte>> Bits;
        public Array5<Array2<byte>> SingleRefProb;
        public Array5<byte> CompRefProb;
        public Array17<byte> Padding58F;
        public Array4<Array2<Array2<Array6<Array6<Array4<byte>>>>>> CoefProbs;
#pragma warning restore CS0649

        public void Convert(ref Vp9EntropyProbs fc)
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        fc.KfYModeProb[i][j][k] = k < 8 ? KfYModeProbE0ToE7[i][j][k] : KfYModeProbE8[i][j];
                    }
                }
            }

            fc.SegTreeProb = SegTreeProbs;
            fc.SegPredProb = SegPredProbs;

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    fc.InterModeProb[i][j] = InterModeProb[i][j];
                }
            }

            fc.IntraInterProb = IntraInterProb;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    fc.KfUvModeProb[i][j] = j < 8 ? KfUvModeProbE0ToE7[i][j] : KfUvModeProbE8[i];
                    fc.UvModeProb[i][j] = j < 8 ? UvModeProbE0ToE7[i][j] : UvModeProbE8[i];
                }
            }

            fc.Tx8x8Prob = Tx8x8Prob;
            fc.Tx16x16Prob = Tx16x16Prob;
            fc.Tx32x32Prob = Tx32x32Prob;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    fc.YModeProb[i][j] = j < 8 ? YModeProbE0ToE7[i][j] : YModeProbE8[i];
                }
            }

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    fc.KfPartitionProb[i][j] = KfPartitionProb[i][j];
                    fc.PartitionProb[i][j] = PartitionProb[i][j];
                }
            }

            fc.SwitchableInterpProb = SwitchableInterpProb;
            fc.CompInterProb = CompInterProb;
            fc.SkipProb[0] = SkipProbs[0];
            fc.SkipProb[1] = SkipProbs[1];
            fc.SkipProb[2] = SkipProbs[2];
            fc.Joints = Joints;
            fc.Sign = Sign;
            fc.Class0 = Class0;
            fc.Fp = Fp;
            fc.Class0Hp = Class0Hp;
            fc.Hp = Hp;
            fc.Classes = Classes;
            fc.Class0Fp = Class0Fp;
            fc.Bits = Bits;
            fc.SingleRefProb = SingleRefProb;
            fc.CompRefProb = CompRefProb;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int l = 0; l < 6; l++)
                        {
                            for (int m = 0; m < 6; m++)
                            {
                                for (int n = 0; n < 3; n++)
                                {
                                    fc.CoefProbs[i][j][k][l][m][n] = CoefProbs[i][j][k][l][m][n];
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
