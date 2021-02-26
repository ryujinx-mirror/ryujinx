using Ryujinx.Audio.Integration;
using Ryujinx.Audio.Renderer.Server;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRenderer
{
    class AudioRenderer : IAudioRenderer
    {
        private AudioRenderSystem _impl;

        public AudioRenderer(AudioRenderSystem impl)
        {
            _impl = impl;
        }

        public ResultCode ExecuteAudioRendererRendering()
        {
            throw new NotImplementedException();
        }

        public uint GetMixBufferCount()
        {
            return _impl.GetMixBufferCount();
        }

        public uint GetRenderingTimeLimit()
        {
            return _impl.GetRenderingTimeLimit();
        }

        public uint GetSampleCount()
        {
            return _impl.GetSampleCount();
        }

        public uint GetSampleRate()
        {
            return _impl.GetSampleRate();
        }

        public int GetState()
        {
            if (_impl.IsActive())
            {
                return 0;
            }

            return 1;
        }

        public ResultCode QuerySystemEvent(out KEvent systemEvent)
        {
            ResultCode resultCode = (ResultCode)_impl.QuerySystemEvent(out IWritableEvent outEvent);

            if (resultCode == ResultCode.Success)
            {
                if (outEvent is AudioKernelEvent)
                {
                    systemEvent = ((AudioKernelEvent)outEvent).Event;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                systemEvent = null;
            }

            return resultCode;
        }

        public ResultCode RequestUpdate(Memory<byte> output, Memory<byte> performanceOutput, ReadOnlyMemory<byte> input)
        {
            return (ResultCode)_impl.Update(output, performanceOutput, input);
        }

        public void SetRenderingTimeLimit(uint percent)
        {
            _impl.SetRenderingTimeLimitPercent(percent);
        }

        public ResultCode Start()
        {
            _impl.Start();

            return ResultCode.Success;
        }

        public ResultCode Stop()
        {
            _impl.Stop();

            return ResultCode.Success;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _impl.Dispose();
            }
        }
    }
}
