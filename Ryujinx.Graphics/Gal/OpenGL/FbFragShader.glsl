#version 330 core

precision highp float;

uniform sampler2D tex;

in vec2 tex_coord;

out vec4 out_frag_color;

void main(void) {
    out_frag_color = texture(tex, tex_coord);
}