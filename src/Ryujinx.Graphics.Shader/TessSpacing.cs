namespace Ryujinx.Graphics.Shader
{
    public enum TessSpacing
    {
        EqualSpacing = 0,
        FractionalEventSpacing = 1,
        FractionalOddSpacing = 2,
    }

    static class TessSpacingExtensions
    {
        public static string ToGlsl(this TessSpacing spacing)
        {
            return spacing switch
            {
                TessSpacing.FractionalEventSpacing => "fractional_even_spacing",
                TessSpacing.FractionalOddSpacing => "fractional_odd_spacing",
                _ => "equal_spacing",
            };
        }
    }
}
