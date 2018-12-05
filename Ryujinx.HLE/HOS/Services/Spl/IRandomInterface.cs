using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Ryujinx.HLE.HOS.Services.Spl
{
    class IRandomInterface : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private RNGCryptoServiceProvider Rng;

        public IRandomInterface()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetRandomBytes }
            };

            Rng = new RNGCryptoServiceProvider();
        }

        public long GetRandomBytes(ServiceCtx Context)
        {
            byte[] RandomBytes = new byte[Context.Request.ReceiveBuff[0].Size];

            Rng.GetBytes(RandomBytes);

            Context.Memory.WriteBytes(Context.Request.ReceiveBuff[0].Position, RandomBytes);

            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                Rng.Dispose();
            }
        }
    }
}