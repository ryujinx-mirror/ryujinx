using NUnit.Framework;
using Ryujinx.Audio.Renderer.Server.Mix;
using System.Runtime.CompilerServices;

namespace Ryujinx.Tests.Audio.Renderer.Server
{
    class MixStateTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0x940, Unsafe.SizeOf<MixState>());
        }
    }
}
