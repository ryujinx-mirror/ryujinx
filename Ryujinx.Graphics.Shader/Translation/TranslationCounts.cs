namespace Ryujinx.Graphics.Shader.Translation
{
    public class TranslationCounts
    {
        public int UniformBuffersCount { get; private set; }
        public int StorageBuffersCount { get; private set; }
        public int TexturesCount { get; private set; }
        public int ImagesCount { get; private set; }

        internal int IncrementUniformBuffersCount()
        {
            return UniformBuffersCount++;
        }

        internal int IncrementStorageBuffersCount()
        {
            return StorageBuffersCount++;
        }

        internal int IncrementTexturesCount()
        {
            return TexturesCount++;
        }

        internal int IncrementImagesCount()
        {
            return ImagesCount++;
        }
    }
}
