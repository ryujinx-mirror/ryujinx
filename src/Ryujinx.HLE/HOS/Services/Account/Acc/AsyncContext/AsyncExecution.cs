using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.HLE.HOS.Services.Account.Acc.AsyncContext
{
    class AsyncExecution
    {
        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _token;

        public KEvent SystemEvent { get; }
        public bool IsInitialized { get; private set; }
        public bool IsRunning { get; private set; }

        public AsyncExecution(KEvent asyncEvent)
        {
            SystemEvent = asyncEvent;

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        public void Initialize(int timeout, Func<CancellationToken, Task> taskAsync)
        {
            Task.Run(async () =>
            {
                IsRunning = true;

                _tokenSource.CancelAfter(timeout);

                try
                {
                    await taskAsync(_token);
                }
                catch (Exception ex)
                {
                    Logger.Warning?.Print(LogClass.ServiceAcc, $"Exception: {ex.Message}");
                }

                SystemEvent.ReadableEvent.Signal();

                IsRunning = false;
            }, _token);

            IsInitialized = true;
        }

        public void Cancel()
        {
            _tokenSource.Cancel();
        }
    }
}
