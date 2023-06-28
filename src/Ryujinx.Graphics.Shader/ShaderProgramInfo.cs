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

        public ShaderIdentification Identification { get; }
        public int GpLayerInputAttribute { get; }
        public ShaderStage Stage { get; }
        public bool UsesInstanceId { get; }
        public bool UsesDrawParameters { get; }
        public bool UsesRtLayer { get; }
        public byte ClipDistancesWritten { get; }
        public int FragmentOutputMap { get; }

        public ShaderProgramInfo(
            BufferDescriptor[] cBuffers,
            BufferDescriptor[] sBuffers,
            TextureDescriptor[] textures,
            TextureDescriptor[] images,
            ShaderIdentification identification,
            int gpLayerInputAttribute,
            ShaderStage stage,
            bool usesInstanceId,
            bool usesDrawParameters,
            bool usesRtLayer,
            byte clipDistancesWritten,
            int fragmentOutputMap)
        {
            CBuffers = Array.AsReadOnly(cBuffers);
            SBuffers = Array.AsReadOnly(sBuffers);
            Textures = Array.AsReadOnly(textures);
            Images = Array.AsReadOnly(images);

            Identification = identification;
            GpLayerInputAttribute = gpLayerInputAttribute;
            Stage = stage;
            UsesInstanceId = usesInstanceId;
            UsesDrawParameters = usesDrawParameters;
            UsesRtLayer = usesRtLayer;
            ClipDistancesWritten = clipDistancesWritten;
            FragmentOutputMap = fragmentOutputMap;
        }
    }
}
