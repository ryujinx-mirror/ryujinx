#version 450 core

#extension GL_ARB_shader_stencil_export : require

layout (binding = 0, set = 2) uniform isampler2D texStencil;

layout (location = 0) in vec2 tex_coord;

void main()
{
    gl_FragStencilRefARB = texture(texStencil, tex_coord).r;
}