namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    static class CommandGenerator
    {
        public static long CalculateCommandBufferSize(AudioRendererParameter parameters)
        {
            return parameters.EffectCount                  * 0x840  +
                   parameters.SubMixCount                  * 0x5A38 +
                   parameters.SinkCount                    * 0x148  +
                   parameters.SplitterDestinationDataCount * 0x540  +
                   (parameters.SplitterCount * 0x68 + 0x2E0) * parameters.VoiceCount +
                   ((parameters.VoiceCount + parameters.SubMixCount + parameters.EffectCount + parameters.SinkCount + 0x65) << 6) +
                   0x3F8;
        }
    }
}