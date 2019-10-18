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

        public ReadOnlyCollection<InterpolationQualifier> InterpolationQualifiers { get; }

        public bool UsesInstanceId { get; }

        internal ShaderProgramInfo(
            BufferDescriptor[]       cBuffers,
            BufferDescriptor[]       sBuffers,
            TextureDescriptor[]      textures,
            TextureDescriptor[]      images,
            InterpolationQualifier[] interpolationQualifiers,
            bool                     usesInstanceId)
        {
            CBuffers = Array.AsReadOnly(cBuffers);
            SBuffers = Array.AsReadOnly(sBuffers);
            Textures = Array.AsReadOnly(textures);
            Images   = Array.AsReadOnly(images);

            InterpolationQualifiers = Array.AsReadOnly(interpolationQualifiers);

            UsesInstanceId = usesInstanceId;
        }
    }
}