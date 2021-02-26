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
using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Dsp.Command;
using System;
using System.Diagnostics;
using static Ryujinx.Audio.Renderer.Parameter.VoiceInParameter;

namespace Ryujinx.Audio.Renderer.Server
{
    /// <summary>
    /// <see cref="ICommandProcessingTimeEstimator"/> version 3. (added with REV8)
    /// </summary>
    public class CommandProcessingTimeEstimatorVersion3 : ICommandProcessingTimeEstimator
    {
        private uint _sampleCount;
        private uint _bufferCount;

        public CommandProcessingTimeEstimatorVersion3(uint sampleCount, uint bufferCount)
        {
            _sampleCount = sampleCount;
            _bufferCount = bufferCount;
        }

        public uint Estimate(PerformanceCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)498.17f;
            }

            return (uint)489.42f;
        }

        public uint Estimate(ClearMixBufferCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            float costPerBuffer = 440.68f;
            float baseCost = 0;

            if (_sampleCount == 160)
            {
                costPerBuffer = 266.65f;
            }

            return (uint)(baseCost + costPerBuffer * _bufferCount);
        }

        public uint Estimate(BiquadFilterCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)4173.2f;
            }

            return (uint)5585.1f;
        }

        public uint Estimate(MixRampGroupedCommand command)
        {
            float costPerSample = 6.4434f;

            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                costPerSample = 6.708f;
            }

            int volumeCount = 0;

            for (int i = 0; i < command.MixBufferCount; i++)
            {
                if (command.Volume0[i] != 0.0f || command.Volume1[i] != 0.0f)
                {
                    volumeCount++;
                }
            }

            return (uint)(_sampleCount * costPerSample * volumeCount);
        }

        public uint Estimate(MixRampCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)1968.7f;
            }

            return (uint)2459.4f;
        }

        public uint Estimate(DepopPrepareCommand command)
        {
            return 0;
        }

        public uint Estimate(VolumeRampCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)1425.3f;
            }

            return (uint)1700.0f;
        }

        public uint Estimate(PcmInt16DataSourceCommandVersion1 command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            float costPerSample = 710.143f;
            float baseCost = 7853.286f;

            if (_sampleCount == 160)
            {
                costPerSample = 427.52f;
                baseCost = 6329.442f;
            }

            return (uint)(baseCost + (costPerSample * (((command.SampleRate / 200.0f) / _sampleCount) * (command.Pitch * 0.000030518f))));
        }

        public uint Estimate(AdpcmDataSourceCommandVersion1 command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            float costPerSample = 3564.1f;
            float baseCost = 9736.702f;

            if (_sampleCount == 160)
            {
                costPerSample = 2125.6f;
                baseCost = 7913.808f;
            }

            return (uint)(baseCost + (costPerSample * (((command.SampleRate / 200.0f) / _sampleCount) * (command.Pitch * 0.000030518f))));
        }

        public uint Estimate(DepopForMixBuffersCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)739.64f;
            }

            return (uint)910.97f;
        }

        public uint Estimate(CopyMixBufferCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)842.59f;
            }

            return (uint)986.72f;
        }

        public uint Estimate(MixCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)1402.8f;
            }

            return (uint)1853.2f;
        }

        public uint Estimate(DelayCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                if (command.Enabled)
                {
                    switch (command.Parameter.ChannelCount)
                    {
                        case 1:
                            return (uint)8929.04f;
                        case 2:
                            return (uint)25500.75f;
                        case 4:
                            return (uint)47759.62f;
                        case 6:
                            return (uint)82203.07f;
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
                            return (uint)1001.55f;
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
                        return (uint)11941.05f;
                    case 2:
                        return (uint)37197.37f;
                    case 4:
                        return (uint)69749.84f;
                    case 6:
                        return (uint)120042.40f;
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
                        return (uint)792.30f;
                    case 6:
                        return (uint)875.43f;
                    default:
                        throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                }
            }
        }

        public uint Estimate(ReverbCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                if (command.Enabled)
                {
                    switch (command.Parameter.ChannelCount)
                    {
                        case 1:
                            return (uint)81475.05f;
                        case 2:
                            return (uint)84975.0f;
                        case 4:
                            return (uint)91625.15f;
                        case 6:
                            return (uint)95332.27f;
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
                            return (uint)588.70f;
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
                        return (uint)120174.47f;
                    case 2:
                        return (uint)25262.22f;
                    case 4:
                        return (uint)135751.23f;
                    case 6:
                        return (uint)141129.23f;
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
                        return (uint)711.43f;
                    case 6:
                        return (uint)778.07f;
                    default:
                        throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                }
            }
        }

        public uint Estimate(Reverb3dCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                if (command.Enabled)
                {
                    switch (command.Parameter.ChannelCount)
                    {
                        case 1:
                            return (uint)116754.0f;
                        case 2:
                            return (uint)125912.05f;
                        case 4:
                            return (uint)146336.03f;
                        case 6:
                            return (uint)165812.66f;
                        default:
                            throw new NotImplementedException($"{command.Parameter.ChannelCount}");
                    }
                }
                else
                {
                    switch (command.Parameter.ChannelCount)
                    {
                        case 1:
                            return (uint)734.0f;
                        case 2:
                            return (uint)766.62f;
                        case 4:
                            return (uint)797.46f;
                        case 6:
                            return (uint)867.43f;
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
                        return (uint)170292.34f;
                    case 2:
                        return (uint)183875.63f;
                    case 4:
                        return (uint)214696.19f;
                    case 6:
                        return (uint)243846.77f;
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

        public uint Estimate(AuxiliaryBufferCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                if (command.Enabled)
                {
                    return (uint)7182.14f;
                }

                return (uint)472.11f;
            }

            if (command.Enabled)
            {
                return (uint)9435.96f;
            }

            return (uint)462.62f;
        }

        public uint Estimate(VolumeCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)1311.1f;
            }

            return (uint)1713.6f;
        }

        public uint Estimate(CircularBufferSinkCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            float costPerBuffer = 770.26f;
            float baseCost = 0f;

            if (_sampleCount == 160)
            {
                costPerBuffer = 531.07f;
            }

            return (uint)(baseCost + costPerBuffer * command.InputCount);
        }

        public uint Estimate(DownMixSurroundToStereoCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)9949.7f;
            }

            return (uint)14679.0f;
        }

        public uint Estimate(UpsampleCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)312990.0f;
            }

            return (uint)0.0f;
        }

        public uint Estimate(DeviceSinkCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);
            Debug.Assert(command.InputCount == 2 || command.InputCount == 6);

            if (command.InputCount == 2)
            {
                if (_sampleCount == 160)
                {
                    return (uint)8980.0f;
                }

                return (uint)9221.9f;
            }

            if (_sampleCount == 160)
            {
                return (uint)9177.9f;
            }

            return (uint)9725.9f;
        }

        public uint Estimate(PcmFloatDataSourceCommandVersion1 command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            float costPerSample = 3490.9f;
            float baseCost = 10090.9f;

            if (_sampleCount == 160)
            {
                costPerSample = 2310.4f;
                baseCost = 7845.25f;
            }

            return (uint)(baseCost + (costPerSample * (((command.SampleRate / 200.0f) / _sampleCount) * (command.Pitch * 0.000030518f))));
        }

        public uint Estimate(DataSourceVersion2Command command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            (float baseCost, float costPerSample) = GetCostByFormat(_sampleCount, command.SampleFormat, command.SrcQuality);

            return (uint)(baseCost + (costPerSample * (((command.SampleRate / 200.0f) / _sampleCount) * (command.Pitch * 0.000030518f) - 1.0f)));
        }

        private static (float, float) GetCostByFormat(uint sampleCount, SampleFormat format, SampleRateConversionQuality quality)
        {
            Debug.Assert(sampleCount == 160 || sampleCount == 240);

            switch (format)
            {
                case SampleFormat.PcmInt16:
                    switch (quality)
                    {
                        case SampleRateConversionQuality.Default:
                            if (sampleCount == 160)
                            {
                                return (6329.44f, 427.52f);
                            }

                            return (7853.28f, 710.14f);
                        case SampleRateConversionQuality.High:
                            if (sampleCount == 160)
                            {
                                return (8049.42f, 371.88f);
                            }

                            return (10138.84f, 610.49f);
                        case SampleRateConversionQuality.Low:
                            if (sampleCount == 160)
                            {
                                return (5062.66f, 423.43f);
                            }

                            return (5810.96f, 676.72f);
                        default:
                            throw new NotImplementedException($"{format} {quality}");
                    }
                case SampleFormat.PcmFloat:
                    switch (quality)
                    {
                        case SampleRateConversionQuality.Default:
                            if (sampleCount == 160)
                            {
                                return (7845.25f, 2310.4f);
                            }

                            return (10090.9f, 3490.9f);
                        case SampleRateConversionQuality.High:
                            if (sampleCount == 160)
                            {
                                return (9446.36f, 2308.91f);
                            }

                            return (12520.85f, 3480.61f);
                        case SampleRateConversionQuality.Low:
                            if (sampleCount == 160)
                            {
                                return (9446.36f, 2308.91f);
                            }

                            return (12520.85f, 3480.61f);
                        default:
                            throw new NotImplementedException($"{format} {quality}");
                    }
                case SampleFormat.Adpcm:
                    switch (quality)
                    {
                        case SampleRateConversionQuality.Default:
                            if (sampleCount == 160)
                            {
                                return (7913.81f, 1827.66f);
                            }

                            return (9736.70f, 2756.37f);
                        case SampleRateConversionQuality.High:
                            if (sampleCount == 160)
                            {
                                return (9607.81f, 1829.29f);
                            }

                            return (12154.38f, 2731.31f);
                        case SampleRateConversionQuality.Low:
                            if (sampleCount == 160)
                            {
                                return (6517.48f, 1824.61f);
                            }

                            return (7929.44f, 2732.15f);
                        default:
                            throw new NotImplementedException($"{format} {quality}");
                    }
                default:
                    throw new NotImplementedException($"{format}");
            }
        }
    }
}
