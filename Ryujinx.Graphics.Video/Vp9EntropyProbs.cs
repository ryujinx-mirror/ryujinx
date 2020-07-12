using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Video
{
    public struct Vp9EntropyProbs
    {
        public Array10<Array10<Array9<byte>>> KfYModeProb;
        public Array7<byte> SegTreeProb;
        public Array3<byte> SegPredProb;
        public Array10<Array9<byte>> KfUvModeProb;
        public Array4<Array9<byte>> YModeProb;
        public Array10<Array9<byte>> UvModeProb;
        public Array16<Array3<byte>> KfPartitionProb;
        public Array16<Array3<byte>> PartitionProb;
        public Array4<Array2<Array2<Array6<Array6<Array3<byte>>>>>> CoefProbs;
        public Array4<Array2<byte>> SwitchableInterpProb;
        public Array7<Array3<byte>> InterModeProb;
        public Array4<byte> IntraInterProb;
        public Array5<byte> CompInterProb;
        public Array5<Array2<byte>> SingleRefProb;
        public Array5<byte> CompRefProb;
        public Array2<Array3<byte>> Tx32x32Prob;
        public Array2<Array2<byte>> Tx16x16Prob;
        public Array2<Array1<byte>> Tx8x8Prob;
        public Array3<byte> SkipProb;
        public Array3<byte> Joints;
        public Array2<byte> Sign;
        public Array2<Array10<byte>> Classes;
        public Array2<Array1<byte>> Class0;
        public Array2<Array10<byte>> Bits;
        public Array2<Array2<Array3<byte>>> Class0Fp;
        public Array2<Array3<byte>> Fp;
        public Array2<byte> Class0Hp;
        public Array2<byte> Hp;
    }
}
