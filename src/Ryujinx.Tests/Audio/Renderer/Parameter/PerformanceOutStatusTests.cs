using NUnit.Framework;
using Ryujinx.Audio.Renderer.Parameter.Performance;
using System.Runtime.CompilerServices;

namespace Ryujinx.Tests.Audio.Renderer.Parameter
{
    class PerformanceOutStatusTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0x10, Unsafe.SizeOf<PerformanceOutStatus>());
        }
    }
}
