using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE
{
    internal class AppletSession
    {
        private readonly IAppletFifo<byte[]> _inputData;
        private readonly IAppletFifo<byte[]> _outputData;

        public event EventHandler DataAvailable;

        public int Length
        {
            get { return _inputData.Count; }
        }

        public AppletSession()
            : this(new AppletFifo<byte[]>(),
                   new AppletFifo<byte[]>())
        { }

        public AppletSession(
            IAppletFifo<byte[]> inputData,
            IAppletFifo<byte[]> outputData)
        {
            _inputData = inputData;
            _outputData = outputData;

            _inputData.DataAvailable += OnDataAvailable;
        }

        private void OnDataAvailable(object sender, EventArgs e)
        {
            DataAvailable?.Invoke(this, null);
        }

        public void Push(byte[] item)
        {
            if (!this.TryPush(item))
            {
                // TODO(jduncanator): Throw a proper exception
                throw new InvalidOperationException();
            }
        }

        public bool TryPush(byte[] item)
        {
            return _outputData.TryAdd(item);
        }

        public byte[] Pop()
        {
            if (this.TryPop(out byte[] item))
            {
                return item;
            }

            throw new InvalidOperationException("Input data empty.");
        }

        public bool TryPop(out byte[] item)
        {
            return _inputData.TryTake(out item);
        }

        /// <summary>
        /// This returns an AppletSession that can be used at the
        /// other end of the pipe. Pushing data into this new session
        /// will put it in the first session's input buffer, and vice
        /// versa.
        /// </summary>
        public AppletSession GetConsumer()
        {
            return new AppletSession(this._outputData, this._inputData);
        }
    }
}
