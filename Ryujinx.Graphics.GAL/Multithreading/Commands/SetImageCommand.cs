using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetImageCommand : IGALCommand, IGALCommand<SetImageCommand>
    {
        public CommandType CommandType => CommandType.SetImage;
        private int _binding;
        private TableRef<ITexture> _texture;
        private Format _imageFormat;

        public void Set(int binding, TableRef<ITexture> texture, Format imageFormat)
        {
            _binding = binding;
            _texture = texture;
            _imageFormat = imageFormat;
        }

        public static void Run(ref SetImageCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetImage(command._binding, command._texture.GetAs<ThreadedTexture>(threaded)?.Base, command._imageFormat);
        }
    }
}
