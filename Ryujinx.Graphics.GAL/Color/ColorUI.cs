namespace Ryujinx.Graphics.GAL.Color
{
    public struct ColorUI
    {
        public uint Red   { get; }
        public uint Green { get; }
        public uint Blue  { get; }
        public uint Alpha { get; }

        public ColorUI(uint red, uint green, uint blue, uint alpha)
        {
            Red   = red;
            Green = green;
            Blue  = blue;
            Alpha = alpha;
        }
    }
}
