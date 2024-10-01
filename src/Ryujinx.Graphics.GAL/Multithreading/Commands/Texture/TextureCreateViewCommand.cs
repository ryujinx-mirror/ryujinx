using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Texture
{
    struct TextureCreateViewCommand : IGALCommand, IGALCommand<TextureCreateViewCommand>
    {
        public readonly CommandType CommandType => CommandType.TextureCreateView;
        private TableRef<ThreadedTexture> _texture;
        private TableRef<ThreadedTexture> _destination;
        private TextureCreateInfo _info;
        private int _firstLayer;
        private int _firstLevel;

        public void Set(TableRef<ThreadedTexture> texture, TableRef<ThreadedTexture> destination, TextureCreateInfo info, int firstLayer, int firstLevel)
        {
            _texture = texture;
            _destination = destination;
            _info = info;
            _firstLayer = firstLayer;
            _firstLevel = firstLevel;
        }

        public static void Run(ref TextureCreateViewCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            ThreadedTexture source = command._texture.Get(threaded);
            command._destination.Get(threaded).Base = source.Base.CreateView(command._info, command._firstLayer, command._firstLevel);
        }
    }
}
