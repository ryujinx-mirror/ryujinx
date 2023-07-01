using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input information for a voice channel resources.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0x70, Pack = 1)]
    public struct VoiceChannelResourceInParameter
    {
        /// <summary>
        /// The id of the voice channel resource.
        /// </summary>
        public uint Id;

        /// <summary>
        /// Mix volumes for the voice channel resource.
        /// </summary>
        public Array24<float> Mix;

        /// <summary>
        /// Indicate if the voice channel resource is used.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsUsed;
    }
}
