#version 430 core
precision mediump float;
layout (local_size_x = 64) in;
layout(rgba8, binding = 0, location=0) uniform image2D imgOutput;
layout( location=1 ) uniform sampler2D source;
layout( location=2 ) uniform float sharpening;

#define A_GPU 1
#define A_GLSL 1
#include "ffx_a.h"

#define FSR_RCAS_F 1
AU4 con0;

AF4 FsrRcasLoadF(ASU2 p) { return AF4(texelFetch(source, p, 0)); }
void FsrRcasInputF(inout AF1 r, inout AF1 g, inout AF1 b) {}

#include "ffx_fsr1.h"

void CurrFilter(AU2 pos)
{
    AF3 c;
    FsrRcasF(c.r, c.g, c.b, pos, con0);
    imageStore(imgOutput, ASU2(pos), AF4(c, 1));
}

void main() {
    FsrRcasCon(con0, sharpening);
    AU2 gxy = ARmp8x8(gl_LocalInvocationID.x) + AU2(gl_WorkGroupID.x << 4u, gl_WorkGroupID.y << 4u);
    CurrFilter(gxy);
    gxy.x += 8u;
    CurrFilter(gxy);
    gxy.y += 8u;
    CurrFilter(gxy);
    gxy.x -= 8u;
    CurrFilter(gxy);
}