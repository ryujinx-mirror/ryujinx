using Ryujinx.Common.Utilities;

namespace Ryujinx.Ui.Common.Configuration
{
    internal static class ConfigurationFileFormatSettings
    {
        public static readonly ConfigurationJsonSerializerContext SerializerContext = new(JsonHelper.GetDefaultSerializerOptions());
    }
}
