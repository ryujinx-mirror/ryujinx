using System;

namespace Ryujinx.Graphics.Gpu.Shader.Cache
{
    struct TransformFeedbackDescriptorOld
    {
        public int BufferIndex { get; }
        public int Stride      { get; }

        public byte[] VaryingLocations { get; }

        public TransformFeedbackDescriptorOld(int bufferIndex, int stride, byte[] varyingLocations)
        {
            BufferIndex      = bufferIndex;
            Stride           = stride;
            VaryingLocations = varyingLocations ?? throw new ArgumentNullException(nameof(varyingLocations));
        }
    }
}
