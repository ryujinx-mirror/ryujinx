using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    enum PrimitiveType
    {
        Points,
        Lines,
        LineLoop,
        LineStrip,
        Triangles,
        TriangleStrip,
        TriangleFan,
        Quads,
        QuadStrip,
        Polygon,
        LinesAdjacency,
        LineStripAdjacency,
        TrianglesAdjacency,
        TriangleStripAdjacency,
        Patches
    }

    static class PrimitiveTypeConverter
    {
        public static PrimitiveTopology Convert(this PrimitiveType topology)
        {
            switch (topology)
            {
                case PrimitiveType.Points:                 return PrimitiveTopology.Points;
                case PrimitiveType.Lines:                  return PrimitiveTopology.Lines;
                case PrimitiveType.LineLoop:               return PrimitiveTopology.LineLoop;
                case PrimitiveType.LineStrip:              return PrimitiveTopology.LineStrip;
                case PrimitiveType.Triangles:              return PrimitiveTopology.Triangles;
                case PrimitiveType.TriangleStrip:          return PrimitiveTopology.TriangleStrip;
                case PrimitiveType.TriangleFan:            return PrimitiveTopology.TriangleFan;
                case PrimitiveType.Quads:                  return PrimitiveTopology.Quads;
                case PrimitiveType.QuadStrip:              return PrimitiveTopology.QuadStrip;
                case PrimitiveType.Polygon:                return PrimitiveTopology.Polygon;
                case PrimitiveType.LinesAdjacency:         return PrimitiveTopology.LinesAdjacency;
                case PrimitiveType.LineStripAdjacency:     return PrimitiveTopology.LineStripAdjacency;
                case PrimitiveType.TrianglesAdjacency:     return PrimitiveTopology.TrianglesAdjacency;
                case PrimitiveType.TriangleStripAdjacency: return PrimitiveTopology.TriangleStripAdjacency;
                case PrimitiveType.Patches:                return PrimitiveTopology.Patches;
            }

            return PrimitiveTopology.Triangles;
        }
    }
}