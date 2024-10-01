#version 450 core

layout (location = 0) in vec4 clear_colour;

void main()
{
    gl_FragDepth = clear_colour.x;
}