using ChocolArm64.Memory;
using Ryujinx.Audio;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Aud.AudioOut
{
    class IAudioOut : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private IAalOutput _audioOut;

        private KEvent _releaseEvent;

        private int _track;

        public IAudioOut(IAalOutput audioOut, KEvent releaseEvent, int track)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, GetAudioOutState              },
                { 1, StartAudioOut                 },
                { 2, StopAudioOut                  },
                { 3, AppendAudioOutBuffer          },
                { 4, RegisterBufferEvent           },
                { 5, GetReleasedAudioOutBuffer     },
                { 6, ContainsAudioOutBuffer        },
                { 7, AppendAudioOutBufferAuto      },
                { 8, GetReleasedAudioOutBufferAuto }
            };

            _audioOut     = audioOut;
            _releaseEvent = releaseEvent;
            _track        = track;
        }

        public long GetAudioOutState(ServiceCtx context)
        {
            context.ResponseData.Write((int)_audioOut.GetState(_track));

            return 0;
        }

        public long StartAudioOut(ServiceCtx context)
        {
            _audioOut.Start(_track);

            return 0;
        }

        public long StopAudioOut(ServiceCtx context)
        {
            _audioOut.Stop(_track);

            return 0;
        }

        public long AppendAudioOutBuffer(ServiceCtx context)
        {
            return AppendAudioOutBufferImpl(context, context.Request.SendBuff[0].Position);
        }

        public long RegisterBufferEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_releaseEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return 0;
        }

        public long GetReleasedAudioOutBuffer(ServiceCtx context)
        {
            long position = context.Request.ReceiveBuff[0].Position;
            long size     = context.Request.ReceiveBuff[0].Size;

            return GetReleasedAudioOutBufferImpl(context, position, size);
        }

        public long ContainsAudioOutBuffer(ServiceCtx context)
        {
            long tag = context.RequestData.ReadInt64();

            context.ResponseData.Write(_audioOut.ContainsBuffer(_track, tag) ? 1 : 0);

            return 0;
        }

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
