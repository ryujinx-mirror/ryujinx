#version 450 core

layout (location = 0) in vec4 clear_colour;
layout (location = 0) out vec4 colour;

void main()
{
    colour = clear_colour;
}