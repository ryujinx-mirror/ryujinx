using NUnit.Framework;
using Ryujinx.Audio.Renderer.Parameter;
using System.Runtime.CompilerServices;

namespace Ryujinx.Tests.Audio.Renderer
{
    class MemoryPoolParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0x20, Unsafe.SizeOf<MemoryPoolInParameter>());
            Assert.AreEqual(0x10, Unsafe.SizeOf<MemoryPoolOutStatus>());
        }
    }
}
