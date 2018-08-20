namespace Ryujinx.Graphics.Gal
{
    public interface IGalTexture
    {
        void LockCache();
        void UnlockCache();

        void Create(long Key, byte[] Data, GalImage Image);

        void CreateFb(long Key, long Size, GalImage Image);

        bool TryGetCachedTexture(long Key, long DataSize, out GalImage Image);

        void Bind(long Key, int Index);

        void SetSampler(GalTextureSampler Sampler);
    }
}