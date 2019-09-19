using ARMeilleure.Memory;
using Ryujinx.Audio;
using Ryujinx.Audio.Adpcm;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    class IAudioRenderer : IpcService, IDisposable
    {
        // This is the amount of samples that are going to be appended
        // each time that RequestUpdateAudioRenderer is called. Ideally,
        // this value shouldn't be neither too small (to avoid the player
        // starving due to running out of samples) or too large (to avoid
        // high latency).
        private const int MixBufferSamplesCount = 960;

        private KEvent _updateEvent;

        private IMemoryManager _memory;

        private IAalOutput _audioOut;

        private AudioRendererParameter _params;

        private MemoryPoolContext[] _memoryPools;

        private VoiceContext[] _voices;

        private int _track;

        private PlayState _playState;

        public IAudioRenderer(
            Horizon                system,
            IMemoryManager         memory,
            IAalOutput             audioOut,
            AudioRendererParameter Params)
        {
            _updateEvent = new KEvent(system);

            _memory   = memory;
            _audioOut = audioOut;
            _params   = Params;

            _track = audioOut.OpenTrack(
                AudioRendererConsts.HostSampleRate,
                AudioRendererConsts.HostChannelsCount,
                AudioCallback);

            _memoryPools = CreateArray<MemoryPoolContext>(Params.EffectCount + Params.VoiceCount * 4);

            _voices = CreateArray<VoiceContext>(Params.VoiceCount);

            InitializeAudioOut();

            _playState = PlayState.Stopped;
        }

        [Command(0)]
        // GetSampleRate() -> u32
        public ResultCode GetSampleRate(ServiceCtx context)
        {
            context.ResponseData.Write(_params.SampleRate);

            return ResultCode.Success;
        }

        [Command(1)]
        // GetSampleCount() -> u32
        public ResultCode GetSampleCount(ServiceCtx context)
        {
            context.ResponseData.Write(_params.SampleCount);

            return ResultCode.Success;
        }

        [Command(2)]
        // GetMixBufferCount() -> u32
        public ResultCode GetMixBufferCount(ServiceCtx context)
        {
            context.ResponseData.Write(_params.SubMixCount);

            return ResultCode.Success;
        }

        [Command(3)]
        // GetState() -> u32
        public ResultCode GetState(ServiceCtx context)
        {
            context.ResponseData.Write((int)_playState);

            Logger.PrintStub(LogClass.ServiceAudio, new { State = Enum.GetName(typeof(PlayState), _playState) });

            return ResultCode.Success;
        }

        private void AudioCallback()
        {
            _updateEvent.ReadableEvent.Signal();
        }

        private static T[] CreateArray<T>(int size) where T : new()
        {
            T[] output = new T[size];

            for (int index = 0; index < size; index++)
            {
                output[index] = new T();
            }

            return output;
        }

        private void InitializeAudioOut()
        {
            AppendMixedBuffer(0);
            AppendMixedBuffer(1);
            AppendMixedBuffer(2);

            _audioOut.Start(_track);
        }

        [Command(4)]
        // RequestUpdateAudioRenderer(buffer<nn::audio::detail::AudioRendererUpdateDataHeader, 5>)
        // -> (buffer<nn::audio::detail::AudioRendererUpdateDataHeader, 6>, buffer<nn::audio::detail::AudioRendererUpdateDataHeader, 6>)
        public ResultCode RequestUpdateAudioRenderer(ServiceCtx context)
        {
            long outputPosition = context.Request.ReceiveBuff[0].Position;
            long outputSize     = context.Request.ReceiveBuff[0].Size;

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, (int)outputSize);

            long inputPosition = context.Request.SendBuff[0].Position;

            StructReader reader = new StructReader(context.Memory, inputPosition);
            StructWriter writer = new StructWriter(context.Memory, outputPosition);

            UpdateDataHeader inputHeader = reader.Read<UpdateDataHeader>();

            BehaviorInfo behaviorInfo = new BehaviorInfo();

            behaviorInfo.SetUserLibRevision(inputHeader.Revision);

            reader.Read<BehaviorIn>(inputHeader.BehaviorSize);

            MemoryPoolIn[] memoryPoolsIn = reader.Read<MemoryPoolIn>(inputHeader.MemoryPoolSize);

            for (int index = 0; index < memoryPoolsIn.Length; index++)
            {
                MemoryPoolIn memoryPool = memoryPoolsIn[index];

                if (memoryPool.State == MemoryPoolState.RequestAttach)
                {
                    _memoryPools[index].OutStatus.State = MemoryPoolState.Attached;
                }
                else if (memoryPool.State == MemoryPoolState.RequestDetach)
                {
                    _memoryPools[index].OutStatus.State = MemoryPoolState.Detached;
                }
            }

            reader.Read<VoiceChannelResourceIn>(inputHeader.VoiceResourceSize);

            VoiceIn[] voicesIn = reader.Read<VoiceIn>(inputHeader.VoiceSize);

            for (int index = 0; index < voicesIn.Length; index++)
            {
                VoiceIn voice = voicesIn[index];

                VoiceContext voiceCtx = _voices[index];

                voiceCtx.SetAcquireState(voice.Acquired != 0);

                if (voice.Acquired == 0)
                {
                    continue;
                }

                if (voice.FirstUpdate != 0)
                {
                    voiceCtx.AdpcmCtx = GetAdpcmDecoderContext(
                        voice.AdpcmCoeffsPosition,
                        voice.AdpcmCoeffsSize);

                    voiceCtx.SampleFormat  = voice.SampleFormat;
                    voiceCtx.SampleRate    = voice.SampleRate;
                    voiceCtx.ChannelsCount = voice.ChannelsCount;

                    voiceCtx.SetBufferIndex(voice.BaseWaveBufferIndex);
                }

                voiceCtx.WaveBuffers[0] = voice.WaveBuffer0;
                voiceCtx.WaveBuffers[1] = voice.WaveBuffer1;
                voiceCtx.WaveBuffers[2] = voice.WaveBuffer2;
                voiceCtx.WaveBuffers[3] = voice.WaveBuffer3;
                voiceCtx.Volume         = voice.Volume;
                voiceCtx.PlayState      = voice.PlayState;
            }

            UpdateAudio();

            UpdateDataHeader outputHeader = new UpdateDataHeader();

            int updateHeaderSize = Marshal.SizeOf<UpdateDataHeader>();

            outputHeader.Revision               = AudioRendererConsts.RevMagic;
            outputHeader.BehaviorSize           = 0xb0;
            outputHeader.MemoryPoolSize         = (_params.EffectCount + _params.VoiceCount * 4) * 0x10;
            outputHeader.VoiceSize              = _params.VoiceCount  * 0x10;
            outputHeader.EffectSize             = _params.EffectCount * 0x10;
            outputHeader.SinkSize               = _params.SinkCount   * 0x20;
            outputHeader.PerformanceManagerSize = 0x10;

            if (behaviorInfo.IsElapsedFrameCountSupported())
            {
                outputHeader.ElapsedFrameCountInfoSize = 0x10;
            }

            outputHeader.TotalSize = updateHeaderSize                    +
                                     outputHeader.BehaviorSize           +
                                     outputHeader.MemoryPoolSize         +
                                     outputHeader.VoiceSize              +
                                     outputHeader.EffectSize             +
                                     outputHeader.SinkSize               +
                                     outputHeader.PerformanceManagerSize +
                                     outputHeader.ElapsedFrameCountInfoSize;

            writer.Write(outputHeader);

            foreach (MemoryPoolContext memoryPool in _memoryPools)
            {
                writer.Write(memoryPool.OutStatus);
            }

            foreach (VoiceContext voice in _voices)
            {
                writer.Write(voice.OutStatus);
            }

            return ResultCode.Success;
        }

        [Command(5)]
        // Start()
        public ResultCode StartAudioRenderer(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAudio);

            _playState = PlayState.Playing;

            return ResultCode.Success;
        }

        [Command(6)]
        // Stop()
        public ResultCode StopAudioRenderer(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAudio);

            _playState = PlayState.Stopped;

            return ResultCode.Success;
        }

        [Command(7)]
        // QuerySystemEvent() -> handle<copy, event>
        public ResultCode QuerySystemEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_updateEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return ResultCode.Success;
        }

        private AdpcmDecoderContext GetAdpcmDecoderContext(long position, long size)
        {
            if (size == 0)
            {
                return null;
            }

            AdpcmDecoderContext context = new AdpcmDecoderContext
            {
                Coefficients = new short[size >> 1]
            };

            for (int offset = 0; offset < size; offset += 2)
            {
                context.Coefficients[offset >> 1] = _memory.ReadInt16(position + offset);
            }

            return context;
        }

        private void UpdateAudio()
        {
            long[] released = _audioOut.GetReleasedBuffers(_track, 2);

            for (int index = 0; index < released.Length; index++)
            {
                AppendMixedBuffer(released[index]);
            }
        }

        private void AppendMixedBuffer(long tag)
        {
            int[] mixBuffer = new int[MixBufferSamplesCount * AudioRendererConsts.HostChannelsCount];

            foreach (VoiceContext voice in _voices)
            {
                if (!voice.Playing || voice.CurrentWaveBuffer.Size == 0)
                {
                    continue;
                }

                int   outOffset      = 0;
                int   pendingSamples = MixBufferSamplesCount;
                float volume         = voice.Volume;

                while (pendingSamples > 0)
                {
                    int[] samples = voice.GetBufferData(_memory, pendingSamples, out int returnedSamples);

                    if (returnedSamples == 0)
                    {
                        break;
                    }

                    pendingSamples -= returnedSamples;

                    for (int offset = 0; offset < samples.Length; offset++)
                    {
                        mixBuffer[outOffset++] += (int)(samples[offset] * voice.Volume);
                    }
                }
            }

            _audioOut.AppendBuffer(_track, tag, GetFinalBuffer(mixBuffer));
        }

        private unsafe static short[] GetFinalBuffer(int[] buffer)
        {
            short[] output = new short[buffer.Length];

            int offset = 0;

            // Perform Saturation using SSE2 if supported
            if (Sse2.IsSupported)
            {
                fixed (int*   inptr  = buffer)
                fixed (short* outptr = output)
                {
                    for (; offset + 32 <= buffer.Length; offset += 32)
                    {
                        // Unroll the loop a little to ensure the CPU pipeline
                        // is always full.
                        Vector128<int> block1A = Sse2.LoadVector128(inptr + offset + 0);
                        Vector128<int> block1B = Sse2.LoadVector128(inptr + offset + 4);

                        Vector128<int> block2A = Sse2.LoadVector128(inptr + offset +  8);
                        Vector128<int> block2B = Sse2.LoadVector128(inptr + offset + 12);

                        Vector128<int> block3A = Sse2.LoadVector128(inptr + offset + 16);
                        Vector128<int> block3B = Sse2.LoadVector128(inptr + offset + 20);

                        Vector128<int> block4A = Sse2.LoadVector128(inptr + offset + 24);
                        Vector128<int> block4B = Sse2.LoadVector128(inptr + offset + 28);

                        Vector128<short> output1 = Sse2.PackSignedSaturate(block1A, block1B);
                        Vector128<short> output2 = Sse2.PackSignedSaturate(block2A, block2B);
                        Vector128<short> output3 = Sse2.PackSignedSaturate(block3A, block3B);
                        Vector128<short> output4 = Sse2.PackSignedSaturate(block4A, block4B);

                        Sse2.Store(outptr + offset +  0, output1);
                        Sse2.Store(outptr + offset +  8, output2);
                        Sse2.Store(outptr + offset + 16, output3);
                        Sse2.Store(outptr + offset + 24, output4);
                    }
                }
            }

            // Process left overs
            for (; offset < buffer.Length; offset++)
            {
                output[offset] = DspUtils.Saturate(buffer[offset]);
            }

            return output;
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