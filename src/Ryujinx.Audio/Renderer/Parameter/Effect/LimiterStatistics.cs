using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter.Effect
{
    /// <summary>
    /// Effect result state for <seealso cref="Common.EffectType.Limiter"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LimiterStatistics
    {
        /// <summary>
        /// The max input sample value recorded by the limiter.
        /// </summary>
        public Array6<float> InputMax;

        /// <summary>
        /// Compression gain min value.
        /// </summary>
        public Array6<float> CompressionGainMin;

        /// <summary>
        /// Reset the statistics.
        /// </summary>
        public void Reset()
        {
            InputMax.AsSpan().Clear();
            CompressionGainMin.AsSpan().Fill(1.0f);
        }
    }
}
