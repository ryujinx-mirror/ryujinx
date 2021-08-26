using System.Linq;

namespace Ryujinx.Graphics.GAL.Multithreading.Resources.Programs
{
    class SourceProgramRequest : IProgramRequest
    {
        public ThreadedProgram Threaded { get; set; }

        private IShader[] _shaders;
        private TransformFeedbackDescriptor[] _transformFeedbackDescriptors;

        public SourceProgramRequest(ThreadedProgram program, IShader[] shaders, TransformFeedbackDescriptor[] transformFeedbackDescriptors)
        {
            Threaded = program;

            _shaders = shaders;
            _transformFeedbackDescriptors = transformFeedbackDescriptors;
        }

        public IProgram Create(IRenderer renderer)
        {
            IShader[] shaders = _shaders.Select(shader =>
            {
                var threaded = (ThreadedShader)shader;
                threaded?.EnsureCreated();
                return threaded?.Base;
            }).ToArray();

            return renderer.CreateProgram(shaders, _transformFeedbackDescriptors);
        }
    }
}
