using System;

namespace Ryujinx.Graphics.Shader.Translation
{
    [Flags]
    public enum TranslationFlags
    {
        None = 0,

        VertexA   = 1 << 0,
        Compute   = 1 << 1,
        Feedback  = 1 << 2,
        DebugMode = 1 << 3
    }
}