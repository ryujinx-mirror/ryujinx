layout(rgba8, binding = 0) uniform image2D imgOutput;

uniform sampler2D inputTexture;
layout( location=0 ) uniform vec2 invResolution;

void main() 
{
    vec2 loc = ivec2(gl_GlobalInvocationID.x * 4, gl_GlobalInvocationID.y * 4);
    for(int i = 0; i < 4; i++)
    {
        for(int j = 0; j < 4; j++)
        {
            ivec2 texelCoord = ivec2(loc.x + i, loc.y + j);
            vec2 coord = (texelCoord + vec2(0.5)) / invResolution;
            vec4 offset[3];
            SMAAEdgeDetectionVS(coord, offset);
            vec2 oColor = SMAAColorEdgeDetectionPS(coord, offset, inputTexture);
            if (oColor != float2(-2.0, -2.0))
            {
                imageStore(imgOutput, texelCoord, vec4(oColor, 0.0, 1.0));
            }
        }
    }
}