namespace Ryujinx.Graphics.Shader.Translation
{
    public enum TranslationFlags
    {
        None = 0,

        Compute       = 1 << 0,
        DebugMode     = 1 << 1,
        Unspecialized = 1 << 2,
        DividePosXY   = 1 << 3
    }
}