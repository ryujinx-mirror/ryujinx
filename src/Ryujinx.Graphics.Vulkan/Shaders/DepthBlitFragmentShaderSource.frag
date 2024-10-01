#version 450 core

layout (binding = 0, set = 2) uniform sampler2D texDepth;

layout (location = 0) in vec2 tex_coord;

void main()
{
    gl_FragDepth = texture(texDepth, tex_coord).r;
}