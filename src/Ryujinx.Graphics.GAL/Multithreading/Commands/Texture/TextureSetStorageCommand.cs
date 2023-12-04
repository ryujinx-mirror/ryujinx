using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Texture
{
    struct TextureSetStorageCommand : IGALCommand, IGALCommand<TextureSetStorageCommand>
    {
        public readonly CommandType CommandType => CommandType.TextureSetStorage;
        private TableRef<ThreadedTexture> _texture;
        private BufferRange _storage;

        public void Set(TableRef<ThreadedTexture> texture, BufferRange storage)
        {
            _texture = texture;
            _storage = storage;
        }

        public static void Run(ref TextureSetStorageCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            command._texture.Get(threaded).Base.SetStorage(threaded.Buffers.MapBufferRange(command._storage));
        }
    }
}
