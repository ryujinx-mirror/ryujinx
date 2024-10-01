namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct ClearRenderTargetColorCommand : IGALCommand, IGALCommand<ClearRenderTargetColorCommand>
    {
        public readonly CommandType CommandType => CommandType.ClearRenderTargetColor;
        private int _index;
        private int _layer;
        private int _layerCount;
        private uint _componentMask;
        private ColorF _color;

        public void Set(int index, int layer, int layerCount, uint componentMask, ColorF color)
        {
            _index = index;
            _layer = layer;
            _layerCount = layerCount;
            _componentMask = componentMask;
            _color = color;
        }

        public static void Run(ref ClearRenderTargetColorCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.ClearRenderTargetColor(command._index, command._layer, command._layerCount, command._componentMask, command._color);
        }
    }
}
