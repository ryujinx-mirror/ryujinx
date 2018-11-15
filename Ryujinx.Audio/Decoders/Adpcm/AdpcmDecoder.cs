namespace Ryujinx.Audio.Adpcm
{
    public static class AdpcmDecoder
    {
        private const int SamplesPerFrame = 14;
        private const int BytesPerFrame   = 8;

        public static int[] Decode(byte[] Buffer, AdpcmDecoderContext Context)
        {
            int Samples = GetSamplesCountFromSize(Buffer.Length);

            int[] Pcm = new int[Samples * 2];

            short History0 = Context.History0;
            short History1 = Context.History1;

            int InputOffset  = 0;
            int OutputOffset = 0;

            while (InputOffset < Buffer.Length)
            {
                byte Header = Buffer[InputOffset++];

                int Scale = 0x800 << (Header & 0xf);

                int CoeffIndex = (Header >> 4) & 7;

                short Coeff0 = Context.Coefficients[CoeffIndex * 2 + 0];
                short Coeff1 = Context.Coefficients[CoeffIndex * 2 + 1];

                int FrameSamples = SamplesPerFrame;

                if (FrameSamples > Samples)
                {
                    FrameSamples = Samples;
                }

                int Value = 0;

                for (int SampleIndex = 0; SampleIndex < FrameSamples; SampleIndex++)
                {
                    int Sample;

                    if ((SampleIndex & 1) == 0)
                    {
                        Value = Buffer[InputOffset++];

                        Sample = (Value << 24) >> 28;
                    }
                    else
                    {
                        Sample = (Value << 28) >> 28;
                    }

                    int Prediction = Coeff0 * History0 + Coeff1 * History1;

                    Sample = (Sample * Scale + Prediction + 0x400) >> 11;

                    short SaturatedSample = DspUtils.Saturate(Sample);

                    History1 = History0;
                    History0 = SaturatedSample;

                    Pcm[OutputOffset++] = SaturatedSample;
                    Pcm[OutputOffset++] = SaturatedSample;
                }

                Samples -= FrameSamples;
            }

            Context.History0 = History0;
            Context.History1 = History1;

            return Pcm;
        }

        public static long GetSizeFromSamplesCount(int SamplesCount)
        {
            int Frames = SamplesCount / SamplesPerFrame;

            return Frames * BytesPerFrame;
        }

        public static int GetSamplesCountFromSize(long Size)
        {
            int Frames = (int)(Size / BytesPerFrame);

            return Frames * SamplesPerFrame;
        }
    }
}