namespace Ryujinx.Graphics.Gpu
{
    /// <summary>
    /// General GPU and graphics configuration.
    /// </summary>
    public static class GraphicsConfig
    {
        /// <summary>
        /// Resolution scale.
        /// </summary>
        public static float ResScale = 1f;

        /// <summary>
        /// Max Anisotropy. Values range from 0 - 16. Set to -1 to let the game decide.
        /// </summary>
        public static float MaxAnisotropy;

        /// <summary>
        /// Base directory used to write shader code dumps.
        /// Set to null to disable code dumping.
        /// </summary>
        public static string ShadersDumpPath;

        /// <summary>
        /// Fast GPU time calculates the internal GPU time ticks as if the GPU was capable of
        /// processing commands almost instantly, instead of using the host timer.
        /// This can avoid lower resolution on some games when GPU performance is poor.
        /// </summary>
        public static bool FastGpuTime = true;

        /// <summary>
        /// Enables or disables the Just-in-Time compiler for GPU Macro code.
        /// </summary>
        public static bool EnableMacroJit = true;

        /// <summary>
        /// Title id of the current running game.
        /// Used by the shader cache.
        /// </summary>
        public static string TitleId;

        /// <summary>
        /// Enables or disables the shader cache.
        /// </summary>
        public static bool EnableShaderCache;
    }
}