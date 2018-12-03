namespace Ryujinx.Graphics.VDec
{
    enum VideoDecoderMeth
    {
        SetVideoCodec        = 0x80,
        Execute              = 0xc0,
        SetDecoderCtxAddr    = 0x101,
        SetFrameDataAddr     = 0x102,
        SetVpxRef0LumaAddr   = 0x10c,
        SetVpxRef1LumaAddr   = 0x10d,
        SetVpxRef2LumaAddr   = 0x10e,
        SetVpxCurrLumaAddr   = 0x10f,
        SetVpxRef0ChromaAddr = 0x11d,
        SetVpxRef1ChromaAddr = 0x11e,
        SetVpxRef2ChromaAddr = 0x11f,
        SetVpxCurrChromaAddr = 0x120,
        SetVpxProbTablesAddr = 0x170
    }
}