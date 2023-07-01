using Ryujinx.Audio.Integration;
using Ryujinx.Common;
using System;
using System.Diagnostics;

namespace Ryujinx.Audio.Common
{
    /// <summary>
    /// An audio device session.
    /// </summary>
    class AudioDeviceSession : IDisposable
    {
        /// <summary>
        /// The volume of the <see cref="AudioDeviceSession"/>.
        /// </summary>
        private float _volume;

        /// <summary>
        /// The state of the <see cref="AudioDeviceSession"/>.
        /// </summary>
        private AudioDeviceState _state;

        /// <summary>
        /// Array of all buffers currently used or released.
        /// </summary>
        private readonly AudioBuffer[] _buffers;

        /// <summary>
        /// The server index inside <see cref="_buffers"/> (appended but not queued to device driver).
        /// </summary>
        private uint _serverBufferIndex;

        /// <summary>
        /// The hardware index inside <see cref="_buffers"/> (queued to device driver).
        /// </summary>
        private uint _hardwareBufferIndex;

        /// <summary>
        /// The released index inside <see cref="_buffers"/> (released by the device driver).
        /// </summary>
        private uint _releasedBufferIndex;

        /// <summary>
        /// The count of buffer appended (server side).
        /// </summary>
        private uint _bufferAppendedCount;

        /// <summary>
        /// The count of buffer registered (driver side).
        /// </summary>
        private uint _bufferRegisteredCount;

        /// <summary>
        /// The count of buffer released (released by the driver side).
        /// </summary>
        private uint _bufferReleasedCount;

        /// <summary>
        /// The released buffer event.
        /// </summary>
        private readonly IWritableEvent _bufferEvent;

        /// <summary>
        /// The session on the device driver.
        /// </summary>
        private readonly IHardwareDeviceSession _hardwareDeviceSession;

        /// <summary>
        /// Max number of buffers that can be registered to the device driver at a time.
        /// </summary>
        private readonly uint _bufferRegisteredLimit;

        /// <summary>
        /// Create a new <see cref="AudioDeviceSession"/>.
        /// </summary>
        /// <param name="deviceSession">The device driver session associated</param>
        /// <param name="bufferEvent">The release buffer event</param>
        /// <param name="bufferRegisteredLimit">The max number of buffers that can be registered to the device driver at a time</param>
        public AudioDeviceSession(IHardwareDeviceSession deviceSession, IWritableEvent bufferEvent, uint bufferRegisteredLimit = 4)
        {
            _bufferEvent = bufferEvent;
            _hardwareDeviceSession = deviceSession;
            _bufferRegisteredLimit = bufferRegisteredLimit;

            _buffers = new AudioBuffer[Constants.AudioDeviceBufferCountMax];
            _serverBufferIndex = 0;
            _hardwareBufferIndex = 0;
            _releasedBufferIndex = 0;

            _bufferAppendedCount = 0;
            _bufferRegisteredCount = 0;
            _bufferReleasedCount = 0;
            _volume = deviceSession.GetVolume();
            _state = AudioDeviceState.Stopped;
        }

        /// <summary>
        /// Get the released buffer event.
        /// </summary>
        /// <returns>The released buffer event</returns>
        public IWritableEvent GetBufferEvent()
        {
            return _bufferEvent;
        }

        /// <summary>
        /// Get the state of the session.
        /// </summary>
        /// <returns>The state of the session</returns>
        public AudioDeviceState GetState()
        {
            Debug.Assert(_state == AudioDeviceState.Started || _state == AudioDeviceState.Stopped);

            return _state;
        }

        /// <summary>
        /// Get the total buffer count (server + driver + released).
        /// </summary>
        /// <returns>Return the total buffer count</returns>
        private uint GetTotalBufferCount()
        {
            uint bufferCount = _bufferAppendedCount + _bufferRegisteredCount + _bufferReleasedCount;

            Debug.Assert(bufferCount <= Constants.AudioDeviceBufferCountMax);

            return bufferCount;
        }

        /// <summary>
        /// Register a new <see cref="AudioBuffer"/> on the server side.
        /// </summary>
        /// <param name="buffer">The <see cref="AudioBuffer"/> to register</param>
        /// <returns>True if the operation succeeded</returns>
        private bool RegisterBuffer(AudioBuffer buffer)
        {
            if (GetTotalBufferCount() == Constants.AudioDeviceBufferCountMax)
            {
                return false;
            }

            _buffers[_serverBufferIndex] = buffer;
            _serverBufferIndex = (_serverBufferIndex + 1) % Constants.AudioDeviceBufferCountMax;
            _bufferAppendedCount++;

            return true;
        }

