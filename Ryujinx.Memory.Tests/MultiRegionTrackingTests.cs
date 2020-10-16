using NUnit.Framework;
using Ryujinx.Memory.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Memory.Tests
{
    public class MultiRegionTrackingTests
    {
        private const int RndCnt = 3;

        private const ulong MemorySize = 0x8000;
        private const int PageSize = 4096;

        private MemoryBlock _memoryBlock;
        private MemoryTracking _tracking;
        private MockVirtualMemoryManager _memoryManager;

        [SetUp]
        public void Setup()
        {
            _memoryBlock = new MemoryBlock(MemorySize);
            _memoryManager = new MockVirtualMemoryManager(MemorySize, PageSize);
            _tracking = new MemoryTracking(_memoryManager, _memoryBlock, PageSize);
        }

        [TearDown]
        public void Teardown()
        {
            _memoryBlock.Dispose();
        }

        private IMultiRegionHandle GetGranular(bool smart, ulong address, ulong size, ulong granularity)
        {
            return smart ?
                _tracking.BeginSmartGranularTracking(address, size, granularity) :
                (IMultiRegionHandle)_tracking.BeginGranularTracking(address, size, granularity);
        }

        private void RandomOrder(Random random, List<int> indices, Action<int> action)
        {
            List<int> choices = indices.ToList();

            while (choices.Count > 0)
            {
                int choice = random.Next(choices.Count);
                action(choices[choice]);
                choices.RemoveAt(choice);
            }
        }

        private int ExpectQueryInOrder(IMultiRegionHandle handle, ulong startAddress, ulong size, Func<ulong, bool> addressPredicate)
        {
            int regionCount = 0;
            ulong lastAddress = startAddress;

            handle.QueryModified(startAddress, size, (address, range) =>
            {
                Assert.IsTrue(addressPredicate(address)); // Written pages must be even.
                Assert.GreaterOrEqual(address, lastAddress); // Must be signalled in ascending order, regardless of write order.
                lastAddress = address;
                regionCount++;
            });

            return regionCount;
        }

        private int ExpectQueryInOrder(IMultiRegionHandle handle, ulong startAddress, ulong size, Func<ulong, bool> addressPredicate, int sequenceNumber)
        {
            int regionCount = 0;
            ulong lastAddress = startAddress;

            handle.QueryModified(startAddress, size, (address, range) =>
            {
                Assert.IsTrue(addressPredicate(address)); // Written pages must be even.
                Assert.GreaterOrEqual(address, lastAddress); // Must be signalled in ascending order, regardless of write order.
                lastAddress = address;
                regionCount++;
            }, sequenceNumber);

            return regionCount;
        }

        private void PreparePages(IMultiRegionHandle handle, int pageCount, ulong address = 0)
        {
            Random random = new Random();

            // Make sure the list has minimum granularity (smart region changes granularity based on requested ranges)
            RandomOrder(random, Enumerable.Range(0, pageCount).ToList(), (i) =>
            {
                ulong resultAddress = ulong.MaxValue;
                handle.QueryModified((ulong)i * PageSize + address, PageSize, (address, range) =>
                {
                    resultAddress = address;
                });
                Assert.AreEqual(resultAddress, (ulong)i * PageSize + address);
            });
        }

        [Test]
        public void DirtyRegionOrdering([Values] bool smart)
        {
            const int pageCount = 32;
            IMultiRegionHandle handle = GetGranular(smart, 0, PageSize * pageCount, PageSize);

            Random random = new Random();

            PreparePages(handle, pageCount);

            IEnumerable<int> halfRange = Enumerable.Range(0, pageCount / 2);
            List<int> odd = halfRange.Select(x => x * 2 + 1).ToList();
            List<int> even = halfRange.Select(x => x * 2).ToList();

            // Write to all the odd pages.
            RandomOrder(random, odd, (i) =>
            {
                _tracking.VirtualMemoryEvent((ulong)i * PageSize, PageSize, true);
            });

            int oddRegionCount = ExpectQueryInOrder(handle, 0, PageSize * pageCount, (address) => (address / PageSize) % 2 == 1);

            Assert.AreEqual(oddRegionCount, pageCount / 2); // Must have written to all odd pages.

            // Write to all the even pages.
            RandomOrder(random, even, (i) =>
            {
                _tracking.VirtualMemoryEvent((ulong)i * PageSize, PageSize, true);
            });

            int evenRegionCount = ExpectQueryInOrder(handle, 0, PageSize * pageCount, (address) => (address / PageSize) % 2 == 0);

            Assert.AreEqual(evenRegionCount, pageCount / 2);
        }

        [Test]
        public void SequenceNumber([Values] bool smart)
        {
            // The sequence number can be used to ignore dirty flags, and defer their consumption until later.
            // If a user consumes a dirty flag with sequence number 1, then there is a write to the protected region,
            // the dirty flag will not be acknowledged until the sequence number is 2.

            // This is useful for situations where we know that the data was complete when the sequence number was set.
            // ...essentially, when that data can only be updated on a future sequence number.

            const int pageCount = 32;
            IMultiRegionHandle handle = GetGranular(smart, 0, PageSize * pageCount, PageSize);

            PreparePages(handle, pageCount);

            Random random = new Random();

            IEnumerable<int> halfRange = Enumerable.Range(0, pageCount / 2);
            List<int> odd = halfRange.Select(x => x * 2 + 1).ToList();
            List<int> even = halfRange.Select(x => x * 2).ToList();

            // Write to all the odd pages.
            RandomOrder(random, odd, (i) =>
            {
                _tracking.VirtualMemoryEvent((ulong)i * PageSize, PageSize, true);
            });

            int oddRegionCount = 0;

            // Track with sequence number 1. Future dirty flags should only be consumed with sequence number != 1.
            // Only track the odd pages, so the even ones don't have their sequence number set.

            foreach (int index in odd)
            {
                handle.QueryModified((ulong)index * PageSize, PageSize, (address, range) =>
                {
                    oddRegionCount++;
                }, 1);
            }

            Assert.AreEqual(oddRegionCount, pageCount / 2); // Must have written to all odd pages.

            // Write to all pages.

            _tracking.VirtualMemoryEvent(0, PageSize * pageCount, true);

            // Only the even regions should be reported for sequence number 1.

            int evenRegionCount = ExpectQueryInOrder(handle, 0, PageSize * pageCount, (address) => (address / PageSize) % 2 == 0, 1);

            Assert.AreEqual(evenRegionCount, pageCount / 2); // Must have written to all even pages.

            oddRegionCount = 0;

            handle.QueryModified(0, PageSize * pageCount, (address, range) => { oddRegionCount++; }, 1);

            Assert.AreEqual(oddRegionCount, 0); // Sequence number has not changed, so found no dirty subregions.

            // With sequence number 2, all all pages should be reported as modified.

            oddRegionCount = ExpectQueryInOrder(handle, 0, PageSize * pageCount, (address) => (address / PageSize) % 2 == 1, 2);

            Assert.AreEqual(oddRegionCount, pageCount / 2); // Must have written to all odd pages.
        }

        [Test]
        public void SmartRegionTracking()
        {
            // Smart multi region handles dynamically change their tracking granularity based on QueryMemory calls.
            // This can save on reprotects on larger resources.

            const int pageCount = 32;
            IMultiRegionHandle handle = GetGranular(true, 0, PageSize * pageCount, PageSize);

            // Query some large regions to prep the subdivision of the tracking region.

            int[] regionSizes = new int[] { 6, 4, 3, 2, 6, 1 };
            ulong address = 0;

            for (int i = 0; i < regionSizes.Length; i++)
            {
                int region = regionSizes[i];
                handle.QueryModified(address, (ulong)(PageSize * region), (address, size) => { });
                
                // There should be a gap between regions,
                // So that they don't combine and we can see the full effects.
                address += (ulong)(PageSize * (region + 1));
            }

            // Clear modified.
            handle.QueryModified((address, size) => { });

            // Trigger each region with a 1 byte write.
            address = 0;

            for (int i = 0; i < regionSizes.Length; i++)
            {
                int region = regionSizes[i];
                _tracking.VirtualMemoryEvent(address, 1, true);
                address += (ulong)(PageSize * (region + 1));
            }

            int regionInd = 0;
            ulong expectedAddress = 0;

            // Expect each region to trigger in its entirety, in address ascending order.
            handle.QueryModified((address, size) => {
                int region = regionSizes[regionInd++];

                Assert.AreEqual(address, expectedAddress);
                Assert.AreEqual(size, (ulong)(PageSize * region));

                expectedAddress += (ulong)(PageSize * (region + 1));
            });
        }

        [Test]
        public void DisposeMultiHandles([Values] bool smart)
        {
            // Create and initialize two overlapping Multi Region Handles, with PageSize granularity.
            const int pageCount = 32;
            const int overlapStart = 16;

            Assert.AreEqual((0, 0), _tracking.GetRegionCounts());

            IMultiRegionHandle handleLow = GetGranular(smart, 0, PageSize * pageCount, PageSize);
            PreparePages(handleLow, pageCount);

            Assert.AreEqual((pageCount, pageCount), _tracking.GetRegionCounts());

            IMultiRegionHandle handleHigh = GetGranular(smart, PageSize * overlapStart, PageSize * pageCount, PageSize);
            PreparePages(handleHigh, pageCount, PageSize * overlapStart);

            // Combined pages (and assuming overlapStart <= pageCount) should be pageCount after overlapStart.
            int totalPages = overlapStart + pageCount;

            Assert.AreEqual((totalPages, totalPages), _tracking.GetRegionCounts());

            handleLow.Dispose(); // After disposing one, the pages for the other remain.

            Assert.AreEqual((pageCount, pageCount), _tracking.GetRegionCounts());

            handleHigh.Dispose(); // After disposing the other, there are no pages left.

            Assert.AreEqual((0, 0), _tracking.GetRegionCounts());
        }
    }
}
