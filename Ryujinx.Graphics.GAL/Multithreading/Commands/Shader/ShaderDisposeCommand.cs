using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Shader
{
    struct ShaderDisposeCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.ShaderDispose;
        private TableRef<ThreadedShader> _shader;

        public void Set(TableRef<ThreadedShader> shader)
        {
            _shader = shader;
        }

        public static void Run(ref ShaderDisposeCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            command._shader.Get(threaded).Base.Dispose();
        }
    }
}
