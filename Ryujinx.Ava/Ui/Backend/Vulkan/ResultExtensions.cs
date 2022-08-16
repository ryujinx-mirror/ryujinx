using System;
using Silk.NET.Vulkan;

namespace Ryujinx.Ava.Ui.Vulkan
{
    public static class ResultExtensions
    {
        public static void ThrowOnError(this Result result)
        {
            // Only negative result codes are errors.
            if ((int)result < (int)Result.Success)
            {
                throw new Exception($"Unexpected API error \"{result}\".");
            }
        }
    }
}
