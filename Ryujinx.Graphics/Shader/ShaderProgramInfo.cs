using System;
using System.Collections.ObjectModel;

namespace Ryujinx.Graphics.Shader
{
    public class ShaderProgramInfo
    {
        public ReadOnlyCollection<CBufferDescriptor> CBuffers { get; }
        public ReadOnlyCollection<TextureDescriptor> Textures { get; }

        internal ShaderProgramInfo(CBufferDescriptor[] cBuffers, TextureDescriptor[] textures)
        {
            CBuffers = Array.AsReadOnly(cBuffers);
            Textures = Array.AsReadOnly(textures);
        }
    }
}