namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    class BehaviorInfo
    {
        private const int _revision = AudioRendererConsts.Revision;

        private int _userRevision = 0;

        public BehaviorInfo()
        {
            /* TODO: this class got a size of 0xC0
                     0x00 - uint - Internal Revision
                     0x04 - uint - User Revision
                     0x08 - ... unknown ...
            */
        }

        public bool IsSplitterSupported()                  => AudioRendererCommon.CheckFeatureSupported(_userRevision, SupportTags.Splitter);
        public bool IsSplitterBugFixed()                   => AudioRendererCommon.CheckFeatureSupported(_userRevision, SupportTags.SplitterBugFix);
        public bool IsVariadicCommandBufferSizeSupported() => AudioRendererCommon.CheckFeatureSupported(_userRevision, SupportTags.VariadicCommandBufferSize);
        public bool IsElapsedFrameCountSupported()         => AudioRendererCommon.CheckFeatureSupported(_userRevision, SupportTags.ElapsedFrameCount);

        public int GetPerformanceMetricsDataFormat() => AudioRendererCommon.CheckFeatureSupported(_userRevision, SupportTags.PerformanceMetricsDataFormatVersion2) ? 2 : 1;

        public void SetUserLibRevision(int revision)
        {
            _userRevision = AudioRendererCommon.GetRevisionVersion(revision);
        }
    }
}