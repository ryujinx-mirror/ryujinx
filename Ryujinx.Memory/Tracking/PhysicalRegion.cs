using Ryujinx.Memory.Range;
using System.Collections.Generic;

namespace Ryujinx.Memory.Tracking
{
    /// <summary>
    /// A region of physical memory.
    /// </summary>
    class PhysicalRegion : AbstractRegion
    {
        public List<VirtualRegion> VirtualParents = new List<VirtualRegion>();
        public MemoryPermission Protection { get; private set; }
        public MemoryTracking Tracking;

        public PhysicalRegion(MemoryTracking tracking, ulong address, ulong size) : base(address, size)
        {
            Tracking = tracking;
            Protection = MemoryPermission.ReadAndWrite;
        }

        public override void Signal(ulong address, ulong size, bool write)
        {
            Protection = MemoryPermission.ReadAndWrite;
            Tracking.ProtectPhysicalRegion(this, MemoryPermission.ReadAndWrite); // Remove our protection immedately.
            foreach (var parent in VirtualParents)
            {
                parent.Signal(address, size, write);
            }
        }

        /// <summary>
        /// Update the protection of this region, based on our parent's requested protection.
        /// </summary>
        public void UpdateProtection()
        {
            // Re-evaluate protection, and commit to the block.

            lock (Tracking.TrackingLock)
            {
                MemoryPermission result = MemoryPermission.ReadAndWrite;
                foreach (var parent in VirtualParents)
                {
                    result &= parent.GetRequiredPermission();
                    if (result == 0) break;
                }

                if (Protection != result)
                {
                    Protection = result;
                    Tracking.ProtectPhysicalRegion(this, result);
                }
            }
        }

        public override INonOverlappingRange Split(ulong splitAddress)
        {
            PhysicalRegion newRegion = new PhysicalRegion(Tracking, splitAddress, EndAddress - splitAddress);
            Size = splitAddress - Address;

            // The new region inherits all of our parents.
            newRegion.VirtualParents = new List<VirtualRegion>(VirtualParents);
            foreach (var parent in VirtualParents)
            {
                parent.AddChild(newRegion);
            }

            return newRegion;
        }

        /// <summary>
        /// Remove a parent virtual region from this physical region. Assumes that the tracking lock has been obtained.
        /// </summary>
        /// <param name="region">Region to remove</param>
        /// <returns>True if there are no more parents and we should be removed, false otherwise.</returns>
        public bool RemoveParent(VirtualRegion region)
        {
            VirtualParents.Remove(region);
            UpdateProtection();
            if (VirtualParents.Count == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes this physical region if there are no more virtual parents.
        /// </summary>
        public void TryDelete()
        {
            if (VirtualParents.Count == 0)
            {
                Tracking.RemovePhysical(this);
            }
        }
    }
}
