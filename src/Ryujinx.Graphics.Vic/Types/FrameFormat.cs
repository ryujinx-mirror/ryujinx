namespace Ryujinx.Graphics.Vic.Types
{
    enum FrameFormat
    {
        Progressive,
        InterlacedTopFieldFirst,
        InterlacedBottomFieldFirst,
        TopField,
        BottomField,
        SubPicProgressive,
        SubPicInterlacedTopFieldFirst,
        SubPicInterlacedBottomFieldFirst,
        SubPicTopField,
        SubPicBottomField,
        TopFieldChromaBottom,
        BottomFieldChromaTop,
        SubPicTopFieldChromaBottom,
        SubPicBottomFieldChromaTop,
    }

    static class FrameFormatExtensions
    {
        public static bool IsField(this FrameFormat frameFormat)
        {
            switch (frameFormat)
            {
                case FrameFormat.TopField:
                case FrameFormat.BottomField:
                case FrameFormat.SubPicTopField:
                case FrameFormat.SubPicBottomField:
                case FrameFormat.TopFieldChromaBottom:
                case FrameFormat.BottomFieldChromaTop:
                case FrameFormat.SubPicTopFieldChromaBottom:
                case FrameFormat.SubPicBottomFieldChromaTop:
                    return true;
            }

            return false;
        }

        public static bool IsInterlaced(this FrameFormat frameFormat)
        {
            switch (frameFormat)
            {
                case FrameFormat.InterlacedTopFieldFirst:
                case FrameFormat.InterlacedBottomFieldFirst:
                case FrameFormat.SubPicInterlacedTopFieldFirst:
                case FrameFormat.SubPicInterlacedBottomFieldFirst:
                    return true;
            }

            return false;
        }

        public static bool IsInterlacedBottomFirst(this FrameFormat frameFormat)
        {
            return frameFormat == FrameFormat.InterlacedBottomFieldFirst ||
                   frameFormat == FrameFormat.SubPicInterlacedBottomFieldFirst;
        }

        public static bool IsTopField(this FrameFormat frameFormat, bool isLuma)
        {
            switch (frameFormat)
            {
                case FrameFormat.TopField:
                case FrameFormat.SubPicTopField:
                    return true;
                case FrameFormat.TopFieldChromaBottom:
                case FrameFormat.SubPicTopFieldChromaBottom:
                    return isLuma;
                case FrameFormat.BottomFieldChromaTop:
                case FrameFormat.SubPicBottomFieldChromaTop:
                    return !isLuma;
            }

            return false;
        }
    }
}
