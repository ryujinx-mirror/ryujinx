using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;
using System.Buffers;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Texture
{
    struct TextureSetDataSliceCommand : IGALCommand, IGALCommand<TextureSetDataSliceCommand>
    {
        public readonly CommandType CommandType => CommandType.TextureSetDataSlice;
        private TableRef<ThreadedTexture> _texture;
        private TableRef<IMemoryOwner<byte>> _data;
        private int _layer;
        private int _level;

        public void Set(TableRef<ThreadedTexture> texture, TableRef<IMemoryOwner<byte>> data, int layer, int level)
        {
            _texture = texture;
            _data = data;
            _layer = layer;
            _level = level;
        }

        public static void Run(ref TextureSetDataSliceCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            ThreadedTexture texture = command._texture.Get(threaded);
            texture.Base.SetData(command._data.Get(threaded), command._layer, command._level);
        }
    }
}
