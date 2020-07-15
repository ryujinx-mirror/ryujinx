using System;

namespace Ryujinx.Graphics.GAL
{
    public struct TransformFeedbackDescriptor
    {
        public int BufferIndex { get; }
        public int Stride      { get; }

        public byte[] VaryingLocations { get; }

        public TransformFeedbackDescriptor(int bufferIndex, int stride, byte[] varyingLocations)
        {
            BufferIndex      = bufferIndex;
            Stride           = stride;
            VaryingLocations = varyingLocations ?? throw new ArgumentNullException(nameof(varyingLocations));
        }
    }
}
