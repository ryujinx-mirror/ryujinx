#version 450 core

layout (std140, binding = 1) uniform clear_colour_in
{
    vec4 clear_colour_in_data;
};

layout (location = 0) out vec4 clear_colour;

void main()
{
    int low = gl_VertexIndex & 1;
    int high = gl_VertexIndex >> 1;
    clear_colour = clear_colour_in_data;
    gl_Position.x = (float(low) - 0.5f) * 2.0f;
    gl_Position.y = (float(high) - 0.5f) * 2.0f;
    gl_Position.z = 0.0f;
    gl_Position.w = 1.0f;
}