using Ryujinx.Audio.Common;
using Ryujinx.Audio.Renderer.Dsp.Command;
using Ryujinx.Audio.Renderer.Parameter.Effect;
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
        protected uint SampleCount;
        protected uint BufferCount;

        public CommandProcessingTimeEstimatorVersion3(uint sampleCount, uint bufferCount)
        {
            SampleCount = sampleCount;
            BufferCount = bufferCount;
        }

        public uint Estimate(PerformanceCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                return (uint)498.17f;
            }

            return (uint)489.42f;
        }

        public uint Estimate(ClearMixBufferCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            float costPerBuffer = 440.68f;
            float baseCost = 0;

            if (SampleCount == 160)
            {
                costPerBuffer = 266.65f;
            }

            return (uint)(baseCost + costPerBuffer * BufferCount);
        }

        public uint Estimate(BiquadFilterCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                return (uint)4173.2f;
            }

            return (uint)5585.1f;
        }

        public uint Estimate(MixRampGroupedCommand command)
        {
            float costPerSample = 6.4434f;

            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
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

            return (uint)(SampleCount * costPerSample * volumeCount);
        }

        public uint Estimate(MixRampCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
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
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                return (uint)1425.3f;
            }

            return (uint)1700.0f;
        }

        public uint Estimate(PcmInt16DataSourceCommandVersion1 command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            float costPerSample = 710.143f;
            float baseCost = 7853.286f;

            if (SampleCount == 160)
            {
                costPerSample = 427.52f;
                baseCost = 6329.442f;
            }

            return (uint)(baseCost + (costPerSample * (((command.SampleRate / 200.0f) / SampleCount) * (command.Pitch * 0.000030518f))));
        }

        public uint Estimate(AdpcmDataSourceCommandVersion1 command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            float costPerSample = 3564.1f;
            float baseCost = 9736.702f;

            if (SampleCount == 160)
            {
                costPerSample = 2125.6f;
                baseCost = 7913.808f;
            }

            return (uint)(baseCost + (costPerSample * (((command.SampleRate / 200.0f) / SampleCount) * (command.Pitch * 0.000030518f))));
        }

        public uint Estimate(DepopForMixBuffersCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                return (uint)739.64f;
            }

            return (uint)910.97f;
        }

        public uint Estimate(CopyMixBufferCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                return (uint)842.59f;
            }

            return (uint)986.72f;
        }

        public uint Estimate(MixCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                return (uint)1402.8f;
            }

            return (uint)1853.2f;
        }

        public virtual uint Estimate(DelayCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                if (command.Enabled)
                {
                    return command.Parameter.ChannelCount switch
                    {
                        1 => (uint)8929.04f,
                        2 => (uint)25500.75f,
                        4 => (uint)47759.62f,
                        6 => (uint)82203.07f,
                        _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                    };
                }

                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)1295.20f,
                    2 => (uint)1213.60f,
                    4 => (uint)942.03f,
                    6 => (uint)1001.55f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };
            }

            if (command.Enabled)
            {
                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)11941.05f,
                    2 => (uint)37197.37f,
                    4 => (uint)69749.84f,
                    6 => (uint)120042.40f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };
            }

            return command.Parameter.ChannelCount switch
            {
                1 => (uint)997.67f,
                2 => (uint)977.63f,
                4 => (uint)792.30f,
                6 => (uint)875.43f,
                _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
            };
        }

        public virtual uint Estimate(ReverbCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                if (command.Enabled)
                {
                    return command.Parameter.ChannelCount switch
                    {
                        1 => (uint)81475.05f,
                        2 => (uint)84975.0f,
                        4 => (uint)91625.15f,
                        6 => (uint)95332.27f,
                        _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                    };
                }

                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)536.30f,
                    2 => (uint)588.70f,
                    4 => (uint)643.70f,
                    6 => (uint)706.0f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };
            }

            if (command.Enabled)
            {
                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)120174.47f,
                    2 => (uint)25262.22f,
                    4 => (uint)135751.23f,
                    6 => (uint)141129.23f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };
            }

            return command.Parameter.ChannelCount switch
            {
                1 => (uint)617.64f,
                2 => (uint)659.54f,
                4 => (uint)711.43f,
                6 => (uint)778.07f,
                _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
            };
        }

        public virtual uint Estimate(Reverb3dCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                if (command.Enabled)
                {
                    return command.Parameter.ChannelCount switch
                    {
                        1 => (uint)116754.0f,
                        2 => (uint)125912.05f,
                        4 => (uint)146336.03f,
                        6 => (uint)165812.66f,
                        _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                    };
                }

                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)734.0f,
                    2 => (uint)766.62f,
                    4 => (uint)797.46f,
                    6 => (uint)867.43f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };
            }

            if (command.Enabled)
            {
                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)170292.34f,
                    2 => (uint)183875.63f,
                    4 => (uint)214696.19f,
                    6 => (uint)243846.77f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };
            }

            return command.Parameter.ChannelCount switch
            {
                1 => (uint)508.47f,
                2 => (uint)582.45f,
                4 => (uint)626.42f,
                6 => (uint)682.47f,
                _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
            };
        }

        public uint Estimate(AuxiliaryBufferCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
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
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                return (uint)1311.1f;
            }

            return (uint)1713.6f;
        }

        public uint Estimate(CircularBufferSinkCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            float costPerBuffer = 770.26f;
            float baseCost = 0f;

            if (SampleCount == 160)
            {
                costPerBuffer = 531.07f;
            }

            return (uint)(baseCost + costPerBuffer * command.InputCount);
        }

        public uint Estimate(DownMixSurroundToStereoCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                return (uint)9949.7f;
            }

            return (uint)14679.0f;
        }

        public uint Estimate(UpsampleCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                return (uint)312990.0f;
            }

            return (uint)0.0f;
        }

        public uint Estimate(DeviceSinkCommand command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);
            Debug.Assert(command.InputCount == 2 || command.InputCount == 6);

            if (command.InputCount == 2)
            {
                if (SampleCount == 160)
                {
                    return (uint)8980.0f;
                }

                return (uint)9221.9f;
            }

            if (SampleCount == 160)
            {
                return (uint)9177.9f;
            }

            return (uint)9725.9f;
        }

        public uint Estimate(PcmFloatDataSourceCommandVersion1 command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            float costPerSample = 3490.9f;
            float baseCost = 10090.9f;

            if (SampleCount == 160)
            {
                costPerSample = 2310.4f;
                baseCost = 7845.25f;
            }

            return (uint)(baseCost + (costPerSample * (((command.SampleRate / 200.0f) / SampleCount) * (command.Pitch * 0.000030518f))));
        }

        public uint Estimate(DataSourceVersion2Command command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            (float baseCost, float costPerSample) = GetCostByFormat(SampleCount, command.SampleFormat, command.SrcQuality);

            return (uint)(baseCost + (costPerSample * (((command.SampleRate / 200.0f) / SampleCount) * (command.Pitch * 0.000030518f) - 1.0f)));
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

        private uint EstimateLimiterCommandCommon(LimiterParameter parameter, bool enabled)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (SampleCount == 160)
            {
                if (enabled)
                {
                    return parameter.ChannelCount switch
                    {
                        1 => (uint)21392.0f,
                        2 => (uint)26829.0f,
                        4 => (uint)32405.0f,
                        6 => (uint)52219.0f,
                        _ => throw new NotImplementedException($"{parameter.ChannelCount}"),
                    };
                }

                return parameter.ChannelCount switch
                {
                    1 => (uint)897.0f,
                    2 => (uint)931.55f,
                    4 => (uint)975.39f,
                    6 => (uint)1016.8f,
                    _ => throw new NotImplementedException($"{parameter.ChannelCount}"),
                };
            }

            if (enabled)
            {
                return parameter.ChannelCount switch
                {
                    1 => (uint)30556.0f,
                    2 => (uint)39011.0f,
                    4 => (uint)48270.0f,
                    6 => (uint)76712.0f,
                    _ => throw new NotImplementedException($"{parameter.ChannelCount}"),
                };
            }

            return parameter.ChannelCount switch
            {
                1 => (uint)874.43f,
                2 => (uint)921.55f,
                4 => (uint)945.26f,
                6 => (uint)992.26f,
                _ => throw new NotImplementedException($"{parameter.ChannelCount}"),
            };
        }

        public uint Estimate(LimiterCommandVersion1 command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            return EstimateLimiterCommandCommon(command.Parameter, command.IsEffectEnabled);
        }

        public uint Estimate(LimiterCommandVersion2 command)
        {
            Debug.Assert(SampleCount == 160 || SampleCount == 240);

            if (!command.Parameter.StatisticsEnabled || !command.IsEffectEnabled)
            {
                return EstimateLimiterCommandCommon(command.Parameter, command.IsEffectEnabled);
            }

            if (SampleCount == 160)
            {
                return command.Parameter.ChannelCount switch
                {
                    1 => (uint)23309.0f,
                    2 => (uint)29954.0f,
                    4 => (uint)35807.0f,
                    6 => (uint)58340.0f,
                    _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
                };
            }

            return command.Parameter.ChannelCount switch
            {
                1 => (uint)33526.0f,
                2 => (uint)43549.0f,
                4 => (uint)52190.0f,
                6 => (uint)85527.0f,
                _ => throw new NotImplementedException($"{command.Parameter.ChannelCount}"),
            };
        }

        public virtual uint Estimate(MultiTapBiquadFilterCommand command)
        {
            return 0;
        }

        public virtual uint Estimate(CaptureBufferCommand command)
        {
            return 0;
        }

        public virtual uint Estimate(CompressorCommand command)
        {
            return 0;
        }

        public virtual uint Estimate(BiquadFilterAndMixCommand command)
        {
            return 0;
        }

        public virtual uint Estimate(MultiTapBiquadFilterAndMixCommand command)
        {
            return 0;
        }
    }
}
