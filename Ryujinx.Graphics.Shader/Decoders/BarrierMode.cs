namespace Ryujinx.Graphics.Shader.Decoders
{
    enum BarrierMode
    {
        ReductionPopCount = 2,
        Scan              = 3,
        ReductionAnd      = 0xa,
        ReductionOr       = 0x12,
        Sync              = 0x80,
        Arrive            = 0x81
    }
}