        /// <summary>
        /// Flush server buffers to hardware.
        /// </summary>
        private void FlushToHardware()
        {
            uint bufferToFlushCount = Math.Min(Math.Min(_bufferAppendedCount, 4), _bufferRegisteredLimit - _bufferRegisteredCount);

            AudioBuffer[] buffersToFlush = new AudioBuffer[bufferToFlushCount];

            uint hardwareBufferIndex = _hardwareBufferIndex;

            for (int i = 0; i < buffersToFlush.Length; i++)
            {
                buffersToFlush[i] = _buffers[hardwareBufferIndex];

                _bufferAppendedCount--;
                _bufferRegisteredCount++;

                hardwareBufferIndex = (hardwareBufferIndex + 1) % Constants.AudioDeviceBufferCountMax;
            }

            _hardwareBufferIndex = hardwareBufferIndex;

            for (int i = 0; i < buffersToFlush.Length; i++)
            {
                _hardwareDeviceSession.QueueBuffer(buffersToFlush[i]);
            }
        }

        /// <summary>
        /// Get the current index of the <see cref="AudioBuffer"/> playing on the driver side.
        /// </summary>
        /// <param name="playingIndex">The output index of the <see cref="AudioBuffer"/> playing on the driver side</param>
        /// <returns>True if any buffer is playing</returns>
        private bool TryGetPlayingBufferIndex(out uint playingIndex)
        {
            if (_bufferRegisteredCount > 0)
            {
                playingIndex = (_hardwareBufferIndex - _bufferRegisteredCount) % Constants.AudioDeviceBufferCountMax;

                return true;
            }

            playingIndex = 0;

            return false;
        }

        /// <summary>
        /// Try to pop the <see cref="AudioBuffer"/> playing on the driver side.
        /// </summary>
        /// <param name="buffer">The output <see cref="AudioBuffer"/> playing on the driver side</param>
        /// <returns>True if any buffer is playing</returns>
        private bool TryPopPlayingBuffer(out AudioBuffer buffer)
        {
            if (_bufferRegisteredCount > 0)
            {
                uint bufferIndex = (_hardwareBufferIndex - _bufferRegisteredCount) % Constants.AudioDeviceBufferCountMax;

                buffer = _buffers[bufferIndex];

                _buffers[bufferIndex] = null;

                _bufferRegisteredCount--;

                return true;
            }

            buffer = null;

            return false;
        }

        /// <summary>
        /// Try to pop a <see cref="AudioBuffer"/> released by the driver side.
        /// </summary>
        /// <param name="buffer">The output <see cref="AudioBuffer"/> released by the driver side</param>
        /// <returns>True if any buffer has been released</returns>
        public bool TryPopReleasedBuffer(out AudioBuffer buffer)
        {
            if (_bufferReleasedCount > 0)
            {
                uint bufferIndex = (_releasedBufferIndex - _bufferReleasedCount) % Constants.AudioDeviceBufferCountMax;

                buffer = _buffers[bufferIndex];

                _buffers[bufferIndex] = null;

                _bufferReleasedCount--;

                return true;
            }

            buffer = null;

            return false;
        }

        /// <summary>
        /// Release a <see cref="AudioBuffer"/>.
        /// </summary>
        /// <param name="buffer">The <see cref="AudioBuffer"/> to release</param>
        private void ReleaseBuffer(AudioBuffer buffer)
        {
            buffer.PlayedTimestamp = (ulong)PerformanceCounter.ElapsedNanoseconds;

            _bufferRegisteredCount--;
            _bufferReleasedCount++;

            _releasedBufferIndex = (_releasedBufferIndex + 1) % Constants.AudioDeviceBufferCountMax;
        }

        /// <summary>
        /// Update the released buffers.
        /// </summary>
        /// <param name="updateForStop">True if the session is currently stopping</param>
        private void UpdateReleaseBuffers(bool updateForStop = false)
        {
            bool wasAnyBuffersReleased = false;

            while (TryGetPlayingBufferIndex(out uint playingIndex))
            {
                if (!updateForStop && !_hardwareDeviceSession.WasBufferFullyConsumed(_buffers[playingIndex]))
                {
                    break;
                }

                if (updateForStop)
                {
                    _hardwareDeviceSession.UnregisterBuffer(_buffers[playingIndex]);
                }

                ReleaseBuffer(_buffers[playingIndex]);

                wasAnyBuffersReleased = true;
            }

            if (wasAnyBuffersReleased)
            {
                _bufferEvent.Signal();
            }
        }

        /// <summary>
        /// Append a new <see cref="AudioBuffer"/>.
        /// </summary>
        /// <param name="buffer">The <see cref="AudioBuffer"/> to append</param>
        /// <returns>True if the buffer was appended</returns>
        public bool AppendBuffer(AudioBuffer buffer)
        {
            if (_hardwareDeviceSession.RegisterBuffer(buffer))
            {
                if (RegisterBuffer(buffer))
                {
                    FlushToHardware();

                    return true;
                }

                _hardwareDeviceSession.UnregisterBuffer(buffer);
            }

            return false;
        }

        public static bool AppendUacBuffer(AudioBuffer buffer, uint handle)
        {
            // NOTE: On hardware, there is another RegisterBuffer method taking a handle.
            // This variant of the call always return false (stubbed?) as a result this logic will never succeed.

            return false;
        }

