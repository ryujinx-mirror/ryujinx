ivec2 Helper_GetStorageBuffer(uint aLow, uint aHigh)
{
    uint64_t address = packUint2x32(uvec2(aLow, aHigh));
    int i;
    for (i = 0; i < 16; i++)
    {
        int offset = 0x40 + i * 4;
        uint baseLow  = fp_c0_data[offset];
        uint baseHigh = fp_c0_data[offset + 1];
        uint size     = fp_c0_data[offset + 2];
        uint64_t baseAddr = packUint2x32(uvec2(baseLow, baseHigh));
        if (address >= baseAddr && address < baseAddr + packUint2x32(uvec2(size, 0)))
        {
            return ivec2(i, int(unpackUint2x32(address - (baseAddr & ~63ul)).x) >> 2);
        }
    }
    return ivec2(0);
}