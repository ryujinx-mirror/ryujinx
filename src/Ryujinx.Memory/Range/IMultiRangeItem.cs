namespace Ryujinx.Memory.Range
{
    public interface IMultiRangeItem
    {
        MultiRange Range { get; }

        ulong BaseAddress
        {
            get
            {
                for (int index = 0; index < Range.Count; index++)
                {
                    MemoryRange subRange = Range.GetSubRange(index);

                    if (!MemoryRange.IsInvalid(ref subRange))
                    {
                        return subRange.Address;
                    }
                }

                return MemoryRange.InvalidAddress;
            }
        }
    }
}
