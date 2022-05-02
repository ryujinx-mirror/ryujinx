using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Memory.Tests
{
    public class Tests
    {
        private const ulong MemorySize = 0x8000;

        private MemoryBlock _memoryBlock;

        [SetUp]
        public void Setup()
        {
            _memoryBlock = new MemoryBlock(MemorySize);
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

        [Test, Explicit]
        public void Test_Alias()
        {
            using MemoryBlock backing = new MemoryBlock(0x10000, MemoryAllocationFlags.Mirrorable);
            using MemoryBlock toAlias = new MemoryBlock(0x10000, MemoryAllocationFlags.Reserve | MemoryAllocationFlags.ViewCompatible);

            toAlias.MapView(backing, 0x1000, 0, 0x4000);
            toAlias.UnmapView(backing, 0x3000, 0x1000);

            toAlias.Write(0, 0xbadc0de);
            Assert.AreEqual(Marshal.ReadInt32(backing.Pointer, 0x1000), 0xbadc0de);
        }

        [Test, Explicit]
        public void Test_AliasRandom()
        {
            using MemoryBlock backing = new MemoryBlock(0x80000, MemoryAllocationFlags.Mirrorable);
            using MemoryBlock toAlias = new MemoryBlock(0x80000, MemoryAllocationFlags.Reserve | MemoryAllocationFlags.ViewCompatible);

            Random rng = new Random(123);

            for (int i = 0; i < 20000; i++)
            {
                int srcPage = rng.Next(0, 64);
                int dstPage = rng.Next(0, 64);
                int pages = rng.Next(1, 65);

                if ((rng.Next() & 1) != 0)
                {
                    toAlias.MapView(backing, (ulong)srcPage << 12, (ulong)dstPage << 12, (ulong)pages << 12);

                    int offset = rng.Next(0, 0x1000 - sizeof(int));

                    toAlias.Write((ulong)((dstPage << 12) + offset), 0xbadc0de);
                    Assert.AreEqual(Marshal.ReadInt32(backing.Pointer, (srcPage << 12) + offset), 0xbadc0de);
                }
                else
                {
                    toAlias.UnmapView(backing, (ulong)dstPage << 12, (ulong)pages << 12);
                }
            }
        }
    }
}