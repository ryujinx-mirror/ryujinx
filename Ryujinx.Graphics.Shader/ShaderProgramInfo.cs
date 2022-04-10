using System;
using System.Collections.ObjectModel;

namespace Ryujinx.Graphics.Shader
{
    public class ShaderProgramInfo
    {
        public ReadOnlyCollection<BufferDescriptor> CBuffers { get; }
        public ReadOnlyCollection<BufferDescriptor> SBuffers { get; }
        public ReadOnlyCollection<TextureDescriptor> Textures { get; }
        public ReadOnlyCollection<TextureDescriptor> Images { get; }

        public ShaderStage Stage { get; }
        public bool UsesInstanceId { get; }
        public bool UsesRtLayer { get; }
        public byte ClipDistancesWritten { get; }
        public int FragmentOutputMap { get; }

        public ShaderProgramInfo(
            BufferDescriptor[] cBuffers,
            BufferDescriptor[] sBuffers,
            TextureDescriptor[] textures,
            TextureDescriptor[] images,
            ShaderStage stage,
            bool usesInstanceId,
            bool usesRtLayer,
            byte clipDistancesWritten,
            int fragmentOutputMap)
        {
            CBuffers = Array.AsReadOnly(cBuffers);
            SBuffers = Array.AsReadOnly(sBuffers);
            Textures = Array.AsReadOnly(textures);
            Images = Array.AsReadOnly(images);

            Stage = stage;
            UsesInstanceId = usesInstanceId;
            UsesRtLayer = usesRtLayer;
            ClipDistancesWritten = clipDistancesWritten;
            FragmentOutputMap = fragmentOutputMap;
        }
    }
}