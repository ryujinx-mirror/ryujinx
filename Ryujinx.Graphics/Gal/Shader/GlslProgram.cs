using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    public struct GlslProgram
    {
        public string Code { get; private set; }

        public IEnumerable<ShaderDeclInfo> Textures { get; private set; }
        public IEnumerable<ShaderDeclInfo> Uniforms { get; private set; }

        public GlslProgram(
            string                      code,
            IEnumerable<ShaderDeclInfo> textures,
            IEnumerable<ShaderDeclInfo> uniforms)
        {
            Code     = code;
            Textures = textures;
            Uniforms = uniforms;
        }
    }
}