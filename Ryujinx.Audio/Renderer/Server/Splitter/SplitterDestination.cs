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
using Ryujinx.Common.Utilities;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Splitter
{
    /// <summary>
    /// Server state for a splitter destination.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0xE0, Pack = Alignment)]
    public struct SplitterDestination
    {
        public const int Alignment = 0x10;

        /// <summary>
        /// The unique id of this <see cref="SplitterDestination"/>.
        /// </summary>
        public int Id;

        /// <summary>
        /// The mix to output the result of the splitter.
        /// </summary>
        public int DestinationId;

        /// <summary>
        /// Mix buffer volumes storage.
        /// </summary>
        private MixArray _mix;
        private MixArray _previousMix;

        /// <summary>
        /// Pointer to the next linked element.
        /// </summary>
        private unsafe SplitterDestination* _next;

        /// <summary>
        /// Set to true if in use.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsUsed;

        /// <summary>
        /// Set to true if the internal state need to be updated.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool NeedToUpdateInternalState;

        [StructLayout(LayoutKind.Sequential, Size = 4 * Constants.MixBufferCountMax, Pack = 1)]
        private struct MixArray { }

        /// <summary>
        /// Mix buffer volumes.
        /// </summary>
        /// <remarks>Used when a splitter id is specified in the mix.</remarks>
        public Span<float> MixBufferVolume => SpanHelpers.AsSpan<MixArray, float>(ref _mix);

        /// <summary>
        /// Previous mix buffer volumes.
        /// </summary>
        /// <remarks>Used when a splitter id is specified in the mix.</remarks>
        public Span<float> PreviousMixBufferVolume => SpanHelpers.AsSpan<MixArray, float>(ref _previousMix);

        /// <summary>
        /// Get the  <see cref="Span{SplitterDestination}"/> of the next element or <see cref="Span{SplitterDestination}.Empty"/> if not present.
        /// </summary>
        public Span<SplitterDestination> Next
        {
            get
            {
                unsafe
                {
                    return _next != null ? new Span<SplitterDestination>(_next, 1) : Span<SplitterDestination>.Empty;
                }
            }
        }

        /// <summary>
        /// Create a new <see cref="SplitterDestination"/>.
        /// </summary>
        /// <param name="id">The unique id of this <see cref="SplitterDestination"/>.</param>
        public SplitterDestination(int id) : this()
        {
            Id = id;
            DestinationId = Constants.UnusedMixId;

            ClearVolumes();
        }

        /// <summary>
        /// Update the <see cref="SplitterDestination"/> from user parameter.
        /// </summary>
        /// <param name="parameter">The user parameter.</param>
        public void Update(SplitterDestinationInParameter parameter)
        {
            Debug.Assert(Id == parameter.Id);

            if (parameter.IsMagicValid() && Id == parameter.Id)
            {
                DestinationId = parameter.DestinationId;

                parameter.MixBufferVolume.CopyTo(MixBufferVolume);

                if (!IsUsed && parameter.IsUsed)
                {
                    MixBufferVolume.CopyTo(PreviousMixBufferVolume);

                    NeedToUpdateInternalState = false;
                }

                IsUsed = parameter.IsUsed;
            }
        }

        /// <summary>
        /// Update the internal state of the instance.
        /// </summary>
        public void UpdateInternalState()
        {
            if (IsUsed && NeedToUpdateInternalState)
            {
                MixBufferVolume.CopyTo(PreviousMixBufferVolume);
            }

            NeedToUpdateInternalState = false;
        }

        /// <summary>
        /// Set the update internal state marker.
        /// </summary>
        public void MarkAsNeedToUpdateInternalState()
        {
            NeedToUpdateInternalState = true;
        }

        /// <summary>
        /// Return true if the <see cref="SplitterDestination"/> is used and has a destination.
        /// </summary>
        /// <returns>True if the <see cref="SplitterDestination"/> is used and has a destination.</returns>
        public bool IsConfigured()
        {
            return IsUsed && DestinationId != Constants.UnusedMixId;
        }

        /// <summary>
        /// Get the volume for a given destination.
        /// </summary>
        /// <param name="destinationIndex">The destination index to use.</param>
        /// <returns>The volume for the given destination.</returns>
        public float GetMixVolume(int destinationIndex)
        {
            Debug.Assert(destinationIndex >= 0 && destinationIndex < Constants.MixBufferCountMax);

            return MixBufferVolume[destinationIndex];
        }

        /// <summary>
        /// Clear the volumes.
        /// </summary>
        public void ClearVolumes()
        {
            MixBufferVolume.Fill(0);
            PreviousMixBufferVolume.Fill(0);
        }

        /// <summary>
        /// Link the next element to the given <see cref="SplitterDestination"/>.
        /// </summary>
        /// <param name="next">The given <see cref="SplitterDestination"/> to link.</param>
        public void Link(ref SplitterDestination next)
        {
            unsafe
            {
                fixed (SplitterDestination *nextPtr = &next)
                {
                    _next = nextPtr;
                }
            }
        }

        /// <summary>
        /// Remove the link to the next element.
        /// </summary>
        public void Unlink()
        {
            unsafe
            {
                _next = null;
            }
        }
    }
}
