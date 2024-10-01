using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Codec
{
    static class CodecResult
    {
        private const int ModuleId = 111;

        public static Result InvalidLength => new(ModuleId, 3);
        public static Result OpusBadArg => new(ModuleId, 130);
        public static Result OpusInvalidPacket => new(ModuleId, 133);
        public static Result InvalidNumberOfStreams => new(ModuleId, 1000);
        public static Result InvalidSampleRate => new(ModuleId, 1001);
        public static Result InvalidChannelCount => new(ModuleId, 1002);
    }
}
