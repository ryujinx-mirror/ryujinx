using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Video;

namespace Ryujinx.Graphics.Nvdec.Types.Vp9
{
    struct BackwardUpdates
    {
        public Array7<Array3<Array2<uint>>> InterModeCounts;
        public Array4<Array10<uint>> YModeCounts;
        public Array10<Array10<uint>> UvModeCounts;
        public Array16<Array4<uint>> PartitionCounts;
        public Array4<Array3<uint>> SwitchableInterpsCount;
        public Array4<Array2<uint>> IntraInterCount;
        public Array5<Array2<uint>> CompInterCount;
        public Array5<Array2<Array2<uint>>> SingleRefCount;
        public Array5<Array2<uint>> CompRefCount;
        public Array2<Array4<uint>> Tx32x32;
        public Array2<Array3<uint>> Tx16x16;
        public Array2<Array2<uint>> Tx8x8;
        public Array3<Array2<uint>> MbSkipCount;
        public Array4<uint> Joints;
        public Array2<Array2<uint>> Sign;
        public Array2<Array11<uint>> Classes;
        public Array2<Array2<uint>> Class0;
        public Array2<Array10<Array2<uint>>> Bits;
        public Array2<Array2<Array4<uint>>> Class0Fp;
        public Array2<Array4<uint>> Fp;
        public Array2<Array2<uint>> Class0Hp;
        public Array2<Array2<uint>> Hp;
        public Array4<Array2<Array2<Array6<Array6<Array4<uint>>>>>> CoefCounts;
        public Array4<Array2<Array2<Array6<Array6<uint>>>>> EobCounts;

        public BackwardUpdates(ref Vp9BackwardUpdates counts)
        {
            InterModeCounts = new Array7<Array3<Array2<uint>>>();

            for (int i = 0; i < 7; i++)
            {
                InterModeCounts[i][0][0] = counts.InterMode[i][2];
                InterModeCounts[i][0][1] = counts.InterMode[i][0] + counts.InterMode[i][1] + counts.InterMode[i][3];
                InterModeCounts[i][1][0] = counts.InterMode[i][0];
                InterModeCounts[i][1][1] = counts.InterMode[i][1] + counts.InterMode[i][3];
                InterModeCounts[i][2][0] = counts.InterMode[i][1];
                InterModeCounts[i][2][1] = counts.InterMode[i][3];
            }

            YModeCounts = counts.YMode;
            UvModeCounts = counts.UvMode;
            PartitionCounts = counts.Partition;
            SwitchableInterpsCount = counts.SwitchableInterp;
            IntraInterCount = counts.IntraInter;
            CompInterCount = counts.CompInter;
            SingleRefCount = counts.SingleRef;
            CompRefCount = counts.CompRef;
            Tx32x32 = counts.Tx32x32;
            Tx16x16 = counts.Tx16x16;
            Tx8x8 = counts.Tx8x8;
            MbSkipCount = counts.Skip;
            Joints = counts.Joints;
            Sign = counts.Sign;
            Classes = counts.Classes;
            Class0 = counts.Class0;
            Bits = counts.Bits;
            Class0Fp = counts.Class0Fp;
            Fp = counts.Fp;
            Class0Hp = counts.Class0Hp;
            Hp = counts.Hp;
            CoefCounts = counts.Coef;
            EobCounts = counts.EobBranch;
        }
    }
}
