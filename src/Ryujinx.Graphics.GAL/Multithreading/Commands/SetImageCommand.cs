using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetImageCommand : IGALCommand, IGALCommand<SetImageCommand>
    {
        public readonly CommandType CommandType => CommandType.SetImage;
        private ShaderStage _stage;
        private int _binding;
        private TableRef<ITexture> _texture;

        public void Set(ShaderStage stage, int binding, TableRef<ITexture> texture)
        {
            _stage = stage;
            _binding = binding;
            _texture = texture;
        }

        public static void Run(ref SetImageCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetImage(command._stage, command._binding, command._texture.GetAs<ThreadedTexture>(threaded)?.Base);
        }
    }
}
