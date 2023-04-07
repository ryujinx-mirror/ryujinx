float Helper_SwizzleAdd(float x, float y, int mask)
{
    vec4 xLut = vec4(1.0, -1.0, 1.0, 0.0);
    vec4 yLut = vec4(1.0, 1.0, -1.0, 1.0);
    int lutIdx = (mask >> (int($SUBGROUP_INVOCATION$ & 3u) * 2)) & 3;
    return x * xLut[lutIdx] + y * yLut[lutIdx];
}