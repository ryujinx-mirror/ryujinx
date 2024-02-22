using Ryujinx.Audio.Common;
using Ryujinx.Audio.Input;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    partial class AudioIn : IAudioIn, IDisposable
    {
        private readonly AudioInputSystem _impl;
        private int _processHandle;

        public AudioIn(AudioInputSystem impl, int processHandle)
        {
            _impl = impl;
            _processHandle = processHandle;
        }

        [CmifCommand(0)]
        public Result GetAudioInState(out AudioDeviceState state)
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
        public Result AppendAudioInBuffer(ulong bufferTag, [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<AudioUserBuffer> buffer)
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
        public Result GetReleasedAudioInBuffers(out uint count, [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias)] Span<ulong> bufferTags)
        {
            return new Result((int)_impl.GetReleasedBuffers(bufferTags, out count));
        }

        [CmifCommand(6)]
        public Result ContainsAudioInBuffer(out bool contains, ulong bufferTag)
        {
            contains = _impl.ContainsBuffer(bufferTag);

            return Result.Success;
        }

        [CmifCommand(7)] // 3.0.0+
        public Result AppendUacInBuffer(
            ulong bufferTag,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<AudioUserBuffer> buffer,
            [CopyHandle] int eventHandle)
        {
            AudioUserBuffer userBuffer = default;

            if (buffer.Length > 0)
            {
                userBuffer = buffer[0];
            }

            return new Result((int)_impl.AppendUacBuffer(bufferTag, ref userBuffer, (uint)eventHandle));
        }

        [CmifCommand(8)] // 3.0.0+
        public Result AppendAudioInBufferAuto(ulong bufferTag, [Buffer(HipcBufferFlags.In | HipcBufferFlags.AutoSelect)] ReadOnlySpan<AudioUserBuffer> buffer)
        {
            return AppendAudioInBuffer(bufferTag, buffer);
        }

        [CmifCommand(9)] // 3.0.0+
        public Result GetReleasedAudioInBuffersAuto(out uint count, [Buffer(HipcBufferFlags.Out | HipcBufferFlags.AutoSelect)] Span<ulong> bufferTags)
        {
            return GetReleasedAudioInBuffers(out count, bufferTags);
        }

        [CmifCommand(10)] // 3.0.0+
        public Result AppendUacInBufferAuto(
            ulong bufferTag,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.AutoSelect)] ReadOnlySpan<AudioUserBuffer> buffer,
            [CopyHandle] int eventHandle)
        {
            return AppendUacInBuffer(bufferTag, buffer, eventHandle);
        }

        [CmifCommand(11)] // 4.0.0+
        public Result GetAudioInBufferCount(out uint bufferCount)
        {
            bufferCount = _impl.GetBufferCount();

            return Result.Success;
        }

        [CmifCommand(12)] // 4.0.0+
        public Result SetDeviceGain(float gain)
        {
            _impl.SetVolume(gain);

            return Result.Success;
        }

        [CmifCommand(13)] // 4.0.0+
        public Result GetDeviceGain(out float gain)
        {
            gain = _impl.GetVolume();

            return Result.Success;
        }

        [CmifCommand(14)] // 6.0.0+
        public Result FlushAudioInBuffers(out bool pending)
        {
            pending = _impl.FlushBuffers();

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
