#version 450 core

layout (std140, binding = 0) uniform sample_counts_log2_in
{
    ivec4 sample_counts_log2;
};

layout (set = 2, binding = 0) uniform sampler2DMS srcMS;

void main()
{
    uvec2 coords = uvec2(gl_FragCoord.xy);

    int deltaX = sample_counts_log2.x - sample_counts_log2.z;
    int deltaY = sample_counts_log2.y - sample_counts_log2.w;
    int samplesInXLog2 = sample_counts_log2.z;
    int samplesInYLog2 = sample_counts_log2.w;
    int samplesInX = 1 << samplesInXLog2;
    int samplesInY = 1 << samplesInYLog2;
    int sampleIdx = ((int(coords.x) >> deltaX) & (samplesInX - 1)) | (((int(coords.y) >> deltaY) & (samplesInY - 1)) << samplesInXLog2);

    samplesInXLog2 = sample_counts_log2.x;
    samplesInYLog2 = sample_counts_log2.y;

    ivec2 shiftedCoords = ivec2(int(coords.x) >> samplesInXLog2, int(coords.y) >> samplesInYLog2);

    gl_FragDepth = texelFetch(srcMS, shiftedCoords, sampleIdx).r;
}