using System;
using System.Collections.Concurrent;

namespace Ryujinx.Audio.Renderer.Utils
{
    /// <summary>
    /// A simple generic message queue for unmanaged types.
    /// </summary>
    /// <typeparam name="T">The target unmanaged type used</typeparam>
    public class Mailbox<T> : IDisposable where T : unmanaged
    {
        private readonly BlockingCollection<T> _messageQueue;
        private readonly BlockingCollection<T> _responseQueue;

        public Mailbox()
        {
            _messageQueue = new BlockingCollection<T>(1);
            _responseQueue = new BlockingCollection<T>(1);
        }

        public void SendMessage(T data)
        {
            _messageQueue.Add(data);
        }

        public void SendResponse(T data)
        {
            _responseQueue.Add(data);
        }

        public T ReceiveMessage()
        {
            return _messageQueue.Take();
        }

        public T ReceiveResponse()
        {
            return _responseQueue.Take();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _messageQueue.Dispose();
                _responseQueue.Dispose();
            }
        }
    }
}
