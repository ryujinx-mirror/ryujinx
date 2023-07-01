namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Generic interface to represent output information for an effect.
    /// </summary>
    public interface IEffectOutStatus
    {
        /// <summary>
        /// Current effect state.
        /// </summary>
        EffectState State { get; set; }
    }
}
