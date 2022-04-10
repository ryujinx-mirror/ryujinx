using Ryujinx.Common.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Transform feedback descriptor.
    /// </summary>
    struct TransformFeedbackDescriptor
    {
        // New fields should be added to the end of the struct to keep disk shader cache compatibility.

        /// <summary>
        /// Index of the transform feedback.
        /// </summary>
        public readonly int BufferIndex;

        /// <summary>
        /// Amount of bytes consumed per vertex.
        /// </summary>
        public readonly int Stride;

        /// <summary>
        /// Number of varyings written into the buffer.
        /// </summary>
        public readonly int VaryingCount;

        /// <summary>
        /// Location of varyings to be written into the buffer. Each byte is one location.
        /// </summary>
        public Array32<uint> VaryingLocations; // Making this readonly breaks AsSpan

        /// <summary>
        /// Creates a new transform feedback descriptor.
        /// </summary>
        /// <param name="bufferIndex">Index of the transform feedback</param>
        /// <param name="stride">Amount of bytes consumed per vertex</param>
        /// <param name="varyingCount">Number of varyings written into the buffer. Indicates size in bytes of <paramref name="varyingLocations"/></param>
        /// <param name="varyingLocations">Location of varyings to be written into the buffer. Each byte is one location</param>
        public TransformFeedbackDescriptor(int bufferIndex, int stride, int varyingCount, ref Array32<uint> varyingLocations)
        {
            BufferIndex = bufferIndex;
            Stride = stride;
            VaryingCount = varyingCount;
            VaryingLocations = varyingLocations;
        }

        /// <summary>
        /// Gets a span of the <see cref="VaryingLocations"/>.
        /// </summary>
        /// <returns>Span of varying locations</returns>
        public ReadOnlySpan<byte> AsSpan()
        {
            return MemoryMarshal.Cast<uint, byte>(VaryingLocations.ToSpan()).Slice(0, Math.Min(128, VaryingCount));
        }
    }
}
