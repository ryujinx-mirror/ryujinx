using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Settings.Factory
{
    [StructLayout(LayoutKind.Sequential, Size = 0x5A, Pack = 0x2)]
    struct SpeakerParameter
    {
        public ushort Version;
        public Array34<byte> Reserved;
        public ushort SpeakerHpf2A1;
        public ushort SpeakerHpf2A2;
        public ushort SpeakerHpf2H0;
        public ushort SpeakerEqInputVolume;
        public ushort SpeakerEqOutputVolume;
        public ushort SpeakerEqCtrl1;
        public ushort SpeakerEqCtrl2;
        public ushort SpeakerDrcAgcCtrl2;
        public ushort SpeakerDrcAgcCtrl3;
        public ushort SpeakerDrcAgcCtrl1;
        public ushort SpeakerAnalogVolume;
        public ushort HeadphoneAnalogVolume;
        public ushort SpeakerDigitalVolumeMin;
        public ushort SpeakerDigitalVolumeMax;
        public ushort HeadphoneDigitalVolumeMin;
        public ushort HeadphoneDigitalVolumeMax;
        public ushort MicFixedGain;
        public ushort MicVariableVolumeMin;
        public ushort MicVariableVolumeMax;
        public Array16<byte> Reserved2;
    }
}
