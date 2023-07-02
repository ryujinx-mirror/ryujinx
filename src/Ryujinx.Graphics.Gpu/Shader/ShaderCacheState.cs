namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>Shader cache loading states</summary>
    public enum ShaderCacheState
    {
        /// <summary>Shader cache started loading</summary>
        Start,
        /// <summary>Shader cache is loading</summary>
        Loading,
        /// <summary>Shader cache is written to disk</summary>
        Packaging,
        /// <summary>Shader cache finished loading</summary>
        Loaded,
    }
}
