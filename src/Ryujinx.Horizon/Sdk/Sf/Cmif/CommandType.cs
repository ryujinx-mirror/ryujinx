namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    enum CommandType
    {
        Invalid = 0,
        LegacyRequest = 1,
        Close = 2,
        LegacyControl = 3,
        Request = 4,
        Control = 5,
        RequestWithContext = 6,
        ControlWithContext = 7,
    }
}
