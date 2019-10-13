using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private void ResetCounter(int argument)
        {
            ResetCounterType type = (ResetCounterType)argument;

            switch (type)
            {
                case ResetCounterType.SamplesPassed:
                    _context.Renderer.ResetCounter(CounterType.SamplesPassed);
                    break;
                case ResetCounterType.PrimitivesGenerated:
                    _context.Renderer.ResetCounter(CounterType.PrimitivesGenerated);
                    break;
                case ResetCounterType.TransformFeedbackPrimitivesWritten:
                    _context.Renderer.ResetCounter(CounterType.TransformFeedbackPrimitivesWritten);
                    break;
            }
        }
    }
}