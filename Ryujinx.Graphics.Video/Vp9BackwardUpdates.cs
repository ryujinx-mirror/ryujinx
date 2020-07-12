using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Video
{
    public struct Vp9BackwardUpdates
    {
        public Array4<Array10<uint>> YMode;
        public Array10<Array10<uint>> UvMode;
        public Array16<Array4<uint>> Partition;
        public Array4<Array2<Array2<Array6<Array6<Array4<uint>>>>>> Coef;
        public Array4<Array2<Array2<Array6<Array6<uint>>>>> EobBranch;
        public Array4<Array3<uint>> SwitchableInterp;
        public Array7<Array4<uint>> InterMode;
        public Array4<Array2<uint>> IntraInter;
        public Array5<Array2<uint>> CompInter;
        public Array5<Array2<Array2<uint>>> SingleRef;
        public Array5<Array2<uint>> CompRef;
        public Array2<Array4<uint>> Tx32x32;
        public Array2<Array3<uint>> Tx16x16;
        public Array2<Array2<uint>> Tx8x8;
        public Array3<Array2<uint>> Skip;
        public Array4<uint> Joints;
        public Array2<Array2<uint>> Sign;
        public Array2<Array11<uint>> Classes;
        public Array2<Array2<uint>> Class0;
        public Array2<Array10<Array2<uint>>> Bits;
        public Array2<Array2<Array4<uint>>> Class0Fp;
        public Array2<Array4<uint>> Fp;
        public Array2<Array2<uint>> Class0Hp;
        public Array2<Array2<uint>> Hp;
    }
}
