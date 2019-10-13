using System;

namespace Ryujinx.Graphics.Shader.Translation
{
    public struct TranslationConfig
    {
        public int MaxCBufferSize { get; }

        public int Version { get; }

        public TranslationFlags Flags { get; }

        public TranslationConfig(int maxCBufferSize, int version, TranslationFlags flags)
        {
            if (maxCBufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCBufferSize));
            }

            MaxCBufferSize = maxCBufferSize;
            Version        = version;
            Flags          = flags;
        }
    }
}