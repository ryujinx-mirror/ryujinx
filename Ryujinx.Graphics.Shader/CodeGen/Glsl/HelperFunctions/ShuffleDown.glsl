float Helper_ShuffleDown(float x, uint index, uint mask)
{
    uint clamp = mask & 0x1fu;
    uint segMask = (mask >> 8) & 0x1fu;
    uint minThreadId = gl_SubGroupInvocationARB & segMask;
    uint maxThreadId = minThreadId | (clamp & ~segMask);
    uint srcThreadId = gl_SubGroupInvocationARB + index;
    return (srcThreadId <= maxThreadId) ? readInvocationARB(x, srcThreadId) : x;
}