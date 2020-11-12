using System;
using System.Collections.ObjectModel;

namespace Ryujinx.Graphics.Shader
{
    public class ShaderProgramInfo
    {
        public ReadOnlyCollection<BufferDescriptor>  CBuffers { get; }
        public ReadOnlyCollection<BufferDescriptor>  SBuffers { get; }
        public ReadOnlyCollection<TextureDescriptor> Textures { get; }
        public ReadOnlyCollection<TextureDescriptor> Images   { get; }

        public bool UsesInstanceId { get; }

        public ShaderProgramInfo(
            BufferDescriptor[]  cBuffers,
            BufferDescriptor[]  sBuffers,
            TextureDescriptor[] textures,
            TextureDescriptor[] images,
            bool                usesInstanceId)
        {
            CBuffers = Array.AsReadOnly(cBuffers);
            SBuffers = Array.AsReadOnly(sBuffers);
            Textures = Array.AsReadOnly(textures);
            Images   = Array.AsReadOnly(images);

            UsesInstanceId = usesInstanceId;
        }
    }
}