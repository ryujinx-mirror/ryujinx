using System;

namespace Ryujinx.Graphics.GAL
{
    public struct ColorF : IEquatable<ColorF>
    {
        public float Red   { get; }
        public float Green { get; }
        public float Blue  { get; }
        public float Alpha { get; }

        public ColorF(float red, float green, float blue, float alpha)
        {
            Red   = red;
            Green = green;
            Blue  = blue;
            Alpha = alpha;
        }

        public bool Equals(ColorF color) => Red   == color.Red &&
                                            Green == color.Green &&
                                            Blue  == color.Blue &&
                                            Alpha == color.Alpha;

        public override bool Equals(object obj) => (obj is ColorF color) && Equals(color);

        public override int GetHashCode() => HashCode.Combine(Red, Green, Blue, Alpha);

        public static bool operator ==(ColorF l, ColorF r) => l.Equals(r);
        public static bool operator !=(ColorF l, ColorF r) => !l.Equals(r);
    }
}
