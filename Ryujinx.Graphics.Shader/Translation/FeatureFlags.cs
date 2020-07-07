using System;

namespace Ryujinx.Graphics.Shader.Translation
{
    /// <summary>
    /// Features used by the shader that are important for the code generator to know in advance.
    /// These typically change the declarations in the shader header.
    /// </summary>
    [Flags]
    public enum FeatureFlags
    {
        None = 0,

        // Affected by resolution scaling.
        FragCoordXY     = 1 << 1,
        IntegerSampling = 1 << 0
    }
}
