#version 450 core

layout (std140, binding = 0) uniform sample_counts_log2_in
{
    ivec4 sample_counts_log2;
};

layout (set = 2, binding = 0) uniform sampler2D src;

void main()
{
    int deltaX = sample_counts_log2.x - sample_counts_log2.z;
    int deltaY = sample_counts_log2.y - sample_counts_log2.w;
    int samplesInXLog2 = sample_counts_log2.z;
    int samplesInYLog2 = sample_counts_log2.w;
    int samplesInX = 1 << samplesInXLog2;
    int samplesInY = 1 << samplesInYLog2;

    int sampleIndex = gl_SampleID;

    int inX = (int(gl_FragCoord.x) << sample_counts_log2.x) | ((sampleIndex & (samplesInX - 1)) << deltaX);
    int inY = (int(gl_FragCoord.y) << sample_counts_log2.y) | ((sampleIndex >> samplesInXLog2) << deltaY);

    gl_FragDepth = texelFetch(src, ivec2(inX, inY), 0).r;
}