float Helper_ShuffleUp(float x, uint index, uint mask, out bool valid)
{
    uint segMask = (mask >> 8) & 0x1fu;
    uint minThreadId = gl_SubGroupInvocationARB & segMask;
    uint srcThreadId = gl_SubGroupInvocationARB - index;
    valid = int(srcThreadId) >= int(minThreadId);
    float v = readInvocationARB(x, srcThreadId);
    return valid ? v : x;
}