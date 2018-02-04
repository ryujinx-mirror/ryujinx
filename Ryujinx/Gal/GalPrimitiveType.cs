namespace Gal
{
    public enum GalPrimitiveType
    {
        Points                 = 0x0,
        Lines                  = 0x1,
        LineLoop               = 0x2,
        LineStrip              = 0x3,
        Triangles              = 0x4,
        TriangleStrip          = 0x5,
        TriangleFan            = 0x6,
        Quads                  = 0x7,
        QuadStrip              = 0x8,
        Polygon                = 0x9,
        LinesAdjacency         = 0xa,
        LineStripAdjacency     = 0xb,
        TrianglesAdjacency     = 0xc,
        TriangleStripAdjacency = 0xd,
        Patches                = 0xe
    }
}