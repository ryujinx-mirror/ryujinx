using Ryujinx.Common.Utilities;

namespace Ryujinx.Graphics.Vic.Types
{
    readonly struct PipeConfig
    {
#pragma warning disable CS0169, CS0649, IDE0051 // Remove unused private member
        private readonly long _word0;
        private readonly long _word1;
#pragma warning restore CS0169, CS0649, IDE0051

        public int DownsampleHoriz => (int)_word0.Extract(0, 11);
        public int DownsampleVert => (int)_word0.Extract(16, 11);
    }
}
