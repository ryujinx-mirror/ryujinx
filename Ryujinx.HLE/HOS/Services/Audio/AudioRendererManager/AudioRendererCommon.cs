namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    static class AudioRendererCommon
    {
        public static bool CheckValidRevision(AudioRendererParameter parameters)      => GetRevisionVersion(parameters.Revision) <= AudioRendererConsts.Revision;
        public static bool CheckFeatureSupported(int revision, int supportedRevision) => revision >= supportedRevision;
        public static int  GetRevisionVersion(int revision)                           => (revision - AudioRendererConsts.Rev0Magic) >> 24;
    }
}