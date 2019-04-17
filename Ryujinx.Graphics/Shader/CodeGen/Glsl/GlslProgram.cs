namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    class GlslProgram
    {
        public CBufferDescriptor[] CBufferDescriptors { get; }
        public TextureDescriptor[] TextureDescriptors { get; }

        public string Code { get; }

        public GlslProgram(
            CBufferDescriptor[] cBufferDescs,
            TextureDescriptor[] textureDescs,
            string              code)
        {
            CBufferDescriptors = cBufferDescs;
            TextureDescriptors = textureDescs;
            Code               = code;
        }
    }
}