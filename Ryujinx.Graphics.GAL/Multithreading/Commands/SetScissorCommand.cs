namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetScissorCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.SetScissor;
        private int _index;
        private bool _enable;
        private int _x;
        private int _y;
        private int _width;
        private int _height;

        public void Set(int index, bool enable, int x, int y, int width, int height)
        {
            _index = index;
            _enable = enable;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public static void Run(ref SetScissorCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetScissor(command._index, command._enable, command._x, command._y, command._width, command._height);
        }
    }
}
