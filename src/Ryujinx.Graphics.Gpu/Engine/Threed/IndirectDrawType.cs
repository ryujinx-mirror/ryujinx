using System.Diagnostics.CodeAnalysis;

namespace Ryujinx.Graphics.Gpu.Engine.Threed
{
    /// <summary>
    /// Indirect draw type, which can be indexed or non-indexed, with or without a draw count.
    /// </summary>
    [SuppressMessage("Design", "CA1069: Enums values should not be duplicated")]
    enum IndirectDrawType
    {
        /// <summary>
        /// Non-indexed draw without draw count.
        /// </summary>
        DrawIndirect = 0,

        /// <summary>
        /// Indexed draw without draw count.
        /// </summary>
        DrawIndexedIndirect = Indexed,

        /// <summary>
        /// Non-indexed draw with draw count.
        /// </summary>
        DrawIndirectCount = Count,

        /// <summary>
        /// Indexed draw with draw count.
        /// </summary>
        DrawIndexedIndirectCount = Indexed | Count,

        /// <summary>
        /// Indexed flag.
        /// </summary>
        Indexed = 1 << 0,

        /// <summary>
        /// Draw count flag.
        /// </summary>
        Count = 1 << 1,
    }
}
