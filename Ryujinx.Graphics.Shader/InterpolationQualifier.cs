using System;

namespace Ryujinx.Graphics.Shader
{
    [Flags]
    public enum InterpolationQualifier
    {
        None = 0,

        Flat          = 1,
        NoPerspective = 2,
        Smooth        = 3,

        Centroid = 1 << 16,
        Sample   = 1 << 17,

        FlagsMask = Centroid | Sample
    }

    public static class InterpolationQualifierExtensions
    {
        public static string ToGlslQualifier(this InterpolationQualifier iq)
        {
            string output = string.Empty;

            switch (iq & ~InterpolationQualifier.FlagsMask)
            {
                case InterpolationQualifier.Flat:          output = "flat";          break;
                case InterpolationQualifier.NoPerspective: output = "noperspective"; break;
                case InterpolationQualifier.Smooth:        output = "smooth";        break;
            }

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