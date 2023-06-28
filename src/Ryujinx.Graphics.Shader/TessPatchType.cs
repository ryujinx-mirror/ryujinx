namespace Ryujinx.Graphics.Shader
{
    public enum TessPatchType
    {
        Isolines = 0,
        Triangles = 1,
        Quads = 2,
    }

    static class TessPatchTypeExtensions
    {
        public static string ToGlsl(this TessPatchType type)
        {
            return type switch
            {
                TessPatchType.Isolines => "isolines",
                TessPatchType.Quads => "quads",
                _ => "triangles",
            };
        }
    }
}
