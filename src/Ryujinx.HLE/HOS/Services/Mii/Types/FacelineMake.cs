namespace Ryujinx.HLE.HOS.Services.Mii.Types
{
    enum FacelineMake : byte
    {
        None,
        CheekPorcelain,
        CheekNatural,
        EyeShadowBlue,
        CheekBlushPorcelain,
        CheekBlushNatural,
        CheekPorcelainEyeShadowBlue,
        CheekPorcelainEyeShadowNatural,
        CheekBlushPorcelainEyeShadowEspresso,
        Freckles,
        LionsManeBeard,
        StubbleBeard,

        Min = None,
        Max = StubbleBeard,
    }
}
