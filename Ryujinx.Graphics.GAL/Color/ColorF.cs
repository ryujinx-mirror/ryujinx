namespace Ryujinx.Graphics.GAL.Color
{
    public struct ColorF
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
    }
}
