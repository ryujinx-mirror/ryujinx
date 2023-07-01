using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Voice
{
    /// <summary>
    /// Server state for a voice channel resource.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0xD0, Pack = Alignment)]
    public struct VoiceChannelResource
    {
        public const int Alignment = 0x10;

        /// <summary>
        /// Mix volumes for the resource.
        /// </summary>
        public Array24<float> Mix;

        /// <summary>
        /// Previous mix volumes for resource.
        /// </summary>
        public Array24<float> PreviousMix;

        /// <summary>
        /// The id of the resource.
        /// </summary>
        public uint Id;

        /// <summary>
        /// Indicate if the resource is used.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsUsed;

        public void UpdateState()
        {
            Mix.AsSpan().CopyTo(PreviousMix.AsSpan());
        }
    }
}
