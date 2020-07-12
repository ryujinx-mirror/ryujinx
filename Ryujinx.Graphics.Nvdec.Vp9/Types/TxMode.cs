namespace Ryujinx.Graphics.Nvdec.Vp9.Types
{
    public enum TxMode
    {
        Only4X4 = 0,      // Only 4x4 transform used
        Allow8X8 = 1,     // Allow block transform size up to 8x8
        Allow16X16 = 2,   // Allow block transform size up to 16x16
        Allow32X32 = 3,   // Allow block transform size up to 32x32
        TxModeSelect = 4, // Transform specified for each block
        TxModes = 5
    }
}
