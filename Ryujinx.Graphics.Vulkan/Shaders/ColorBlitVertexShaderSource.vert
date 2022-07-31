#version 450 core

layout (std140, binding = 1) uniform tex_coord_in
{
    vec4 tex_coord_in_data;
};

layout (location = 0) out vec2 tex_coord;

void main()
{
    int low = gl_VertexIndex & 1;
    int high = gl_VertexIndex >> 1;
    tex_coord.x = tex_coord_in_data[low];
    tex_coord.y = tex_coord_in_data[2 + high];
    gl_Position.x = (float(low) - 0.5f) * 2.0f;
    gl_Position.y = (float(high) - 0.5f) * 2.0f;
    gl_Position.z = 0.0f;
    gl_Position.w = 1.0f;
}