using ChocolArm64.Memory;
using Ryujinx.Audio;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Aud.AudioOut
{
    class IAudioOut : IpcService, IDisposable
    {
        private IAalOutput _audioOut;
        private KEvent     _releaseEvent;
        private int        _track;

        public IAudioOut(IAalOutput audioOut, KEvent releaseEvent, int track)
        {
            _audioOut     = audioOut;
            _releaseEvent = releaseEvent;
            _track        = track;
        }

        [Command(0)]
        // GetAudioOutState() -> u32 state
        public long GetAudioOutState(ServiceCtx context)
        {
            context.ResponseData.Write((int)_audioOut.GetState(_track));

            return 0;
        }

        [Command(1)]
        // StartAudioOut()
        public long StartAudioOut(ServiceCtx context)
        {
            _audioOut.Start(_track);

            return 0;
        }

        [Command(2)]
        // StopAudioOut()
        public long StopAudioOut(ServiceCtx context)
        {
            _audioOut.Stop(_track);

            return 0;
        }

        [Command(3)]
        // AppendAudioOutBuffer(u64 tag, buffer<nn::audio::AudioOutBuffer, 5>)
        public long AppendAudioOutBuffer(ServiceCtx context)
        {
            return AppendAudioOutBufferImpl(context, context.Request.SendBuff[0].Position);
        }

        [Command(4)]
        // RegisterBufferEvent() -> handle<copy>
        public long RegisterBufferEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_releaseEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return 0;
        }

        [Command(5)]
        // GetReleasedAudioOutBuffer() -> (u32 count, buffer<nn::audio::AudioOutBuffer, 6>)
        public long GetReleasedAudioOutBuffer(ServiceCtx context)
        {
            long position = context.Request.ReceiveBuff[0].Position;
            long size     = context.Request.ReceiveBuff[0].Size;

            return GetReleasedAudioOutBufferImpl(context, position, size);
        }

        [Command(6)]
        // ContainsAudioOutBuffer(u64 tag) -> b8
        public long ContainsAudioOutBuffer(ServiceCtx context)
        {
            long tag = context.RequestData.ReadInt64();

            context.ResponseData.Write(_audioOut.ContainsBuffer(_track, tag) ? 1 : 0);

            return 0;
        }

        [Command(7)] // 3.0.0+
        // AppendAudioOutBufferAuto(u64 tag, buffer<nn::audio::AudioOutBuffer, 0x21>)
        public long AppendAudioOutBufferAuto(ServiceCtx context)
        {
            (long position, long size) = context.Request.GetBufferType0x21();

            return AppendAudioOutBufferImpl(context, position);
        }

        public long AppendAudioOutBufferImpl(ServiceCtx context, long position)
        {
            long tag = context.RequestData.ReadInt64();

            AudioOutData data = MemoryHelper.Read<AudioOutData>(
                context.Memory,
                position);

            byte[] buffer = context.Memory.ReadBytes(
                data.SampleBufferPtr,
                data.SampleBufferSize);

            _audioOut.AppendBuffer(_track, tag, buffer);

            return 0;
        }

        [Command(8)] // 3.0.0+
        // GetReleasedAudioOutBufferAuto() -> (u32 count, buffer<nn::audio::AudioOutBuffer, 0x22>)
        public long GetReleasedAudioOutBufferAuto(ServiceCtx context)
        {
            (long position, long size) = context.Request.GetBufferType0x22();

            return GetReleasedAudioOutBufferImpl(context, position, size);
        }

        public long GetReleasedAudioOutBufferImpl(ServiceCtx context, long position, long size)
        {
            uint count = (uint)((ulong)size >> 3);

            long[] releasedBuffers = _audioOut.GetReleasedBuffers(_track, (int)count);

            for (uint index = 0; index < count; index++)
            {
                long tag = 0;

                if (index < releasedBuffers.Length)
                {
                    tag = releasedBuffers[index];
                }

                context.Memory.WriteInt64(position + index * 8, tag);
            }

            context.ResponseData.Write(releasedBuffers.Length);

            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _audioOut.CloseTrack(_track);
            }
        }
    }
}