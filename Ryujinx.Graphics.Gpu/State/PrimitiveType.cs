using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Draw primitive type.
    /// </summary>
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

    /// <summary>
    /// Alternative primitive type that might override <see cref="PrimitiveType"/>.
    /// </summary>
    enum PrimitiveTypeOverride
    {
        Invalid = 0,
        Points = 1,
        Lines = 2,
        LineStrip = 3,
        Triangles = 4,
        TriangleStrip = 5,
        TriangleFan = 0x1015,
        LinesAdjacency = 10,
        LineStripAdjacency = 11,
        TrianglesAdjacency = 12,
        TriangleStripAdjacency = 13,
        Patches = 14
    }

    static class PrimitiveTypeConverter
    {
        /// <summary>
        /// Converts the primitive type into something that can be used with the host API.
        /// </summary>
        /// <param name="type">The primitive type to convert</param>
        /// <returns>A host compatible enum value</returns>
        public static PrimitiveTopology Convert(this PrimitiveType type)
        {
            return type switch
            {
                PrimitiveType.Points                 => PrimitiveTopology.Points,
                PrimitiveType.Lines                  => PrimitiveTopology.Lines,
                PrimitiveType.LineLoop               => PrimitiveTopology.LineLoop,
                PrimitiveType.LineStrip              => PrimitiveTopology.LineStrip,
                PrimitiveType.Triangles              => PrimitiveTopology.Triangles,
                PrimitiveType.TriangleStrip          => PrimitiveTopology.TriangleStrip,
                PrimitiveType.TriangleFan            => PrimitiveTopology.TriangleFan,
                PrimitiveType.Quads                  => PrimitiveTopology.Quads,
                PrimitiveType.QuadStrip              => PrimitiveTopology.QuadStrip,
                PrimitiveType.Polygon                => PrimitiveTopology.Polygon,
                PrimitiveType.LinesAdjacency         => PrimitiveTopology.LinesAdjacency,
                PrimitiveType.LineStripAdjacency     => PrimitiveTopology.LineStripAdjacency,
                PrimitiveType.TrianglesAdjacency     => PrimitiveTopology.TrianglesAdjacency,
                PrimitiveType.TriangleStripAdjacency => PrimitiveTopology.TriangleStripAdjacency,
                PrimitiveType.Patches                => PrimitiveTopology.Patches,
                _                                    => PrimitiveTopology.Triangles
            };
        }

        /// <summary>
        /// Converts the primitive type into something that can be used with the host API.
        /// </summary>
        /// <param name="type">The primitive type to convert</param>
        /// <returns>A host compatible enum value</returns>
        public static PrimitiveTopology Convert(this PrimitiveTypeOverride type)
        {
            return type switch
            {
                PrimitiveTypeOverride.Points                 => PrimitiveTopology.Points,
                PrimitiveTypeOverride.Lines                  => PrimitiveTopology.Lines,
                PrimitiveTypeOverride.LineStrip              => PrimitiveTopology.LineStrip,
                PrimitiveTypeOverride.Triangles              => PrimitiveTopology.Triangles,
                PrimitiveTypeOverride.TriangleStrip          => PrimitiveTopology.TriangleStrip,
                PrimitiveTypeOverride.TriangleFan            => PrimitiveTopology.TriangleFan,
                PrimitiveTypeOverride.LinesAdjacency         => PrimitiveTopology.LinesAdjacency,
                PrimitiveTypeOverride.LineStripAdjacency     => PrimitiveTopology.LineStripAdjacency,
                PrimitiveTypeOverride.TrianglesAdjacency     => PrimitiveTopology.TrianglesAdjacency,
                PrimitiveTypeOverride.TriangleStripAdjacency => PrimitiveTopology.TriangleStripAdjacency,
                PrimitiveTypeOverride.Patches                => PrimitiveTopology.Patches,
                _                                            => PrimitiveTopology.Triangles
            };
        }
    }
}