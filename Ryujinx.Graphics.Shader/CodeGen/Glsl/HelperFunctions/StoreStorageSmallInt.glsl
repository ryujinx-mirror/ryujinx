void Helper_StoreStorage16(int index, int offset, uint value)
{
    int wordOffset = offset >> 2;
    int bitOffset = (offset & 3) * 8;
    uint oldValue, newValue;
    do
    {
        oldValue = $STORAGE_MEM$[index].data[wordOffset];
        newValue = bitfieldInsert(oldValue, value, bitOffset, 16);
    } while (atomicCompSwap($STORAGE_MEM$[index].data[wordOffset], oldValue, newValue) != oldValue);
}

void Helper_StoreStorage8(int index, int offset, uint value)
{
    int wordOffset = offset >> 2;
    int bitOffset = (offset & 3) * 8;
    uint oldValue, newValue;
    do
    {
        oldValue = $STORAGE_MEM$[index].data[wordOffset];
        newValue = bitfieldInsert(oldValue, value, bitOffset, 8);
    } while (atomicCompSwap($STORAGE_MEM$[index].data[wordOffset], oldValue, newValue) != oldValue);
}