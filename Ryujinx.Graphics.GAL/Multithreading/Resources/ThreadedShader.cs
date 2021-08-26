using Ryujinx.Graphics.GAL.Multithreading.Commands.Shader;
using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.GAL.Multithreading.Resources
{
    class ThreadedShader : IShader
    {
        private ThreadedRenderer _renderer;
        private ShaderStage _stage;
        private string _code;

        public IShader Base;

        public ThreadedShader(ThreadedRenderer renderer, ShaderStage stage, string code)
        {
            _renderer = renderer;
            
            _stage = stage;
            _code = code;
        }

        internal void EnsureCreated()
        {
            if (_code != null && Base == null)
            {
                Base = _renderer.BaseRenderer.CompileShader(_stage, _code);
                _code = null;
            }
        }

        public void Dispose()
        {
            _renderer.New<ShaderDisposeCommand>().Set(new TableRef<ThreadedShader>(_renderer, this));
            _renderer.QueueCommand();
        }
    }
}
