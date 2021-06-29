using System;
using System.Security.Cryptography;

namespace Ryujinx.HLE.HOS.Services.Spl
{
    [Service("csrng")]
    class IRandomInterface : DisposableIpcService
    {
        private RNGCryptoServiceProvider _rng;

        private object _lock = new object();

        public IRandomInterface(ServiceCtx context)
        {
            _rng = new RNGCryptoServiceProvider();
        }

        [CommandHipc(0)]
        // GetRandomBytes() -> buffer<unknown, 6>
        public ResultCode GetRandomBytes(ServiceCtx context)
        {
            byte[] randomBytes = new byte[context.Request.ReceiveBuff[0].Size];

            _rng.GetBytes(randomBytes);

            context.Memory.Write(context.Request.ReceiveBuff[0].Position, randomBytes);

            return ResultCode.Success;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _rng.Dispose();
            }
        }
    }
}