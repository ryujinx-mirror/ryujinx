using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    static class PerformanceManager
    {
        public static long GetRequiredBufferSizeForPerformanceMetricsPerFrame(BehaviorInfo behaviorInfo, AudioRendererParameter parameters)
        {
            int performanceMetricsDataFormat = behaviorInfo.GetPerformanceMetricsDataFormat();

            if (performanceMetricsDataFormat == 2)
            {
                return 24 * (parameters.VoiceCount  + 
                             parameters.EffectCount + 
                             parameters.SubMixCount + 
                             parameters.SinkCount   + 1) + 0x990;
            }

            if (performanceMetricsDataFormat != 1)
            {
                Logger.PrintWarning(LogClass.ServiceAudio, $"PerformanceMetricsDataFormat: {performanceMetricsDataFormat} is not supported!");
            }

            return (((parameters.VoiceCount  + 
                      parameters.EffectCount +
                      parameters.SubMixCount + 
                      parameters.SinkCount   + 1) << 32) >> 0x1C) + 0x658;
        }
    }
}