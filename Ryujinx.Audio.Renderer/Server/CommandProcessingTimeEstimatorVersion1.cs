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

using Ryujinx.Audio.Renderer.Dsp.Command;
using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Server
{
    /// <summary>
    /// <see cref="ICommandProcessingTimeEstimator"/> version 1.
    /// </summary>
    public class CommandProcessingTimeEstimatorVersion1 : ICommandProcessingTimeEstimator
    {
        private uint _sampleCount;
        private uint _bufferCount;

        public CommandProcessingTimeEstimatorVersion1(uint sampleCount, uint bufferCount)
        {
            _sampleCount = sampleCount;
            _bufferCount = bufferCount;
        }

        public uint Estimate(PerformanceCommand command)
        {
            return 1454;
        }

        public uint Estimate(ClearMixBufferCommand command)
        {
            return (uint)(_sampleCount * 0.83f * _bufferCount * 1.2f);
        }

        public uint Estimate(BiquadFilterCommand command)
        {
            return (uint)(_sampleCount * 58.0f * 1.2f);
        }

        public uint Estimate(MixRampGroupedCommand command)
        {
            int volumeCount = 0;

            for (int i = 0; i < command.MixBufferCount; i++)
            {
                if (command.Volume0[i] != 0.0f || command.Volume1[i] != 0.0f)
                {
                    volumeCount++;
                }
            }

            return (uint)(_sampleCount * 14.4f * 1.2f * volumeCount);
        }

        public uint Estimate(MixRampCommand command)
        {
            return (uint)(_sampleCount * 14.4f * 1.2f);
        }

        public uint Estimate(DepopPrepareCommand command)
        {
            return 1080;
        }

        public uint Estimate(VolumeRampCommand command)
        {
            return (uint)(_sampleCount * 9.8f * 1.2f);
        }

        public uint Estimate(PcmInt16DataSourceCommandVersion1 command)
        {
            return (uint)(command.Pitch * 0.25f * 1.2f);
        }

        public uint Estimate(AdpcmDataSourceCommandVersion1 command)
        {
            return (uint)(command.Pitch * 0.46f * 1.2f);
        }

        public uint Estimate(DepopForMixBuffersCommand command)
        {
            return (uint)(_sampleCount * 8.9f * command.MixBufferCount);
        }

        public uint Estimate(CopyMixBufferCommand command)
        {
            // NOTE: Nintendo returns 0 here for some reasons even if it will generate a command like that on version 1.. maybe a mistake?
            return 0;
        }

        public uint Estimate(MixCommand command)
        {
            return (uint)(_sampleCount * 10.0f * 1.2f);
        }

        public uint Estimate(DelayCommand command)
        {
            return (uint)(_sampleCount * command.Parameter.ChannelCount * 202.5f);
        }

        public uint Estimate(ReverbCommand command)
        {
            Debug.Assert(command.Parameter.IsChannelCountValid());

            if (command.Enabled)
            {
                return (uint)(750 * _sampleCount * command.Parameter.ChannelCount * 1.2f);
            }

            return 0;
        }

        public uint Estimate(Reverb3dCommand command)
        {
            if (command.Enabled)
            {
                return (uint)(530 * _sampleCount * command.Parameter.ChannelCount * 1.2f);
            }

            return 0;
        }

        public uint Estimate(AuxiliaryBufferCommand command)
        {
            if (command.Enabled)
            {
                return 15956;
            }

            return 3765;
        }

        public uint Estimate(VolumeCommand command)
        {
            return (uint)(_sampleCount * 8.8f * 1.2f);
        }

        public uint Estimate(CircularBufferSinkCommand command)
        {
            return 55;
        }

        public uint Estimate(DownMixSurroundToStereoCommand command)
        {
            return 16108;
        }

        public uint Estimate(UpsampleCommand command)
        {
            return 357915;
        }

        public uint Estimate(DeviceSinkCommand command)
        {
            return 10042;
        }

        public uint Estimate(PcmFloatDataSourceCommandVersion1 command)
        {
            return 0;
        }

        public uint Estimate(DataSourceVersion2Command command)
        {
            return 0;
        }
    }
}
