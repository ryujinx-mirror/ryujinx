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
        IntegerSampling = 1 << 0,
        FragCoordXY     = 1 << 1,

        Bindless = 1 << 2,
        InstanceId = 1 << 3,
        RtLayer = 1 << 4,
        CbIndexing = 1 << 5,
        IaIndexing = 1 << 6,
        OaIndexing = 1 << 7
    }
}
