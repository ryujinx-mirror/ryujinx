namespace Ryujinx.Core.OsHle
{
    class SystemStateMgr
    {
        internal static string[] AudioOutputs = new string[]
        {
            "AudioTvOutput",
            "AudioStereoJackOutput",
            "AudioBuiltInSpeakerOutput"
        };

        public string ActiveAudioOutput { get; private set; }

        public SystemStateMgr()
        {
            SetAudioOutputAsBuiltInSpeaker();
        }

        public void SetAudioOutputAsTv()
        {
            ActiveAudioOutput = AudioOutputs[0];
        }

        public void SetAudioOutputAsStereoJack()
        {
            ActiveAudioOutput = AudioOutputs[1];
        }

        public void SetAudioOutputAsBuiltInSpeaker()
        {
            ActiveAudioOutput = AudioOutputs[2];
        }
    }
}