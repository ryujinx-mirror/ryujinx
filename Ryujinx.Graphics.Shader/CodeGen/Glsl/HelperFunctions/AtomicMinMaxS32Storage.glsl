int Helper_AtomicMaxS32(int index, int offset, int value)
{
    uint oldValue, newValue;
    do
    {
        oldValue = $STORAGE_MEM$[index].data[offset];
        newValue = uint(max(int(oldValue), value));
    } while (atomicCompSwap($STORAGE_MEM$[index].data[offset], newValue, oldValue) != oldValue);
    return int(oldValue);
}

int Helper_AtomicMinS32(int index, int offset, int value)
{
    uint oldValue, newValue;
    do
    {
        oldValue = $STORAGE_MEM$[index].data[offset];
        newValue = uint(min(int(oldValue), value));
    } while (atomicCompSwap($STORAGE_MEM$[index].data[offset], newValue, oldValue) != oldValue);
    return int(oldValue);
}