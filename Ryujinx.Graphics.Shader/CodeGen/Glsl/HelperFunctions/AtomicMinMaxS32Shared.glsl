int Helper_AtomicMaxS32(int offset, int value)
{
    uint oldValue, newValue;
    do
    {
        oldValue = $SHARED_MEM$[offset];
        newValue = uint(max(int(oldValue), value));
    } while (atomicCompSwap($SHARED_MEM$[offset], newValue, oldValue) != oldValue);
    return int(oldValue);
}

int Helper_AtomicMinS32(int offset, int value)
{
    uint oldValue, newValue;
    do
    {
        oldValue = $SHARED_MEM$[offset];
        newValue = uint(min(int(oldValue), value));
    } while (atomicCompSwap($SHARED_MEM$[offset], newValue, oldValue) != oldValue);
    return int(oldValue);
}