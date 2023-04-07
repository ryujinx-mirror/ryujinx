namespace Ryujinx.Graphics.Nvdec.Types.Vp9
{
    enum FrameFlags : uint
    {
        IsKeyFrame = 1 << 0,
        LastFrameIsKeyFrame = 1 << 1,
        FrameSizeChanged = 1 << 2,
        ErrorResilientMode = 1 << 3,
        LastShowFrame = 1 << 4,
        IntraOnly = 1 << 5
    }
}
