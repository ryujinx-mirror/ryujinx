namespace Ryujinx.Graphics.Gal
{
    public struct GalColorF
    {
        public float Red   { get; private set; }
        public float Green { get; private set; }
        public float Blue  { get; private set; }
        public float Alpha { get; private set; }

        public GalColorF(
            float red,
            float green,
            float blue,
            float alpha)
        {
            Red   = red;
            Green = green;
            Blue  = blue;
            Alpha = alpha;
        }
    }
}