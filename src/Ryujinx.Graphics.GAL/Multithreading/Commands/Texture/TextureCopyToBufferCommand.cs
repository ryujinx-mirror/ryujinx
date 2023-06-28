using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Texture
{
    struct TextureCopyToBufferCommand : IGALCommand, IGALCommand<TextureCopyToBufferCommand>
    {
        public readonly CommandType CommandType => CommandType.TextureCopyToBuffer;
        private TableRef<ThreadedTexture> _texture;
        private BufferRange _range;
        private int _layer;
        private int _level;
        private int _stride;

        public void Set(TableRef<ThreadedTexture> texture, BufferRange range, int layer, int level, int stride)
        {
            _texture = texture;
            _range = range;
            _layer = layer;
            _level = level;
            _stride = stride;
        }

        public static void Run(ref TextureCopyToBufferCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            command._texture.Get(threaded).Base.CopyTo(threaded.Buffers.MapBufferRange(command._range), command._layer, command._level, command._stride);
        }
    }
}
