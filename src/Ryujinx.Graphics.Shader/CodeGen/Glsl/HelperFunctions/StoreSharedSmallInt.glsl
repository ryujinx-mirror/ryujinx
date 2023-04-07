void Helper_StoreShared16(int offset, uint value)
{
    int wordOffset = offset >> 2;
    int bitOffset = (offset & 3) * 8;
    uint oldValue, newValue;
    do
    {
        oldValue = $SHARED_MEM$[wordOffset];
        newValue = bitfieldInsert(oldValue, value, bitOffset, 16);
    } while (atomicCompSwap($SHARED_MEM$[wordOffset], oldValue, newValue) != oldValue);
}

void Helper_StoreShared8(int offset, uint value)
{
    int wordOffset = offset >> 2;
    int bitOffset = (offset & 3) * 8;
    uint oldValue, newValue;
    do
    {
        oldValue = $SHARED_MEM$[wordOffset];
        newValue = bitfieldInsert(oldValue, value, bitOffset, 8);
    } while (atomicCompSwap($SHARED_MEM$[wordOffset], oldValue, newValue) != oldValue);
}