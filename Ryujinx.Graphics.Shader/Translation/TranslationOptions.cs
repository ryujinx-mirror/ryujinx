namespace Ryujinx.Graphics.Shader.Translation
{
    public readonly struct TranslationOptions
    {
        public TargetLanguage TargetLanguage { get; }
        public TargetApi TargetApi { get; }
        public TranslationFlags Flags { get; }

        public TranslationOptions(TargetLanguage targetLanguage, TargetApi targetApi, TranslationFlags flags)
        {
            TargetLanguage = targetLanguage;
            TargetApi = targetApi;
            Flags = flags;
        }
    }
}
