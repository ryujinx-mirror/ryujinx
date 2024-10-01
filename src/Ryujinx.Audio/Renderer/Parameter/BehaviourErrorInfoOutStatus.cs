using Ryujinx.Common.Memory;
using System;
using System.Runtime.InteropServices;
using static Ryujinx.Audio.Renderer.Common.BehaviourParameter;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Output information for behaviour.
    /// </summary>
    /// <remarks>This is used to report errors to the user during <see cref="Server.AudioRenderSystem.Update(Memory{byte}, Memory{byte}, System.Buffers.ReadOnlySequence{byte})"/> processing.</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BehaviourErrorInfoOutStatus
    {
        /// <summary>
        /// The reported errors.
        /// </summary>
        public Array10<ErrorInfo> ErrorInfos;

        /// <summary>
        /// The amount of error that got reported.
        /// </summary>
        public uint ErrorInfosCount;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed uint _reserved[3];
    }
}
