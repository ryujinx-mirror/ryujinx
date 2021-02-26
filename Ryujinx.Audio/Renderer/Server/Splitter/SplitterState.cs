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
using Ryujinx.Audio.Renderer.Parameter;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Splitter
{
    /// <summary>
    /// Server state for a splitter.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0x20, Pack = Alignment)]
    public struct SplitterState
    {
        public const int Alignment = 0x10;

        /// <summary>
        /// The unique id of this <see cref="SplitterState"/>.
        /// </summary>
        public int Id;

        /// <summary>
        /// Target sample rate to use on the splitter.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// Count of splitter destinations (<see cref="SplitterDestination"/>).
        /// </summary>
        public int DestinationCount;

        /// <summary>
        /// Set to true if the splitter has a new connection.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool HasNewConnection;

        /// <summary>
        /// Linked list of <see cref="SplitterDestination"/>.
        /// </summary>
        private unsafe SplitterDestination* _destinationsData;

        /// <summary>
        /// Span to the first element of the linked list of <see cref="SplitterDestination"/>.
        /// </summary>
        public Span<SplitterDestination> Destinations
        {
            get
            {
                unsafe
                {
                    return (IntPtr)_destinationsData != IntPtr.Zero ? new Span<SplitterDestination>(_destinationsData, 1) : Span<SplitterDestination>.Empty;
                }
            }
        }

        /// <summary>
        /// Create a new <see cref="SplitterState"/>.
        /// </summary>
        /// <param name="id">The unique id of this <see cref="SplitterState"/>.</param>
        public SplitterState(int id) : this()
        {
            Id = id;
        }

        public Span<SplitterDestination> GetData(int index)
        {
            int i = 0;

            Span<SplitterDestination> result = Destinations;

            while (i < index)
            {
                if (result.IsEmpty)
                {
                    break;
                }

                result = result[0].Next;
                i++;
            }

            return result;
        }

        /// <summary>
        /// Clear the new connection flag.
        /// </summary>
        public void ClearNewConnectionFlag()
        {
            HasNewConnection = false;
        }

        /// <summary>
        /// Utility function to apply a given <see cref="SpanAction{T, TArg}"/> to all <see cref="Destinations"/>.
        /// </summary>
        /// <param name="action">The action to execute on each elements.</param>
        private void ForEachDestination(SpanAction<SplitterDestination, int> action)
        {
            Span<SplitterDestination> temp = Destinations;

            int i = 0;

            while (true)
            {
                if (temp.IsEmpty)
                {
                    break;
                }

                Span<SplitterDestination> next = temp[0].Next;

                action.Invoke(temp, i++);

                temp = next;
            }
        }

        /// <summary>
        /// Update the <see cref="SplitterState"/> from user parameter.
        /// </summary>
        /// <param name="context">The splitter context.</param>
        /// <param name="parameter">The user parameter.</param>
        /// <param name="input">The raw input data after the <paramref name="parameter"/>.</param>
        public void Update(SplitterContext context, ref SplitterInParameter parameter, ReadOnlySpan<byte> input)
        {
            ClearLinks();

            int destinationCount;

            if (context.IsBugFixed)
            {
                destinationCount = parameter.DestinationCount;
            }
            else
            {
                destinationCount = Math.Min(context.GetDestinationCountPerStateForCompatibility(), parameter.DestinationCount);
            }

            if (destinationCount > 0)
            {
                ReadOnlySpan<int> destinationIds = MemoryMarshal.Cast<byte, int>(input);

                Memory<SplitterDestination> destination = context.GetDestinationMemory(destinationIds[0]);

                SetDestination(ref destination.Span[0]);

                DestinationCount = destinationCount;

                for (int i = 1; i < destinationCount; i++)
                {
                    Memory<SplitterDestination> nextDestination = context.GetDestinationMemory(destinationIds[i]);

                    destination.Span[0].Link(ref nextDestination.Span[0]);
                    destination = nextDestination;
                }
            }

            Debug.Assert(parameter.Id == Id);

            if (parameter.Id == Id)
            {
                SampleRate = parameter.SampleRate;
                HasNewConnection = true;
            }
        }

        /// <summary>
        /// Set the head of the linked list of <see cref="Destinations"/>.
        /// </summary>
        /// <param name="newValue">A reference to a <see cref="SplitterDestination"/>.</param>
        public void SetDestination(ref SplitterDestination newValue)
        {
            unsafe
            {
                fixed (SplitterDestination* newValuePtr = &newValue)
                {
                    _destinationsData = newValuePtr;
                }
            }
        }

        /// <summary>
        /// Update the internal state of this instance.
        /// </summary>
        public void UpdateInternalState()
        {
            ForEachDestination((destination, _) => destination[0].UpdateInternalState());
        }

        /// <summary>
        /// Clear all links from the <see cref="Destinations"/>.
        /// </summary>
        public void ClearLinks()
        {
            ForEachDestination((destination, _) => destination[0].Unlink());

            unsafe
            {
                _destinationsData = (SplitterDestination*)IntPtr.Zero;
            }
        }

        /// <summary>
        /// Initialize a given <see cref="Span{SplitterState}"/>.
        /// </summary>
        /// <param name="splitters">All the <see cref="SplitterState"/> to initialize.</param>
        public static void InitializeSplitters(Span<SplitterState> splitters)
        {
            foreach (ref SplitterState splitter in splitters)
            {
                unsafe
                {
                    splitter._destinationsData = (SplitterDestination*)IntPtr.Zero;
                }

                splitter.DestinationCount = 0;
            }
        }
    }
}
