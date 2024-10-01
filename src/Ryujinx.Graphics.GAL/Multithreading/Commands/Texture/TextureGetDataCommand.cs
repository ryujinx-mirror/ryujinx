using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Texture
{
    struct TextureGetDataCommand : IGALCommand, IGALCommand<TextureGetDataCommand>
    {
        public readonly CommandType CommandType => CommandType.TextureGetData;
        private TableRef<ThreadedTexture> _texture;
        private TableRef<ResultBox<PinnedSpan<byte>>> _result;

        public void Set(TableRef<ThreadedTexture> texture, TableRef<ResultBox<PinnedSpan<byte>>> result)
        {
            _texture = texture;
            _result = result;
        }

        public static void Run(ref TextureGetDataCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            PinnedSpan<byte> result = command._texture.Get(threaded).Base.GetData();

            command._result.Get(threaded).Result = result;
        }
    }
}
