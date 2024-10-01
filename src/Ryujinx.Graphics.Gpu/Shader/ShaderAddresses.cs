using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Shader code addresses in memory for each shader stage.
    /// </summary>
    struct ShaderAddresses : IEquatable<ShaderAddresses>
    {
#pragma warning disable CS0649 // Field is never assigned to
        public ulong VertexA;
        public ulong VertexB;
        public ulong TessControl;
        public ulong TessEvaluation;
        public ulong Geometry;
        public ulong Fragment;
#pragma warning restore CS0649

        /// <summary>
        /// Check if the addresses are equal.
        /// </summary>
        /// <param name="other">Shader addresses structure to compare with</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public readonly override bool Equals(object other)
        {
            return other is ShaderAddresses addresses && Equals(addresses);
        }

        /// <summary>
        /// Check if the addresses are equal.
        /// </summary>
        /// <param name="other">Shader addresses structure to compare with</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public readonly bool Equals(ShaderAddresses other)
        {
            return VertexA == other.VertexA &&
                   VertexB == other.VertexB &&
                   TessControl == other.TessControl &&
                   TessEvaluation == other.TessEvaluation &&
                   Geometry == other.Geometry &&
                   Fragment == other.Fragment;
        }

        /// <summary>
        /// Computes hash code from the addresses.
        /// </summary>
        /// <returns>Hash code</returns>
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(VertexA, VertexB, TessControl, TessEvaluation, Geometry, Fragment);
        }

        /// <summary>
        /// Gets a view of the structure as a span of addresses.
        /// </summary>
        /// <returns>Span of addresses</returns>
        public Span<ulong> AsSpan()
        {
            return MemoryMarshal.CreateSpan(ref VertexA, Unsafe.SizeOf<ShaderAddresses>() / sizeof(ulong));
        }
    }
}
