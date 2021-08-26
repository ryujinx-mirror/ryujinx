namespace Ryujinx.Graphics.GAL.Multithreading.Resources.Programs
{
    class BinaryProgramRequest : IProgramRequest
    {
        public ThreadedProgram Threaded { get; set; }

        private byte[] _data;

        public BinaryProgramRequest(ThreadedProgram program, byte[] data)
        {
            Threaded = program;

            _data = data;
        }

        public IProgram Create(IRenderer renderer)
        {
            return renderer.LoadProgramBinary(_data);
        }
    }
}
