using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Common.Extensions;
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

        private delegate void SplitterDestinationAction(SplitterDestination destination, int index);

        /// <summary>
        /// The unique id of this <see cref="SplitterState"/>.
        /// </summary>
        public int Id;

        /// <summary>
        /// Target sample rate to use on the splitter.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// Count of splitter destinations.
        /// </summary>
        public int DestinationCount;

        /// <summary>
        /// Set to true if the splitter has a new connection.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool HasNewConnection;

        /// <summary>
        /// Linked list of <see cref="SplitterDestinationVersion1"/>.
        /// </summary>
        private unsafe SplitterDestinationVersion1* _destinationDataV1;

        /// <summary>
        /// Linked list of <see cref="SplitterDestinationVersion2"/>.
        /// </summary>
        private unsafe SplitterDestinationVersion2* _destinationDataV2;

        /// <summary>
        /// First element of the linked list of splitter destinations data.
        /// </summary>
        public readonly SplitterDestination Destination
        {
            get
            {
                unsafe
                {
                    return new SplitterDestination(_destinationDataV1, _destinationDataV2);
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

        public readonly SplitterDestination GetData(int index)
        {
            int i = 0;

            SplitterDestination result = Destination;

            while (i < index)
            {
                if (result.IsNull)
                {
                    break;
                }

                result = result.Next;
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
        /// Utility function to apply an action to all <see cref="Destination"/>.
        /// </summary>
        /// <param name="action">The action to execute on each elements.</param>
        private readonly void ForEachDestination(SplitterDestinationAction action)
        {
            SplitterDestination temp = Destination;

            int i = 0;

            while (true)
            {
                if (temp.IsNull)
                {
                    break;
                }

                SplitterDestination next = temp.Next;

                action(temp, i++);

                temp = next;
            }
        }

        /// <summary>
        /// Update the <see cref="SplitterState"/> from user parameter.
        /// </summary>
        /// <param name="context">The splitter context.</param>
        /// <param name="parameter">The user parameter.</param>
        /// <param name="input">The raw input data after the <paramref name="parameter"/>.</param>
        public void Update(SplitterContext context, in SplitterInParameter parameter, ref SequenceReader<byte> input)
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
                input.ReadLittleEndian(out int destinationId);

                SplitterDestination destination = context.GetDestination(destinationId);

                SetDestination(destination);

                DestinationCount = destinationCount;

                for (int i = 1; i < destinationCount; i++)
                {
                    input.ReadLittleEndian(out destinationId);

                    SplitterDestination nextDestination = context.GetDestination(destinationId);

                    destination.Link(nextDestination);
                    destination = nextDestination;
                }
            }

            if (destinationCount < parameter.DestinationCount)
            {
                input.Advance((parameter.DestinationCount - destinationCount) * sizeof(int));
            }

            Debug.Assert(parameter.Id == Id);

            if (parameter.Id == Id)
            {
                SampleRate = parameter.SampleRate;
                HasNewConnection = true;
            }
        }

        /// <summary>
        /// Set the head of the linked list of <see cref="Destination"/>.
        /// </summary>
        /// <param name="newValue">New destination value.</param>
        public void SetDestination(SplitterDestination newValue)
        {
            unsafe
            {
                fixed (SplitterDestinationVersion1* newValuePtr = &newValue.GetV1RefOrNull())
                {
                    _destinationDataV1 = newValuePtr;
                }

                fixed (SplitterDestinationVersion2* newValuePtr = &newValue.GetV2RefOrNull())
                {
                    _destinationDataV2 = newValuePtr;
                }
            }
        }

        /// <summary>
        /// Update the internal state of this instance.
        /// </summary>
        public readonly void UpdateInternalState()
        {
            ForEachDestination((destination, _) => destination.UpdateInternalState());
        }

        /// <summary>
        /// Clear all links from the <see cref="Destination"/>.
        /// </summary>
        public void ClearLinks()
        {
            ForEachDestination((destination, _) => destination.Unlink());

            unsafe
            {
                _destinationDataV1 = null;
                _destinationDataV2 = null;
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
                    splitter._destinationDataV1 = null;
                    splitter._destinationDataV2 = null;
                }

                splitter.DestinationCount = 0;
            }
        }
    }
}
