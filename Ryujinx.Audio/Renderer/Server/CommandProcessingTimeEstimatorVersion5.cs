//
// Copyright (c) 2019-2022 Ryujinx
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

using Ryujinx.Audio.Renderer.Dsp.Command;
using System;
using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Server
{
    /// <summary>
    /// <see cref="ICommandProcessingTimeEstimator"/> version 5. (added with REV11)
    /// </summary>
    public class CommandProcessingTimeEstimatorVersion5 : CommandProcessingTimeEstimatorVersion4
    {
        public CommandProcessingTimeEstimatorVersion5(uint sampleCount, uint bufferCount) : base(sampleCount, bufferCount) { }

        public override uint Estimate(DelayCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                if (command.Enabled)
                {
                    switch (command.Parameter.ChannelCount)
                    {
                        case 1:
                            return 8929;
                        case 2:
                            return 25501;
                        case 4:
                            return 47760;
                        case 6:
                            return 82203;
                        default:
                            throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                    }
                }
                else
                {
                    switch (command.Parameter.ChannelCount)
                    {
                        case 1:
                            return (uint)1295.20f;
                        case 2:
                            return (uint)1213.60f;
                        case 4:
                            return (uint)942.03f;
                        case 6:
                            return (uint)1001.6f;
                        default:
                            throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                    }
                }
            }

            if (command.Enabled)
            {
                switch (command.Parameter.ChannelCount)
                {
                    case 1:
                        return 11941;
                    case 2:
                        return 37197;
                    case 4:
                        return 69750;
                    case 6:
                        return 12004;
                    default:
                        throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                }
            }
            else
            {
                switch (command.Parameter.ChannelCount)
                {
                    case 1:
                        return (uint)997.67f;
                    case 2:
                        return (uint)977.63f;
                    case 4:
                        return (uint)792.31f;
                    case 6:
                        return (uint)875.43f;
                    default:
                        throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                }
            }
        }

        public override uint Estimate(ReverbCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                if (command.Enabled)
                {
                    switch (command.Parameter.ChannelCount)
                    {
                        case 1:
                            return 81475;
                        case 2:
                            return 84975;
                        case 4:
                            return 91625;
                        case 6:
                            return 95332;
                        default:
                            throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                    }
                }
                else
                {
                    switch (command.Parameter.ChannelCount)
                    {
                        case 1:
                            return (uint)536.30f;
                        case 2:
                            return (uint)588.80f;
                        case 4:
                            return (uint)643.70f;
                        case 6:
                            return (uint)706.0f;
                        default:
                            throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                    }
                }
            }

            if (command.Enabled)
            {
                switch (command.Parameter.ChannelCount)
                {
                    case 1:
                        return 120170;
                    case 2:
                        return 125260;
                    case 4:
                        return 135750;
                    case 6:
                        return 141130;
                    default:
                        throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                }
            }
            else
            {
                switch (command.Parameter.ChannelCount)
                {
                    case 1:
                        return (uint)617.64f;
                    case 2:
                        return (uint)659.54f;
                    case 4:
                        return (uint)711.44f;
                    case 6:
                        return (uint)778.07f;
                    default:
                        throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                }
            }
        }

        public override uint Estimate(Reverb3dCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                if (command.Enabled)
                {
                    switch (command.Parameter.ChannelCount)
                    {
                        case 1:
                            return 116750;
                        case 2:
                            return 125910;
                        case 4:
                            return 146340;
                        case 6:
                            return 165810;
                        default:
                            throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                    }
                }
                else
                {
                    switch (command.Parameter.ChannelCount)
                    {
                        case 1:
                            return 735;
                        case 2:
                            return (uint)766.62f;
                        case 4:
                            return (uint)834.07f;
                        case 6:
                            return (uint)875.44f;
                        default:
                            throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                    }
                }
            }

            if (command.Enabled)
            {
                switch (command.Parameter.ChannelCount)
                {
                    case 1:
                        return 170290;
                    case 2:
                        return 183880;
                    case 4:
                        return 214700;
                    case 6:
                        return 243850;
                    default:
                        throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                }
            }
            else
            {
                switch (command.Parameter.ChannelCount)
                {
                    case 1:
                        return (uint)508.47f;
                    case 2:
                        return (uint)582.45f;
                    case 4:
                        return (uint)626.42f;
                    case 6:
                        return (uint)682.47f;
                    default:
                        throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                }
            }
        }
    }
}
