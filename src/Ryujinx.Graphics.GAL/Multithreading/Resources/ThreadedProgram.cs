using Ryujinx.Graphics.GAL.Multithreading.Commands.Program;
using Ryujinx.Graphics.GAL.Multithreading.Model;

namespace Ryujinx.Graphics.GAL.Multithreading.Resources
{
    class ThreadedProgram : IProgram
    {
        private readonly ThreadedRenderer _renderer;

        public IProgram Base;

        internal bool Compiled;

        public ThreadedProgram(ThreadedRenderer renderer)
        {
            _renderer = renderer;
        }

        private TableRef<T> Ref<T>(T reference)
        {
            return new TableRef<T>(_renderer, reference);
        }

        public void Dispose()
        {
            _renderer.New<ProgramDisposeCommand>().Set(Ref(this));
            _renderer.QueueCommand();
        }

        public byte[] GetBinary()
        {
            ResultBox<byte[]> box = new();
            _renderer.New<ProgramGetBinaryCommand>().Set(Ref(this), Ref(box));
            _renderer.InvokeCommand();

            return box.Result;
        }

        public ProgramLinkStatus CheckProgramLink(bool blocking)
        {
            ResultBox<ProgramLinkStatus> box = new();
            _renderer.New<ProgramCheckLinkCommand>().Set(Ref(this), blocking, Ref(box));
            _renderer.InvokeCommand();

            return box.Result;
        }
    }
}
