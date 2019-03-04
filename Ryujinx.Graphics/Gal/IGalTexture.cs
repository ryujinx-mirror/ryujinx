namespace Ryujinx.Graphics.Gal
{
    public interface IGalTexture
    {
        void LockCache();
        void UnlockCache();

        void Create(long key, int size, GalImage image);

        void Create(long key, byte[] data, GalImage image);

        bool TryGetImage(long key, out GalImage image);

        void Bind(long key, int index, GalImage image);

        void SetSampler(GalImage image, GalTextureSampler sampler);
    }
}