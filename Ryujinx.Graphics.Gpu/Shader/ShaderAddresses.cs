using System;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Shader code addresses in memory for each shader stage.
    /// </summary>
    struct ShaderAddresses : IEquatable<ShaderAddresses>
    {
#pragma warning disable CS0649
        public ulong VertexA;
        public ulong Vertex;
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
        public override bool Equals(object other)
        {
            return other is ShaderAddresses addresses && Equals(addresses);
        }

        /// <summary>
        /// Check if the addresses are equal.
        /// </summary>
        /// <param name="other">Shader addresses structure to compare with</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public bool Equals(ShaderAddresses other)
        {
            return VertexA        == other.VertexA &&
                   Vertex         == other.Vertex &&
                   TessControl    == other.TessControl &&
                   TessEvaluation == other.TessEvaluation &&
                   Geometry       == other.Geometry &&
                   Fragment       == other.Fragment;
        }

        /// <summary>
        /// Computes hash code from the addresses.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(VertexA, Vertex, TessControl, TessEvaluation, Geometry, Fragment);
        }
    }
}