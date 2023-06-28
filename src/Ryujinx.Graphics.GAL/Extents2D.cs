using Ryujinx.Common;

namespace Ryujinx.Graphics.GAL
{
    public readonly struct Extents2D
    {
        public int X1 { get; }
        public int Y1 { get; }
        public int X2 { get; }
        public int Y2 { get; }

        public Extents2D(int x1, int y1, int x2, int y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        public Extents2D Reduce(int level)
        {
            int div = 1 << level;

            return new Extents2D(
                X1 >> level,
                Y1 >> level,
                BitUtils.DivRoundUp(X2, div),
                BitUtils.DivRoundUp(Y2, div));
        }
    }
}
