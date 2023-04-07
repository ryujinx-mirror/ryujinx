#version 450 core

layout (binding = 0, set = 2) uniform sampler2DMS tex;

layout (location = 0) in vec2 tex_coord;
layout (location = 0) out vec4 colour;

void main()
{
    colour = texelFetch(tex, ivec2(tex_coord * vec2(textureSize(tex).xy)), gl_SampleID);
}