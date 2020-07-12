namespace Ryujinx.Graphics.Nvdec.Vp9.Types
{
    internal enum TxType
    {
        DctDct = 0,   // DCT  in both horizontal and vertical
        AdstDct = 1,  // ADST in vertical, DCT in horizontal
        DctAdst = 2,  // DCT  in vertical, ADST in horizontal
        AdstAdst = 3, // ADST in both directions
        TxTypes = 4
    }
}
