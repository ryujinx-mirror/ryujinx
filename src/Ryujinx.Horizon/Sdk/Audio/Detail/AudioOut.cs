using Ryujinx.Audio.Common;
using Ryujinx.Audio.Output;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    partial class AudioOut : IAudioOut, IDisposable
    {
        private readonly AudioOutputSystem _impl;
        private int _processHandle;

        public AudioOut(AudioOutputSystem impl, int processHandle)
        {
            _impl = impl;
            _processHandle = processHandle;
        }

        [CmifCommand(0)]
        public Result GetAudioOutState(out AudioDeviceState state)
        {
            state = _impl.GetState();

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result Start()
        {
            return new Result((int)_impl.Start());
        }

        [CmifCommand(2)]
        public Result Stop()
        {
            return new Result((int)_impl.Stop());
        }

        [CmifCommand(3)]
        public Result AppendAudioOutBuffer(ulong bufferTag, [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<AudioUserBuffer> buffer)
        {
            AudioUserBuffer userBuffer = default;

            if (buffer.Length > 0)
            {
                userBuffer = buffer[0];
            }

            return new Result((int)_impl.AppendBuffer(bufferTag, ref userBuffer));
        }

        [CmifCommand(4)]
        public Result RegisterBufferEvent([CopyHandle] out int eventHandle)
        {
            eventHandle = 0;

            if (_impl.RegisterBufferEvent() is AudioEvent audioEvent)
            {
                eventHandle = audioEvent.GetReadableHandle();
            }

            return Result.Success;
        }

        [CmifCommand(5)]
        public Result GetReleasedAudioOutBuffers(out uint count, [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias)] Span<ulong> bufferTags)
        {
            return new Result((int)_impl.GetReleasedBuffer(bufferTags, out count));
        }

        [CmifCommand(6)]
        public Result ContainsAudioOutBuffer(out bool contains, ulong bufferTag)
        {
            contains = _impl.ContainsBuffer(bufferTag);

            return Result.Success;
        }

        [CmifCommand(7)] // 3.0.0+
        public Result AppendAudioOutBufferAuto(ulong bufferTag, [Buffer(HipcBufferFlags.In | HipcBufferFlags.AutoSelect)] ReadOnlySpan<AudioUserBuffer> buffer)
        {
            return AppendAudioOutBuffer(bufferTag, buffer);
        }

        [CmifCommand(8)] // 3.0.0+
        public Result GetReleasedAudioOutBuffersAuto(out uint count, [Buffer(HipcBufferFlags.Out | HipcBufferFlags.AutoSelect)] Span<ulong> bufferTags)
        {
            return GetReleasedAudioOutBuffers(out count, bufferTags);
        }

        [CmifCommand(9)] // 4.0.0+
        public Result GetAudioOutBufferCount(out uint bufferCount)
        {
            bufferCount = _impl.GetBufferCount();

            return Result.Success;
        }

        [CmifCommand(10)] // 4.0.0+
        public Result GetAudioOutPlayedSampleCount(out ulong sampleCount)
        {
            sampleCount = _impl.GetPlayedSampleCount();

            return Result.Success;
        }

        [CmifCommand(11)] // 4.0.0+
        public Result FlushAudioOutBuffers(out bool pending)
        {
            pending = _impl.FlushBuffers();

            return Result.Success;
        }

        [CmifCommand(12)] // 6.0.0+
        public Result SetAudioOutVolume(float volume)
        {
            _impl.SetVolume(volume);

            return Result.Success;
        }

        [CmifCommand(13)] // 6.0.0+
        public Result GetAudioOutVolume(out float volume)
        {
            volume = _impl.GetVolume();

            return Result.Success;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _impl.Dispose();

                if (_processHandle != 0)
                {
                    HorizonStatic.Syscall.CloseHandle(_processHandle);

                    _processHandle = 0;
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
