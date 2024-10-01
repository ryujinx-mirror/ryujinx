using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Texture
{
    struct TextureCopyToCommand : IGALCommand, IGALCommand<TextureCopyToCommand>
    {
        public readonly CommandType CommandType => CommandType.TextureCopyTo;
        private TableRef<ThreadedTexture> _texture;
        private TableRef<ThreadedTexture> _destination;
        private int _firstLayer;
        private int _firstLevel;

        public void Set(TableRef<ThreadedTexture> texture, TableRef<ThreadedTexture> destination, int firstLayer, int firstLevel)
        {
            _texture = texture;
            _destination = destination;
            _firstLayer = firstLayer;
            _firstLevel = firstLevel;
        }

        public static void Run(ref TextureCopyToCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            ThreadedTexture source = command._texture.Get(threaded);
            source.Base.CopyTo(command._destination.Get(threaded).Base, command._firstLayer, command._firstLevel);
        }
    }
}
