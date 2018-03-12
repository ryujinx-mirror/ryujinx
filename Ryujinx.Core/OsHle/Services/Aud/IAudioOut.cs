using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.Core.OsHle.IpcServices.Aud
{
    class IAudioOut : IIpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IAudioOut()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetAudioOutState             },
                { 1, StartAudioOut                },
                { 2, StopAudioOut                 },
                { 3, AppendAudioOutBuffer         },
                { 4, RegisterBufferEvent          },
                { 5, GetReleasedAudioOutBuffer    },
                { 6, ContainsAudioOutBuffer       },
                { 7, AppendAudioOutBuffer_ex      },
                { 8, GetReleasedAudioOutBuffer_ex }
            };
        }

        enum AudioOutState
        {
            Started,
            Stopped
        };

        //IAudioOut
        private AudioOutState State = AudioOutState.Stopped;
        private Queue<long> BufferIdQueue = new Queue<long>();

        //OpenAL
        private bool OpenALInstalled = true;
        private AudioContext AudioCtx;
        private int Source;
        private int Buffer;

        //Return State of IAudioOut
        public long GetAudioOutState(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)State);

            return 0;
        }

        public long StartAudioOut(ServiceCtx Context)
        {
            if (State == AudioOutState.Stopped)
            {
                State = AudioOutState.Started;

                try
                { 
                    AudioCtx = new AudioContext(); //Create the audio context
                }
                catch (Exception)
                {
                    Logging.Warn("OpenAL Error! PS: Install OpenAL Core SDK!");
                    OpenALInstalled = false;
                }

                if (OpenALInstalled) AL.Listener(ALListenerf.Gain, 8.0f); //Add more gain to it
            }

            return 0;
        }

        public long StopAudioOut(ServiceCtx Context)
        {
            if (State == AudioOutState.Started)
            {
                if (OpenALInstalled)
                { 
                    if (AudioCtx == null) //Needed to call the instance of AudioContext()
                        return 0;

                    AL.SourceStop(Source);
                    AL.DeleteSource(Source);
                    AL.DeleteBuffers(1, ref Buffer);
                }
                State = AudioOutState.Stopped;
            }

            return 0;
        }

        public long AppendAudioOutBuffer(ServiceCtx Context)
        {
            long BufferId = Context.RequestData.ReadInt64();

            byte[] AudioOutBuffer = AMemoryHelper.ReadBytes(Context.Memory, Context.Request.SendBuff[0].Position, sizeof(long) * 5);

            using (MemoryStream MS = new MemoryStream(AudioOutBuffer))
            {
                BinaryReader Reader = new BinaryReader(MS);
                long PointerNextBuffer        = Reader.ReadInt64();
                long PointerSampleBuffer      = Reader.ReadInt64();
                long CapacitySampleBuffer     = Reader.ReadInt64();
                long SizeDataInSampleBuffer   = Reader.ReadInt64();
                long OffsetDataInSampleBuffer = Reader.ReadInt64();

                if (SizeDataInSampleBuffer > 0)
                {
                    BufferIdQueue.Enqueue(BufferId);

                    byte[] AudioSampleBuffer = AMemoryHelper.ReadBytes(Context.Memory, PointerSampleBuffer + OffsetDataInSampleBuffer, (int)SizeDataInSampleBuffer);

                    if (OpenALInstalled)
                    {
                        if (AudioCtx == null) //Needed to call the instance of AudioContext()
                            return 0;
                        
                        EnsureAudioFinalized();

                        Source = AL.GenSource();
                        Buffer = AL.GenBuffer();

                        AL.BufferData(Buffer, ALFormat.Stereo16, AudioSampleBuffer, AudioSampleBuffer.Length, 48000);
                        AL.SourceQueueBuffer(Source, Buffer);
                        AL.SourcePlay(Source);
                    }
                }
            }

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
            int ReleasedBuffersCount = 0;

            for(int i = 0; i < BufferIdQueue.Count; i++)
            {
                long BufferId = BufferIdQueue.Dequeue();

                AMemoryHelper.WriteBytes(Context.Memory, Context.Request.ReceiveBuff[0].Position + (8 * i), BitConverter.GetBytes(BufferId));

                ReleasedBuffersCount++;
            }

            Context.ResponseData.Write(ReleasedBuffersCount);

            return 0;
        }

        public long ContainsAudioOutBuffer(ServiceCtx Context)
        {
            return 0;
        }

        public long AppendAudioOutBuffer_ex(ServiceCtx Context)
        {
            return 0;
        }

        public long GetReleasedAudioOutBuffer_ex(ServiceCtx Context)
        {
            return 0;
        }

        private void EnsureAudioFinalized()
        {
            if (Source != 0 ||
                Buffer != 0)
            {
                AL.SourceStop(Source);
                AL.SourceUnqueueBuffer(Buffer);
                AL.DeleteSource(Source);
                AL.DeleteBuffers(1, ref Buffer);

                Source = 0;
                Buffer = 0;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EnsureAudioFinalized();
            }
        }
    }
}
