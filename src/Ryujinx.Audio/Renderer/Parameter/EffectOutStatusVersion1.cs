using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Output information for an effect version 1.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EffectOutStatusVersion1 : IEffectOutStatus
    {
        /// <summary>
        /// Current effect state.
        /// </summary>
        public EffectState State;

        /// <summary>
        /// Unused/Reserved.
        /// </summary>
        private unsafe fixed byte _reserved[15];

        EffectState IEffectOutStatus.State { readonly get => State; set => State = value; }
    }
}
