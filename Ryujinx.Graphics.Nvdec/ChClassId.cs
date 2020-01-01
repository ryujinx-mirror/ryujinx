namespace Ryujinx.Graphics
{
    enum ChClassId
    {
        Host1X              = 0x1,
        VideoEncodeMpeg     = 0x20,
        VideoEncodeNvEnc    = 0x21,
        VideoStreamingVi    = 0x30,
        VideoStreamingIsp   = 0x32,
        VideoStreamingIspB  = 0x34,
        VideoStreamingViI2c = 0x36,
        GraphicsVic         = 0x5d,
        Graphics3D          = 0x60,
        GraphicsGpu         = 0x61,
        Tsec                = 0xe0,
        TsecB               = 0xe1,
        NvJpg               = 0xc0,
        NvDec               = 0xf0
    }
}