using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;
using System.Buffers;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Texture
{
    struct TextureSetDataCommand : IGALCommand, IGALCommand<TextureSetDataCommand>
    {
        public readonly CommandType CommandType => CommandType.TextureSetData;
        private TableRef<ThreadedTexture> _texture;
        private TableRef<IMemoryOwner<byte>> _data;

        public void Set(TableRef<ThreadedTexture> texture, TableRef<IMemoryOwner<byte>> data)
        {
            _texture = texture;
            _data = data;
        }

        public static void Run(ref TextureSetDataCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            ThreadedTexture texture = command._texture.Get(threaded);
            texture.Base.SetData(command._data.Get(threaded));
        }
    }
}
