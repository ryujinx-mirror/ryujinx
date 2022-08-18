using Ryujinx.Graphics.GAL.Multithreading.Model;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct UpdateRenderScaleCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.UpdateRenderScale;
        private SpanRef<float> _scales;
        private int _totalCount;
        private int _fragmentCount;

        public void Set(SpanRef<float> scales, int totalCount, int fragmentCount)
        {
            _scales = scales;
            _totalCount = totalCount;
            _fragmentCount = fragmentCount;
        }

        public static void Run(ref UpdateRenderScaleCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.UpdateRenderScale(command._scales.Get(threaded), command._totalCount, command._fragmentCount);
            command._scales.Dispose(threaded);
        }
    }
}
