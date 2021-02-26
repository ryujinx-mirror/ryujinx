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

using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Utils;
using System;
using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Server.Voice
{
    /// <summary>
    /// Voice context.
    /// </summary>
    public class VoiceContext
    {
        /// <summary>
        /// Storage of the sorted indices to <see cref="VoiceState"/>.
        /// </summary>
        private Memory<int> _sortedVoices;

        /// <summary>
        /// Storage for <see cref="VoiceState"/>.
        /// </summary>
        private Memory<VoiceState> _voices;

        /// <summary>
        /// Storage for <see cref="VoiceChannelResource"/>.
        /// </summary>
        private Memory<VoiceChannelResource> _voiceChannelResources;

        /// <summary>
        /// Storage for <see cref="VoiceUpdateState"/> that are used during audio renderer server updates.
        /// </summary>
        private Memory<VoiceUpdateState> _voiceUpdateStatesCpu;

        /// <summary>
        /// Storage for <see cref="VoiceUpdateState"/> for the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        private Memory<VoiceUpdateState> _voiceUpdateStatesDsp;

        /// <summary>
        /// The total voice count.
        /// </summary>
        private uint _voiceCount;

        public void Initialize(Memory<int> sortedVoices, Memory<VoiceState> voices, Memory<VoiceChannelResource> voiceChannelResources, Memory<VoiceUpdateState> voiceUpdateStatesCpu, Memory<VoiceUpdateState> voiceUpdateStatesDsp, uint voiceCount)
        {
            _sortedVoices = sortedVoices;
            _voices = voices;
            _voiceChannelResources = voiceChannelResources;
            _voiceUpdateStatesCpu = voiceUpdateStatesCpu;
            _voiceUpdateStatesDsp = voiceUpdateStatesDsp;
            _voiceCount = voiceCount;
        }

        /// <summary>
        /// Get the total voice count.
        /// </summary>
        /// <returns>The total voice count.</returns>
        public uint GetCount()
        {
            return _voiceCount;
        }

        /// <summary>
        /// Get a reference to a <see cref="VoiceChannelResource"/> at the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The index to use.</param>
        /// <returns>A reference to a <see cref="VoiceChannelResource"/> at the given <paramref name="id"/>.</returns>
        public ref VoiceChannelResource GetChannelResource(int id)
        {
            return ref SpanIOHelper.GetFromMemory(_voiceChannelResources, id, _voiceCount);
        }

        /// <summary>
        /// Get a <see cref="Memory{VoiceUpdateState}"/> at the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The index to use.</param>
        /// <returns>A <see cref="Memory{VoiceUpdateState}"/> at the given <paramref name="id"/>.</returns>
        /// <remarks>The returned <see cref="Memory{VoiceUpdateState}"/> should only be used when updating the server state.</remarks>
        public Memory<VoiceUpdateState> GetUpdateStateForCpu(int id)
        {
            return SpanIOHelper.GetMemory(_voiceUpdateStatesCpu, id, _voiceCount);
        }

        /// <summary>
        /// Get a <see cref="Memory{VoiceUpdateState}"/> at the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The index to use.</param>
        /// <returns>A <see cref="Memory{VoiceUpdateState}"/> at the given <paramref name="id"/>.</returns>
        /// <remarks>The returned <see cref="Memory{VoiceUpdateState}"/> should only be used in the context of processing on the <see cref="Dsp.AudioProcessor"/>.</remarks>
        public Memory<VoiceUpdateState> GetUpdateStateForDsp(int id)
        {
            return SpanIOHelper.GetMemory(_voiceUpdateStatesDsp, id, _voiceCount);
        }

        /// <summary>
        /// Get a reference to a <see cref="VoiceState"/> at the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The index to use.</param>
        /// <returns>A reference to a <see cref="VoiceState"/> at the given <paramref name="id"/>.</returns>
        public ref VoiceState GetState(int id)
        {
            return ref SpanIOHelper.GetFromMemory(_voices, id, _voiceCount);
        }

        public ref VoiceState GetSortedState(int id)
        {
            Debug.Assert(id >= 0 && id < _voiceCount);

            return ref GetState(_sortedVoices.Span[id]);
        }

        /// <summary>
        /// Update internal state during command generation.
        /// </summary>
        public void UpdateForCommandGeneration()
        {
            _voiceUpdateStatesDsp.CopyTo(_voiceUpdateStatesCpu);
        }

        /// <summary>
        /// Sort the internal voices by priority and sorting order (if the priorities match).
        /// </summary>
        public void Sort()
        {
            for (int i = 0; i < _voiceCount; i++)
            {
                _sortedVoices.Span[i] = i;
            }

            int[] sortedVoicesTemp = _sortedVoices.Slice(0, (int)GetCount()).ToArray();

            Array.Sort(sortedVoicesTemp, (a, b) =>
            {
                ref VoiceState aState = ref GetState(a);
                ref VoiceState bState = ref GetState(b);

                int result = aState.Priority.CompareTo(bState.Priority);

                if (result == 0)
                {
                    return aState.SortingOrder.CompareTo(bState.SortingOrder);
                }

                return result;
            });

            sortedVoicesTemp.AsSpan().CopyTo(_sortedVoices.Span);
        }
    }
}
