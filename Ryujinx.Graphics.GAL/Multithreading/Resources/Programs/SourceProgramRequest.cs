using System.Linq;

namespace Ryujinx.Graphics.GAL.Multithreading.Resources.Programs
{
    class SourceProgramRequest : IProgramRequest
    {
        public ThreadedProgram Threaded { get; set; }

        private IShader[] _shaders;

        public SourceProgramRequest(ThreadedProgram program, IShader[] shaders)
        {
            Threaded = program;

            _shaders = shaders;
        }

        public IProgram Create(IRenderer renderer)
        {
            IShader[] shaders = _shaders.Select(shader =>
            {
                var threaded = (ThreadedShader)shader;
                threaded?.EnsureCreated();
                return threaded?.Base;
            }).ToArray();

            return renderer.CreateProgram(shaders);
        }
    }
}
