#version 450 core

layout (binding = 0, set = 2) uniform sampler2DMS texDepth;

layout (location = 0) in vec2 tex_coord;

void main()
{
    gl_FragDepth = texelFetch(texDepth, ivec2(tex_coord * vec2(textureSize(texDepth).xy)), gl_SampleID).r;
}