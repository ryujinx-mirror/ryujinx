namespace Ryujinx.Graphics.Gpu.Image
{
    enum TextureMsaaMode
    {
        Ms1x1 = 0,
        Ms2x2 = 2,
        Ms4x2 = 4,
        Ms2x1 = 5,
        Ms4x4 = 6
    }

    static class TextureMsaaModeConverter
    {
        public static int SamplesCount(this TextureMsaaMode msaaMode)
        {
            switch (msaaMode)
            {
                case TextureMsaaMode.Ms2x1: return 2;
                case TextureMsaaMode.Ms2x2: return 4;
                case TextureMsaaMode.Ms4x2: return 8;
                case TextureMsaaMode.Ms4x4: return 16;
            }

            return 1;
        }

        public static int SamplesInX(this TextureMsaaMode msaaMode)
        {
            switch (msaaMode)
            {
                case TextureMsaaMode.Ms2x1: return 2;
                case TextureMsaaMode.Ms2x2: return 2;
                case TextureMsaaMode.Ms4x2: return 4;
                case TextureMsaaMode.Ms4x4: return 4;
            }

            return 1;
        }

        public static int SamplesInY(this TextureMsaaMode msaaMode)
        {
            switch (msaaMode)
            {
                case TextureMsaaMode.Ms2x1: return 1;
                case TextureMsaaMode.Ms2x2: return 2;
                case TextureMsaaMode.Ms4x2: return 2;
                case TextureMsaaMode.Ms4x4: return 4;
            }

            return 1;
        }
    }
}