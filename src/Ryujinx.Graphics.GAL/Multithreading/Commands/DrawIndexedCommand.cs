namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct DrawCommand : IGALCommand, IGALCommand<DrawCommand>
    {
        public readonly CommandType CommandType => CommandType.Draw;
        private int _vertexCount;
        private int _instanceCount;
        private int _firstVertex;
        private int _firstInstance;

        public void Set(int vertexCount, int instanceCount, int firstVertex, int firstInstance)
        {
            _vertexCount = vertexCount;
            _instanceCount = instanceCount;
            _firstVertex = firstVertex;
            _firstInstance = firstInstance;
        }

        public static void Run(ref DrawCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.Draw(command._vertexCount, command._instanceCount, command._firstVertex, command._firstInstance);
        }
    }
}
