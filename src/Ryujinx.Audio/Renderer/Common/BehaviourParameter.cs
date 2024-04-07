using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// Represents the input parameter for <see cref="Server.BehaviourContext"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BehaviourParameter
    {
        /// <summary>
        /// The current audio renderer revision in use.
        /// </summary>
        public int UserRevision;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private readonly uint _padding;

        /// <summary>
        /// The flags given controlling behaviour of the audio renderer
        /// </summary>
        /// <remarks>See <see cref="Server.BehaviourContext.UpdateFlags(ulong)"/> and <see cref="Server.BehaviourContext.IsMemoryPoolForceMappingEnabled"/>.</remarks>
        public ulong Flags;

        /// <summary>
        /// Represents an error during <see cref="Server.AudioRenderSystem.Update(System.Memory{byte}, System.Memory{byte}, System.Buffers.ReadOnlySequence{byte})"/>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ErrorInfo
        {
            /// <summary>
            /// The error code to report.
            /// </summary>
            public ResultCode ErrorCode;

            /// <summary>
            /// Reserved/padding.
            /// </summary>
            private readonly uint _padding;

            /// <summary>
            /// Extra information given with the <see cref="ResultCode"/>
            /// </summary>
            /// <remarks>This is usually used to report a faulting cpu address when a <see cref="Server.MemoryPool.MemoryPoolState"/> mapping fail.</remarks>
            public ulong ExtraErrorInfo;
        }
    }
}
