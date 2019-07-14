using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Aud.AudioRenderer
{
    [StructLayout(LayoutKind.Sequential, Size = 0x170, Pack = 1)]
    struct VoiceIn
    {
        public int VoiceSlot;
        public int NodeId;

        public byte FirstUpdate;
        public byte Acquired;

        public PlayState PlayState;

        public SampleFormat SampleFormat;

        public int SampleRate;

        public int Priority;

        public int Unknown14;

        public int ChannelsCount;

        public float Pitch;
        public float Volume;

        public BiquadFilter BiquadFilter0;
        public BiquadFilter BiquadFilter1;

        public int AppendedWaveBuffersCount;

        public int BaseWaveBufferIndex;

        public int Unknown44;

        public long AdpcmCoeffsPosition;
        public long AdpcmCoeffsSize;

        public int VoiceDestination;
        public int Padding;

        public WaveBuffer WaveBuffer0;
        public WaveBuffer WaveBuffer1;
        public WaveBuffer WaveBuffer2;
        public WaveBuffer WaveBuffer3;
    }
}