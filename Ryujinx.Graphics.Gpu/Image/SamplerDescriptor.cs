using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Sampler;

namespace Ryujinx.Graphics.Gpu.Image
{
    struct SamplerDescriptor
    {
        private static readonly float[] _f5ToF32ConversionLut = new float[]
        {
            0.0f,
            0.055555556f,
            0.1f,
            0.13636364f,
            0.16666667f,
            0.1923077f,
            0.21428572f,
            0.23333333f,
            0.25f,
            0.2777778f,
            0.3f,
            0.3181818f,
            0.33333334f,
            0.34615386f,
            0.35714287f,
            0.36666667f,
            0.375f,
            0.3888889f,
            0.4f,
            0.4090909f,
            0.41666666f,
            0.42307693f,
            0.42857143f,
            0.43333334f,
            0.4375f,
            0.44444445f,
            0.45f,
            0.45454547f,
            0.45833334f,
            0.46153846f,
            0.4642857f,
            0.46666667f
        };

        private static readonly float[] _maxAnisotropyLut = new float[]
        {
            1, 2, 4, 6, 8, 10, 12, 16
        };

        private const float Frac8ToF32 = 1.0f / 256.0f;

        public uint Word0;
        public uint Word1;
        public uint Word2;
        public uint Word3;
        public uint BorderColorR;
        public uint BorderColorG;
        public uint BorderColorB;
        public uint BorderColorA;

        public AddressMode UnpackAddressU()
        {
            return (AddressMode)(Word0 & 7);
        }

        public AddressMode UnpackAddressV()
        {
            return (AddressMode)((Word0 >> 3) & 7);
        }

        public AddressMode UnpackAddressP()
        {
            return (AddressMode)((Word0 >> 6) & 7);
        }

        public CompareMode UnpackCompareMode()
        {
            return (CompareMode)((Word0 >> 9) & 1);
        }

        public CompareOp UnpackCompareOp()
        {
            return (CompareOp)(((Word0 >> 10) & 7) + 1);
        }

        public float UnpackMaxAnisotropy()
        {
            return _maxAnisotropyLut[(Word0 >> 20) & 7];
        }

        public MagFilter UnpackMagFilter()
        {
            return (MagFilter)(Word1 & 3);
        }

        public MinFilter UnpackMinFilter()
        {
            int minFilter = (int)(Word1 >> 4) & 3;
            int mipFilter = (int)(Word1 >> 6) & 3;

            return (MinFilter)(minFilter + (mipFilter - 1) * 2);
        }

        public ReductionFilter UnpackReductionFilter()
        {
            return (ReductionFilter)((Word1 >> 10) & 3);
        }

        public float UnpackMipLodBias()
        {
            int fixedValue = (int)(Word1 >> 12) & 0x1fff;

            fixedValue = (fixedValue << 19) >> 19;

            return fixedValue * Frac8ToF32;
        }

        public float UnpackLodSnap()
        {
            return _f5ToF32ConversionLut[(Word1 >> 26) & 0x1f];
        }

        public float UnpackMinLod()
        {
            return (Word2 & 0xfff) * Frac8ToF32;
        }

        public float UnpackMaxLod()
        {
            return ((Word2 >> 12) & 0xfff) * Frac8ToF32;
        }
    }
}
