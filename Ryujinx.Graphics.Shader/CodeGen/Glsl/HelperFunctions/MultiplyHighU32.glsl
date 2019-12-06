uint Helper_MultiplyHighU32(uint x, uint y)
{
    uint msb;
    uint lsb;
    umulExtended(x, y, msb, lsb);
    return msb;
}