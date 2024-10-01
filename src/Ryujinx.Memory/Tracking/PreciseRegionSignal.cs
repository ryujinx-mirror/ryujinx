namespace Ryujinx.Memory.Tracking
{
    public delegate bool PreciseRegionSignal(ulong address, ulong size, bool write);
}
