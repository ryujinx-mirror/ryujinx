namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct ClearRenderTargetColorCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.ClearRenderTargetColor;
        private int _index;
        private uint _componentMask;
        private ColorF _color;

        public void Set(int index, uint componentMask, ColorF color)
        {
            _index = index;
            _componentMask = componentMask;
            _color = color;
        }

        public static void Run(ref ClearRenderTargetColorCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.ClearRenderTargetColor(command._index, command._componentMask, command._color);
        }
    }
}
