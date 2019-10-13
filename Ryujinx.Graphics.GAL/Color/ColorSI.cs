namespace Ryujinx.Graphics.GAL.Color
{
    public struct ColorSI
    {
        public int Red   { get; }
        public int Green { get; }
        public int Blue  { get; }
        public int Alpha { get; }

        public ColorSI(int red, int green, int blue, int alpha)
        {
            Red   = red;
            Green = green;
            Blue  = blue;
            Alpha = alpha;
        }
    }
}
