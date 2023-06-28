using System.Diagnostics.CodeAnalysis;

namespace Ryujinx.Graphics.GAL
{
    [SuppressMessage("Design", "CA1069: Enums values should not be duplicated")]
    public enum ViewportSwizzle
    {
        PositiveX = 0,
        NegativeX = 1,
        PositiveY = 2,
        NegativeY = 3,
        PositiveZ = 4,
        NegativeZ = 5,
        PositiveW = 6,
        NegativeW = 7,

        NegativeFlag = 1,
    }
}
