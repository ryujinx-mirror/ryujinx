using ChocolArm64.Memory;
using Ryujinx.OsHle.Handles;
using Ryujinx.OsHle.Ipc;
using static Ryujinx.OsHle.Objects.ObjHelper;

using System;
using System.Collections.Generic;
using System.IO;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Ryujinx.OsHle.Objects
{
    class AudIAudioOut
    {
        enum AudioOutState
        {
            Started,
            Stopped
        };

        //IAudioOut
        private static AudioOutState State = AudioOutState.Stopped;
        private static Queue<long> KeysQueue = new Queue<long>();

        //OpenAL
        private static bool OpenALInstalled = true;
        private static AudioContext AudioCtx;
        private static int Source;
        private static int Buffer;

        //Return State of IAudioOut
        public static long GetAudioOutState(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)State);

            return 0;
        }

        public static long StartAudioOut(ServiceCtx Context)
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

                if (OpenALInstalled) AL.Listener(ALListenerf.Gain, (float)8.0); //Add more gain to it
            }

            return 0;
        }

        public static long StopAudioOut(ServiceCtx Context)
        {
            if (State == AudioOutState.Started)
            {
                if (OpenALInstalled)
                { 
                    if (AudioCtx == null) //Needed to call the instance of AudioContext()
                        return 0;

                    AL.SourceStop(Source);
                    AL.DeleteSource(Source);
                }
                State = AudioOutState.Stopped;
            }

            return 0;
        }

        public static long AppendAudioOutBuffer(ServiceCtx Context)
        {
            long BufferId = Context.RequestData.ReadInt64();

            KeysQueue.Enqueue(BufferId);

            byte[] AudioOutBuffer = AMemoryHelper.ReadBytes(Context.Memory, Context.Request.SendBuff[0].Position, 0x28);
            using (MemoryStream MS = new MemoryStream(AudioOutBuffer))
            {
                BinaryReader Reader = new BinaryReader(MS);
                long PointerToSampleDataPointer = Reader.ReadInt64();
                long PointerToSampleData = Reader.ReadInt64();
                long CapacitySampleBuffer = Reader.ReadInt64();
                long SizeDataSampleBuffer = Reader.ReadInt64();
                long Unknown = Reader.ReadInt64();

                byte[] AudioSampleBuffer = AMemoryHelper.ReadBytes(Context.Memory, PointerToSampleData, (int)SizeDataSampleBuffer);

                if (OpenALInstalled)
                {
                    if (AudioCtx == null) //Needed to call the instance of AudioContext()
                        return 0;

                    Buffer = AL.GenBuffer();
                    AL.BufferData(Buffer, ALFormat.Stereo16, AudioSampleBuffer, AudioSampleBuffer.Length, 48000);

                    Source = AL.GenSource();
                    AL.SourceQueueBuffer(Source, Buffer);
                }
            }

            return 0;
        }

        public static long RegisterBufferEvent(ServiceCtx Context)
        {
            int Handle = Context.Ns.Os.Handles.GenerateId(new HEvent());

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        public static long GetReleasedAudioOutBuffer(ServiceCtx Context)
        {
            long TempKey = 0;

            if (KeysQueue.Count > 0) TempKey = KeysQueue.Dequeue();

            AMemoryHelper.WriteBytes(Context.Memory, Context.Request.ReceiveBuff[0].Position, BitConverter.GetBytes(TempKey));

            Context.ResponseData.Write((int)TempKey);

            if (OpenALInstalled)
            {
                if (AudioCtx == null) //Needed to call the instance of AudioContext()
                    return 0;

                AL.SourcePlay(Source);
                int[] FreeBuffers = AL.SourceUnqueueBuffers(Source, 1);
                AL.DeleteBuffers(FreeBuffers);
            }

            return 0;
        }

        public static long ContainsAudioOutBuffer(ServiceCtx Context)
        {
            return 0;
        }

        public static long AppendAudioOutBuffer_ex(ServiceCtx Context)
        {
            return 0;
        }

        public static long GetReleasedAudioOutBuffer_ex(ServiceCtx Context)
        {
            return 0;
        }
    }
}
