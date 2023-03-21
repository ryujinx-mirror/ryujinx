namespace Ryujinx.Common.Configuration
{
    public enum AspectRatio
    {
        Fixed4x3,
        Fixed16x9,
        Fixed16x10,
        Fixed21x9,
        Fixed32x9,
        Stretched
    }

    public static class AspectRatioExtensions
    {
        public static float ToFloat(this AspectRatio aspectRatio)
        {
            return aspectRatio.ToFloatX() / aspectRatio.ToFloatY();
        }

        public static float ToFloatX(this AspectRatio aspectRatio)
        {
            return aspectRatio switch
            {
                AspectRatio.Fixed4x3   => 4.0f,
                AspectRatio.Fixed16x9  => 16.0f,
                AspectRatio.Fixed16x10 => 16.0f,
                AspectRatio.Fixed21x9  => 21.0f,
                AspectRatio.Fixed32x9  => 32.0f,
                _                      => 16.0f
            };
        }

        public static float ToFloatY(this AspectRatio aspectRatio)
        {
            return aspectRatio switch
            {
                AspectRatio.Fixed4x3   => 3.0f,
                AspectRatio.Fixed16x9  => 9.0f,
                AspectRatio.Fixed16x10 => 10.0f,
                AspectRatio.Fixed21x9  => 9.0f,
                AspectRatio.Fixed32x9  => 9.0f,
                _                      => 9.0f
            };
        }

        public static string ToText(this AspectRatio aspectRatio)
        {
            return aspectRatio switch
            {
                AspectRatio.Fixed4x3   => "4:3",
                AspectRatio.Fixed16x9  => "16:9",
                AspectRatio.Fixed16x10 => "16:10",
                AspectRatio.Fixed21x9  => "21:9",
                AspectRatio.Fixed32x9  => "32:9",
                _                      => "Stretched"
            };
        }
    }
}