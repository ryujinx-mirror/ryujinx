namespace Ryujinx.Graphics.Gal
{
    public interface IGalTexture
    {
        void LockCache();
        void UnlockCache();

        void Create(long Key, byte[] Data, GalTexture Texture);

        bool TryGetCachedTexture(long Key, long DataSize, out GalTexture Texture);

        void Bind(long Key, int Index);

        void SetSampler(GalTextureSampler Sampler);
    }
}