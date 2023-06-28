using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct CreateTextureCommand : IGALCommand, IGALCommand<CreateTextureCommand>
    {
        public readonly CommandType CommandType => CommandType.CreateTexture;
        private TableRef<ThreadedTexture> _texture;
        private TextureCreateInfo _info;
        private float _scale;

        public void Set(TableRef<ThreadedTexture> texture, TextureCreateInfo info, float scale)
        {
            _texture = texture;
            _info = info;
            _scale = scale;
        }

        public static void Run(ref CreateTextureCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            command._texture.Get(threaded).Base = renderer.CreateTexture(command._info, command._scale);
        }
    }
}
