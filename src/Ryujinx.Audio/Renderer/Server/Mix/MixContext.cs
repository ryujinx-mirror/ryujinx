using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Server.Splitter;
using Ryujinx.Audio.Renderer.Utils;
using System;
using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Server.Mix
{
    /// <summary>
    /// Mix context.
    /// </summary>
    public class MixContext
    {
        /// <summary>
        /// The total mix count.
        /// </summary>
        private uint _mixesCount;

        /// <summary>
        /// Storage for <see cref="MixState"/>.
        /// </summary>
        private Memory<MixState> _mixes;

        /// <summary>
        /// Storage of the sorted indices to <see cref="MixState"/>.
        /// </summary>
        private Memory<int> _sortedMixes;

        /// <summary>
        /// Graph state.
        /// </summary>
        public NodeStates NodeStates { get; }

        /// <summary>
        /// The instance of the adjacent matrix.
        /// </summary>
        public EdgeMatrix EdgeMatrix { get; }

        /// <summary>
        /// Create a new instance of <see cref="MixContext"/>.
        /// </summary>
        public MixContext()
        {
            NodeStates = new NodeStates();
            EdgeMatrix = new EdgeMatrix();
        }

        /// <summary>
        /// Initialize the <see cref="MixContext"/>.
        /// </summary>
        /// <param name="sortedMixes">The storage for sorted indices.</param>
        /// <param name="mixes">The storage of <see cref="MixState"/>.</param>
        /// <param name="nodeStatesWorkBuffer">The storage used for the <see cref="NodeStates"/>.</param>
        /// <param name="edgeMatrixWorkBuffer">The storage used for the <see cref="EdgeMatrix"/>.</param>
        public void Initialize(Memory<int> sortedMixes, Memory<MixState> mixes, Memory<byte> nodeStatesWorkBuffer, Memory<byte> edgeMatrixWorkBuffer)
        {
            _mixesCount = (uint)mixes.Length;
            _mixes = mixes;
            _sortedMixes = sortedMixes;

            if (!nodeStatesWorkBuffer.IsEmpty && !edgeMatrixWorkBuffer.IsEmpty)
            {
                NodeStates.Initialize(nodeStatesWorkBuffer, mixes.Length);
                EdgeMatrix.Initialize(edgeMatrixWorkBuffer, mixes.Length);
            }

            int sortedId = 0;
            for (int i = 0; i < _mixes.Length; i++)
            {
                SetSortedState(sortedId++, i);
            }
        }

        /// <summary>
        /// Associate the given <paramref name="targetIndex"/> to a given <paramref cref="id"/>.
        /// </summary>
        /// <param name="id">The sorted id.</param>
        /// <param name="targetIndex">The index to associate.</param>
        private void SetSortedState(int id, int targetIndex)
        {
            _sortedMixes.Span[id] = targetIndex;
        }

        /// <summary>
        /// Get a reference to the final <see cref="MixState"/>.
        /// </summary>
        /// <returns>A reference to the final <see cref="MixState"/>.</returns>
        public ref MixState GetFinalState()
        {
            return ref GetState(Constants.FinalMixId);
        }

        /// <summary>
        /// Get a reference to a <see cref="MixState"/> at the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The index to use.</param>
        /// <returns>A reference to a <see cref="MixState"/> at the given <paramref name="id"/>.</returns>
        public ref MixState GetState(int id)
        {
            return ref SpanIOHelper.GetFromMemory(_mixes, id, _mixesCount);
        }

        /// <summary>
        /// Get a reference to a <see cref="MixState"/> at the given <paramref name="id"/> of the sorted mix info.
        /// </summary>
        /// <param name="id">The index to use.</param>
        /// <returns>A reference to a <see cref="MixState"/> at the given <paramref name="id"/>.</returns>
        public ref MixState GetSortedState(int id)
        {
            Debug.Assert(id >= 0 && id < _mixesCount);

            return ref GetState(_sortedMixes.Span[id]);
        }

        /// <summary>
        /// Get the total mix count.
        /// </summary>
        /// <returns>The total mix count.</returns>
        public uint GetCount()
        {
            return _mixesCount;
        }

        /// <summary>
        /// Update the internal distance from the final mix value of every <see cref="MixState"/>.
        /// </summary>
        private void UpdateDistancesFromFinalMix()
        {
            foreach (ref MixState mix in _mixes.Span)
            {
                mix.ClearDistanceFromFinalMix();
            }

            for (int i = 0; i < GetCount(); i++)
            {
                ref MixState mix = ref GetState(i);

                SetSortedState(i, i);

                if (mix.IsUsed)
                {
                    uint distance;

                    if (mix.MixId != Constants.FinalMixId)
                    {
                        int mixId = mix.MixId;

                        for (distance = 0; distance < GetCount(); distance++)
                        {
                            if (mixId == Constants.UnusedMixId)
                            {
                                distance = MixState.InvalidDistanceFromFinalMix;
                                break;
                            }

                            ref MixState distanceMix = ref GetState(mixId);

                            if (distanceMix.DistanceFromFinalMix != MixState.InvalidDistanceFromFinalMix)
                            {
                                distance = distanceMix.DistanceFromFinalMix + 1;
                                break;
                            }

                            mixId = distanceMix.DestinationMixId;

                            if (mixId == Constants.FinalMixId)
                            {
                                break;
                            }
                        }

                        if (distance > GetCount())
                        {
                            distance = MixState.InvalidDistanceFromFinalMix;
                        }
                    }
                    else
                    {
                        distance = MixState.InvalidDistanceFromFinalMix;
                    }

                    mix.DistanceFromFinalMix = distance;
                }
            }
        }

        /// <summary>
        /// Update the internal mix buffer offset of all <see cref="MixState"/>.
        /// </summary>
        private void UpdateMixBufferOffset()
        {
            uint offset = 0;

            foreach (ref MixState mix in _mixes.Span)
            {
                mix.BufferOffset = offset;

                offset += mix.BufferCount;
            }
        }

        /// <summary>
        /// Sort the mixes using distance from the final mix.
        /// </summary>
        public void Sort()
        {
            UpdateDistancesFromFinalMix();

            int[] sortedMixesTemp = _sortedMixes[..(int)GetCount()].ToArray();

            Array.Sort(sortedMixesTemp, (a, b) =>
            {
                ref MixState stateA = ref GetState(a);
                ref MixState stateB = ref GetState(b);

                return stateB.DistanceFromFinalMix.CompareTo(stateA.DistanceFromFinalMix);
            });

            sortedMixesTemp.AsSpan().CopyTo(_sortedMixes.Span);

            UpdateMixBufferOffset();
        }

        /// <summary>
        /// Sort the mixes and splitters using an adjacency matrix.
        /// </summary>
        /// <param name="splitterContext">The <see cref="SplitterContext"/> used.</param>
        /// <returns>Return true, if no errors in the graph were detected.</returns>
        public bool Sort(SplitterContext splitterContext)
        {
            if (splitterContext.UsingSplitter())
            {
                bool isValid = NodeStates.Sort(EdgeMatrix);

                if (isValid)
                {
                    ReadOnlySpan<int> sortedMixesIndex = NodeStates.GetTsortResult();

                    int id = 0;

                    for (int i = sortedMixesIndex.Length - 1; i >= 0; i--)
                    {
                        SetSortedState(id++, sortedMixesIndex[i]);
                    }

                    UpdateMixBufferOffset();
                }

                return isValid;
            }

            UpdateMixBufferOffset();

            return true;
        }
    }
}
