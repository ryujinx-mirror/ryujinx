namespace Ryujinx.Memory.Range
{
    public interface IMultiRangeItem
    {
        MultiRange Range { get; }

        ulong BaseAddress => Range.GetSubRange(0).Address;
    }
}
