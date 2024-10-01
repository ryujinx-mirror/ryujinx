using Ryujinx.Audio.Renderer.Dsp.Command;
using System;
using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Server
{
    /// <summary>
    /// <see cref="ICommandProcessingTimeEstimator"/> version 2. (added with REV5)
    /// </summary>
    public class CommandProcessingTimeEstimatorVersion2 : ICommandProcessingTimeEstimator
    {
        private readonly uint _sampleCount;
        private readonly uint _bufferCount;

        public CommandProcessingTimeEstimatorVersion2(uint sampleCount, uint bufferCount)
        {
            _sampleCount = sampleCount;
            _bufferCount = bufferCount;
        }

        public uint Estimate(PerformanceCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)489.35f;
            }

            return (uint)491.18f;
        }

        public uint Estimate(ClearMixBufferCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            float costPerBuffer = 668.8f;
            float baseCost = 193.2f;

            if (_sampleCount == 160)
            {
                costPerBuffer = 260.4f;
                baseCost = 139.65f;
            }

            return (uint)(baseCost + costPerBuffer * _bufferCount);
        }

        public uint Estimate(BiquadFilterCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)4813.2f;
            }

            return (uint)6915.4f;
        }

        public uint Estimate(MixRampGroupedCommand command)
        {
            const float CostPerSample = 7.245f;

            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            int volumeCount = 0;

            for (int i = 0; i < command.MixBufferCount; i++)
            {
                if (command.Volume0[i] != 0.0f || command.Volume1[i] != 0.0f)
                {
                    volumeCount++;
                }
            }

            return (uint)(_sampleCount * CostPerSample * volumeCount);
        }

        public uint Estimate(MixRampCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)1859.0f;
            }

            return (uint)2286.1f;
        }

        public uint Estimate(DepopPrepareCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)306.62f;
            }

            return (uint)293.22f;
        }

        public uint Estimate(VolumeRampCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)1403.9f;
            }

            return (uint)1884.3f;
        }

        public uint Estimate(PcmInt16DataSourceCommandVersion1 command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            float costPerSample = 1195.5f;
            float baseCost = 7797.0f;

            if (_sampleCount == 160)
            {
                costPerSample = 749.27f;
                baseCost = 6138.9f;
            }

            return (uint)(baseCost + (costPerSample * (((command.SampleRate / 200.0f) / _sampleCount) * (command.Pitch * 0.000030518f))));
        }

        public uint Estimate(AdpcmDataSourceCommandVersion1 command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            float costPerSample = 3564.1f;
            float baseCost = 6225.5f;

            if (_sampleCount == 160)
            {
                costPerSample = 2125.6f;
                baseCost = 9039.5f;
            }

            return (uint)(baseCost + (costPerSample * (((command.SampleRate / 200.0f) / _sampleCount) * (command.Pitch * 0.000030518f))));
        }

        public uint Estimate(DepopForMixBuffersCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)762.96f;
            }

            return (uint)726.96f;
        }

        public uint Estimate(CopyMixBufferCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)836.32f;
            }

            return (uint)1000.9f;
        }

        public uint Estimate(MixCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)1342.2f;
            }

            return (uint)1833.2f;
        }

        public uint Estimate(DelayCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                if (command.Enabled)
                {
                    return command.Parameter.ChannelCount switch
                    {
                        1 => (uint)41636.0f,
                        2 => (uint)97861.0f,
                        4 => (uint)192520.0f,
                        6 => (uint)301760.0f,
                        _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                    };
                }

                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)578.53f,
                    2 => (uint)663.06f,
                    4 => (uint)703.98f,
                    6 => (uint)760.03f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };

            }

            if (command.Enabled)
            {
                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)8770.3f,
                    2 => (uint)25741.0f,
                    4 => (uint)47551.0f,
                    6 => (uint)81629.0f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };
            }

            return command.Parameter.ChannelCount switch
            {
                1 => (uint)521.28f,
                2 => (uint)585.4f,
                4 => (uint)629.88f,
                6 => (uint)713.57f,
                _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
            };
        }

        public uint Estimate(ReverbCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                if (command.Enabled)
                {
                    return command.Parameter.ChannelCount switch
                    {
                        1 => (uint)97192.0f,
                        2 => (uint)103280.0f,
                        4 => (uint)109580.0f,
                        6 => (uint)115070.0f,
                        _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                    };
                }

                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)492.01f,
                    2 => (uint)554.46f,
                    4 => (uint)595.86f,
                    6 => (uint)656.62f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };

            }

            if (command.Enabled)
            {
                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)136460.0f,
                    2 => (uint)145750.0f,
                    4 => (uint)154800.0f,
                    6 => (uint)161970.0f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };
            }

            return command.Parameter.ChannelCount switch
            {
                1 => (uint)495.79f,
                2 => (uint)527.16f,
                4 => (uint)598.75f,
                6 => (uint)666.03f,
                _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
            };
        }

        public uint Estimate(Reverb3dCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                if (command.Enabled)
                {
                    return command.Parameter.ChannelCount switch
                    {
                        1 => (uint)138840.0f,
                        2 => (uint)135430.0f,
                        4 => (uint)199180.0f,
                        6 => (uint)247350.0f,
                        _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                    };
                }

                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)718.7f,
                    2 => (uint)751.3f,
                    4 => (uint)797.46f,
                    6 => (uint)867.43f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };
            }

            if (command.Enabled)
            {
                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)199950.0f,
                    2 => (uint)195200.0f,
                    4 => (uint)290580.0f,
                    6 => (uint)363490.0f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };
            }

            return command.Parameter.ChannelCount switch
            {
                1 => (uint)534.24f,
                2 => (uint)570.87f,
                4 => (uint)660.93f,
                6 => (uint)694.6f,
                _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
            };
        }

        public uint Estimate(AuxiliaryBufferCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                if (command.Enabled)
                {
                    return (uint)7177.9f;
                }

                return (uint)489.16f;
            }

            if (command.Enabled)
            {
                return (uint)9499.8f;
            }

            return (uint)485.56f;
        }

        public uint Estimate(VolumeCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)1280.3f;
            }

            return (uint)1737.8f;
        }

        public uint Estimate(CircularBufferSinkCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            float costPerBuffer = 1726.0f;
            float baseCost = 1369.7f;

            if (_sampleCount == 160)
            {
                costPerBuffer = 853.63f;
                baseCost = 1284.5f;
            }

            return (uint)(baseCost + costPerBuffer * command.InputCount);
        }

        public uint Estimate(DownMixSurroundToStereoCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)10009.0f;
            }

            return (uint)14577.0f;
        }

        public uint Estimate(UpsampleCommand command)
        {
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            if (_sampleCount == 160)
            {
                return (uint)292000.0f;
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
                    return (uint)9261.5f;
                }

                return (uint)9336.1f;
            }

            if (_sampleCount == 160)
            {
                return (uint)9111.8f;
            }

            return (uint)9566.7f;
        }

        public uint Estimate(PcmFloatDataSourceCommandVersion1 command)
        {
            // NOTE: This was added between REV7 and REV8 and for some reasons the estimator v2 was changed...
            Debug.Assert(_sampleCount == 160 || _sampleCount == 240);

            float costPerSample = 3490.9f;
            float baseCost = 10091.0f;

            if (_sampleCount == 160)
            {
                costPerSample = 2310.4f;
                baseCost = 7845.3f;
            }

            return (uint)(baseCost + (costPerSample * (((command.SampleRate / 200.0f) / _sampleCount) * (command.Pitch * 0.000030518f))));
        }

        public uint Estimate(DataSourceVersion2Command command)
        {
            return 0;
        }

        public uint Estimate(LimiterCommandVersion1 command)
        {
            return 0;
        }

        public uint Estimate(LimiterCommandVersion2 command)
        {
            return 0;
        }

        public uint Estimate(MultiTapBiquadFilterCommand command)
        {
            return 0;
        }

        public uint Estimate(CaptureBufferCommand command)
        {
            return 0;
        }

        public uint Estimate(CompressorCommand command)
        {
            return 0;
        }

        public uint Estimate(BiquadFilterAndMixCommand command)
        {
            return 0;
        }

        public uint Estimate(MultiTapBiquadFilterAndMixCommand command)
        {
            return 0;
        }
    }
}
