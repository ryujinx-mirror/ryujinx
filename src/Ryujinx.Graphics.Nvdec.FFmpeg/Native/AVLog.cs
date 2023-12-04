namespace Ryujinx.Graphics.Nvdec.FFmpeg.Native
{
    enum AVLog
    {
        Panic = 0,
        Fatal = 8,
        Error = 16,
        Warning = 24,
        Info = 32,
        Verbose = 40,
        Debug = 48,
        Trace = 56,
        MaxOffset = 64,
    }
}
