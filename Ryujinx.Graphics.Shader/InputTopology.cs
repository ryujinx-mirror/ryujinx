namespace Ryujinx.Graphics.Shader
{
    public enum InputTopology : byte
    {
        Points,
        Lines,
        LinesAdjacency,
        Triangles,
        TrianglesAdjacency
    }

    static class InputTopologyExtensions
    {
        public static string ToGlslString(this InputTopology topology)
        {
            switch (topology)
            {
                case InputTopology.Points:             return "points";
                case InputTopology.Lines:              return "lines";
                case InputTopology.LinesAdjacency:     return "lines_adjacency";
                case InputTopology.Triangles:          return "triangles";
                case InputTopology.TrianglesAdjacency: return "triangles_adjacency";
            }

            return "points";
        }
    }
}