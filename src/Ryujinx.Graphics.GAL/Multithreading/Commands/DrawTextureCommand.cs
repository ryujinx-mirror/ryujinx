using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct DrawTextureCommand : IGALCommand, IGALCommand<DrawTextureCommand>
    {
        public readonly CommandType CommandType => CommandType.DrawTexture;
        private TableRef<ITexture> _texture;
        private TableRef<ISampler> _sampler;
        private Extents2DF _srcRegion;
        private Extents2DF _dstRegion;

        public void Set(TableRef<ITexture> texture, TableRef<ISampler> sampler, Extents2DF srcRegion, Extents2DF dstRegion)
        {
            _texture = texture;
            _sampler = sampler;
            _srcRegion = srcRegion;
            _dstRegion = dstRegion;
        }

        public static void Run(ref DrawTextureCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.DrawTexture(
                command._texture.GetAs<ThreadedTexture>(threaded)?.Base,
                command._sampler.GetAs<ThreadedSampler>(threaded)?.Base,
                command._srcRegion,
                command._dstRegion);
        }
    }
}
