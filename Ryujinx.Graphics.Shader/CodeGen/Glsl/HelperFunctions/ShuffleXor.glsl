float Helper_ShuffleXor(float x, uint index, uint mask, out bool valid)
{
    uint clamp = mask & 0x1fu;
    uint segMask = (mask >> 8) & 0x1fu;
    uint minThreadId = $SUBGROUP_INVOCATION$ & segMask;
    uint maxThreadId = minThreadId | (clamp & ~segMask);
    uint srcThreadId = $SUBGROUP_INVOCATION$ ^ index;
    valid = srcThreadId <= maxThreadId;
    float v = $SUBGROUP_BROADCAST$(x, srcThreadId);
    return valid ? v : x;
}