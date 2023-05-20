ivec2 Helper_TexelFetchScale(ivec2 inputVec, int samplerIndex)
{
    float scale = abs(support_buffer.s_render_scale[1 + samplerIndex + support_buffer.s_frag_scale_count]);
    if (scale == 1.0)
    {
        return inputVec;
    }

    return ivec2(vec2(inputVec) * scale);
}

int Helper_TextureSizeUnscale(int size, int samplerIndex)
{
    float scale = abs(support_buffer.s_render_scale[1 + samplerIndex + support_buffer.s_frag_scale_count]);
    if (scale == 1.0)
    {
        return size;
    }
    return int(float(size) / scale);
}