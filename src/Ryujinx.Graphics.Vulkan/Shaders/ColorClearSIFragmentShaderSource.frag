#version 450 core

layout (location = 0) in vec4 clear_colour;
layout (location = 0) out ivec4 colour;

void main()
{
    colour = floatBitsToInt(clear_colour);
}