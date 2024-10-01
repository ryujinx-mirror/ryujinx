#version 450 core

layout (location = 0) in vec4 clear_colour;
layout (location = 0) out uvec4 colour;

void main()
{
    colour = floatBitsToUint(clear_colour);
}