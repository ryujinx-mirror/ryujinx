using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Ngc;
using Ryujinx.Horizon.Sdk.Ngc.Detail;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Ryujinx.Horizon.Ngc.Ipc
{
    partial class Service : INgcService
    {
        private readonly ProfanityFilter _profanityFilter;

        public Service(ProfanityFilter profanityFilter)
        {
            _profanityFilter = profanityFilter;
        }

        [CmifCommand(0)]
        public Result GetContentVersion(out uint version)
        {
            lock (_profanityFilter)
            {
                return _profanityFilter.GetContentVersion(out version);
            }
        }

        [CmifCommand(1)]
        public Result Check(
            out uint checkMask,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<byte> text,
            uint regionMask,
            ProfanityFilterOption option)
        {
            lock (_profanityFilter)
            {
                return _profanityFilter.CheckProfanityWords(out checkMask, text, regionMask, option);
            }
        }

        [CmifCommand(2)]
        public Result Mask(
            out int maskedWordsCount,
            [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias)] Span<byte> filteredText,
            [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] ReadOnlySpan<byte> text,
            uint regionMask,
            ProfanityFilterOption option)
        {
            lock (_profanityFilter)
            {
                int length = Math.Min(filteredText.Length, text.Length);

                text[..length].CopyTo(filteredText[..length]);

                return _profanityFilter.MaskProfanityWordsInText(out maskedWordsCount, filteredText, regionMask, option);
            }
        }

        [CmifCommand(3)]
        public Result Reload()
        {
            lock (_profanityFilter)
            {
                return _profanityFilter.Reload();
            }
        }
    }
}