        /// <summary>
        /// Start the audio session.
        /// </summary>
        /// <returns>A <see cref="ResultCode"/> reporting an error or a success</returns>
        public ResultCode Start()
        {
            if (_state == AudioDeviceState.Started)
            {
                return ResultCode.OperationFailed;
            }

            _hardwareDeviceSession.Start();

            _state = AudioDeviceState.Started;

            FlushToHardware();

            _hardwareDeviceSession.SetVolume(_volume);

            return ResultCode.Success;
        }

        /// <summary>
        /// Stop the audio session.
        /// </summary>
        /// <returns>A <see cref="ResultCode"/> reporting an error or a success</returns>
        public ResultCode Stop()
        {
            if (_state == AudioDeviceState.Started)
            {
                _hardwareDeviceSession.Stop();

                UpdateReleaseBuffers(true);

                _state = AudioDeviceState.Stopped;
            }

            return ResultCode.Success;
        }

        /// <summary>
        /// Get the volume of the session.
        /// </summary>
        /// <returns>The volume of the session</returns>
        public float GetVolume()
        {
            return _hardwareDeviceSession.GetVolume();
        }

        /// <summary>
        /// Set the volume of the session.
        /// </summary>
        /// <param name="volume">The new volume to set</param>
        public void SetVolume(float volume)
        {
            _volume = volume;

            if (_state == AudioDeviceState.Started)
            {
                _hardwareDeviceSession.SetVolume(volume);
            }
        }

        /// <summary>
        /// Get the count of buffer currently in use (server + driver side).
        /// </summary>
        /// <returns>The count of buffer currently in use</returns>
        public uint GetBufferCount()
        {
            return _bufferAppendedCount + _bufferRegisteredCount;
        }

        /// <summary>
        /// Check if a buffer is present.
        /// </summary>
        /// <param name="bufferTag">The unique tag of the buffer</param>
        /// <returns>Return true if a buffer is present</returns>
        public bool ContainsBuffer(ulong bufferTag)
        {
            uint bufferIndex = (_releasedBufferIndex - _bufferReleasedCount) % Constants.AudioDeviceBufferCountMax;

            uint totalBufferCount = GetTotalBufferCount();

            for (int i = 0; i < totalBufferCount; i++)
            {
                if (_buffers[bufferIndex].BufferTag == bufferTag)
                {
                    return true;
                }

                bufferIndex = (bufferIndex + 1) % Constants.AudioDeviceBufferCountMax;
            }

            return false;
        }

        /// <summary>
        /// Get the count of sample played in this session.
        /// </summary>
        /// <returns>The count of sample played in this session</returns>
        public ulong GetPlayedSampleCount()
        {
            if (_state == AudioDeviceState.Stopped)
            {
                return 0;
            }

            return _hardwareDeviceSession.GetPlayedSampleCount();
        }

        /// <summary>
        /// Flush all buffers to the initial state.
        /// </summary>
        /// <returns>True if any buffer was flushed</returns>
        public bool FlushBuffers()
        {
            if (_state == AudioDeviceState.Stopped)
            {
                return false;
            }

            uint bufferCount = GetBufferCount();

            while (TryPopReleasedBuffer(out AudioBuffer buffer))
            {
                _hardwareDeviceSession.UnregisterBuffer(buffer);
            }

            while (TryPopPlayingBuffer(out AudioBuffer buffer))
            {
                _hardwareDeviceSession.UnregisterBuffer(buffer);
            }

            if (_bufferRegisteredCount == 0 || (_bufferReleasedCount + _bufferAppendedCount) > Constants.AudioDeviceBufferCountMax)
            {
                return false;
            }

            _bufferReleasedCount += _bufferAppendedCount;
            _releasedBufferIndex = (_releasedBufferIndex + _bufferAppendedCount) % Constants.AudioDeviceBufferCountMax;
            _bufferAppendedCount = 0;
            _hardwareBufferIndex = _serverBufferIndex;

            if (bufferCount > 0)
            {
                _bufferEvent.Signal();
            }

            return true;
        }

        /// <summary>
        /// Update the session.
        /// </summary>
        public void Update()
        {
            if (_state == AudioDeviceState.Started)
            {
                UpdateReleaseBuffers();
                FlushToHardware();
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
                // Tell the hardware session that we are ending.
                _hardwareDeviceSession.PrepareToClose();

                // Unregister all buffers

                while (TryPopReleasedBuffer(out AudioBuffer buffer))
                {
                    _hardwareDeviceSession.UnregisterBuffer(buffer);
                }

                while (TryPopPlayingBuffer(out AudioBuffer buffer))
                {
                    _hardwareDeviceSession.UnregisterBuffer(buffer);
                }

                // Finally dispose hardware session.
                _hardwareDeviceSession.Dispose();

                _bufferEvent.Signal();
            }
        }
    }
}
