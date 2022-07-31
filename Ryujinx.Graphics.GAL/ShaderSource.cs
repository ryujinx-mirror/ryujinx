using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.GAL
{
    public struct ShaderSource
    {
        public string Code { get; }
        public byte[] BinaryCode { get; }
        public ShaderBindings Bindings { get; }
        public ShaderStage Stage { get; }
        public TargetLanguage Language { get; }

        public ShaderSource(string code, byte[] binaryCode, ShaderBindings bindings, ShaderStage stage, TargetLanguage language)
        {
            Code = code;
            BinaryCode = binaryCode;
            Bindings = bindings;
            Stage = stage;
            Language = language;
        }

        public ShaderSource(string code, ShaderBindings bindings, ShaderStage stage, TargetLanguage language) : this(code, null, bindings, stage, language)
        {
        }

        public ShaderSource(byte[] binaryCode, ShaderBindings bindings, ShaderStage stage, TargetLanguage language) : this(null, binaryCode, bindings, stage, language)
        {
        }
    }
}