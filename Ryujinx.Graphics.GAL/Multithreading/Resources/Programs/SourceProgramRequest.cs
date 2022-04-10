using System.Linq;

namespace Ryujinx.Graphics.GAL.Multithreading.Resources.Programs
{
    class SourceProgramRequest : IProgramRequest
    {
        public ThreadedProgram Threaded { get; set; }

        private ShaderSource[] _shaders;
        private ShaderInfo _info;

        public SourceProgramRequest(ThreadedProgram program, ShaderSource[] shaders, ShaderInfo info)
        {
            Threaded = program;

            _shaders = shaders;
            _info = info;
        }

        public IProgram Create(IRenderer renderer)
        {
            return renderer.CreateProgram(_shaders, _info);
        }
    }
}
