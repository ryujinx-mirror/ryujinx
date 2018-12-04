namespace Ryujinx.HLE.Input
{
    interface IHidDevice
    {
        long Offset    { get; }
        bool Connected { get; }
    }
}
