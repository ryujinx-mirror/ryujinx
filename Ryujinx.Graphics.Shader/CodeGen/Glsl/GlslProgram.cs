namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    class GlslProgram
    {
        public BufferDescriptor[]  CBufferDescriptors { get; }
        public BufferDescriptor[]  SBufferDescriptors { get; }
        public TextureDescriptor[] TextureDescriptors { get; }
        public TextureDescriptor[] ImageDescriptors   { get; }

        public string Code { get; }

        public GlslProgram(
            BufferDescriptor[]  cBufferDescriptors,
            BufferDescriptor[]  sBufferDescriptors,
            TextureDescriptor[] textureDescriptors,
            TextureDescriptor[] imageDescriptors,
            string              code)
        {
            CBufferDescriptors = cBufferDescriptors;
            SBufferDescriptors = sBufferDescriptors;
            TextureDescriptors = textureDescriptors;
            ImageDescriptors   = imageDescriptors;
            Code               = code;
        }
    }
}