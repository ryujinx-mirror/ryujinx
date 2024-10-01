using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Texture
{
    struct TextureGetDataSliceCommand : IGALCommand, IGALCommand<TextureGetDataSliceCommand>
    {
        public readonly CommandType CommandType => CommandType.TextureGetDataSlice;
        private TableRef<ThreadedTexture> _texture;
        private TableRef<ResultBox<PinnedSpan<byte>>> _result;
        private int _layer;
        private int _level;

        public void Set(TableRef<ThreadedTexture> texture, TableRef<ResultBox<PinnedSpan<byte>>> result, int layer, int level)
        {
            _texture = texture;
            _result = result;
            _layer = layer;
            _level = level;
        }

        public static void Run(ref TextureGetDataSliceCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            PinnedSpan<byte> result = command._texture.Get(threaded).Base.GetData(command._layer, command._level);

            command._result.Get(threaded).Result = result;
        }
    }
}
