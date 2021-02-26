using Ryujinx.Audio.Common;
using Ryujinx.Audio.Input;
using Ryujinx.Audio.Integration;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Audio.AudioRenderer;
using System;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioIn
{
    class AudioIn : IAudioIn
    {
        private AudioInputSystem _system;
        private uint _processHandle;
        private KernelContext _kernelContext;

        public AudioIn(AudioInputSystem system, KernelContext kernelContext, uint processHandle)
        {
            _system = system;
            _kernelContext = kernelContext;
            _processHandle = processHandle;
        }

        public ResultCode AppendBuffer(ulong bufferTag, ref AudioUserBuffer buffer)
        {
            return (ResultCode)_system.AppendBuffer(bufferTag, ref buffer);
        }

        public ResultCode AppendUacBuffer(ulong bufferTag, ref AudioUserBuffer buffer, uint handle)
        {
            return (ResultCode)_system.AppendUacBuffer(bufferTag, ref buffer, handle);
        }

        public bool ContainsBuffer(ulong bufferTag)
        {
            return _system.ContainsBuffer(bufferTag);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _system.Dispose();

                _kernelContext.Syscall.CloseHandle((int)_processHandle);
            }
        }

        public bool FlushBuffers()
        {
            return _system.FlushBuffers();
        }

        public uint GetBufferCount()
        {
            return _system.GetBufferCount();
        }

        public ResultCode GetReleasedBuffers(Span<ulong> releasedBuffers, out uint releasedCount)
        {
            return (ResultCode)_system.GetReleasedBuffers(releasedBuffers, out releasedCount);
        }

        public AudioDeviceState GetState()
        {
            return _system.GetState();
        }

        public float GetVolume()
        {
            return _system.GetVolume();
        }

        public KEvent RegisterBufferEvent()
        {
            IWritableEvent outEvent = _system.RegisterBufferEvent();

            if (outEvent is AudioKernelEvent)
            {
                return ((AudioKernelEvent)outEvent).Event;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void SetVolume(float volume)
        {
            _system.SetVolume(volume);
        }

        public ResultCode Start()
        {
            return (ResultCode)_system.Start();
        }

        public ResultCode Stop()
        {
            return (ResultCode)_system.Stop();
        }
    }
}