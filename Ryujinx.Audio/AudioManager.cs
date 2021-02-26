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
using System.Threading;

namespace Ryujinx.Audio
{
    /// <summary>
    /// Manage audio input and output system.
    /// </summary>
    public class AudioManager : IDisposable
    {
        /// <summary>
        /// Lock used to control the waiters registration.
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// Events signaled when the driver played audio buffers.
        /// </summary>
        private ManualResetEvent[] _updateRequiredEvents;

        /// <summary>
        /// Action to execute when the driver played audio buffers.
        /// </summary>
        private Action[] _actions;

        /// <summary>
        /// The worker thread in charge of handling sessions update.
        /// </summary>
        private Thread _workerThread;

        /// <summary>
        /// Create a new <see cref="AudioManager"/>.
        /// </summary>
        public AudioManager()
        {
            _updateRequiredEvents = new ManualResetEvent[2];
            _actions = new Action[2];

            // Termination event.
            _updateRequiredEvents[1] = new ManualResetEvent(false);

            _workerThread = new Thread(Update)
            {
                Name = "AudioManager.Worker"
            };
        }

        /// <summary>
        /// Start the <see cref="AudioManager"/>.
        /// </summary>
        public void Start()
        {
            if (_workerThread.IsAlive)
            {
                throw new InvalidOperationException();
            }

            _workerThread.Start();
        }

        /// <summary>
        /// Initialize update handlers.
        /// </summary>
        /// <param name="updatedRequiredEvent ">The driver event that will get signaled by the device driver when an audio buffer finished playing/being captured</param>
        /// <param name="outputCallback">The callback to call when an audio buffer finished playing</param>
        /// <param name="inputCallback">The callback to call when an audio buffer was captured</param>
        public void Initialize(ManualResetEvent updatedRequiredEvent, Action outputCallback, Action inputCallback)
        {
            lock (_lock)
            {
                _updateRequiredEvents[0] = updatedRequiredEvent;
                _actions[0] = outputCallback;
                _actions[1] = inputCallback;
            }
        }

        /// <summary>
        /// Entrypoint of the <see cref="_workerThread"/> in charge of updating the <see cref="AudioManager"/>.
        /// </summary>
        private void Update()
        {
            while (true)
            {
                int index = WaitHandle.WaitAny(_updateRequiredEvents);

                // Last index is here to indicate thread termination.
                if (index + 1 == _updateRequiredEvents.Length)
                {
                    break;
                }

                lock (_lock)
                {
                    foreach (Action action in _actions)
                    {
                        action?.Invoke();
                    }

                    _updateRequiredEvents[0].Reset();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _updateRequiredEvents[1].Set();
                _workerThread.Join();

                _updateRequiredEvents[1].Dispose();
            }
        }
    }
}
