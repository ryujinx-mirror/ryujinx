using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Biquad filter parameters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0xC, Pack = 1)]
    public struct BiquadFilterParameter
    {
        /// <summary>
        /// Set to true if the biquad filter is active.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool Enable;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private readonly byte _reserved;

        /// <summary>
        /// Biquad filter numerator (b0, b1, b2).
        /// </summary>
        public Array3<short> Numerator;

        /// <summary>
        /// Biquad filter denominator (a1, a2).
        /// </summary>
        /// <remarks>a0 = 1</remarks>
        public Array2<short> Denominator;
    }
}
