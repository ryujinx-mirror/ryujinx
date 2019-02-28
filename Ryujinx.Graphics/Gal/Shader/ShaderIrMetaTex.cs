using Ryujinx.Graphics.Texture;

namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrMetaTex : ShaderIrMeta
    {
        public int                      Elem { get; private set; }
        public GalTextureTarget         TextureTarget { get; private set; }
        public ShaderIrNode[]           Coordinates { get; private set; }
        public TextureInstructionSuffix TextureInstructionSuffix { get; private set; }
        public ShaderIrOperGpr          LevelOfDetail;
        public ShaderIrOperGpr          Offset;
        public ShaderIrOperGpr          DepthCompare;
        public int                      Component; // for TLD4(S)

        public ShaderIrMetaTex(int Elem, GalTextureTarget TextureTarget, TextureInstructionSuffix TextureInstructionSuffix, params ShaderIrNode[] Coordinates)
        {
            this.Elem                     = Elem;
            this.TextureTarget            = TextureTarget;
            this.TextureInstructionSuffix = TextureInstructionSuffix;
            this.Coordinates              = Coordinates;
        }
    }
}