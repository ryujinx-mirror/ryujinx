using Ryujinx.Audio.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioIn
{
    interface IAudioIn : IDisposable
    {
        AudioDeviceState GetState();

        ResultCode Start();

        ResultCode Stop();

        ResultCode AppendBuffer(ulong bufferTag, ref AudioUserBuffer buffer);

        // NOTE: This is broken by design... not quite sure what it's used for (if anything in production).
        ResultCode AppendUacBuffer(ulong bufferTag, ref AudioUserBuffer buffer, uint handle);

        KEvent RegisterBufferEvent();

        ResultCode GetReleasedBuffers(Span<ulong> releasedBuffers, out uint releasedCount);

        bool ContainsBuffer(ulong bufferTag);

        uint GetBufferCount();

        bool FlushBuffers();

        void SetVolume(float volume);

        float GetVolume();
    }
}
