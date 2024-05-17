using Ryujinx.Audio.Renderer.Dsp.Command;
using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Server
{
    /// <summary>
    /// <see cref="ICommandProcessingTimeEstimator"/> version 4. (added with REV10)
    /// </summary>
    public class CommandProcessingTimeEstimatorVersion4 : CommandProcessingTimeEstimatorVersion3
    {
        public CommandProcessingTimeEstimatorVersion4(uint sampleCount, uint bufferCount) : base(sampleCount, bufferCount) { }

        public override uint Estimate(MultiTapBiquadFilterCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                return (uint)7424.5f;
            }

            return (uint)9730.4f;
        }

        public override uint Estimate(CaptureBufferCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                if (command.Enabled)
                {
                    return (uint)435.2f;
                }

                return (uint)4261.0f;
            }

            if (command.Enabled)
            {
                return (uint)5858.26f;
            }

            return (uint)435.2f;
        }
    }
}
