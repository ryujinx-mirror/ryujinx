using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Settings.System
{
    [Flags]
    enum TvFlag : uint
    {
        Allows4k = 1 << 0,
        Allows3d = 1 << 1,
        AllowsCec = 1 << 2,
        PreventsScreenBurnIn = 1 << 3,
    }

    enum TvResolution : uint
    {
        Auto,
        At1080p,
        At720p,
        At480p,
    }

    enum HdmiContentType : uint
    {
        None,
        Graphics,
        Cinema,
        Photo,
        Game,
    }

    enum RgbRange : uint
    {
        Auto,
        Full,
        Limited,
    }

    enum CmuMode : uint
    {
        None,
        ColorInvert,
        HighContrast,
        GrayScale,
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x20, Pack = 0x4)]
    struct TvSettings
    {
        public TvFlag Flags;
        public TvResolution TvResolution;
        public HdmiContentType HdmiContentType;
        public RgbRange RgbRange;
        public CmuMode CmuMode;
        public float TvUnderscan;
        public float TvGamma;
        public float ContrastRatio;
    }
}
