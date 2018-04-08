namespace Ryujinx.Graphics.Gal
{
    public struct GalColorF
    {
        public float Red   { get; private set; }
        public float Green { get; private set; }
        public float Blue  { get; private set; }
        public float Alpha { get; private set; }

        public GalColorF(
            float Red,
            float Green,
            float Blue,
            float Alpha)
        {
            this.Red   = Red;
            this.Green = Green;
            this.Blue  = Blue;
            this.Alpha = Alpha;
        }
    }
}