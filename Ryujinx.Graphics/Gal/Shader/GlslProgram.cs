using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    struct GlslProgram
    {
        public string Code { get; private set; }

        public IEnumerable<ShaderDeclInfo> Textures { get; private set; }
        public IEnumerable<ShaderDeclInfo> Uniforms { get; private set; }

        public GlslProgram(
            string                      Code,
            IEnumerable<ShaderDeclInfo> Textures,
            IEnumerable<ShaderDeclInfo> Uniforms)
        {
            this.Code     = Code;
            this.Textures = Textures;
            this.Uniforms = Uniforms;
        }
    }
}