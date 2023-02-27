layout(rgba8, binding = 0) uniform image2D imgOutput;

uniform sampler2D inputTexture;
layout( location=0 ) uniform vec2 invResolution;
uniform sampler2D samplerArea;
uniform sampler2D samplerSearch;

void main() {
    ivec2 loc = ivec2(gl_GlobalInvocationID.x * 4, gl_GlobalInvocationID.y * 4);
    for(int i = 0; i < 4; i++)
    {
        for(int j = 0; j < 4; j++)
        {
            ivec2 texelCoord = ivec2(loc.x + i, loc.y + j);
            vec2 coord = (texelCoord + vec2(0.5)) / invResolution;
            vec2 pixCoord;
            vec4 offset[3];

            SMAABlendingWeightCalculationVS(coord, pixCoord, offset);

            vec4 oColor = SMAABlendingWeightCalculationPS(coord, pixCoord, offset, inputTexture, samplerArea, samplerSearch, ivec4(0));    

            imageStore(imgOutput, texelCoord, oColor);
        }
    }
}
