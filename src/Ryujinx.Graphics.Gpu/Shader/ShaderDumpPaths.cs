using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Paths where shader code was dumped on disk.
    /// </summary>
    readonly struct ShaderDumpPaths
    {
        /// <summary>
        /// Path where the full shader code with header was dumped, or null if not dumped.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Path where the shader code without header was dumped, or null if not dumped.
        /// </summary>
        public string CodePath { get; }

        /// <summary>
        /// True if the shader was dumped, false otherwise.
        /// </summary>
        public bool HasPath => FullPath != null && CodePath != null;

        /// <summary>
        /// Creates a new shader dumps path structure.
        /// </summary>
        /// <param name="fullPath">Path where the full shader code with header was dumped, or null if not dumped</param>
        /// <param name="codePath">Path where the shader code without header was dumped, or null if not dumped</param>
        public ShaderDumpPaths(string fullPath, string codePath)
        {
            FullPath = fullPath;
            CodePath = codePath;
        }

        /// <summary>
        /// Prepends the shader paths on the program source, as a comment.
        /// </summary>
        /// <param name="program">Program to prepend into</param>
        public void Prepend(ShaderProgram program)
        {
            if (HasPath)
            {
                program.Prepend("// " + CodePath);
                program.Prepend("// " + FullPath);
            }
        }
    }
}
