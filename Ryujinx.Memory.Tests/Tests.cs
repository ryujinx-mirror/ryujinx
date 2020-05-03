using NUnit.Framework;
using Ryujinx.Memory;
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
    }
}