using Ryujinx.Common.Utilities;

namespace Ryujinx.Graphics.Vic.Types
{
    readonly struct ClearRectStruct
    {
#pragma warning disable CS0649 // Field is never assigned to
        private readonly long _word0;
        private readonly long _word1;
#pragma warning restore CS0649

        public int ClearRect0Left => (int)_word0.Extract(0, 14);
        public int ClearRect0Right => (int)_word0.Extract(16, 14);
        public int ClearRect0Top => (int)_word0.Extract(32, 14);
        public int ClearRect0Bottom => (int)_word0.Extract(48, 14);
        public int ClearRect1Left => (int)_word1.Extract(64, 14);
        public int ClearRect1Right => (int)_word1.Extract(80, 14);
        public int ClearRect1Top => (int)_word1.Extract(96, 14);
        public int ClearRect1Bottom => (int)_word1.Extract(112, 14);
    }
}
