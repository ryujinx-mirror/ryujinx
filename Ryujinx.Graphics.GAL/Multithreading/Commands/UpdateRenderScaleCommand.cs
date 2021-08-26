using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct UpdateRenderScaleCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.UpdateRenderScale;
        private ShaderStage _stage;
        private SpanRef<float> _scales;
        private int _textureCount;
        private int _imageCount;

        public void Set(ShaderStage stage, SpanRef<float> scales, int textureCount, int imageCount)
        {
            _stage = stage;
            _scales = scales;
            _textureCount = textureCount;
            _imageCount = imageCount;
        }

        public static void Run(ref UpdateRenderScaleCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.UpdateRenderScale(command._stage, command._scales.Get(threaded), command._textureCount, command._imageCount);
            command._scales.Dispose(threaded);
        }
    }
}
