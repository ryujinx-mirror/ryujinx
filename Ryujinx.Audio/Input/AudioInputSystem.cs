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

using Ryujinx.Audio.Common;
using Ryujinx.Audio.Integration;
using System;

namespace Ryujinx.Audio.Input
{
    /// <summary>
    /// Audio input system.
    /// </summary>
    public class AudioInputSystem : IDisposable
    {
        /// <summary>
        /// The session id associated to the <see cref="AudioInputSystem"/>.
        /// </summary>
        private int _sessionId;

        /// <summary>
        /// The session the <see cref="AudioInputSystem"/>.
        /// </summary>
        private AudioDeviceSession _session;

        /// <summary>
        /// The target device name of the <see cref="AudioInputSystem"/>.
        /// </summary>
        public string DeviceName { get; private set; }

        /// <summary>
        /// The target sample rate of the <see cref="AudioInputSystem"/>.
        /// </summary>
        public uint SampleRate { get; private set; }

        /// <summary>
        /// The target channel count of the <see cref="AudioInputSystem"/>.
        /// </summary>
        public uint ChannelCount { get; private set; }

        /// <summary>
        /// The target sample format of the <see cref="AudioInputSystem"/>.
        /// </summary>
        public SampleFormat SampleFormat { get; private set; }

        /// <summary>
        /// The <see cref="AudioInputManager"/> owning this.
        /// </summary>
        private AudioInputManager _manager;

        /// <summary>
        /// THe lock of the parent.
        /// </summary>
        private object _parentLock;

        /// <summary>
        /// Create a new <see cref="AudioInputSystem"/>.
        /// </summary>
        /// <param name="manager">The manager instance</param>
        /// <param name="parentLock">The lock of the manager</param>
        /// <param name="deviceSession">The hardware device session</param>
        /// <param name="bufferEvent">The buffer release event of the audio input</param>
        public AudioInputSystem(AudioInputManager manager, object parentLock, IHardwareDeviceSession deviceSession, IWritableEvent bufferEvent)
        {
            _manager = manager;
            _parentLock = parentLock;
            _session = new AudioDeviceSession(deviceSession, bufferEvent);
        }

        /// <summary>
        /// Get the default device name on the system.
        /// </summary>
        /// <returns>The default device name on the system.</returns>
        private static string GetDeviceDefaultName()
        {
            return Constants.DefaultDeviceInputName;
        }

        /// <summary>
        /// Check if a given configuration and device name is valid on the system.
        /// </summary>
        /// <param name="configuration">The configuration to check.</param>
        /// <param name="deviceName">The device name to check.</param>
        /// <returns>A <see cref="ResultCode"/> reporting an error or a success.</returns>
        private static ResultCode IsConfigurationValid(ref AudioInputConfiguration configuration, string deviceName)
        {
            if (deviceName.Length != 0 && !deviceName.Equals(GetDeviceDefaultName()))
            {
                return ResultCode.DeviceNotFound;
            }
            else if (configuration.SampleRate != 0 && configuration.SampleRate != Constants.TargetSampleRate)
            {
                return ResultCode.UnsupportedSampleRate;
            }
            else if (configuration.ChannelCount != 0 && configuration.ChannelCount != 1 && configuration.ChannelCount != 2 && configuration.ChannelCount != 6)
            {
                return ResultCode.UnsupportedChannelConfiguration;
            }

            return ResultCode.Success;
        }

        /// <summary>
        /// Get the released buffer event.
        /// </summary>
        /// <returns>The released buffer event</returns>
        public IWritableEvent RegisterBufferEvent()
        {
            lock (_parentLock)
            {
                return _session.GetBufferEvent();
            }
        }

        /// <summary>
        /// Update the <see cref="AudioInputSystem"/>.
        /// </summary>
        public void Update()
        {
            lock (_parentLock)
            {
                _session.Update();
            }
        }

        /// <summary>
        /// Get the id of this session.
        /// </summary>
        /// <returns>The id of this session</returns>
        public int GetSessionId()
        {
            return _sessionId;
        }

        /// <summary>
        /// Initialize the <see cref="AudioInputSystem"/>.
        /// </summary>
        /// <param name="inputDeviceName">The input device name wanted by the user</param>
        /// <param name="sampleFormat">The sample format to use</param>
        /// <param name="parameter">The user configuration</param>
        /// <param name="sessionId">The session id associated to this <see cref="AudioInputSystem"/></param>
        /// <returns>A <see cref="ResultCode"/> reporting an error or a success.</returns>
        public ResultCode Initialize(string inputDeviceName, SampleFormat sampleFormat, ref AudioInputConfiguration parameter, int sessionId)
        {
            _sessionId = sessionId;

            ResultCode result = IsConfigurationValid(ref parameter, inputDeviceName);

            if (result == ResultCode.Success)
            {
                if (inputDeviceName.Length == 0)
                {
                    DeviceName = GetDeviceDefaultName();
                }
                else
                {
                    DeviceName = inputDeviceName;
                }

                if (parameter.ChannelCount == 6)
                {
                    ChannelCount = 6;
                }
                else
                {
                    ChannelCount = 2;
                }

                SampleFormat = sampleFormat;
                SampleRate = Constants.TargetSampleRate;
            }

            return result;
        }

