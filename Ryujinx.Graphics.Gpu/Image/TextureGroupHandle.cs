using Ryujinx.Cpu.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// A tracking handle for a texture group, which represents a range of views in a storage texture.
    /// Retains a list of overlapping texture views, a modified flag, and tracking for each
    /// CPU VA range that the views cover.
    /// Also tracks copy dependencies for the handle - references to other handles that must be kept 
    /// in sync with this one before use.
    /// </summary>
    class TextureGroupHandle : IDisposable
    {
        private TextureGroup _group;
        private int _bindCount;
        private int _firstLevel;
        private int _firstLayer;

        // Sync state for texture flush.

        /// <summary>
        /// The sync number last registered.
        /// </summary>
        private ulong _registeredSync;

        /// <summary>
        /// The sync number when the texture was last modified by GPU.
        /// </summary>
        private ulong _modifiedSync;

        /// <summary>
        /// Whether a tracking action is currently registered or not. (0/1)
        /// </summary>
        private int _actionRegistered;

        /// <summary>
        /// Whether a sync action is currently registered or not.
        /// </summary>
        private bool _syncActionRegistered;

        /// <summary>
        /// The byte offset from the start of the storage of this handle.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// The size in bytes covered by this handle.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// The base slice index for this handle.
        /// </summary>
        public int BaseSlice { get; }

        /// <summary>
        /// The number of slices covered by this handle.
        /// </summary>
        public int SliceCount { get; }

        /// <summary>
        /// The textures which this handle overlaps with.
        /// </summary>
        public List<Texture> Overlaps { get; }

        /// <summary>
        /// The CPU memory tracking handles that cover this handle.
        /// </summary>
        public CpuRegionHandle[] Handles { get; }

        /// <summary>
        /// True if a texture overlapping this handle has been modified. Is set false when the flush action is called.
        /// </summary>
        public bool Modified { get; set; }

        /// <summary>
        /// Dependencies to handles from other texture groups.
        /// </summary>
        public List<TextureDependency> Dependencies { get; }

        /// <summary>
        /// A flag indicating that a copy is required from one of the dependencies.
        /// </summary>
        public bool NeedsCopy => DeferredCopy != null;

        /// <summary>
        /// A data copy that must be acknowledged the next time this handle is used.
        /// </summary>
        public TextureGroupHandle DeferredCopy { get; set; }

        /// <summary>
        /// Create a new texture group handle, representing a range of views in a storage texture.
        /// </summary>
        /// <param name="group">The TextureGroup that the handle belongs to</param>
        /// <param name="offset">The byte offset from the start of the storage of the handle</param>
        /// <param name="size">The size in bytes covered by the handle</param>
        /// <param name="views">All views of the storage texture, used to calculate overlaps</param>
        /// <param name="firstLayer">The first layer of this handle in the storage texture</param>
        /// <param name="firstLevel">The first level of this handle in the storage texture</param>
        /// <param name="baseSlice">The base slice index of this handle</param>
        /// <param name="sliceCount">The number of slices this handle covers</param>
        /// <param name="handles">The memory tracking handles that cover this handle</param>
        public TextureGroupHandle(TextureGroup group,
                                  int offset,
                                  ulong size,
                                  List<Texture> views,
                                  int firstLayer,
                                  int firstLevel,
                                  int baseSlice,
                                  int sliceCount,
                                  CpuRegionHandle[] handles)
        {
            _group = group;
            _firstLayer = firstLayer;
            _firstLevel = firstLevel;

            Offset = offset;
            Size = (int)size;
            Overlaps = new List<Texture>();
            Dependencies = new List<TextureDependency>();

            BaseSlice = baseSlice;
            SliceCount = sliceCount;

            if (views != null)
            {
                RecalculateOverlaps(group, views);
            }

            Handles = handles;
        }

        /// <summary>
        /// Calculate a list of which views overlap this handle.
        /// </summary>
        /// <param name="group">The parent texture group, used to find a view's base CPU VA offset</param>
        /// <param name="views">The list of views to search for overlaps</param>
        public void RecalculateOverlaps(TextureGroup group, List<Texture> views)
        {
            // Overlaps can be accessed from the memory tracking signal handler, so access must be atomic.
            lock (Overlaps)
            {
                int endOffset = Offset + Size;

                Overlaps.Clear();

                foreach (Texture view in views)
                {
                    int viewOffset = group.FindOffset(view);
                    if (viewOffset < endOffset && Offset < viewOffset + (int)view.Size)
                    {
                        Overlaps.Add(view);
                    }
                }
            }
        }

        /// <summary>
        /// Registers a sync action to happen for this handle, and an interim flush action on the tracking handle.
        /// </summary>
        /// <param name="context">The GPU context to register a sync action on</param>
        private void RegisterSync(GpuContext context)
        {
            if (!_syncActionRegistered)
            {
                _modifiedSync = context.SyncNumber;
                context.RegisterSyncAction(SyncAction, true);
                _syncActionRegistered = true;
            }

            if (Interlocked.Exchange(ref _actionRegistered, 1) == 0)
            {
                _group.RegisterAction(this);
            }
        }

        /// <summary>
        /// Signal that this handle has been modified to any existing dependencies, and set the modified flag.
        /// </summary>
        /// <param name="context">The GPU context to register a sync action on</param>
        public void SignalModified(GpuContext context)
        {
            Modified = true;

            // If this handle has any copy dependencies, notify the other handle that a copy needs to be performed.

            foreach (TextureDependency dependency in Dependencies)
            {
                dependency.SignalModified();
            }

            RegisterSync(context);
        }

        /// <summary>
        /// Signal that this handle has either started or ended being modified.
        /// </summary>
        /// <param name="bound">True if this handle is being bound, false if unbound</param>
        /// <param name="context">The GPU context to register a sync action on</param>
        public void SignalModifying(bool bound, GpuContext context)
        {
            SignalModified(context);

            // Note: Bind count currently resets to 0 on inherit for safety, as the handle <-> view relationship can change.
            _bindCount = Math.Max(0, _bindCount + (bound ? 1 : -1));
        }

        /// <summary>
        /// Synchronize dependent textures, if any of them have deferred a copy from this texture.
        /// </summary>
        public void SynchronizeDependents()
        {
            foreach (TextureDependency dependency in Dependencies)
            {
                TextureGroupHandle otherHandle = dependency.Other.Handle;

                if (otherHandle.DeferredCopy == this)
                {
                    otherHandle._group.Storage.SynchronizeMemory();
                }
            }
        }

        /// <summary>
        /// Wait for the latest sync number that the texture handle was written to,
        /// removing the modified flag if it was reached, or leaving it set if it has not yet been created.
        /// </summary>
        /// <param name="context">The GPU context used to wait for sync</param>
        public void Sync(GpuContext context)
        {
            bool needsSync = !context.IsGpuThread();

            if (needsSync)
            {
                ulong registeredSync = _registeredSync;
                long diff = (long)(context.SyncNumber - registeredSync);

                if (diff > 0)
                {
                    context.Renderer.WaitSync(registeredSync);

                    if ((long)(_modifiedSync - registeredSync) > 0)
                    {
                        // Flush the data in a previous state. Do not remove the modified flag - it will be removed to ignore following writes.
                        return;
                    }

                    Modified = false;
                }
                
                // If the difference is <= 0, no data is not ready yet. Flush any data we can without waiting or removing modified flag.
            }
            else
            {
                Modified = false;
            }
        }

        /// <summary>
        /// Clears the action registered variable, indicating that the tracking action should be
        /// re-registered on the next modification.
        /// </summary>
        public void ClearActionRegistered()
        {
            Interlocked.Exchange(ref _actionRegistered, 0);
        }

        /// <summary>
        /// Action to perform when a sync number is registered after modification.
        /// This action will register a read tracking action on the memory tracking handle so that a flush from CPU can happen.
        /// </summary>
        private void SyncAction()
        {
            // The storage will need to signal modified again to update the sync number in future.
            _group.Storage.SignalModifiedDirty();

            lock (Overlaps)
            {
                foreach (Texture texture in Overlaps)
                {
                    texture.SignalModifiedDirty();
                }
            }

            // Register region tracking for CPU? (again)
            _registeredSync = _modifiedSync;
            _syncActionRegistered = false;

            if (Interlocked.Exchange(ref _actionRegistered, 1) == 0)
            {
                _group.RegisterAction(this);
            }
        }

        /// <summary>
        /// Signal that a copy dependent texture has been modified, and must have its data copied to this one.
        /// </summary>
        /// <param name="copyFrom">The texture handle that must defer a copy to this one</param>
        public void DeferCopy(TextureGroupHandle copyFrom)
        {
            Modified = false;

            DeferredCopy = copyFrom;

            _group.Storage.SignalGroupDirty();

            foreach (Texture overlap in Overlaps)
            {
                overlap.SignalGroupDirty();
            }
        }

        /// <summary>
        /// Create a copy dependency between this handle, and another.
        /// </summary>
        /// <param name="other">The handle to create a copy dependency to</param>
        /// <param name="copyToOther">True if a copy should be deferred to all of the other handle's dependencies</param>
        public void CreateCopyDependency(TextureGroupHandle other, bool copyToOther = false)
        {
            // Does this dependency already exist?
            foreach (TextureDependency existing in Dependencies)
            {
                if (existing.Other.Handle == other)
                {
                    // Do not need to create it again. May need to set the dirty flag.
                    return;
                }
            }

            _group.HasCopyDependencies = true;
            other._group.HasCopyDependencies = true;

            TextureDependency dependency = new TextureDependency(this);
            TextureDependency otherDependency = new TextureDependency(other);

            dependency.Other = otherDependency;
            otherDependency.Other = dependency;

            Dependencies.Add(dependency);
            other.Dependencies.Add(otherDependency);

            // Recursively create dependency:
            // All of this handle's dependencies must depend on the other.
            foreach (TextureDependency existing in Dependencies.ToArray())
            {
                if (existing != dependency && existing.Other.Handle != other)
                {
                    existing.Other.Handle.CreateCopyDependency(other);
                }
            }

            // All of the other handle's dependencies must depend on this.
            foreach (TextureDependency existing in other.Dependencies.ToArray())
            {
                if (existing != otherDependency && existing.Other.Handle != this)
                {
                    existing.Other.Handle.CreateCopyDependency(this);

                    if (copyToOther)
                    {
                        existing.Other.Handle.DeferCopy(this);
                    }
                }
            }
        }

        /// <summary>
        /// Remove a dependency from this handle's dependency list.
        /// </summary>
        /// <param name="dependency">The dependency to remove</param>
        public void RemoveDependency(TextureDependency dependency)
        {
            Dependencies.Remove(dependency);
        }

        /// <summary>
        /// Check if any of this handle's memory tracking handles are dirty.
        /// </summary>
        /// <returns>True if at least one of the handles is dirty</returns>
        private bool CheckDirty()
        {
            return Handles.Any(handle => handle.Dirty);
        }

        /// <summary>
        /// Perform a copy from the provided handle to this one, or perform a deferred copy if none is provided.
        /// </summary>
        /// <param name="context">GPU context to register sync for modified handles</param>
        /// <param name="fromHandle">The handle to copy from. If not provided, this method will copy from and clear the deferred copy instead</param>
        /// <returns>True if the copy was performed, false otherwise</returns>
        public bool Copy(GpuContext context, TextureGroupHandle fromHandle = null)
        {
            bool result = false;
            bool shouldCopy = false;

            if (fromHandle == null)
            {
                fromHandle = DeferredCopy;

                if (fromHandle != null)
                {
                    // Only copy if the copy texture is still modified.
                    // It will be set as unmodified if new data is written from CPU, as the data previously in the texture will flush.
                    // It will also set as unmodified if a copy is deferred to it.

                    shouldCopy = fromHandle.Modified;

                    if (fromHandle._bindCount == 0)
                    {
                        // Repeat the copy in future if the bind count is greater than 0.
                        DeferredCopy = null;
                    }
                }
            }
            else
            {
                // Copies happen directly when initializing a copy dependency.
                // If dirty, do not copy. Its data no longer matters, and this handle should also be dirty.
                // Also, only direct copy if the data in this handle is not already modified (can be set by copies from modified handles).
                shouldCopy = !fromHandle.CheckDirty() && (fromHandle.Modified || !Modified);
            }

            if (shouldCopy)
            {
                Texture from = fromHandle._group.Storage;
                Texture to = _group.Storage;

                if (from.ScaleFactor != to.ScaleFactor)
                {
                    to.PropagateScale(from);
                }

                from.HostTexture.CopyTo(
                    to.HostTexture,
                    fromHandle._firstLayer,
                    _firstLayer,
                    fromHandle._firstLevel,
                    _firstLevel);

                if (fromHandle.Modified)
                {
                    Modified = true;

                    RegisterSync(context);
                }

                result = true;
            }

            return result;
        }

        /// <summary>
        /// Check if this handle has a dependency to a given texture group.
        /// </summary>
        /// <param name="group">The texture group to check for</param>
        /// <returns>True if there is a dependency, false otherwise</returns>
        public bool HasDependencyTo(TextureGroup group)
        {
            foreach (TextureDependency dep in Dependencies)
            {
                if (dep.Other.Handle._group == group)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Inherit modified flags and dependencies from another texture handle.
        /// </summary>
        /// <param name="old">The texture handle to inherit from</param>
        /// <param name="withCopies">Whether the handle should inherit copy dependencies or not</param>
        public void Inherit(TextureGroupHandle old, bool withCopies)
        {
            Modified |= old.Modified;

            if (withCopies)
            {
                foreach (TextureDependency dependency in old.Dependencies.ToArray())
                {
                    CreateCopyDependency(dependency.Other.Handle);

                    if (dependency.Other.Handle.DeferredCopy == old)
                    {
                        dependency.Other.Handle.DeferredCopy = this;
                    }
                }

                DeferredCopy = old.DeferredCopy;
            }
        }

        /// <summary>
        /// Check if this region overlaps with another.
        /// </summary>
        /// <param name="address">Base address</param>
        /// <param name="size">Size of the region</param>
        /// <returns>True if overlapping, false otherwise</returns>
        public bool OverlapsWith(int offset, int size)
        {
            return Offset < offset + size && offset < Offset + Size;
        }

        /// <summary>
        /// Dispose this texture group handle, removing all its dependencies and disposing its memory tracking handles.
        /// </summary>
        public void Dispose()
        {
            foreach (CpuRegionHandle handle in Handles)
            {
                handle.Dispose();
            }

            foreach (TextureDependency dependency in Dependencies.ToArray())
            {
                dependency.Other.Handle.RemoveDependency(dependency.Other);
            }
        }
    }
}
