#version 450 core

layout (binding = 0, set = 2) uniform sampler2D tex;

layout (location = 0) in vec2 tex_coord;
layout (location = 0) out vec4 colour;

void main()
{
    colour = vec4(texture(tex, tex_coord).rgb, 1.0f);
}