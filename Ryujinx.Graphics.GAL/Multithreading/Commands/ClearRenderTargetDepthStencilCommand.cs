namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct ClearRenderTargetDepthStencilCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.ClearRenderTargetDepthStencil;
        private int _layer;
        private float _depthValue;
        private bool _depthMask;
        private int _stencilValue;
        private int _stencilMask;

        public void Set(int layer, float depthValue, bool depthMask, int stencilValue, int stencilMask)
        {
            _layer = layer;
            _depthValue = depthValue;
            _depthMask = depthMask;
            _stencilValue = stencilValue;
            _stencilMask = stencilMask;
        }

        public static void Run(ref ClearRenderTargetDepthStencilCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.ClearRenderTargetDepthStencil(command._layer, command._depthValue, command._depthMask, command._stencilValue, command._stencilMask);
        }
    }
}
