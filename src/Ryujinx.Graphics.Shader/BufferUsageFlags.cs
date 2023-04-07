using System;

namespace Ryujinx.Graphics.Shader
{
    /// <summary>
    /// Flags that indicate how a buffer will be used in a shader.
    /// </summary>
    [Flags]
    public enum BufferUsageFlags
    {
        None = 0,

        /// <summary>
        /// Buffer is written to.
        /// </summary>
        Write = 1 << 0
    }
}
