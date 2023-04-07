#version 450 core

layout (binding = 0, set = 2) uniform sampler2D tex;

layout (location = 0) in vec2 tex_coord;
layout (location = 0) out vec4 colour;

void main()
{
    colour = texture(tex, tex_coord);
}