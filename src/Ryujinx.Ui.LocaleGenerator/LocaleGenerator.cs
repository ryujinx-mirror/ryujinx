using Microsoft.CodeAnalysis;
using System.Linq;

namespace Ryujinx.Ui.LocaleGenerator
{
    [Generator]
    public class LocaleGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var englishLocaleFile = context.AdditionalTextsProvider.Where(static x => x.Path.EndsWith("en_US.json"));

            IncrementalValuesProvider<string> contents = englishLocaleFile.Select((text, cancellationToken) => text.GetText(cancellationToken)!.ToString());

            context.RegisterSourceOutput(contents, (spc, content) =>
            {
                var lines = content.Split('\n').Where(x => x.Trim().StartsWith("\"")).Select(x => x.Split(':')[0].Trim().Replace("\"", ""));
                string enumSource = "namespace Ryujinx.Ava.Common.Locale;\n";
                enumSource += "internal enum LocaleKeys\n{\n";
                foreach (var line in lines)
                {
                    enumSource += $"    {line},\n";
                }
                enumSource += "}\n";

                spc.AddSource("LocaleKeys", enumSource);
            });
        }
    }
}
