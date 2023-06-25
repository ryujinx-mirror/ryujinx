using NUnit.Framework;
using Ryujinx.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Tests.Memory
{
    public class Tests
    {
        private static readonly ulong _memorySize = MemoryBlock.GetPageSize() * 8;

        private MemoryBlock _memoryBlock;

        [SetUp]
        public void Setup()
        {
            _memoryBlock = new MemoryBlock(_memorySize);
        }

        [TearDown]
        public void Teardown()
        {
            _memoryBlock.Dispose();
        }

        [Test]
        public void Test_Read()
        {
            Marshal.WriteInt32(_memoryBlock.Pointer, 0x2020, 0x1234abcd);

            Assert.AreEqual(_memoryBlock.Read<int>(0x2020), 0x1234abcd);
        }

        [Test]
        public void Test_Write()
        {
            _memoryBlock.Write(0x2040, 0xbadc0de);

            Assert.AreEqual(Marshal.ReadInt32(_memoryBlock.Pointer, 0x2040), 0xbadc0de);
        }

        [Test]
        // Memory aliasing tests fail on CI at the moment.
        [Platform(Exclude = "MacOsX")]
        public void Test_Alias()
        {
            ulong pageSize = MemoryBlock.GetPageSize();
            ulong blockSize = MemoryBlock.GetPageSize() * 16;

            using MemoryBlock backing = new(blockSize, MemoryAllocationFlags.Mirrorable);
            using MemoryBlock toAlias = new(blockSize, MemoryAllocationFlags.Reserve | MemoryAllocationFlags.ViewCompatible);

            toAlias.MapView(backing, pageSize, 0, pageSize * 4);
            toAlias.UnmapView(backing, pageSize * 3, pageSize);

            toAlias.Write(0, 0xbadc0de);
            Assert.AreEqual(Marshal.ReadInt32(backing.Pointer, (int)pageSize), 0xbadc0de);
        }

        [Test]
        // Memory aliasing tests fail on CI at the moment.
        [Platform(Exclude = "MacOsX")]
        public void Test_AliasRandom()
        {
            ulong pageSize = MemoryBlock.GetPageSize();
            int pageBits = (int)ulong.Log2(pageSize);
            ulong blockSize = MemoryBlock.GetPageSize() * 128;

            using MemoryBlock backing = new(blockSize, MemoryAllocationFlags.Mirrorable);
            using MemoryBlock toAlias = new(blockSize, MemoryAllocationFlags.Reserve | MemoryAllocationFlags.ViewCompatible);

            Random rng = new(123);

            for (int i = 0; i < 20000; i++)
            {
                int srcPage = rng.Next(0, 64);
                int dstPage = rng.Next(0, 64);
                int pages = rng.Next(1, 65);

                if ((rng.Next() & 1) != 0)
                {
                    toAlias.MapView(backing, (ulong)srcPage << pageBits, (ulong)dstPage << pageBits, (ulong)pages << pageBits);

                    int offset = rng.Next(0, (int)pageSize - sizeof(int));

                    toAlias.Write((ulong)((dstPage << pageBits) + offset), 0xbadc0de);
                    Assert.AreEqual(Marshal.ReadInt32(backing.Pointer, (srcPage << pageBits) + offset), 0xbadc0de);
                }
                else
                {
                    toAlias.UnmapView(backing, (ulong)dstPage << pageBits, (ulong)pages << pageBits);
                }
            }
        }

        [Test]
        // Memory aliasing tests fail on CI at the moment.
        [Platform(Exclude = "MacOsX")]
        public void Test_AliasMapLeak()
        {
            ulong pageSize = MemoryBlock.GetPageSize();
            ulong size = 100000 * pageSize; // The mappings limit on Linux is usually around 65K, so let's make sure we are above that.

            using MemoryBlock backing = new(pageSize, MemoryAllocationFlags.Mirrorable);
            using MemoryBlock toAlias = new(size, MemoryAllocationFlags.Reserve | MemoryAllocationFlags.ViewCompatible);

            for (ulong offset = 0; offset < size; offset += pageSize)
            {
                toAlias.MapView(backing, 0, offset, pageSize);

                toAlias.Write(offset, 0xbadc0de);
                Assert.AreEqual(0xbadc0de, backing.Read<int>(0));

                toAlias.UnmapView(backing, offset, pageSize);
            }
        }
    }
}
