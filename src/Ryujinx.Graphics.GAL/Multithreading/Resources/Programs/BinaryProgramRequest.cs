namespace Ryujinx.Graphics.GAL.Multithreading.Resources.Programs
{
    class BinaryProgramRequest : IProgramRequest
    {
        public ThreadedProgram Threaded { get; set; }

        private readonly byte[] _data;
        private readonly bool _hasFragmentShader;
        private ShaderInfo _info;

        public BinaryProgramRequest(ThreadedProgram program, byte[] data, bool hasFragmentShader, ShaderInfo info)
        {
            Threaded = program;

            _data = data;
            _hasFragmentShader = hasFragmentShader;
            _info = info;
        }

        public IProgram Create(IRenderer renderer)
        {
            return renderer.LoadProgramBinary(_data, _hasFragmentShader, _info);
        }
    }
}