        /// <summary>
        /// Append a new audio buffer to the audio input.
        /// </summary>
        /// <param name="bufferTag">The unique tag of this buffer.</param>
        /// <param name="userBuffer">The buffer informations.</param>
        /// <returns>A <see cref="ResultCode"/> reporting an error or a success.</returns>
        public ResultCode AppendBuffer(ulong bufferTag, ref AudioUserBuffer userBuffer)
        {
            lock (_parentLock)
            {
                AudioBuffer buffer = new AudioBuffer
                {
                    BufferTag = bufferTag,
                    DataPointer = userBuffer.Data,
                    DataSize = userBuffer.DataSize
                };

                if (_session.AppendBuffer(buffer))
                {
                    return ResultCode.Success;
                }

                return ResultCode.BufferRingFull;
            }
        }

        /// <summary>
        /// Append a new audio buffer to the audio input.
        /// </summary>
        /// <remarks>This is broken by design, only added for completness.</remarks>
        /// <param name="bufferTag">The unique tag of this buffer.</param>
        /// <param name="userBuffer">The buffer informations.</param>
        /// <param name="handle">Some unknown handle.</param>
        /// <returns>A <see cref="ResultCode"/> reporting an error or a success.</returns>
        public ResultCode AppendUacBuffer(ulong bufferTag, ref AudioUserBuffer userBuffer, uint handle)
        {
            lock (_parentLock)
            {
                AudioBuffer buffer = new AudioBuffer
                {
                    BufferTag = bufferTag,
                    DataPointer = userBuffer.Data,
                    DataSize = userBuffer.DataSize
                };

                if (_session.AppendUacBuffer(buffer, handle))
                {
                    return ResultCode.Success;
                }

                return ResultCode.BufferRingFull;
            }
        }

        /// <summary>
        /// Get the release buffers.
        /// </summary>
        /// <param name="releasedBuffers">The buffer to write the release buffers</param>
        /// <param name="releasedCount">The count of released buffers</param>
        /// <returns>A <see cref="ResultCode"/> reporting an error or a success.</returns>
        public ResultCode GetReleasedBuffers(Span<ulong> releasedBuffers, out uint releasedCount)
        {
            releasedCount = 0;

            // Ensure that the first entry is set to zero if no entries are returned.
            if (releasedBuffers.Length > 0)
            {
                releasedBuffers[0] = 0;
            }

            lock (_parentLock)
            {
                for (int i = 0; i < releasedBuffers.Length; i++)
                {
                    if (!_session.TryPopReleasedBuffer(out AudioBuffer buffer))
                    {
                        break;
                    }

                    releasedBuffers[i] = buffer.BufferTag;
                    releasedCount++;
                }
            }

            return ResultCode.Success;
        }

        /// <summary>
        /// Get the current state of the <see cref="AudioInputSystem"/>.
        /// </summary>
        /// <returns>Return the curent sta\te of the <see cref="AudioInputSystem"/></returns>
        public AudioDeviceState GetState()
        {
            lock (_parentLock)
            {
                return _session.GetState();
            }
        }

        /// <summary>
        /// Start the audio session.
        /// </summary>
        /// <returns>A <see cref="ResultCode"/> reporting an error or a success</returns>
        public ResultCode Start()
        {
            lock (_parentLock)
            {
                return _session.Start();
            }
        }

        /// <summary>
        /// Stop the audio session.
        /// </summary>
        /// <returns>A <see cref="ResultCode"/> reporting an error or a success</returns>
        public ResultCode Stop()
        {
            lock (_parentLock)
            {
                return _session.Stop();
            }
        }

        /// <summary>
        /// Get the volume of the session.
        /// </summary>
        /// <returns>The volume of the session</returns>
        public float GetVolume()
        {
            lock (_parentLock)
            {
                return _session.GetVolume();
            }
        }

        /// <summary>
        /// Set the volume of the session.
        /// </summary>
        /// <param name="volume">The new volume to set</param>
        public void SetVolume(float volume)
        {
            lock (_parentLock)
            {
                _session.SetVolume(volume);
            }
        }

        /// <summary>
        /// Get the count of buffer currently in use (server + driver side).
        /// </summary>
        /// <returns>The count of buffer currently in use</returns>
        public uint GetBufferCount()
        {
            lock (_parentLock)
            {
                return _session.GetBufferCount();
            }
        }

        /// <summary>
        /// Check if a buffer is present.
        /// </summary>
        /// <param name="bufferTag">The unique tag of the buffer</param>
        /// <returns>Return true if a buffer is present</returns>
        public bool ContainsBuffer(ulong bufferTag)
        {
            lock (_parentLock)
            {
                return _session.ContainsBuffer(bufferTag);
            }
        }

        /// <summary>
        /// Get the count of sample played in this session.
        /// </summary>
        /// <returns>The count of sample played in this session</returns>
        public ulong GetPlayedSampleCount()
        {
            lock (_parentLock)
            {
                return _session.GetPlayedSampleCount();
            }
        }

        /// <summary>
        /// Flush all buffers to the initial state.
        /// </summary>
        /// <returns>True if any buffers was flushed</returns>
        public bool FlushBuffers()
        {
            lock (_parentLock)
            {
                return _session.FlushBuffers();
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
                _session.Dispose();

                _manager.Unregister(this);
            }
        }
    }
}
