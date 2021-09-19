//
// Copyright (c) 2019-2021 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

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
