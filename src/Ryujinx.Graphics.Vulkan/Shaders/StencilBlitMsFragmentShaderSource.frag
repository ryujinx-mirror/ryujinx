#version 450 core

#extension GL_ARB_shader_stencil_export : require

layout (binding = 0, set = 2) uniform isampler2DMS texStencil;

layout (location = 0) in vec2 tex_coord;

void main()
{
    gl_FragStencilRefARB = texelFetch(texStencil, ivec2(tex_coord * vec2(textureSize(texStencil).xy)), gl_SampleID).r;
}