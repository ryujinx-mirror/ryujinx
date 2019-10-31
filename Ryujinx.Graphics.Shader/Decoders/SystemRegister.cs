namespace Ryujinx.Graphics.Shader.Decoders
{
    enum SystemRegister
    {
        YDirection = 0x12,
        ThreadId   = 0x20,
        ThreadIdX  = 0x21,
        ThreadIdY  = 0x22,
        ThreadIdZ  = 0x23,
        CtaIdX     = 0x25,
        CtaIdY     = 0x26,
        CtaIdZ     = 0x27
    }
}