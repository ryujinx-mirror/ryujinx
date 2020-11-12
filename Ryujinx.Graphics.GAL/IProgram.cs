using System;

namespace Ryujinx.Graphics.GAL
{
    public interface IProgram : IDisposable
    {
        byte[] GetBinary();
    }
}
