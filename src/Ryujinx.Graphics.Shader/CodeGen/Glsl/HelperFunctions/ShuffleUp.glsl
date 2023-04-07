float Helper_ShuffleUp(float x, uint index, uint mask, out bool valid)
{
    uint segMask = (mask >> 8) & 0x1fu;
    uint minThreadId = $SUBGROUP_INVOCATION$ & segMask;
    uint srcThreadId = $SUBGROUP_INVOCATION$ - index;
    valid = int(srcThreadId) >= int(minThreadId);
    float v = $SUBGROUP_BROADCAST$(x, srcThreadId);
    return valid ? v : x;
}