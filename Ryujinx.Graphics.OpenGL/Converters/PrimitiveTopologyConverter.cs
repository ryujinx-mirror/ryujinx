using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class PrimitiveTopologyConverter
    {
        public static PrimitiveType Convert(this PrimitiveTopology topology)
        {
            switch (topology)
            {
                case PrimitiveTopology.Points:                 return PrimitiveType.Points;
                case PrimitiveTopology.Lines:                  return PrimitiveType.Lines;
                case PrimitiveTopology.LineLoop:               return PrimitiveType.LineLoop;
                case PrimitiveTopology.LineStrip:              return PrimitiveType.LineStrip;
                case PrimitiveTopology.Triangles:              return PrimitiveType.Triangles;
                case PrimitiveTopology.TriangleStrip:          return PrimitiveType.TriangleStrip;
                case PrimitiveTopology.TriangleFan:            return PrimitiveType.TriangleFan;
                case PrimitiveTopology.Quads:                  return PrimitiveType.Quads;
                case PrimitiveTopology.QuadStrip:              return PrimitiveType.QuadStrip;
                case PrimitiveTopology.Polygon:                return PrimitiveType.Polygon;
                case PrimitiveTopology.LinesAdjacency:         return PrimitiveType.LinesAdjacency;
                case PrimitiveTopology.LineStripAdjacency:     return PrimitiveType.LineStripAdjacency;
                case PrimitiveTopology.TrianglesAdjacency:     return PrimitiveType.TrianglesAdjacency;
                case PrimitiveTopology.TriangleStripAdjacency: return PrimitiveType.TriangleStripAdjacency;
                case PrimitiveTopology.Patches:                return PrimitiveType.Patches;
            }

            throw new ArgumentException($"Invalid primitive topology \"{topology}\".");
        }
    }
}
