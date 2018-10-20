using ChocolArm64.Memory;
using Ryujinx.Audio.Adpcm;
using System;

namespace Ryujinx.HLE.HOS.Services.Aud.AudioRenderer
{
    class VoiceContext
    {
        private bool Acquired;
        private bool BufferReload;

        private int ResamplerFracPart;

        private int BufferIndex;
        private int Offset;

        public int SampleRate;
        public int ChannelsCount;

        public float Volume;

        public PlayState PlayState;

        public SampleFormat SampleFormat;

        public AdpcmDecoderContext AdpcmCtx;

        public WaveBuffer[] WaveBuffers;

        public VoiceOut OutStatus;

        private int[] Samples;

        public bool Playing => Acquired && PlayState == PlayState.Playing;

        public VoiceContext()
        {
            WaveBuffers = new WaveBuffer[4];
        }

        public void SetAcquireState(bool NewState)
        {
            if (Acquired && !NewState)
            {
                //Release.
                Reset();
            }

            Acquired = NewState;
        }

        private void Reset()
        {
            BufferReload = true;

            BufferIndex = 0;
            Offset      = 0;

            OutStatus.PlayedSamplesCount     = 0;
            OutStatus.PlayedWaveBuffersCount = 0;
            OutStatus.VoiceDropsCount        = 0;
        }

        public int[] GetBufferData(AMemory Memory, int MaxSamples, out int SamplesCount)
        {
            if (!Playing)
            {
                SamplesCount = 0;

                return null;
            }

            if (BufferReload)
            {
                BufferReload = false;

                UpdateBuffer(Memory);
            }

            WaveBuffer Wb = WaveBuffers[BufferIndex];

            int MaxSize = Samples.Length - Offset;

            int Size = MaxSamples * AudioConsts.HostChannelsCount;

            if (Size > MaxSize)
            {
                Size = MaxSize;
            }

            int[] Output = new int[Size];

            Array.Copy(Samples, Offset, Output, 0, Size);

            SamplesCount = Size / AudioConsts.HostChannelsCount;

            OutStatus.PlayedSamplesCount += SamplesCount;

            Offset += Size;

            if (Offset == Samples.Length)
            {
                Offset = 0;

                if (Wb.Looping == 0)
                {
                    SetBufferIndex((BufferIndex + 1) & 3);
                }

                OutStatus.PlayedWaveBuffersCount++;

                if (Wb.LastBuffer != 0)
                {
                    PlayState = PlayState.Paused;
                }
            }

            return Output;
        }

        private void UpdateBuffer(AMemory Memory)
        {
            //TODO: Implement conversion for formats other
            //than interleaved stereo (2 channels).
            //As of now, it assumes that HostChannelsCount == 2.
            WaveBuffer Wb = WaveBuffers[BufferIndex];

            if (Wb.Position == 0)
            {
                Samples = new int[0];

                return;
            }

            if (SampleFormat == SampleFormat.PcmInt16)
            {
                int SamplesCount = (int)(Wb.Size / (sizeof(short) * ChannelsCount));

                Samples = new int[SamplesCount * AudioConsts.HostChannelsCount];

                if (ChannelsCount == 1)
                {
                    for (int Index = 0; Index < SamplesCount; Index++)
                    {
                        short Sample = Memory.ReadInt16(Wb.Position + Index * 2);

                        Samples[Index * 2 + 0] = Sample;
                        Samples[Index * 2 + 1] = Sample;
                    }
                }
                else
                {
                    for (int Index = 0; Index < SamplesCount * 2; Index++)
                    {
                        Samples[Index] = Memory.ReadInt16(Wb.Position + Index * 2);
                    }
                }
            }
            else if (SampleFormat == SampleFormat.Adpcm)
            {
                byte[] Buffer = Memory.ReadBytes(Wb.Position, Wb.Size);

                Samples = AdpcmDecoder.Decode(Buffer, AdpcmCtx);
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (SampleRate != AudioConsts.HostSampleRate)
            {
                //TODO: We should keep the frames being discarded (see the 4 below)
                //on a buffer and include it on the next samples buffer, to allow
                //the resampler to do seamless interpolation between wave buffers.
                int SamplesCount = Samples.Length / AudioConsts.HostChannelsCount;

                SamplesCount = Math.Max(SamplesCount - 4, 0);

                Samples = Resampler.Resample2Ch(
                    Samples,
                    SampleRate,
                    AudioConsts.HostSampleRate,
                    SamplesCount,
                    ref ResamplerFracPart);
            }
        }

        public void SetBufferIndex(int Index)
        {
            BufferIndex = Index & 3;

            BufferReload = true;
        }
    }
}
