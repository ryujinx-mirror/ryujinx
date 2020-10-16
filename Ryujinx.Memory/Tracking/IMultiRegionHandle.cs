using System;

namespace Ryujinx.Memory.Tracking
{
    public interface IMultiRegionHandle : IDisposable
    {
        /// <summary>
        /// True if any write has occurred to the whole region since the last use of QueryModified (with no subregion specified).
        /// </summary>
        bool Dirty { get; }

        /// <summary>
        /// Check if any part of the region has been modified, and perform an action for each.
        /// Contiguous modified regions are combined.
        /// </summary>
        /// <param name="modifiedAction">Action to perform for modified regions</param>
        void QueryModified(Action<ulong, ulong> modifiedAction);


        /// <summary>
        /// Check if part of the region has been modified within a given range, and perform an action for each.
        /// The range is aligned to the level of granularity of the contained handles.
        /// Contiguous modified regions are combined.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size of the range</param>
        /// <param name="modifiedAction">Action to perform for modified regions</param>
        void QueryModified(ulong address, ulong size, Action<ulong, ulong> modifiedAction);

        /// <summary>
        /// Check if part of the region has been modified within a given range, and perform an action for each.
        /// The sequence number provided is compared with each handle's saved sequence number. 
        /// If it is equal, then the handle's dirty flag is ignored. Otherwise, the sequence number is saved.
        /// The range is aligned to the level of granularity of the contained handles.
        /// Contiguous modified regions are combined.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size of the range</param>
        /// <param name="modifiedAction">Action to perform for modified regions</param>
        /// <param name="sequenceNumber">Current sequence number</param>
        void QueryModified(ulong address, ulong size, Action<ulong, ulong> modifiedAction, int sequenceNumber);

        /// <summary>
        /// Signal that one of the subregions of this multi-region has been modified. This sets the overall dirty flag.
        /// </summary>
        void SignalWrite();
    }
}
