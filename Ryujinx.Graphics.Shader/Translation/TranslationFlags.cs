using System;

namespace Ryujinx.Graphics.Shader.Translation
{
    [Flags]
    public enum TranslationFlags
    {
        None = 0,

        Compute   = 1 << 0,
        DebugMode = 1 << 1
    }
}