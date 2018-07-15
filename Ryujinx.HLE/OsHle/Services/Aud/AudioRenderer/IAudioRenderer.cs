using ChocolArm64.Memory;
using Ryujinx.Audio;
using Ryujinx.Audio.Adpcm;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Handles;
using Ryujinx.HLE.OsHle.Ipc;
using Ryujinx.HLE.OsHle.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.OsHle.Services.Aud.AudioRenderer
{
    class IAudioRenderer : IpcService, IDisposable
    {
        //This is the amount of samples that are going to be appended
        //each time that RequestUpdateAudioRenderer is called. Ideally,
        //this value shouldn't be neither too small (to avoid the player
        //starving due to running out of samples) or too large (to avoid
        //high latency).
        private const int MixBufferSamplesCount = 960;

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent UpdateEvent;

        private AMemory Memory;

        private IAalOutput AudioOut;

        private AudioRendererParameter Params;

        private MemoryPoolContext[] MemoryPools;

        private VoiceContext[] Voices;

        private int Track;

        public IAudioRenderer(AMemory Memory, IAalOutput AudioOut, AudioRendererParameter Params)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 4, RequestUpdateAudioRenderer },
                { 5, StartAudioRenderer         },
                { 6, StopAudioRenderer          },
                { 7, QuerySystemEvent           }
            };

            UpdateEvent = new KEvent();

            this.Memory   = Memory;
            this.AudioOut = AudioOut;
            this.Params   = Params;

            Track = AudioOut.OpenTrack(
                AudioConsts.HostSampleRate,
                AudioConsts.HostChannelsCount,
                AudioCallback);

            MemoryPools = CreateArray<MemoryPoolContext>(Params.EffectCount + Params.VoiceCount * 4);

            Voices = CreateArray<VoiceContext>(Params.VoiceCount);

            InitializeAudioOut();
        }

        private void AudioCallback()
        {
            UpdateEvent.WaitEvent.Set();
        }

        private static T[] CreateArray<T>(int Size) where T : new()
        {
            T[] Output = new T[Size];

            for (int Index = 0; Index < Size; Index++)
            {
                Output[Index] = new T();
            }

            return Output;
        }

        private void InitializeAudioOut()
        {
            AppendMixedBuffer(0);
            AppendMixedBuffer(1);
            AppendMixedBuffer(2);

            AudioOut.Start(Track);
        }

        public long RequestUpdateAudioRenderer(ServiceCtx Context)
        {
            long OutputPosition = Context.Request.ReceiveBuff[0].Position;
            long OutputSize     = Context.Request.ReceiveBuff[0].Size;

            AMemoryHelper.FillWithZeros(Context.Memory, OutputPosition, (int)OutputSize);

            long InputPosition = Context.Request.SendBuff[0].Position;

            StructReader Reader = new StructReader(Context.Memory, InputPosition);
            StructWriter Writer = new StructWriter(Context.Memory, OutputPosition);

            UpdateDataHeader InputHeader = Reader.Read<UpdateDataHeader>();

            Reader.Read<BehaviorIn>(InputHeader.BehaviorSize);

            MemoryPoolIn[] MemoryPoolsIn = Reader.Read<MemoryPoolIn>(InputHeader.MemoryPoolSize);

            for (int Index = 0; Index < MemoryPoolsIn.Length; Index++)
            {
                MemoryPoolIn MemoryPool = MemoryPoolsIn[Index];

                if (MemoryPool.State == MemoryPoolState.RequestAttach)
                {
                    MemoryPools[Index].OutStatus.State = MemoryPoolState.Attached;
                }
                else if (MemoryPool.State == MemoryPoolState.RequestDetach)
                {
                    MemoryPools[Index].OutStatus.State = MemoryPoolState.Detached;
                }
            }

            Reader.Read<VoiceChannelResourceIn>(InputHeader.VoiceResourceSize);

            VoiceIn[] VoicesIn = Reader.Read<VoiceIn>(InputHeader.VoiceSize);

            for (int Index = 0; Index < VoicesIn.Length; Index++)
            {
                VoiceIn Voice = VoicesIn[Index];

                VoiceContext VoiceCtx = Voices[Index];

                VoiceCtx.SetAcquireState(Voice.Acquired != 0);

                if (Voice.Acquired == 0)
                {
                    continue;
                }

                if (Voice.FirstUpdate != 0)
                {
                    VoiceCtx.AdpcmCtx = GetAdpcmDecoderContext(
                        Voice.AdpcmCoeffsPosition,
                        Voice.AdpcmCoeffsSize);

                    VoiceCtx.SampleFormat  = Voice.SampleFormat;
                    VoiceCtx.SampleRate    = Voice.SampleRate;
                    VoiceCtx.ChannelsCount = Voice.ChannelsCount;

                    VoiceCtx.SetBufferIndex(Voice.BaseWaveBufferIndex);
                }

                VoiceCtx.WaveBuffers[0] = Voice.WaveBuffer0;
                VoiceCtx.WaveBuffers[1] = Voice.WaveBuffer1;
                VoiceCtx.WaveBuffers[2] = Voice.WaveBuffer2;
                VoiceCtx.WaveBuffers[3] = Voice.WaveBuffer3;
                VoiceCtx.Volume         = Voice.Volume;
                VoiceCtx.PlayState      = Voice.PlayState;
            }

            UpdateAudio();

            UpdateDataHeader OutputHeader = new UpdateDataHeader();

            int UpdateHeaderSize = Marshal.SizeOf<UpdateDataHeader>();

            OutputHeader.Revision               = IAudioRendererManager.RevMagic;
            OutputHeader.BehaviorSize           = 0xb0;
            OutputHeader.MemoryPoolSize         = (Params.EffectCount + Params.VoiceCount * 4) * 0x10;
            OutputHeader.VoiceSize              = Params.VoiceCount  * 0x10;
            OutputHeader.EffectSize             = Params.EffectCount * 0x10;
            OutputHeader.SinkSize               = Params.SinkCount   * 0x20;
            OutputHeader.PerformanceManagerSize = 0x10;
            OutputHeader.TotalSize              = UpdateHeaderSize             +
                                                  OutputHeader.BehaviorSize    +
                                                  OutputHeader.MemoryPoolSize +
                                                  OutputHeader.VoiceSize      +
                                                  OutputHeader.EffectSize     +
                                                  OutputHeader.SinkSize       +
                                                  OutputHeader.PerformanceManagerSize;

            Writer.Write(OutputHeader);

            foreach (MemoryPoolContext MemoryPool in MemoryPools)
            {
                Writer.Write(MemoryPool.OutStatus);
            }

            foreach (VoiceContext Voice in Voices)
            {
                Writer.Write(Voice.OutStatus);
            }

            return 0;
        }

        public long StartAudioRenderer(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long StopAudioRenderer(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long QuerySystemEvent(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(UpdateEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        private AdpcmDecoderContext GetAdpcmDecoderContext(long Position, long Size)
        {
            if (Size == 0)
            {
                return null;
            }

            AdpcmDecoderContext Context = new AdpcmDecoderContext();

            Context.Coefficients = new short[Size >> 1];

            for (int Offset = 0; Offset < Size; Offset += 2)
            {
                Context.Coefficients[Offset >> 1] = Memory.ReadInt16(Position + Offset);
            }

            return Context;
        }

        private void UpdateAudio()
        {
            long[] Released = AudioOut.GetReleasedBuffers(Track, 2);

            for (int Index = 0; Index < Released.Length; Index++)
            {
                AppendMixedBuffer(Released[Index]);
            }
        }

        private void AppendMixedBuffer(long Tag)
        {
            int[] MixBuffer = new int[MixBufferSamplesCount * AudioConsts.HostChannelsCount];

            foreach (VoiceContext Voice in Voices)
            {
                if (!Voice.Playing)
                {
                    continue;
                }

                int OutOffset = 0;

                int PendingSamples = MixBufferSamplesCount;

                while (PendingSamples > 0)
                {
                    int[] Samples = Voice.GetBufferData(Memory, PendingSamples, out int ReturnedSamples);

                    if (ReturnedSamples == 0)
                    {
                        break;
                    }

                    PendingSamples -= ReturnedSamples;

                    for (int Offset = 0; Offset < Samples.Length; Offset++)
                    {
                        int Sample = (int)(Samples[Offset] * Voice.Volume);

                        MixBuffer[OutOffset++] += Sample;
                    }
                }
            }

            AudioOut.AppendBuffer(Track, Tag, GetFinalBuffer(MixBuffer));
        }

        private static short[] GetFinalBuffer(int[] Buffer)
        {
            short[] Output = new short[Buffer.Length];

            for (int Offset = 0; Offset < Buffer.Length; Offset++)
            {
                Output[Offset] = DspUtils.Saturate(Buffer[Offset]);
            }

            return Output;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                UpdateEvent.Dispose();
            }
        }
    }
}
