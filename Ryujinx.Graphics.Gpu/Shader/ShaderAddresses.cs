using System;

namespace Ryujinx.Graphics.Gpu.Shader
{
    struct ShaderAddresses : IEquatable<ShaderAddresses>
    {
        public ulong VertexA;
        public ulong Vertex;
        public ulong TessControl;
        public ulong TessEvaluation;
        public ulong Geometry;
        public ulong Fragment;

        public override bool Equals(object other)
        {
            return other is ShaderAddresses addresses && Equals(addresses);
        }

        public bool Equals(ShaderAddresses other)
        {
            return VertexA        == other.VertexA &&
                   Vertex         == other.Vertex &&
                   TessControl    == other.TessControl &&
                   TessEvaluation == other.TessEvaluation &&
                   Geometry       == other.Geometry &&
                   Fragment       == other.Fragment;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VertexA, Vertex, TessControl, TessEvaluation, Geometry, Fragment);
        }
    }
}