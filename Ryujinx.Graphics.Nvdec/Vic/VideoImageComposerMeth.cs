namespace Ryujinx.Graphics.Vic
{
    enum VideoImageComposerMeth
    {
        Execute                       = 0xc0,
        SetControlParams              = 0x1c1,
        SetConfigStructOffset         = 0x1c2,
        SetOutputSurfaceLumaOffset    = 0x1c8,
        SetOutputSurfaceChromaUOffset = 0x1c9,
        SetOutputSurfaceChromaVOffset = 0x1ca
    }
}