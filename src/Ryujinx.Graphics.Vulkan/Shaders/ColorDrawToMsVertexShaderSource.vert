#version 450 core

void main()
{
    int low = gl_VertexIndex & 1;
    int high = gl_VertexIndex >> 1;
    gl_Position.x = (float(low) - 0.5f) * 2.0f;
    gl_Position.y = (float(high) - 0.5f) * 2.0f;
    gl_Position.z = 0.0f;
    gl_Position.w = 1.0f;
}