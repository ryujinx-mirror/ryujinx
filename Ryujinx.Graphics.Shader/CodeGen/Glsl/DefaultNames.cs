namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    static class DefaultNames
    {
        public const string LocalNamePrefix = "temp";

        public const string SamplerNamePrefix = "tex";
        public const string ImageNamePrefix   = "img";

        public const string IAttributePrefix = "in_attr";
        public const string OAttributePrefix = "out_attr";

        public const string StorageNamePrefix = "s";
        public const string StorageNameSuffix = "data";

        public const string UniformNamePrefix = "c";
        public const string UniformNameSuffix = "data";

        public const string LocalMemoryName = "local_mem";

        public const string UndefinedName = "undef";
    }
}