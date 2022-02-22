using Ryujinx.Cpu.Tracking;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Texture;
using Ryujinx.Memory;
using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// An overlapping texture group with a given view compatibility.
    /// </summary>
    struct TextureIncompatibleOverlap
    {
        public readonly TextureGroup Group;
        public readonly TextureViewCompatibility Compatibility;

        /// <summary>
        /// Create a new texture incompatible overlap.
        /// </summary>
        /// <param name="group">The group that is incompatible</param>
        /// <param name="compatibility">The view compatibility for the group</param>
        public TextureIncompatibleOverlap(TextureGroup group, TextureViewCompatibility compatibility)
        {
            Group = group;
            Compatibility = compatibility;
        }
    }

    /// <summary>
    /// A texture group represents a group of textures that belong to the same storage.
    /// When views are created, this class will track memory accesses for them separately.
    /// The group iteratively adds more granular tracking as views of different kinds are added.
    /// Note that a texture group can be absorbed into another when it becomes a view parent.
    /// </summary>
    class TextureGroup : IDisposable
    {
        private delegate void HandlesCallbackDelegate(int baseHandle, int regionCount, bool split = false);

        /// <summary>
        /// The storage texture associated with this group.
        /// </summary>
        public Texture Storage { get; }

        /// <summary>
        /// Indicates if the texture has copy dependencies. If true, then all modifications
        /// must be signalled to the group, rather than skipping ones still to be flushed.
        /// </summary>
        public bool HasCopyDependencies { get; set; }

        /// <summary>
        /// Indicates if this texture has any incompatible overlaps alive.
        /// </summary>
        public bool HasIncompatibleOverlaps => _incompatibleOverlaps.Count > 0;

        private readonly GpuContext _context;
        private readonly PhysicalMemory _physicalMemory;

        private int[] _allOffsets;
        private int[] _sliceSizes;
        private bool _is3D;
        private bool _hasMipViews;
        private bool _hasLayerViews;
        private int _layers;
        private int _levels;

        private MultiRange TextureRange => Storage.Range;

        /// <summary>
        /// The views list from the storage texture.
        /// </summary>
        private List<Texture> _views;
        private TextureGroupHandle[] _handles;
        private bool[] _loadNeeded;

        /// <summary>
        /// Other texture groups that have incompatible overlaps with this one.
        /// </summary>
        private List<TextureIncompatibleOverlap> _incompatibleOverlaps;
        private bool _incompatibleOverlapsDirty = true;
        private bool _flushIncompatibleOverlaps;

        /// <summary>
        /// Create a new texture group.
        /// </summary>
        /// <param name="context">GPU context that the texture group belongs to</param>
        /// <param name="physicalMemory">Physical memory where the <paramref name="storage"/> texture is mapped</param>
        /// <param name="storage">The storage texture for this group</param>
        /// <param name="incompatibleOverlaps">Groups that overlap with this one but are incompatible</param>
        public TextureGroup(GpuContext context, PhysicalMemory physicalMemory, Texture storage, List<TextureIncompatibleOverlap> incompatibleOverlaps)
        {
            Storage = storage;
            _context = context;
            _physicalMemory = physicalMemory;

            _is3D = storage.Info.Target == Target.Texture3D;
            _layers = storage.Info.GetSlices();
            _levels = storage.Info.Levels;

            _incompatibleOverlaps = incompatibleOverlaps;
            _flushIncompatibleOverlaps = TextureCompatibility.IsFormatHostIncompatible(storage.Info, context.Capabilities);
        }

        /// <summary>
        /// Initialize a new texture group's dirty regions and offsets.
        /// </summary>
        /// <param name="size">Size info for the storage texture</param>
        /// <param name="hasLayerViews">True if the storage will have layer views</param>
        /// <param name="hasMipViews">True if the storage will have mip views</param>
        public void Initialize(ref SizeInfo size, bool hasLayerViews, bool hasMipViews)
        {
            _allOffsets = size.AllOffsets;
            _sliceSizes = size.SliceSizes;

            (_hasLayerViews, _hasMipViews) = PropagateGranularity(hasLayerViews, hasMipViews);

            RecalculateHandleRegions();
        }

        /// <summary>
        /// Initialize all incompatible overlaps in the list, registering them with the other texture groups
        /// and creating copy dependencies when partially compatible.
        /// </summary>
        public void InitializeOverlaps()
        {
            foreach (TextureIncompatibleOverlap overlap in _incompatibleOverlaps)
            {
                if (overlap.Compatibility == TextureViewCompatibility.LayoutIncompatible)
                {
                    CreateCopyDependency(overlap.Group, false);
                }

                overlap.Group._incompatibleOverlaps.Add(new TextureIncompatibleOverlap(this, overlap.Compatibility));
                overlap.Group._incompatibleOverlapsDirty = true;
            }

            if (_incompatibleOverlaps.Count > 0)
            {
                SignalIncompatibleOverlapModified();
            }
        }

        /// <summary>
        /// Signal that the group is dirty to all views and the storage.
        /// </summary>
        private void SignalAllDirty()
        {
            Storage.SignalGroupDirty();
            if (_views != null)
            {
                foreach (Texture texture in _views)
                {
                    texture.SignalGroupDirty();
                }
            }
        }

        /// <summary>
        /// Signal that an incompatible overlap has been modified.
        /// If this group must flush incompatible overlaps, the group is signalled as dirty too.
        /// </summary>
        private void SignalIncompatibleOverlapModified()
        {
            _incompatibleOverlapsDirty = true;

            if (_flushIncompatibleOverlaps)
            {
                SignalAllDirty();
            }
        }


        /// <summary>
        /// Flushes incompatible overlaps if the storage format requires it, and they have been modified.
        /// This allows unsupported host formats to accept data written to format aliased textures.
        /// </summary>
        /// <returns>True if data was flushed, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FlushIncompatibleOverlapsIfNeeded()
        {
            if (_flushIncompatibleOverlaps && _incompatibleOverlapsDirty)
            {
                bool flushed = false;

                foreach (var overlap in _incompatibleOverlaps)
                {
                    flushed |= overlap.Group.Storage.FlushModified(true);
                }

                _incompatibleOverlapsDirty = false;

                return flushed;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check and optionally consume the dirty flags for a given texture.
        /// The state is shared between views of the same layers and levels.
        /// </summary>
        /// <param name="texture">The texture being used</param>
        /// <param name="consume">True to consume the dirty flags and reprotect, false to leave them as is</param>
        /// <returns>True if a flag was dirty, false otherwise</returns>
        public bool CheckDirty(Texture texture, bool consume)
        {
            bool dirty = false;

            EvaluateRelevantHandles(texture, (baseHandle, regionCount, split) =>
            {
                for (int i = 0; i < regionCount; i++)
                {
                    TextureGroupHandle group = _handles[baseHandle + i];

                    foreach (CpuRegionHandle handle in group.Handles)
                    {
                        if (handle.Dirty)
                        {
                            if (consume)
                            {
                                handle.Reprotect();
                            }

                            dirty = true;
                        }
                    }
                }
            });

            return dirty;
        }

        /// <summary>
        /// Synchronize memory for a given texture.
        /// If overlapping tracking handles are dirty, fully or partially synchronize the texture data.
        /// </summary>
        /// <param name="texture">The texture being used</param>
        public void SynchronizeMemory(Texture texture)
        {
            FlushIncompatibleOverlapsIfNeeded();

            EvaluateRelevantHandles(texture, (baseHandle, regionCount, split) =>
            {
                bool dirty = false;
                bool anyModified = false;
                bool anyUnmapped = false;

                for (int i = 0; i < regionCount; i++)
                {
                    TextureGroupHandle group = _handles[baseHandle + i];

                    bool modified = group.Modified;
                    bool handleDirty = false;
                    bool handleUnmapped = false;

                    foreach (CpuRegionHandle handle in group.Handles)
                    {
                        if (handle.Dirty)
                        {
                            handle.Reprotect();
                            handleDirty = true;
                        }
                        else
                        {
                            handleUnmapped |= handle.Unmapped;
                        }
                    }

                    // If the modified flag is still present, prefer the data written from gpu.
                    // A write from CPU will do a flush before writing its data, which should unset this.
                    if (modified)
                    {
                        handleDirty = false;
                    }

                    // Evaluate if any copy dependencies need to be fulfilled. A few rules:
                    // If the copy handle needs to be synchronized, prefer our own state.
                    // If we need to be synchronized and there is a copy present, prefer the copy.

                    if (group.NeedsCopy && group.Copy(_context))
                    {
                        anyModified |= true; // The copy target has been modified.
                        handleDirty = false;
                    }
                    else
                    {
                        anyModified |= modified;
                        dirty |= handleDirty;
                    }

                    anyUnmapped |= handleUnmapped;

                    if (group.NeedsCopy)
                    {
                        // The texture we copied from is still being written to. Copy from it again the next time this texture is used.
                        texture.SignalGroupDirty();
                    }

                    _loadNeeded[baseHandle + i] = handleDirty && !handleUnmapped;
                }

                if (dirty)
                {
                    if (anyUnmapped || (_handles.Length > 1 && (anyModified || split)))
                    {
                        // Partial texture invalidation. Only update the layers/levels with dirty flags of the storage.

                        SynchronizePartial(baseHandle, regionCount);
                    }
                    else
                    {
                        // Full texture invalidation.

                        texture.SynchronizeFull();
                    }
                }
            });
        }

        /// <summary>
        /// Synchronize part of the storage texture, represented by a given range of handles.
        /// Only handles marked by the _loadNeeded array will be synchronized.
        /// </summary>
        /// <param name="baseHandle">The base index of the range of handles</param>
        /// <param name="regionCount">The number of handles to synchronize</param>
        private void SynchronizePartial(int baseHandle, int regionCount)
        {
            for (int i = 0; i < regionCount; i++)
            {
                if (_loadNeeded[baseHandle + i])
                {
                    var info = GetHandleInformation(baseHandle + i);
                    int offsetIndex = info.Index;

                    // Only one of these will be greater than 1, as partial sync is only called when there are sub-image views.
                    for (int layer = 0; layer < info.Layers; layer++)
                    {
                        for (int level = 0; level < info.Levels; level++)
                        {
                            int offset = _allOffsets[offsetIndex];
                            int endOffset = Math.Min(offset + _sliceSizes[info.BaseLevel + level], (int)Storage.Size);
                            int size = endOffset - offset;

                            ReadOnlySpan<byte> data = _physicalMemory.GetSpan(Storage.Range.GetSlice((ulong)offset, (ulong)size));

                            data = Storage.ConvertToHostCompatibleFormat(data, info.BaseLevel, true);

                            Storage.SetData(data, info.BaseLayer, info.BaseLevel);

                            offsetIndex++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Synchronize dependent textures, if any of them have deferred a copy from the given texture.
        /// </summary>
        /// <param name="texture">The texture to synchronize dependents of</param>
        public void SynchronizeDependents(Texture texture)
        {
            EvaluateRelevantHandles(texture, (baseHandle, regionCount, split) =>
            {
                for (int i = 0; i < regionCount; i++)
                {
                    TextureGroupHandle group = _handles[baseHandle + i];

                    group.SynchronizeDependents();
                }
            });
        }

        /// <summary>
        /// Determines whether flushes in this texture group should be tracked.
        /// Incompatible overlaps may need data from this texture to flush tracked for it to be visible to them.
        /// </summary>
        /// <returns>True if flushes should be tracked, false otherwise</returns>
        private bool ShouldFlushTriggerTracking()
        {
            foreach (var overlap in _incompatibleOverlaps)
            {
                if (overlap.Group._flushIncompatibleOverlaps)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets data from the host GPU, and flushes a slice to guest memory.
        /// </summary>
        /// <remarks>
        /// This method should be used to retrieve data that was modified by the host GPU.
        /// This is not cheap, avoid doing that unless strictly needed.
        /// When possible, the data is written directly into guest memory, rather than copied.
        /// </remarks>
        /// <param name="tracked">True if writing the texture data is tracked, false otherwise</param>
        /// <param name="sliceIndex">The index of the slice to flush</param>
        /// <param name="texture">The specific host texture to flush. Defaults to the storage texture</param>
        private void FlushTextureDataSliceToGuest(bool tracked, int sliceIndex, ITexture texture = null)
        {
            (int layer, int level) = GetLayerLevelForView(sliceIndex);

            int offset = _allOffsets[sliceIndex];
            int endOffset = Math.Min(offset + _sliceSizes[level], (int)Storage.Size);
            int size = endOffset - offset;

            using WritableRegion region = _physicalMemory.GetWritableRegion(Storage.Range.GetSlice((ulong)offset, (ulong)size), tracked);

            Storage.GetTextureDataSliceFromGpu(region.Memory.Span, layer, level, tracked, texture);
        }

        /// <summary>
        /// Gets and flushes a number of slices of the storage texture to guest memory.
        /// </summary>
        /// <param name="tracked">True if writing the texture data is tracked, false otherwise</param>
        /// <param name="sliceStart">The first slice to flush</param>
        /// <param name="sliceEnd">The slice to finish flushing on (exclusive)</param>
        /// <param name="texture">The specific host texture to flush. Defaults to the storage texture</param>
        private void FlushSliceRange(bool tracked, int sliceStart, int sliceEnd, ITexture texture = null)
        {
            for (int i = sliceStart; i < sliceEnd; i++)
            {
                FlushTextureDataSliceToGuest(tracked, i, texture);
            }
        }

        /// <summary>
        /// Flush modified ranges for a given texture.
        /// </summary>
        /// <param name="texture">The texture being used</param>
        /// <param name="tracked">True if the flush writes should be tracked, false otherwise</param>
        /// <returns>True if data was flushed, false otherwise</returns>
        public bool FlushModified(Texture texture, bool tracked)
        {
            tracked = tracked || ShouldFlushTriggerTracking();
            bool flushed = false;

            EvaluateRelevantHandles(texture, (baseHandle, regionCount, split) =>
            {
                int startSlice = 0;
                int endSlice = 0;
                bool allModified = true;

                for (int i = 0; i < regionCount; i++)
                {
                    TextureGroupHandle group = _handles[baseHandle + i];

                    if (group.Modified)
                    {
                        if (endSlice < group.BaseSlice)
                        {
                            if (endSlice > startSlice)
                            {
                                FlushSliceRange(tracked, startSlice, endSlice);
                                flushed = true;
                            }

                            startSlice = group.BaseSlice;
                        }

                        endSlice = group.BaseSlice + group.SliceCount;

                        if (tracked)
                        {
                            group.Modified = false;

                            foreach (Texture texture in group.Overlaps)
                            {
                                texture.SignalModifiedDirty();
                            }
                        }
                    }
                    else
                    {
                        allModified = false;
                    }
                }

                if (endSlice > startSlice)
                {
                    if (allModified && !split)
                    {
                        texture.Flush(tracked);
                    }
                    else
                    {
                        FlushSliceRange(tracked, startSlice, endSlice);
                    }

                    flushed = true;
                }
            });

            Storage.SignalModifiedDirty();

            return flushed;
        }

        /// <summary>
        /// Clears competing modified flags for all incompatible ranges, if they have possibly been modified.
        /// </summary>
        /// <param name="texture">The texture that has been modified</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearIncompatibleOverlaps(Texture texture)
        {
            if (_incompatibleOverlapsDirty)
            {
                foreach (TextureIncompatibleOverlap incompatible in _incompatibleOverlaps)
                {
                    incompatible.Group.ClearModified(texture.Range, this);

                    incompatible.Group.SignalIncompatibleOverlapModified();
                }

                _incompatibleOverlapsDirty = false;
            }
        }

        /// <summary>
        /// Signal that a texture in the group has been modified by the GPU.
        /// </summary>
        /// <param name="texture">The texture that has been modified</param>
        public void SignalModified(Texture texture)
        {
            ClearIncompatibleOverlaps(texture);

            EvaluateRelevantHandles(texture, (baseHandle, regionCount, split) =>
            {
                for (int i = 0; i < regionCount; i++)
                {
                    TextureGroupHandle group = _handles[baseHandle + i];

                    group.SignalModified(_context);
                }
            });
        }

        /// <summary>
        /// Signal that a texture in the group is actively bound, or has been unbound by the GPU.
        /// </summary>
        /// <param name="texture">The texture that has been modified</param>
        /// <param name="bound">True if this texture is being bound, false if unbound</param>
        public void SignalModifying(Texture texture, bool bound)
        {
            ClearIncompatibleOverlaps(texture);

            EvaluateRelevantHandles(texture, (baseHandle, regionCount, split) =>
            {
                for (int i = 0; i < regionCount; i++)
                {
                    TextureGroupHandle group = _handles[baseHandle + i];

                    group.SignalModifying(bound, _context);
                }
            });
        }

        /// <summary>
        /// Register a read/write action to flush for a texture group.
        /// </summary>
        /// <param name="group">The group to register an action for</param>
        public void RegisterAction(TextureGroupHandle group)
        {
            foreach (CpuRegionHandle handle in group.Handles)
            {
                handle.RegisterAction((address, size) => FlushAction(group, address, size));
            }
        }

        /// <summary>
        /// Propagates the mip/layer view flags depending on the texture type.
        /// When the most granular type of subresource has views, the other type of subresource must be segmented granularly too.
        /// </summary>
        /// <param name="hasLayerViews">True if the storage has layer views</param>
        /// <param name="hasMipViews">True if the storage has mip views</param>
        /// <returns>The input values after propagation</returns>
        private (bool HasLayerViews, bool HasMipViews) PropagateGranularity(bool hasLayerViews, bool hasMipViews)
        {
            if (_is3D)
            {
                hasMipViews |= hasLayerViews;
            }
            else
            {
                hasLayerViews |= hasMipViews;
            }

            return (hasLayerViews, hasMipViews);
        }

        /// <summary>
        /// Evaluate the range of tracking handles which a view texture overlaps with.
        /// </summary>
        /// <param name="texture">The texture to get handles for</param>
        /// <param name="callback">
        /// A function to be called with the base index of the range of handles for the given texture, and the number of handles it covers.
        /// This can be called for multiple disjoint ranges, if required.
        /// </param>
        private void EvaluateRelevantHandles(Texture texture, HandlesCallbackDelegate callback)
        {
            if (texture == Storage || !(_hasMipViews || _hasLayerViews))
            {
                callback(0, _handles.Length);

                return;
            }

            EvaluateRelevantHandles(texture.FirstLayer, texture.FirstLevel, texture.Info.GetSlices(), texture.Info.Levels, callback);
        }

        /// <summary>
        /// Evaluate the range of tracking handles which a view texture overlaps with,
        /// using the view's position and slice/level counts.
        /// </summary>
        /// <param name="firstLayer">The first layer of the texture</param>
        /// <param name="firstLevel">The first level of the texture</param>
        /// <param name="slices">The slice count of the texture</param>
        /// <param name="levels">The level count of the texture</param>
        /// <param name="callback">
        /// A function to be called with the base index of the range of handles for the given texture, and the number of handles it covers.
        /// This can be called for multiple disjoint ranges, if required.
        /// </param>
        private void EvaluateRelevantHandles(int firstLayer, int firstLevel, int slices, int levels, HandlesCallbackDelegate callback)
        {
            int targetLayerHandles = _hasLayerViews ? slices : 1;
            int targetLevelHandles = _hasMipViews ? levels : 1;

            if (_is3D)
            {
                // Future mip levels come after all layers of the last mip level. Each mipmap has less layers (depth) than the last.

                if (!_hasLayerViews)
                {
                    // When there are no layer views, the mips are at a consistent offset.

                    callback(firstLevel, targetLevelHandles);
                }
                else
                {
                    (int levelIndex, int layerCount) = Get3DLevelRange(firstLevel);

                    if (levels > 1 && slices < _layers)
                    {
                        // The given texture only covers some of the depth of multiple mips. (a "depth slice")
                        // Callback with each mip's range separately.
                        // Can assume that the group is fully subdivided (both slices and levels > 1 for storage)

                        while (levels-- > 1)
                        {
                            callback(firstLayer + levelIndex, slices);

                            levelIndex += layerCount;
                            layerCount = Math.Max(layerCount >> 1, 1);
                            slices = Math.Max(layerCount >> 1, 1);
                        }
                    }
                    else
                    {
                        int totalSize = Math.Min(layerCount, slices);

                        while (levels-- > 1)
                        {
                            layerCount = Math.Max(layerCount >> 1, 1);
                            totalSize += layerCount;
                        }

                        callback(firstLayer + levelIndex, totalSize);
                    }
                }
            }
            else
            {
                // Future layers come after all mipmaps of the last.
                int levelHandles = _hasMipViews ? _levels : 1;

                if (slices > 1 && levels < _levels)
                {
                    // The given texture only covers some of the mipmaps of multiple slices. (a "mip slice")
                    // Callback with each layer's range separately.
                    // Can assume that the group is fully subdivided (both slices and levels > 1 for storage)

                    for (int i = 0; i < slices; i++)
                    {
                        callback(firstLevel + (firstLayer + i) * levelHandles, targetLevelHandles, true);
                    }
                }
                else
                {
                    callback(firstLevel + firstLayer * levelHandles, targetLevelHandles + (targetLayerHandles - 1) * levelHandles);
                }
            }
        }

        /// <summary>
        /// Get the range of offsets for a given mip level of a 3D texture.
        /// </summary>
        /// <param name="level">The level to return</param>
        /// <returns>Start index and count of offsets for the given level</returns>
        private (int Index, int Count) Get3DLevelRange(int level)
        {
            int index = 0;
            int count = _layers; // Depth. Halves with each mip level.

            while (level-- > 0)
            {
                index += count;
                count = Math.Max(count >> 1, 1);
            }

            return (index, count);
        }

        /// <summary>
        /// Get view information for a single tracking handle.
        /// </summary>
        /// <param name="handleIndex">The index of the handle</param>
        /// <returns>The layers and levels that the handle covers, and its index in the offsets array</returns>
        private (int BaseLayer, int BaseLevel, int Levels, int Layers, int Index) GetHandleInformation(int handleIndex)
        {
            int baseLayer;
            int baseLevel;
            int levels = _hasMipViews ? 1 : _levels;
            int layers = _hasLayerViews ? 1 : _layers;
            int index;

            if (_is3D)
            {
                if (_hasLayerViews)
                {
                    // NOTE: Will also have mip views, or only one level in storage.

                    index = handleIndex;
                    baseLevel = 0;

                    int levelLayers = _layers;

                    while (handleIndex >= levelLayers)
                    {
                        handleIndex -= levelLayers;
                        baseLevel++;
                        levelLayers = Math.Max(levelLayers >> 1, 1);
                    }

                    baseLayer = handleIndex;
                }
                else
                {
                    baseLayer = 0;
                    baseLevel = handleIndex;

                    (index, _) = Get3DLevelRange(baseLevel);
                }
            }
            else
            {
                baseLevel = _hasMipViews ? handleIndex % _levels : 0;
                baseLayer = _hasMipViews ? handleIndex / _levels : handleIndex;
                index = baseLevel + baseLayer * _levels;
            }

            return (baseLayer, baseLevel, levels, layers, index);
        }

        /// <summary>
        /// Gets the layer and level for a given view.
        /// </summary>
        /// <param name="index">The index of the view</param>
        /// <returns>The layer and level of the specified view</returns>
        private (int BaseLayer, int BaseLevel) GetLayerLevelForView(int index)
        {
            if (_is3D)
            {
                int baseLevel = 0;

                int levelLayers = _layers;

                while (index >= levelLayers)
                {
                    index -= levelLayers;
                    baseLevel++;
                    levelLayers = Math.Max(levelLayers >> 1, 1);
                }

                return (index, baseLevel);
            }
            else
            {
                return (index / _levels, index % _levels);
            }
        }

        /// <summary>
        /// Find the byte offset of a given texture relative to the storage.
        /// </summary>
        /// <param name="texture">The texture to locate</param>
        /// <returns>The offset of the texture in bytes</returns>
        public int FindOffset(Texture texture)
        {
            return _allOffsets[GetOffsetIndex(texture.FirstLayer, texture.FirstLevel)];
        }

        /// <summary>
        /// Find the offset index of a given layer and level.
        /// </summary>
        /// <param name="layer">The view layer</param>
        /// <param name="level">The view level</param>
        /// <returns>The offset index of the given layer and level</returns>
        public int GetOffsetIndex(int layer, int level)
        {
            if (_is3D)
            {
                return layer + Get3DLevelRange(level).Index;
            }
            else
            {
                return level + layer * _levels;
            }
        }

        /// <summary>
        /// The action to perform when a memory tracking handle is flipped to dirty.
        /// This notifies overlapping textures that the memory needs to be synchronized.
        /// </summary>
        /// <param name="groupHandle">The handle that a dirty flag was set on</param>
        private void DirtyAction(TextureGroupHandle groupHandle)
        {
            // Notify all textures that belong to this handle.

            Storage.SignalGroupDirty();

            lock (groupHandle.Overlaps)
            {
                foreach (Texture overlap in groupHandle.Overlaps)
                {
                    overlap.SignalGroupDirty();
                }
            }
        }

        /// <summary>
        /// Generate a CpuRegionHandle for a given address and size range in CPU VA.
        /// </summary>
        /// <param name="address">The start address of the tracked region</param>
        /// <param name="size">The size of the tracked region</param>
        /// <returns>A CpuRegionHandle covering the given range</returns>
        private CpuRegionHandle GenerateHandle(ulong address, ulong size)
        {
            return _physicalMemory.BeginTracking(address, size);
        }

        /// <summary>
        /// Generate a TextureGroupHandle covering a specified range of views.
        /// </summary>
        /// <param name="viewStart">The start view of the handle</param>
        /// <param name="views">The number of views to cover</param>
        /// <returns>A TextureGroupHandle covering the given views</returns>
        private TextureGroupHandle GenerateHandles(int viewStart, int views)
        {
            int offset = _allOffsets[viewStart];
            int endOffset = (viewStart + views == _allOffsets.Length) ? (int)Storage.Size : _allOffsets[viewStart + views];
            int size = endOffset - offset;

            var result = new List<CpuRegionHandle>();

            for (int i = 0; i < TextureRange.Count; i++)
            {
                MemoryRange item = TextureRange.GetSubRange(i);
                int subRangeSize = (int)item.Size;

                int sliceStart = Math.Clamp(offset, 0, subRangeSize);
                int sliceEnd = Math.Clamp(endOffset, 0, subRangeSize);

                if (sliceStart != sliceEnd && item.Address != MemoryManager.PteUnmapped)
                {
                    result.Add(GenerateHandle(item.Address + (ulong)sliceStart, (ulong)(sliceEnd - sliceStart)));
                }

                offset -= subRangeSize;
                endOffset -= subRangeSize;

                if (endOffset <= 0)
                {
                    break;
                }
            }

            (int firstLayer, int firstLevel) = GetLayerLevelForView(viewStart);

            if (_hasLayerViews && _hasMipViews)
            {
                size = _sliceSizes[firstLevel];
            }

            offset = _allOffsets[viewStart];
            ulong maxSize = Storage.Size - (ulong)offset;

            var groupHandle = new TextureGroupHandle(
                this,
                offset,
                Math.Min(maxSize, (ulong)size),
                _views,
                firstLayer,
                firstLevel,
                viewStart,
                views,
                result.ToArray());

            foreach (CpuRegionHandle handle in result)
            {
                handle.RegisterDirtyEvent(() => DirtyAction(groupHandle));
            }

            return groupHandle;
        }

        /// <summary>
        /// Update the views in this texture group, rebuilding the memory tracking if required.
        /// </summary>
        /// <param name="views">The views list of the storage texture</param>
        public void UpdateViews(List<Texture> views)
        {
            // This is saved to calculate overlapping views for each handle.
            _views = views;

            bool layerViews = _hasLayerViews;
            bool mipViews = _hasMipViews;
            bool regionsRebuilt = false;

            if (!(layerViews && mipViews))
            {
                foreach (Texture view in views)
                {
                    if (view.Info.GetSlices() < _layers)
                    {
                        layerViews = true;
                    }

                    if (view.Info.Levels < _levels)
                    {
                        mipViews = true;
                    }
                }

                (layerViews, mipViews) = PropagateGranularity(layerViews, mipViews);

                if (layerViews != _hasLayerViews || mipViews != _hasMipViews)
                {
                    _hasLayerViews = layerViews;
                    _hasMipViews = mipViews;

                    RecalculateHandleRegions();
                    regionsRebuilt = true;
                }
            }

            if (!regionsRebuilt)
            {
                // Must update the overlapping views on all handles, but only if they were not just recreated.

                foreach (TextureGroupHandle handle in _handles)
                {
                    handle.RecalculateOverlaps(this, views);
                }
            }

            SignalAllDirty();
        }

        /// <summary>
        /// Inherit handle state from an old set of handles, such as modified and dirty flags.
        /// </summary>
        /// <param name="oldHandles">The set of handles to inherit state from</param>
        /// <param name="handles">The set of handles inheriting the state</param>
        /// <param name="relativeOffset">The offset of the old handles in relation to the new ones</param>
        private void InheritHandles(TextureGroupHandle[] oldHandles, TextureGroupHandle[] handles, int relativeOffset)
        {
            foreach (var group in handles)
            {
                foreach (var handle in group.Handles)
                {
                    bool dirty = false;

                    foreach (var oldGroup in oldHandles)
                    {
                        if (group.OverlapsWith(oldGroup.Offset + relativeOffset, oldGroup.Size))
                        {
                            foreach (var oldHandle in oldGroup.Handles)
                            {
                                if (handle.OverlapsWith(oldHandle.Address, oldHandle.Size))
                                {
                                    dirty |= oldHandle.Dirty;
                                }
                            }

                            group.Inherit(oldGroup, group.Offset == oldGroup.Offset + relativeOffset);
                        }
                    }

                    if (dirty && !handle.Dirty)
                    {
                        handle.Reprotect(true);
                    }

                    if (group.Modified)
                    {
                        handle.RegisterAction((address, size) => FlushAction(group, address, size));
                    }
                }
            }

            foreach (var oldGroup in oldHandles)
            {
                oldGroup.Modified = false;
            }
        }

        /// <summary>
        /// Inherit state from another texture group.
        /// </summary>
        /// <param name="other">The texture group to inherit from</param>
        public void Inherit(TextureGroup other)
        {
            bool layerViews = _hasLayerViews || other._hasLayerViews;
            bool mipViews = _hasMipViews || other._hasMipViews;

            if (layerViews != _hasLayerViews || mipViews != _hasMipViews)
            {
                _hasLayerViews = layerViews;
                _hasMipViews = mipViews;

                RecalculateHandleRegions();
            }

            foreach (TextureIncompatibleOverlap incompatible in other._incompatibleOverlaps)
            {
                RegisterIncompatibleOverlap(incompatible, false);

                incompatible.Group._incompatibleOverlaps.RemoveAll(overlap => overlap.Group == other);
            }

            int relativeOffset = Storage.Range.FindOffset(other.Storage.Range);

            InheritHandles(other._handles, _handles, relativeOffset);
        }

        /// <summary>
        /// Replace the current handles with the new handles. It is assumed that the new handles start dirty.
        /// The dirty flags from the previous handles will be kept.
        /// </summary>
        /// <param name="handles">The handles to replace the current handles with</param>
        private void ReplaceHandles(TextureGroupHandle[] handles)
        {
            if (_handles != null)
            {
                // When replacing handles, they should start as non-dirty.

                foreach (TextureGroupHandle groupHandle in handles)
                {
                    foreach (CpuRegionHandle handle in groupHandle.Handles)
                    {
                        handle.Reprotect();
                    }
                }

                InheritHandles(_handles, handles, 0);

                foreach (var oldGroup in _handles)
                {
                    foreach (var oldHandle in oldGroup.Handles)
                    {
                        oldHandle.Dispose();
                    }
                }
            }

            _handles = handles;
            _loadNeeded = new bool[_handles.Length];
        }

        /// <summary>
        /// Recalculate handle regions for this texture group, and inherit existing state into the new handles.
        /// </summary>
        private void RecalculateHandleRegions()
        {
            TextureGroupHandle[] handles;

            if (!(_hasMipViews || _hasLayerViews))
            {
                // Single dirty region.
                var cpuRegionHandles = new CpuRegionHandle[TextureRange.Count];
                int count = 0;

                for (int i = 0; i < TextureRange.Count; i++)
                {
                    var currentRange = TextureRange.GetSubRange(i);
                    if (currentRange.Address != MemoryManager.PteUnmapped)
                    {
                        cpuRegionHandles[count++] = GenerateHandle(currentRange.Address, currentRange.Size);
                    }
                }

                if (count != TextureRange.Count)
                {
                    Array.Resize(ref cpuRegionHandles, count);
                }

                var groupHandle = new TextureGroupHandle(this, 0, Storage.Size, _views, 0, 0, 0, _allOffsets.Length, cpuRegionHandles);

                foreach (CpuRegionHandle handle in cpuRegionHandles)
                {
                    handle.RegisterDirtyEvent(() => DirtyAction(groupHandle));
                }

                handles = new TextureGroupHandle[] { groupHandle };
            }
            else
            {
                // Get views for the host texture.
                // It's worth noting that either the texture has layer views or mip views when getting to this point, which simplifies the logic a little.
                // Depending on if the texture is 3d, either the mip views imply that layer views are present (2d) or the other way around (3d).
                // This is enforced by the way the texture matched as a view, so we don't need to check.

                int layerHandles = _hasLayerViews ? _layers : 1;
                int levelHandles = _hasMipViews ? _levels : 1;

                int handleIndex = 0;

                if (_is3D)
                {
                    var handlesList = new List<TextureGroupHandle>();

                    for (int i = 0; i < levelHandles; i++)
                    {
                        for (int j = 0; j < layerHandles; j++)
                        {
                            (int viewStart, int views) = Get3DLevelRange(i);
                            viewStart += j;
                            views = _hasLayerViews ? 1 : views; // A layer view is also a mip view.

                            handlesList.Add(GenerateHandles(viewStart, views));
                        }

                        layerHandles = Math.Max(1, layerHandles >> 1);
                    }

                    handles = handlesList.ToArray();
                }
                else
                {
                    handles = new TextureGroupHandle[layerHandles * levelHandles];

                    for (int i = 0; i < layerHandles; i++)
                    {
                        for (int j = 0; j < levelHandles; j++)
                        {
                            int viewStart = j + i * _levels;
                            int views = _hasMipViews ? 1 : _levels; // A mip view is also a layer view.

                            handles[handleIndex++] = GenerateHandles(viewStart, views);
                        }
                    }
                }
            }

            ReplaceHandles(handles);
        }

        /// <summary>
        /// Ensure that there is a handle for each potential texture view. Required for copy dependencies to work.
        /// </summary>
        private void EnsureFullSubdivision()
        {
            if (!(_hasLayerViews && _hasMipViews))
            {
                _hasLayerViews = true;
                _hasMipViews = true;

                RecalculateHandleRegions();
            }
        }

        /// <summary>
        /// Create a copy dependency between this texture group, and a texture at a given layer/level offset.
        /// </summary>
        /// <param name="other">The view compatible texture to create a dependency to</param>
        /// <param name="firstLayer">The base layer of the given texture relative to the storage</param>
        /// <param name="firstLevel">The base level of the given texture relative to the storage</param>
        /// <param name="copyTo">True if this texture is first copied to the given one, false for the opposite direction</param>
        public void CreateCopyDependency(Texture other, int firstLayer, int firstLevel, bool copyTo)
        {
            TextureGroup otherGroup = other.Group;

            EnsureFullSubdivision();
            otherGroup.EnsureFullSubdivision();

            // Get the location of each texture within its storage, so we can find the handles to apply the dependency to.
            // This can consist of multiple disjoint regions, for example if this is a mip slice of an array texture.

            var targetRange = new List<(int BaseHandle, int RegionCount)>();
            var otherRange = new List<(int BaseHandle, int RegionCount)>();

            EvaluateRelevantHandles(firstLayer, firstLevel, other.Info.GetSlices(), other.Info.Levels, (baseHandle, regionCount, split) => targetRange.Add((baseHandle, regionCount)));
            otherGroup.EvaluateRelevantHandles(other, (baseHandle, regionCount, split) => otherRange.Add((baseHandle, regionCount)));

            int targetIndex = 0;
            int otherIndex = 0;
            (int Handle, int RegionCount) targetRegion = (0, 0);
            (int Handle, int RegionCount) otherRegion = (0, 0);

            while (true)
            {
                if (targetRegion.RegionCount == 0)
                {
                    if (targetIndex >= targetRange.Count)
                    {
                        break;
                    }

                    targetRegion = targetRange[targetIndex++];
                }

                if (otherRegion.RegionCount == 0)
                {
                    if (otherIndex >= otherRange.Count)
                    {
                        break;
                    }

                    otherRegion = otherRange[otherIndex++];
                }

                TextureGroupHandle handle = _handles[targetRegion.Handle++];
                TextureGroupHandle otherHandle = other.Group._handles[otherRegion.Handle++];

                targetRegion.RegionCount--;
                otherRegion.RegionCount--;

                handle.CreateCopyDependency(otherHandle, copyTo);

                // If "copyTo" is true, this texture must copy to the other.
                // Otherwise, it must copy to this texture.

                if (copyTo)
                {
                    otherHandle.Copy(_context, handle);
                }
                else
                {
                    handle.Copy(_context, otherHandle);
                }
            }
        }

        /// <summary>
        /// Creates a copy dependency to another texture group, where handles overlap.
        /// Scans through all handles to find compatible patches in the other group.
        /// </summary>
        /// <param name="other">The texture group that overlaps this one</param>
        /// <param name="copyTo">True if this texture is first copied to the given one, false for the opposite direction</param>
        public void CreateCopyDependency(TextureGroup other, bool copyTo)
        {
            for (int i = 0; i < _allOffsets.Length; i++)
            {
                (int layer, int level) = GetLayerLevelForView(i);
                MultiRange handleRange = Storage.Range.GetSlice((ulong)_allOffsets[i], 1);
                ulong handleBase = handleRange.GetSubRange(0).Address;

                for (int j = 0; j < other._handles.Length; j++)
                {
                    (int otherLayer, int otherLevel) = other.GetLayerLevelForView(j);
                    MultiRange otherHandleRange = other.Storage.Range.GetSlice((ulong)other._allOffsets[j], 1);
                    ulong otherHandleBase = otherHandleRange.GetSubRange(0).Address;

                    if (handleBase == otherHandleBase)
                    {
                        // Check if the two sizes are compatible.
                        TextureInfo info = Storage.Info;
                        TextureInfo otherInfo = other.Storage.Info;

                        if (TextureCompatibility.ViewLayoutCompatible(info, otherInfo, level, otherLevel) &&
                            TextureCompatibility.CopySizeMatches(info, otherInfo, level, otherLevel))
                        {
                            // These textures are copy compatible. Create the dependency.

                            EnsureFullSubdivision();
                            other.EnsureFullSubdivision();

                            TextureGroupHandle handle = _handles[i];
                            TextureGroupHandle otherHandle = other._handles[j];

                            handle.CreateCopyDependency(otherHandle, copyTo);

                            // If "copyTo" is true, this texture must copy to the other.
                            // Otherwise, it must copy to this texture.

                            if (copyTo)
                            {
                                otherHandle.Copy(_context, handle);
                            }
                            else
                            {
                                handle.Copy(_context, otherHandle);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers another texture group as an incompatible overlap, if not already registered.
        /// </summary>
        /// <param name="other">The texture group to add to the incompatible overlaps list</param>
        /// <param name="copy">True if the overlap should register copy dependencies</param>
        public void RegisterIncompatibleOverlap(TextureIncompatibleOverlap other, bool copy)
        {
            if (!_incompatibleOverlaps.Exists(overlap => overlap.Group == other.Group))
            {
                if (copy && other.Compatibility == TextureViewCompatibility.LayoutIncompatible)
                {
                    // Any of the group's views may share compatibility, even if the parents do not fully.
                    CreateCopyDependency(other.Group, false);
                }

                _incompatibleOverlaps.Add(other);
                other.Group._incompatibleOverlaps.Add(new TextureIncompatibleOverlap(this, other.Compatibility));
            }

            other.Group.SignalIncompatibleOverlapModified();
            SignalIncompatibleOverlapModified();
        }

        /// <summary>
        /// Clear modified flags in the given range.
        /// This will stop any GPU written data from flushing or copying to dependent textures.
        /// </summary>
        /// <param name="range">The range to clear modified flags in</param>
        /// <param name="ignore">Ignore handles that have a copy dependency to the specified group</param>
        public void ClearModified(MultiRange range, TextureGroup ignore = null)
        {
            TextureGroupHandle[] handles = _handles;

            foreach (TextureGroupHandle handle in handles)
            {
                // Handles list is not modified by another thread, only replaced, so this is thread safe.
                // Remove modified flags from all overlapping handles, so that the textures don't flush to unmapped/remapped GPU memory.

                MultiRange subRange = Storage.Range.GetSlice((ulong)handle.Offset, (ulong)handle.Size);

                if (range.OverlapsWith(subRange))
                {
                    if ((ignore == null || !handle.HasDependencyTo(ignore)) && handle.Modified)
                    {
                        handle.Modified = false;
                        Storage.SignalModifiedDirty();

                        lock (handle.Overlaps)
                        {
                            foreach (Texture texture in handle.Overlaps)
                            {
                                texture.SignalModifiedDirty();
                            }
                        }
                    }
                }
            }

            Storage.SignalModifiedDirty();

            if (_views != null)
            {
                foreach (Texture texture in _views)
                {
                    texture.SignalModifiedDirty();
                }
            }
        }

        /// <summary>
        /// A flush has been requested on a tracked region. Flush texture data for the given handle.
        /// </summary>
        /// <param name="handle">The handle this flush action is for</param>
        /// <param name="address">The address of the flushing memory access</param>
        /// <param name="size">The size of the flushing memory access</param>
        public void FlushAction(TextureGroupHandle handle, ulong address, ulong size)
        {
            if (!handle.Modified)
            {
                return;
            }

            _context.Renderer.BackgroundContextAction(() =>
            {
                handle.Sync(_context);

                Storage.SignalModifiedDirty();

                lock (handle.Overlaps)
                {
                    foreach (Texture texture in handle.Overlaps)
                    {
                        texture.SignalModifiedDirty();
                    }
                }

                if (TextureCompatibility.CanTextureFlush(Storage.Info, _context.Capabilities))
                {
                    FlushSliceRange(false, handle.BaseSlice, handle.BaseSlice + handle.SliceCount, Storage.GetFlushTexture());
                }
            });
        }

        /// <summary>
        /// Dispose this texture group, disposing all related memory tracking handles.
        /// </summary>
        public void Dispose()
        {
            foreach (TextureGroupHandle group in _handles)
            {
                group.Dispose();
            }

            foreach (TextureIncompatibleOverlap incompatible in _incompatibleOverlaps)
            {
                incompatible.Group._incompatibleOverlaps.RemoveAll(overlap => overlap.Group == this);
            }
        }
    }
}
