using System.Text.Json.Serialization;

namespace Ryujinx.UI.Common.Models.Github
{
    [JsonSerializable(typeof(GithubReleasesJsonResponse), GenerationMode = JsonSourceGenerationMode.Metadata)]
    public partial class GithubReleasesJsonSerializerContext : JsonSerializerContext
    {
    }
}
