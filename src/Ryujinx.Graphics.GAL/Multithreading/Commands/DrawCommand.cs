namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct DrawIndexedCommand : IGALCommand, IGALCommand<DrawIndexedCommand>
    {
        public readonly CommandType CommandType => CommandType.DrawIndexed;
        private int _indexCount;
        private int _instanceCount;
        private int _firstIndex;
        private int _firstVertex;
        private int _firstInstance;

        public void Set(int indexCount, int instanceCount, int firstIndex, int firstVertex, int firstInstance)
        {
            _indexCount = indexCount;
            _instanceCount = instanceCount;
            _firstIndex = firstIndex;
            _firstVertex = firstVertex;
            _firstInstance = firstInstance;
        }

        public static void Run(ref DrawIndexedCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.DrawIndexed(command._indexCount, command._instanceCount, command._firstIndex, command._firstVertex, command._firstInstance);
        }
    }
}
