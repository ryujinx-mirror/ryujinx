namespace Ryujinx.Graphics.Shader
{
    enum OutputTopology
    {
        PointList = 1,
        LineStrip = 6,
        TriangleStrip = 7,
    }

    static class OutputTopologyExtensions
    {
        public static string ToGlslString(this OutputTopology topology)
        {
            return topology switch
            {
                OutputTopology.LineStrip => "line_strip",
                OutputTopology.PointList => "points",
                OutputTopology.TriangleStrip => "triangle_strip",
                _ => "points",
            };
        }
    }
}
