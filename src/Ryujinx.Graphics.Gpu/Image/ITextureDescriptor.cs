namespace Ryujinx.Graphics.Gpu.Image
{
    interface ITextureDescriptor
    {
        public uint UnpackFormat();
        public TextureTarget UnpackTextureTarget();
        public bool UnpackSrgb();
        public bool UnpackTextureCoordNormalized();
    }
}
