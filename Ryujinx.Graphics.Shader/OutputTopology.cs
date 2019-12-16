namespace Ryujinx.Graphics.Shader
{
    enum OutputTopology
    {
        PointList     = 1,
        LineStrip     = 6,
        TriangleStrip = 7
    }

    static class OutputTopologyExtensions
    {
        public static string ToGlslString(this OutputTopology topology)
        {
            switch (topology)
            {
                case OutputTopology.LineStrip:     return "line_strip";
                case OutputTopology.PointList:     return "points";
                case OutputTopology.TriangleStrip: return "triangle_strip";
            }

            return "points";
        }
    }
}