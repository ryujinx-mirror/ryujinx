using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon
{
    public static class LibHacResultExtensions
    {
        public static Result ToHorizonResult(this LibHac.Result result)
        {
            return new Result((int)result.Module, (int)result.Description);
        }
    }
}
