using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Utils;
using Ryujinx.Common;
using Ryujinx.Common.Extensions;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Server.Splitter
{
    /// <summary>
    /// Splitter context.
    /// </summary>
    public class SplitterContext
    {
        /// <summary>
        /// Storage for <see cref="SplitterState"/>.
        /// </summary>
        private Memory<SplitterState> _splitters;

        /// <summary>
        /// Storage for <see cref="SplitterDestination"/>.
        /// </summary>
        private Memory<SplitterDestination> _splitterDestinations;

        /// <summary>
        /// If set to true, trust the user destination count in <see cref="SplitterState.Update(SplitterContext, in SplitterInParameter, ref SequenceReader{byte})"/>.
        /// </summary>
        public bool IsBugFixed { get; private set; }

        /// <summary>
        /// Initialize <see cref="SplitterContext"/>.
        /// </summary>
        /// <param name="behaviourContext">The behaviour context.</param>
        /// <param name="parameter">The audio renderer configuration.</param>
        /// <param name="workBufferAllocator">The <see cref="WorkBufferAllocator"/>.</param>
        /// <returns>Return true if the initialization was successful.</returns>
        public bool Initialize(ref BehaviourContext behaviourContext, ref AudioRendererConfiguration parameter, WorkBufferAllocator workBufferAllocator)
        {
            if (!behaviourContext.IsSplitterSupported() || parameter.SplitterCount <= 0 || parameter.SplitterDestinationCount <= 0)
            {
                Setup(Memory<SplitterState>.Empty, Memory<SplitterDestination>.Empty, false);

                return true;
            }

            Memory<SplitterState> splitters = workBufferAllocator.Allocate<SplitterState>(parameter.SplitterCount, SplitterState.Alignment);

            if (splitters.IsEmpty)
            {
                return false;
            }

            int splitterId = 0;

            foreach (ref SplitterState splitter in splitters.Span)
            {
                splitter = new SplitterState(splitterId++);
            }

            Memory<SplitterDestination> splitterDestinations = workBufferAllocator.Allocate<SplitterDestination>(parameter.SplitterDestinationCount,
                SplitterDestination.Alignment);

            if (splitterDestinations.IsEmpty)
            {
                return false;
            }

            int splitterDestinationId = 0;
            foreach (ref SplitterDestination data in splitterDestinations.Span)
            {
                data = new SplitterDestination(splitterDestinationId++);
            }

            SplitterState.InitializeSplitters(splitters.Span);

            Setup(splitters, splitterDestinations, behaviourContext.IsSplitterBugFixed());

            return true;
        }

        /// <summary>
        /// Get the work buffer size while adding the size needed for splitter to operate.
        /// </summary>
        /// <param name="size">The current size.</param>
        /// <param name="behaviourContext">The behaviour context.</param>
        /// <param name="parameter">The renderer configuration.</param>
        /// <returns>Return the new size taking splitter into account.</returns>
        public static ulong GetWorkBufferSize(ulong size, ref BehaviourContext behaviourContext, ref AudioRendererConfiguration parameter)
        {
            if (behaviourContext.IsSplitterSupported())
            {
                size = WorkBufferAllocator.GetTargetSize<SplitterState>(size, parameter.SplitterCount, SplitterState.Alignment);
                size = WorkBufferAllocator.GetTargetSize<SplitterDestination>(size, parameter.SplitterDestinationCount, SplitterDestination.Alignment);

                if (behaviourContext.IsSplitterBugFixed())
                {
                    size = WorkBufferAllocator.GetTargetSize<int>(size, parameter.SplitterDestinationCount, 0x10);
                }

                return size;
            }

            return size;
        }

        /// <summary>
        /// Setup the <see cref="SplitterContext"/> instance.
        /// </summary>
        /// <param name="splitters">The <see cref="SplitterState"/> storage.</param>
        /// <param name="splitterDestinations">The <see cref="SplitterDestination"/> storage.</param>
        /// <param name="isBugFixed">If set to true, trust the user destination count in <see cref="SplitterState.Update(SplitterContext, in SplitterInParameter, ref SequenceReader{byte})"/>.</param>
        private void Setup(Memory<SplitterState> splitters, Memory<SplitterDestination> splitterDestinations, bool isBugFixed)
        {
            _splitters = splitters;
            _splitterDestinations = splitterDestinations;
            IsBugFixed = isBugFixed;
        }

        /// <summary>
        /// Clear the new connection flag.
        /// </summary>
        private void ClearAllNewConnectionFlag()
        {
            foreach (ref SplitterState splitter in _splitters.Span)
            {
                splitter.ClearNewConnectionFlag();
            }
        }

        /// <summary>
        /// Get the destination count using the count of splitter.
        /// </summary>
        /// <returns>The destination count using the count of splitter.</returns>
        public int GetDestinationCountPerStateForCompatibility()
        {
            if (_splitters.IsEmpty)
            {
                return 0;
            }

            return _splitterDestinations.Length / _splitters.Length;
        }

        /// <summary>
        /// Update one or multiple <see cref="SplitterState"/> from user parameters.
        /// </summary>
        /// <param name="inputHeader">The splitter header.</param>
        /// <param name="input">The raw data after the splitter header.</param>
        private void UpdateState(in SplitterInParameterHeader inputHeader, ref SequenceReader<byte> input)
        {
            for (int i = 0; i < inputHeader.SplitterCount; i++)
            {
                ref readonly SplitterInParameter parameter = ref input.GetRefOrRefToCopy<SplitterInParameter>(out _);

                Debug.Assert(parameter.IsMagicValid());

                if (parameter.IsMagicValid())
                {
                    if (parameter.Id >= 0 && parameter.Id < _splitters.Length)
                    {
                        ref SplitterState splitter = ref GetState(parameter.Id);

                        splitter.Update(this, in parameter, ref input);
                    }

                    // NOTE: there are 12 bytes of unused/unknown data after the destination IDs array.
                    input.Advance(0xC);
                }
                else
                {
                    input.Rewind(Unsafe.SizeOf<SplitterInParameter>());
                    break;
                }
            }
        }

        /// <summary>
        /// Update one or multiple <see cref="SplitterDestination"/> from user parameters.
        /// </summary>
        /// <param name="inputHeader">The splitter header.</param>
        /// <param name="input">The raw data after the splitter header.</param>
        private void UpdateData(in SplitterInParameterHeader inputHeader, ref SequenceReader<byte> input)
        {
            for (int i = 0; i < inputHeader.SplitterDestinationCount; i++)
            {
                ref readonly SplitterDestinationInParameter parameter = ref input.GetRefOrRefToCopy<SplitterDestinationInParameter>(out _);

                Debug.Assert(parameter.IsMagicValid());

                if (parameter.IsMagicValid())
                {
                    if (parameter.Id >= 0 && parameter.Id < _splitterDestinations.Length)
                    {
                        ref SplitterDestination destination = ref GetDestination(parameter.Id);

                        destination.Update(parameter);
                    }
                }
                else
                {
                    input.Rewind(Unsafe.SizeOf<SplitterDestinationInParameter>());
                    break;
                }
            }
        }

        /// <summary>
        /// Update splitter from user parameters.
        /// </summary>
        /// <param name="input">The input raw user data.</param>
        /// <returns>Return true if the update was successful.</returns>
        public bool Update(ref SequenceReader<byte> input)
        {
            if (_splitterDestinations.IsEmpty || _splitters.IsEmpty)
            {
                return true;
            }

            ref readonly SplitterInParameterHeader header = ref input.GetRefOrRefToCopy<SplitterInParameterHeader>(out _);

            if (header.IsMagicValid())
            {
                ClearAllNewConnectionFlag();

                UpdateState(in header, ref input);
                UpdateData(in header, ref input);

                input.SetConsumed(BitUtils.AlignUp(input.Consumed, 0x10));

                return true;
            }
            else
            {
                input.Rewind(Unsafe.SizeOf<SplitterInParameterHeader>());

                return false;
            }
        }

        /// <summary>
        /// Get a reference to a <see cref="SplitterState"/> at the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The index to use.</param>
        /// <returns>A reference to a <see cref="SplitterState"/> at the given <paramref name="id"/>.</returns>
        public ref SplitterState GetState(int id)
        {
            return ref SpanIOHelper.GetFromMemory(_splitters, id, (uint)_splitters.Length);
        }

        /// <summary>
        /// Get a reference to a <see cref="SplitterDestination"/> at the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The index to use.</param>
        /// <returns>A reference to a <see cref="SplitterDestination"/> at the given <paramref name="id"/>.</returns>
        public ref SplitterDestination GetDestination(int id)
        {
            return ref SpanIOHelper.GetFromMemory(_splitterDestinations, id, (uint)_splitterDestinations.Length);
        }

        /// <summary>
        /// Get a <see cref="Memory{SplitterDestination}"/> at the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The index to use.</param>
        /// <returns>A <see cref="Memory{SplitterDestination}"/> at the given <paramref name="id"/>.</returns>
        public Memory<SplitterDestination> GetDestinationMemory(int id)
        {
            return SpanIOHelper.GetMemory(_splitterDestinations, id, (uint)_splitterDestinations.Length);
        }

        /// <summary>
        /// Get a <see cref="Span{SplitterDestination}"/> in the <see cref="SplitterState"/> at <paramref name="id"/> and pass <paramref name="destinationId"/> to <see cref="SplitterState.GetData(int)"/>.
        /// </summary>
        /// <param name="id">The index to use to get the <see cref="SplitterState"/>.</param>
        /// <param name="destinationId">The index of the <see cref="SplitterDestination"/>.</param>
        /// <returns>A <see cref="Span{SplitterDestination}"/>.</returns>
        public Span<SplitterDestination> GetDestination(int id, int destinationId)
        {
            ref SplitterState splitter = ref GetState(id);

            return splitter.GetData(destinationId);
        }

        /// <summary>
        /// Return true if the audio renderer has any splitters.
        /// </summary>
        /// <returns>True if the audio renderer has any splitters.</returns>
        public bool UsingSplitter()
        {
            return !_splitters.IsEmpty && !_splitterDestinations.IsEmpty;
        }

        /// <summary>
        /// Update the internal state of all splitters.
        /// </summary>
        public void UpdateInternalState()
        {
            foreach (ref SplitterState splitter in _splitters.Span)
            {
                splitter.UpdateInternalState();
            }
        }
    }
}
