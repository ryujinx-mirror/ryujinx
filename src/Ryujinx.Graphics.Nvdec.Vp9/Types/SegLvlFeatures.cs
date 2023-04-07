namespace Ryujinx.Graphics.Nvdec.Vp9.Types
{
    internal enum SegLvlFeatures
    {
        SegLvlAltQ = 0,      // Use alternate Quantizer ....
        SegLvlAltLf = 1,     // Use alternate loop filter value...
        SegLvlRefFrame = 2,  // Optional Segment reference frame
        SegLvlSkip = 3,      // Optional Segment (0,0) + skip mode
        SegLvlMax = 4        // Number of features supported
    }
}
