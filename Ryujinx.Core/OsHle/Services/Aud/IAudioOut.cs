using ChocolArm64.Memory;
using Ryujinx.Audio;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Aud
{
    class IAudioOut : IIpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private IAalOutput AudioOut;

        private int Track;

        public IAudioOut(IAalOutput AudioOut, int Track)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetAudioOutState            },
                { 1, StartAudioOut               },
                { 2, StopAudioOut                },
                { 3, AppendAudioOutBuffer        },
                { 4, RegisterBufferEvent         },
                { 5, GetReleasedAudioOutBuffer   },
                { 6, ContainsAudioOutBuffer      },
                { 7, AppendAudioOutBufferEx      },
                { 8, GetReleasedAudioOutBufferEx }
            };

            this.AudioOut = AudioOut;
            this.Track    = Track;
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
            long Tag = Context.RequestData.ReadInt64();

            AudioOutData Data = AMemoryHelper.Read<AudioOutData>(
                Context.Memory,
                Context.Request.SendBuff[0].Position);
            
            byte[] Buffer = AMemoryHelper.ReadBytes(
                Context.Memory,
                Data.SampleBufferPtr,
                Data.SampleBufferSize);

            AudioOut.AppendBuffer(Track, Tag, Buffer);

            return 0;
        }

        public long RegisterBufferEvent(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(new HEvent());

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        public long GetReleasedAudioOutBuffer(ServiceCtx Context)
        {
            long Position = Context.Request.ReceiveBuff[0].Position;
            long Size     = Context.Request.ReceiveBuff[0].Size;

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

        public long ContainsAudioOutBuffer(ServiceCtx Context)
        {
            long Tag = Context.RequestData.ReadInt64();

            Context.ResponseData.Write(AudioOut.ContainsBuffer(Track, Tag) ? 1 : 0);

            return 0;
        }

        public long AppendAudioOutBufferEx(ServiceCtx Context)
        {
            Logging.Warn("Not implemented!");

            return 0;
        }

        public long GetReleasedAudioOutBufferEx(ServiceCtx Context)
        {
            Logging.Warn("Not implemented!");

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
