using ARMeilleure.Memory;
using Ryujinx.Audio.Adpcm;
using System;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    class VoiceContext
    {
        private bool _acquired;
        private bool _bufferReload;

        private int _resamplerFracPart;

        private int _bufferIndex;
        private int _offset;

        public int SampleRate    { get; set; }
        public int ChannelsCount { get; set; }

        public float Volume { get; set; }

        public PlayState PlayState { get; set; }

        public SampleFormat SampleFormat { get; set; }

        public AdpcmDecoderContext AdpcmCtx { get; set; }

        public WaveBuffer[] WaveBuffers { get; }

        public WaveBuffer CurrentWaveBuffer => WaveBuffers[_bufferIndex];

        private VoiceOut _outStatus;

        public VoiceOut OutStatus => _outStatus;

        private int[] _samples;

        public bool Playing => _acquired && PlayState == PlayState.Playing;

        public VoiceContext()
        {
            WaveBuffers = new WaveBuffer[4];
        }

        public void SetAcquireState(bool newState)
        {
            if (_acquired && !newState)
            {
                // Release.
                Reset();
            }

            _acquired = newState;
        }

        private void Reset()
        {
            _bufferReload = true;

            _bufferIndex = 0;
            _offset      = 0;

            _outStatus.PlayedSamplesCount     = 0;
            _outStatus.PlayedWaveBuffersCount = 0;
            _outStatus.VoiceDropsCount        = 0;
        }

        public int[] GetBufferData(MemoryManager memory, int maxSamples, out int samplesCount)
        {
            if (!Playing)
            {
                samplesCount = 0;

                return null;
            }

            if (_bufferReload)
            {
                _bufferReload = false;

                UpdateBuffer(memory);
            }

            WaveBuffer wb = WaveBuffers[_bufferIndex];

            int maxSize = _samples.Length - _offset;

            int size = maxSamples * AudioRendererConsts.HostChannelsCount;

            if (size > maxSize)
            {
                size = maxSize;
            }

            int[] output = new int[size];

            Array.Copy(_samples, _offset, output, 0, size);

            samplesCount = size / AudioRendererConsts.HostChannelsCount;

            _outStatus.PlayedSamplesCount += samplesCount;

            _offset += size;

            if (_offset == _samples.Length)
            {
                _offset = 0;

                if (wb.Looping == 0)
                {
                    SetBufferIndex(_bufferIndex + 1);
                }

                _outStatus.PlayedWaveBuffersCount++;

                if (wb.LastBuffer != 0)
                {
                    PlayState = PlayState.Paused;
                }
            }

            return output;
        }

        private void UpdateBuffer(MemoryManager memory)
        {
            // TODO: Implement conversion for formats other
            // than interleaved stereo (2 channels).
            // As of now, it assumes that HostChannelsCount == 2.
            WaveBuffer wb = WaveBuffers[_bufferIndex];

            if (wb.Position == 0)
            {
                _samples = new int[0];

                return;
            }

            if (SampleFormat == SampleFormat.PcmInt16)
            {
                int samplesCount = (int)(wb.Size / (sizeof(short) * ChannelsCount));

                _samples = new int[samplesCount * AudioRendererConsts.HostChannelsCount];

                if (ChannelsCount == 1)
                {
                    for (int index = 0; index < samplesCount; index++)
                    {
                        short sample = memory.ReadInt16(wb.Position + index * 2);

                        _samples[index * 2 + 0] = sample;
                        _samples[index * 2 + 1] = sample;
                    }
                }
                else
                {
                    for (int index = 0; index < samplesCount * 2; index++)
                    {
                        _samples[index] = memory.ReadInt16(wb.Position + index * 2);
                    }
                }
            }
            else if (SampleFormat == SampleFormat.Adpcm)
            {
                byte[] buffer = memory.ReadBytes(wb.Position, wb.Size);

                _samples = AdpcmDecoder.Decode(buffer, AdpcmCtx);
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (SampleRate != AudioRendererConsts.HostSampleRate)
            {
                // TODO: We should keep the frames being discarded (see the 4 below)
                // on a buffer and include it on the next samples buffer, to allow
                // the resampler to do seamless interpolation between wave buffers.
                int samplesCount = _samples.Length / AudioRendererConsts.HostChannelsCount;

                samplesCount = Math.Max(samplesCount - 4, 0);

                _samples = Resampler.Resample2Ch(
                    _samples,
                    SampleRate,
                    AudioRendererConsts.HostSampleRate,
                    samplesCount,
                    ref _resamplerFracPart);
            }
        }

        public void SetBufferIndex(int index)
        {
            _bufferIndex = index & 3;

            _bufferReload = true;
        }
    }
}