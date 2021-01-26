ivec2 Helper_TexelFetchScale(ivec2 inputVec, int samplerIndex)
{
    float scale = cp_renderScale[samplerIndex];
    if (scale == 1.0)
    {
        return inputVec;
    }
    return ivec2(vec2(inputVec) * scale);
}