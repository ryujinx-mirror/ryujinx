namespace Ryujinx.Graphics.Shader.CodeGen
{
    /// <summary>
    /// Shader code generation logging interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Prints a log message.
        /// </summary>
        /// <param name="message">Message to print</param>
        void Log(string message)
        {
            // No default log output.
        }
    }
}
