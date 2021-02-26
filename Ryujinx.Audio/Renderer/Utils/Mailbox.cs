//
// Copyright (c) 2019-2021 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

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
        private BlockingCollection<T> _messageQueue;
        private BlockingCollection<T> _responseQueue;

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
