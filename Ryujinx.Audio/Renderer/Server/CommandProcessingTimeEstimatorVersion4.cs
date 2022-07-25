using Ryujinx.Audio.Common;
using Ryujinx.Audio.Renderer.Dsp.Command;
using Ryujinx.Audio.Renderer.Parameter.Effect;
using System;
using System.Diagnostics;
using static Ryujinx.Audio.Renderer.Parameter.VoiceInParameter;

namespace Ryujinx.Audio.Renderer.Server
{
    /// <summary>
    /// <see cref="ICommandProcessingTimeEstimator"/> version 4. (added with REV10)
    /// </summary>
    public class CommandProcessingTimeEstimatorVersion4 : CommandProcessingTimeEstimatorVersion3
    {
        public CommandProcessingTimeEstimatorVersion4(uint sampleCount, uint bufferCount) : base(sampleCount, bufferCount) { }

        public override uint Estimate(GroupedBiquadFilterCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)7424.5f;
            }

            return (uint)9730.4f;
        }

        public override uint Estimate(CaptureBufferCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
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