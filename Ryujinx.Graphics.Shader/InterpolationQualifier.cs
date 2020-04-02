using System;

namespace Ryujinx.Graphics.Shader
{
    [Flags]
    enum InterpolationQualifier
    {
        None = 0,

        Flat          = 1,
        NoPerspective = 2,
        Smooth        = 3,

        Centroid = 1 << 16,
        Sample   = 1 << 17,

        FlagsMask = Centroid | Sample
    }

    static class InterpolationQualifierExtensions
    {
        public static string ToGlslQualifier(this InterpolationQualifier iq)
        {
            string output = (iq & ~InterpolationQualifier.FlagsMask) switch
            {
                InterpolationQualifier.Flat => "flat",
                InterpolationQualifier.NoPerspective => "noperspective",
                InterpolationQualifier.Smooth => "smooth",
                _ => string.Empty
            };

            if ((iq & InterpolationQualifier.Centroid) != 0)
            {
                output = "centroid " + output;
            }
            else if ((iq & InterpolationQualifier.Sample) != 0)
            {
                output = "sample " + output;
            }

            return output;
        }
    }
}