using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetTextureCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.SetTexture;
        private int _binding;
        private TableRef<ITexture> _texture;

        public void Set(int binding, TableRef<ITexture> texture)
        {
            _binding = binding;
            _texture = texture;
        }

        public static void Run(ref SetTextureCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetTexture(command._binding, command._texture.GetAs<ThreadedTexture>(threaded)?.Base);
        }
    }
}
