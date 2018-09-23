using ChocolArm64.Memory;
using Ryujinx.Audio;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Aud.AudioOut
{
    class IAudioOut : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private IAalOutput AudioOut;

        private KEvent ReleaseEvent;

        private int Track;

        public IAudioOut(IAalOutput AudioOut, KEvent ReleaseEvent, int Track)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
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

            this.AudioOut     = AudioOut;
            this.ReleaseEvent = ReleaseEvent;
            this.Track        = Track;
        }

        public long GetAudioOutState(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)AudioOut.GetState(Track));

            return 0;
        }

        public long StartAudioOut(ServiceCtx Context)
        {
            AudioOut.Start(Track);

            return 0;
        }

        public long StopAudioOut(ServiceCtx Context)
        {
            AudioOut.Stop(Track);

            return 0;
        }

        public long AppendAudioOutBuffer(ServiceCtx Context)
        {
            return AppendAudioOutBufferImpl(Context, Context.Request.SendBuff[0].Position);
        }

        public long RegisterBufferEvent(ServiceCtx Context)
        {
            if (Context.Process.HandleTable.GenerateHandle(ReleaseEvent.ReadableEvent, out int Handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        public long GetReleasedAudioOutBuffer(ServiceCtx Context)
        {
            long Position = Context.Request.ReceiveBuff[0].Position;
            long Size     = Context.Request.ReceiveBuff[0].Size;

            return GetReleasedAudioOutBufferImpl(Context, Position, Size);
        }

        public long ContainsAudioOutBuffer(ServiceCtx Context)
        {
            long Tag = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(AudioOut.ContainsBuffer(Track, Tag) ? 1 : 0);

            return 0;
        }

        public long AppendAudioOutBufferAuto(ServiceCtx Context)
        {
            (long Position, long Size) = Context.Request.GetBufferType0x21();

            return AppendAudioOutBufferImpl(Context, Position);
        }

        public long AppendAudioOutBufferImpl(ServiceCtx Context, long Position)
        {
            long Tag = Context.RequestData.ReadInt64();

            AudioOutData Data = AMemoryHelper.Read<AudioOutData>(
                Context.Memory,
                Position);

            byte[] Buffer = Context.Memory.ReadBytes(
                Data.SampleBufferPtr,
                Data.SampleBufferSize);

            AudioOut.AppendBuffer(Track, Tag, Buffer);

            return 0;
        }

        public long GetReleasedAudioOutBufferAuto(ServiceCtx Context)
        {
            (long Position, long Size) = Context.Request.GetBufferType0x22();

            return GetReleasedAudioOutBufferImpl(Context, Position, Size);
        }

        public long GetReleasedAudioOutBufferImpl(ServiceCtx Context, long Position, long Size)
        {
            uint Count = (uint)((ulong)Size >> 3);

            long[] ReleasedBuffers = AudioOut.GetReleasedBuffers(Track, (int)Count);

            for (uint Index = 0; Index < Count; Index++)
            {
                long Tag = 0;

                if (Index < ReleasedBuffers.Length)
                {
                    Tag = ReleasedBuffers[Index];
                }

                Context.Memory.WriteInt64(Position + Index * 8, Tag);
            }

            Context.ResponseData.Write(ReleasedBuffers.Length);

            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                AudioOut.CloseTrack(Track);
            }
        }
    }
}
