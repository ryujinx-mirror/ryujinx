#version 330 core

precision highp float;

uniform vec2 window_size;
uniform mat2 transform;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_tex_coord;

out vec2 tex_coord;

// Have a fixed aspect ratio, fit the image within the available space.
vec2 get_scale_ratio(void) {
    vec2 native_size = vec2(1280, 720);
    vec2 ratio = vec2(
        (window_size.y * native_size.x) / (native_size.y * window_size.x),
        (window_size.x * native_size.y) / (native_size.x * window_size.y)
    );
    return min(ratio, 1);
}

void main(void) {
    tex_coord = in_tex_coord;
    gl_Position = vec4((transform * in_position) * get_scale_ratio(), 0, 1);
}