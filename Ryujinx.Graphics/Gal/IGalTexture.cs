namespace Ryujinx.Graphics.Gal
{
    public interface IGalTexture
    {
        void LockCache();
        void UnlockCache();

        void Create(long Key, int Size, GalImage Image);

        void Create(long Key, byte[] Data, GalImage Image);

        bool TryGetImage(long Key, out GalImage Image);

        void Bind(long Key, int Index, GalImage Image);

        void SetSampler(GalTextureSampler Sampler);
    }
}