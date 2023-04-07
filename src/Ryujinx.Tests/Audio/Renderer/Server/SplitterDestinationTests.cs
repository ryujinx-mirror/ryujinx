using NUnit.Framework;
using Ryujinx.Audio.Renderer.Server.Splitter;
using System.Runtime.CompilerServices;

namespace Ryujinx.Tests.Audio.Renderer.Server
{
    class SplitterDestinationTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0xE0, Unsafe.SizeOf<SplitterDestination>());
        }
    }
}
