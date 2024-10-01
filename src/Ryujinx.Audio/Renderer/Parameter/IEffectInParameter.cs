using Ryujinx.Audio.Renderer.Common;
using System;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Generic interface to represent input information for an effect.
    /// </summary>
    public interface IEffectInParameter
    {
        /// <summary>
        /// Type of the effect.
        /// </summary>
        EffectType Type { get; }

        /// <summary>
        /// Set to true if the effect is new.
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// Set to true if the effect must be active.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// The target mix id of the effect.
        /// </summary>
        int MixId { get; }

        /// <summary>
        /// Address of the processing workbuffer.
        /// </summary>
        /// <remarks>This is additional data that could be required by the effect processing.</remarks>
        ulong BufferBase { get; }

        /// <summary>
        /// Size of the processing workbuffer.
        /// </summary>
        /// <remarks>This is additional data that could be required by the effect processing.</remarks>
        ulong BufferSize { get; }

        /// <summary>
        /// Position of the effect while processing effects.
        /// </summary>
        uint ProcessingOrder { get; }

        /// <summary>
        /// Specific data changing depending of the <see cref="Type"/>. See also the <see cref="Effect"/> namespace.
        /// </summary>
        Span<byte> SpecificData { get; }
    }
}
