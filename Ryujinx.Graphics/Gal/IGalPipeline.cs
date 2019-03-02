namespace Ryujinx.Graphics.Gal
{
    public interface IGalPipeline
    {
        void Bind(GalPipelineState State);
        void Unbind(GalPipelineState State);

        void ResetDepthMask();
        void ResetColorMask(int Index);
    }
}