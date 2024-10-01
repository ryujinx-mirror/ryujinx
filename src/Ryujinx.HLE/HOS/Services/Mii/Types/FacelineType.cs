namespace Ryujinx.HLE.HOS.Services.Mii.Types
{
    enum FacelineType : byte
    {
        Sharp,
        Rounded,
        SharpRounded,
        SharpRoundedSmall,
        Large,
        LargeRounded,
        SharpSmall,
        Flat,
        Bump,
        Angular,
        FlatRounded,
        AngularSmall,

        Min = Sharp,
        Max = AngularSmall,
    }
}
